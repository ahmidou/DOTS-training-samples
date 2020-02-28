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
                var smallestDistanceToBrake = track.minDistanceToSlowDown / track.length;

              
                for (int i = 0; i < movers.Length; i++)
                {
                    var normalizedDistance =  movers[i].distanceOnLane / (track.length * (float)System.Math.Pow(track.laneDistanceMultiplier, (double)movers[i].currentLane));


                    // Room ahead ?
                    if (movers[i].currentLane == referenceMover.currentLane)
                    {
                        if (referenceMover.currentLaneAvailable)
                        {
                            float d = normalizedDistance - currentNormalizedDistance;
                            if (d > 0 && d < smallestDistanceToBrake)
                            {
                                referenceMover.currentLaneAvailable = false;
                                referenceMover.frontCarSpeed = movers[i].speed;
                                referenceMover.frontCarDistance = movers[i].distanceOnLane - referenceMover.distanceOnLane;

                            }
                        }
                    }

                    // room on left ?
                    else if (movers[i].currentLane == referenceMover.currentLane + 1)
                    {
                        if (referenceMover.leftLaneAvailable)
                        {
                            float d = normalizedDistance - currentNormalizedDistance;
                            if (abs(d) < smallestDistanceToBrake * 2.0f)
                            {
                                //string s = string.Format("cur:{0} = other:{1} Dist:{2}", referenceMover.currentLane, movers[i].currentLane, d);
                                //Debug.Log(s);
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
                            if (abs(d) < smallestDistanceToBrake * 2.0f)
                            {
                                referenceMover.rightLaneAvailable = false;
                            }
                        }
                    }

                    // Exits as soon as all decisions have been made
                    if(referenceMover.StopProbbing())
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