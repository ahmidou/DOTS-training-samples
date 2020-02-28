﻿using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class CircularizeSystem : JobComponentSystem
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
    struct CircularizeSystemJob : IJobForEach<Translation, Rotation, Mover>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        public float trackLength;
        
        
        
        public void Execute(ref Translation translation, ref Rotation rotation, [ReadOnly] ref Mover myMover)
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as [ReadOnly], which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //     translation.Value += mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;

            //translation.Value = new float3(myMover.distanceOnLane, 0, 4.0f * (float)myMover.currentLane);
            float angle = math.PI * 2f * myMover.distanceOnLane / trackLength;
            float laneWidth = 2f;
            float laneOffset = math.lerp(myMover.currentLane, myMover.futureLane, myMover.laneChangeRatio) * laneWidth;
            float radius = trackLength / (math.PI * 2f) - laneOffset;
            translation.Value = new float3(math.cos(angle), 0f, math.sin(angle)) * radius;
            rotation.Value = Unity.Mathematics.quaternion.AxisAngle(math.up(), -angle);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        EntityQuery m_Group = GetEntityQuery(typeof(Track));
        var track = m_Group.GetSingleton<Track>();
        var job = new CircularizeSystemJob() { trackLength = track.length };
        
        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;
        
        
        
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}