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

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        EntityQuery m_Group = GetEntityQuery(typeof(Track));
        var track = m_Group.GetSingleton<Track>();
        var movers = m_Query.ToComponentDataArray<Mover>(Allocator.TempJob
            );

        var jobHandle = Entities
            .ForEach((Entity entity, ref Mover mine) =>
            {
                if (mine.drivingBehavior == Mover.DrivingBehavior.LimitSpeed)
                {
                    //mine.speed = math.lerp(mine.frontCarSpeed, mine.speed, mine.frontCarDistance / track.minDistanceToSlowDown);
                    //mine.speed = math.max(mine.frontCarSpeed, mine.speed *(mine.frontCarDistance / track.minDistanceToSlowDown + 1));
                    mine.speed = mine.frontCarSpeed;
                }
                else   
                {
                    mine.speed = mine.baseSpeed;
                }
            })
            .Schedule(inputDependencies);
        jobHandle.Complete();
        movers.Dispose();

        return jobHandle;
    }
}