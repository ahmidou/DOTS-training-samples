using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct Mover : IComponentData
{
    public enum DrivingBehavior
    {
        Regular,
        LimitSpeed,
        Overtake,
        MergeRight
    };

    // Initial speed
    public float baseSpeed;

    // current speed
    public float speed;     

    public int currentLane;
    public int futureLane;
    public float laneChangeSpeed;
    public float laneChangeRatio;
    public float distanceOnLane;

    public DrivingBehavior drivingBehavior;

    public bool leftLaneAvailable;
    public bool currentLaneAvailable;
    public bool rightLaneAvailable;

    public float frontCarSpeed;

    public bool StopProbbing()
    {
        return leftLaneAvailable == false &&
            currentLaneAvailable == false &&
            rightLaneAvailable == false;
    }
}
