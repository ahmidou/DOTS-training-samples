using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class LaneProbeSystem : JobComponentSystem
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
        var movers = m_Query.ToComponentDataArray<Mover>(Allocator.TempJob);

        var jobHandle = Entities
            .ForEach((Entity entity, Mover referenceMover, ref LaneProbe probe) =>
            {
                probe.leftLaneAvailable = referenceMover.currentLane + 1 < track.laneCount;
                probe.currentLaneAvailable = true;
                probe.rightLaneAvailable = referenceMover.currentLane > 0;

                float smallestDistanceToBrake = track.minDistanceToSlowDown;

                for (int i = 0; i < movers.Length; i++)
                {
                    // Room ahead ?
                    if (movers[i].currentLane == referenceMover.currentLane)
                    {
                        if (probe.currentLaneAvailable)
                        {
                            float d = movers[i].distanceOnLane - referenceMover.distanceOnLane;
                            if (d > 0 && d < smallestDistanceToBrake)
                            {
                                probe.currentLaneAvailable = false;
                            }
                        }
                    }

                    // room on left ?
                    else if (movers[i].currentLane == referenceMover.currentLane + 1)
                    {
                        if (probe.leftLaneAvailable)
                        {
                            float d = movers[i].distanceOnLane - referenceMover.distanceOnLane;
                            if (abs(d) < track.minDistanceToSlowDown * 2.0f)
                            {
                                probe.leftLaneAvailable = false;
                            }
                        }
                    }

                    else if (movers[i].currentLane == referenceMover.currentLane - 1)
                    {
                        if (probe.rightLaneAvailable)
                        {
                            float d = movers[i].distanceOnLane - referenceMover.distanceOnLane;
                            if (abs(d) < track.minDistanceToSlowDown * 2.0f)
                            {
                                probe.rightLaneAvailable = false;
                            }
                        }
                    }

                    if(probe.StopProbbing())
                    {
                        break;
                    }
                }
            })
            .Schedule(inputDependencies);
        jobHandle.Complete();
        movers.Dispose();

        return jobHandle;
    }
}