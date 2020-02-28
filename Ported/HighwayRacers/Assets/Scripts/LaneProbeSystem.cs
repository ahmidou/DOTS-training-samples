using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
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
            .ForEach((Entity entity, ref Mover referenceMover) =>
            {
                referenceMover.leftLaneAvailable = referenceMover.currentLane + 1 < track.laneCount;
                referenceMover.currentLaneAvailable = true;
                referenceMover.rightLaneAvailable = referenceMover.currentLane > 0;

                // all distances are normalized in [0,1]
                var currentNormalizedDistance = referenceMover.distanceOnLane / (track.length * (float)System.Math.Pow(track.laneDistanceMultiplier,(double)referenceMover.currentLane));
                var futureNormalizedDistance = referenceMover.distanceOnLane / (track.length * (float)System.Math.Pow(track.laneDistanceMultiplier, (double)referenceMover.futureLane));
                var smallestDistanceToBrake = track.minDistanceToSlowDown / track.length;

              
                for (int i = 0; i < movers.Length; i++)
                {
                    var normalizedDistance =  movers[i].distanceOnLane / (track.length * (float)System.Math.Pow(track.laneDistanceMultiplier, (double)movers[i].currentLane));

                    // --- Probe currentLane -------------------------------------------------------------

                    // Room ahead ?
                    if (movers[i].currentLane == referenceMover.currentLane)
                    {
                        if (referenceMover.currentLaneAvailable)
                        {
                            float d = normalizedDistance - currentNormalizedDistance;
                            if (d > 0 && d < smallestDistanceToBrake)
                            {
                                referenceMover.currentLaneAvailable = false;
                            }
                        }
                    }

                    // room on left ?
                    else if (movers[i].currentLane == referenceMover.currentLane + 1)
                    {
                        if (referenceMover.leftLaneAvailable)
                        {
                            float d = normalizedDistance - currentNormalizedDistance;
                            if (abs(d) < smallestDistanceToBrake * 3.0f)
                            {
                                referenceMover.leftLaneAvailable = false;
                            }
                        }
                    }

                    // room on right
                    else if (movers[i].currentLane == referenceMover.currentLane - 1)
                    {
                        if (referenceMover.rightLaneAvailable)
                        {
                            float d = normalizedDistance - currentNormalizedDistance;
                            if (abs(d) < smallestDistanceToBrake * 3.0f)
                            {
                                referenceMover.rightLaneAvailable = false;
                            }
                        }
                    }

                    // --- Probe futureLane (if any) -------------------------------------------------------------

                    if (referenceMover.futureLane != referenceMover.currentLane)
                    {
                        // Room ahead ?
                        if (movers[i].currentLane == referenceMover.futureLane)
                        {
                            if (referenceMover.currentLaneAvailable)
                            {
                                float d = normalizedDistance - futureNormalizedDistance;
                                if (d > 0 && d < smallestDistanceToBrake)
                                {
                                    referenceMover.currentLaneAvailable = false;
                                }
                            }
                        }

                        // room on left ?
                        else if (movers[i].currentLane == referenceMover.futureLane + 1)
                        {
                            if (referenceMover.leftLaneAvailable)
                            {
                                float d = normalizedDistance - futureNormalizedDistance;
                                if (abs(d) < smallestDistanceToBrake * 3.0f)
                                {
                                    referenceMover.leftLaneAvailable = false;
                                }
                            }
                        }

                        // room on right
                        else if (movers[i].currentLane == referenceMover.futureLane - 1)
                        {
                            if (referenceMover.rightLaneAvailable)
                            {
                                float d = normalizedDistance - futureNormalizedDistance;
                                if (abs(d) < smallestDistanceToBrake * 3.0f)
                                {
                                    referenceMover.rightLaneAvailable = false;
                                }
                            }
                        }
                    }

                    // Exits as soon as all decisions have been made
                    if (referenceMover.StopProbbing())
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