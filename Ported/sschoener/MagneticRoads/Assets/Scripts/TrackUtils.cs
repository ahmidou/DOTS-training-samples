﻿using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public static class TrackUtils
{
    public static float3 Extrude(in CubicBezier bezier, in TrackGeometry geometry, int twistMode,
        float2 point, float t)
        => Extrude(bezier, geometry, twistMode, point, t, out _, out _, out _);
    
    public static float3 Extrude(in CubicBezier bezier, in TrackGeometry geometry, int twistMode,
        float2 point, float t, out float3 tangent, out float3 up, out bool error)
    {
        t = math.clamp(t, 0, 1);
        float3 sample1 = bezier.Evaluate(t);
        float3 sample2;

        float flipper = 1f;
        if (t + .01f < 1f)
        {
            sample2 = bezier.Evaluate(t + .01f);
        }
        else
        {
            sample2 = bezier.Evaluate(math.clamp(t - .01f, 0, 1));
            flipper = -1f;
        }

        tangent = math.normalize(sample2 - sample1) * flipper;

        // each spline uses one out of three possible twisting methods:
        quaternion fromTo = quaternion.identity;
        if (twistMode == 0)
        {
            // method 1 - rotate startNormal around our current 
            float angle = Vector3.SignedAngle(geometry.startNormal, geometry.endNormal, tangent);
            fromTo = quaternion.AxisAngle(tangent, angle);
        }
        else if (twistMode == 1)
        {
            // method 2 - rotate startNormal toward endNormal
            fromTo = Quaternion.FromToRotation(geometry.startNormal, geometry.endNormal);
        }
        else if (twistMode == 2)
        {
            // method 3 - rotate startNormal by "startOrientation-to-endOrientation" rotation
            quaternion startRotation = quaternion.LookRotation(geometry.startTangent, geometry.startNormal);
            quaternion endRotation = quaternion.LookRotation(geometry.endTangent * -1, geometry.endNormal);
            fromTo = math.mul(endRotation, math.inverse(startRotation));
        }

        // other twisting methods can be added, but they need to
        // respect the relationship between startNormal and endNormal.
        // for example: if startNormal and endNormal are equal, the road
        // can twist 0 or 360 degrees, but NOT 180.

        float smoothT = math.smoothstep(0f, 1f, t * 1.02f - .01f);

        up = math.mul(math.slerp(quaternion.identity, fromTo, smoothT), geometry.startNormal);
        float3 right = math.cross(tangent, up);

        // measure twisting errors:
        // we have three possible spline-twisting methods, and
        // we test each spline with all three to find the best pick
        error = math.length(up) < .5f || math.length(right) < .5f;

        return sample1 + right * point.x + up * point.y;
    }

    public static int SelectTwistMode(in CubicBezier bezier, in TrackGeometry geometry, int resolution)
    {
        int minErrors = int.MaxValue;
        int bestTwistMode = 0;
        for (int i = 0; i < 3; i++)
        {
            int currentTwistMode = i;
            int numErrors = 0;
            for (int j = 0; j <= resolution; j++)
            {
                float t = (float)j / resolution;
                Extrude(bezier, geometry, currentTwistMode, Vector2.zero, t, out _, out _, out var error);
                numErrors += error ? 1 : 0;
            }

            if (numErrors < minErrors)
            {
                minErrors = numErrors;
                bestTwistMode = i;
            }
        }

        return bestTwistMode;
    }

    public static void SizeOfMeshData(int resolution, out int numVertices, out int numIndices)
    {
        numVertices = 4 * (resolution + 1) * 2;
        numIndices = 4 * resolution * 6;
    }
}
