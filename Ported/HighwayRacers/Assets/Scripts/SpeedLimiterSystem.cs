using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

public class SpeedLimiter : JobComponentSystem
{
    EntityQuery m_Query;

    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(typeof(Mover));
    }

    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    //[BurstCompile]
    struct SpeedLimiterJob : IJobForEach<SpeedLimit>
    {
        // Add fields here that your job needs to do its work.
        [DeallocateOnJobCompletion] public NativeArray<Mover> movers;
        public static float trackInvLength;
        static readonly float minDistanceToLimit = 10.0f;

        public void Execute(ref SpeedLimit limitInfos)
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as [ReadOnly], which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;

            movers.Sort(new sortMoverAscendingDistanceComparer());

            for (int iMover = 0; iMover < movers.Length; iMover++)
            {
                int sameLanePredecessor = iMover + 1;

                if(sameLanePredecessor < movers.Length && movers[sameLanePredecessor].currentLane == movers[iMover].currentLane)
                {
                    var d = movers[sameLanePredecessor].distanceOnLane - movers[iMover].distanceOnLane;
                    if(d < minDistanceToLimit)
                    {
                        Mover updatedMover = new Mover()
                        {
                            speed = movers[sameLanePredecessor].speed ,
                            distanceOnLane = movers[iMover].distanceOnLane
                        };
                        movers[iMover] = updatedMover ;
                        //Debug.Log("<Limiting mover>"+i);
                    }
                }
            }
        }

        public struct sortMoverAscendingDistanceComparer : System.Collections.Generic.IComparer<Mover>
        {
            public int Compare(Mover a, Mover b)
            {
                float da = a.distanceOnLane * SpeedLimiterJob.trackInvLength;
                float db = b.distanceOnLane * SpeedLimiterJob.trackInvLength;
                return a.currentLane.CompareTo(b.currentLane) * 10 + da.CompareTo(db);
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        EntityQuery m_Group = GetEntityQuery(typeof(Track));
        var track = m_Group.GetSingleton<Track>();
        SpeedLimiterJob.trackInvLength = 1.0f / track.length;

        var movers = m_Query.ToComponentDataArray<Mover>(Allocator.TempJob);

        var job = new SpeedLimiterJob()
        {
            movers = movers
        };

        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;

        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}