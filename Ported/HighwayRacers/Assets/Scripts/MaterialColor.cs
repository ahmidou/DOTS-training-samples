using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[GenerateAuthoringComponent]
[MaterialProperty("_Color", MaterialPropertyFormat.Float4)]
public struct MaterialColor : IComponentData
{
    public float4 Value;
}   