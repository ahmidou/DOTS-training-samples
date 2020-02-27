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
        var movers = m_Query.ToComponentDataArray<Mover>(Allocator.TempJob\
            );

        var jobHandle = Entities
            .ForEach((Entity entity, ref Mover mine, ref SpeedLimit limitInfos) =>
            {
                float smallest = track.minDistanceToSlowDown;
                int closest = -1;
                for (int i = 0; i < movers.Length; i++)
                {
                    if (movers[i].distanceOnLane != mine.distanceOnLane)
                    {
                        float d = movers[i].distanceOnLane - mine.distanceOnLane;
                        if (d > 0 && d < smallest)
                        {
                            smallest = d;
                            closest = i;
                        }
                    }
                }

                if (closest >= 0)
                {
                    mine.speed = movers[closest].speed;
                }
            })
            .Schedule(inputDependencies);
        jobHandle.Complete();
        movers.Dispose();

        return jobHandle;
    }
}