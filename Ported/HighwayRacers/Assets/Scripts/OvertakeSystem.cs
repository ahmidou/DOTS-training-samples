using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class OvertakeSystem : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    [BurstCompile]
    struct OvertakeSystemJob : IJobForEach<Overtake>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;
        public float deltaTime;


        public void Execute(ref Overtake o)
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as [ReadOnly], which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;

            if (o.laneChangeRatio < 0.0f)
            {
                // Component has just been activated
                o.laneChangeRatio = 0.0f;
            }
            if (o.laneChangeRatio < 1.0f)
            {
                // Regular case, update overtake stuff
                o.laneChangeRatio += deltaTime * o.laneChangeSpeed;
            }
            else
            {
                // Overtake complete
                o.currentLane = o.futureLane;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new OvertakeSystemJob()
        {
            deltaTime = UnityEngine.Time.deltaTime
        };
        

        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;

        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}