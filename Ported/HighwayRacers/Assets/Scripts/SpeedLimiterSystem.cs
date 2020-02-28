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
    [BurstCompile]
    struct SpeedLimiterJob : IJobForEach<Mover>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        public Track track;

        public void Execute(ref Mover mover)
        {
            // Implement the work to perform for each entity here.
            // You should only access data that is local or that is a
            // field on this job. Note that the 'rotation' parameter is
            // marked as [ReadOnly], which means it cannot be modified,
            // but allows this job to run in parallel with other jobs
            // that want to read Rotation component data.
            // For example,
            //  translation.Value += mul(rotation.Value, new float3(0, 0, 1)) * deltaTime;

            if (mover.drivingBehavior == Mover.DrivingBehavior.LimitSpeed)
            {
                //mine.speed = math.lerp(mine.frontCarSpeed, mine.speed, mine.frontCarDistance / track.minDistanceToSlowDown);
                //mine.speed = math.max(mine.frontCarSpeed, mine.speed *(mine.frontCarDistance / track.minDistanceToSlowDown + 1));
                mover.speed = mover.frontCarSpeed;
            }
            else if (mover.drivingBehavior == Mover.DrivingBehavior.Overtake)
            {
                mover.speed = mover.baseSpeed * 1.2f;
            }
            else if (mover.speed != mover.baseSpeed)
            {
                if (mover.speed > mover.baseSpeed)
                {
                    mover.speed -= 0.05f;
                    if (mover.speed < mover.baseSpeed)
                    {
                        mover.speed = mover.baseSpeed;
                    }
                }
                else
                {
                    mover.speed += 0.05f;
                    if (mover.speed > mover.baseSpeed)
                    {
                        mover.speed = mover.baseSpeed;
                    }
                }
            }

            // Debug.Log(mover.distanceOnLane);            
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        var job = new SpeedLimiterJob();
        EntityQuery m_Group = GetEntityQuery(typeof(Track));
        var track = m_Group.GetSingleton<Track>();
        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        job.track = track;

        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}