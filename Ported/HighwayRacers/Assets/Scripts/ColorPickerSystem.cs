using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

public class ColorPickerSystem : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        // Assign values to the fields on your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     job.deltaTime = UnityEngine.Time.deltaTime;

        var jobHandle = Entities
             .ForEach((Entity entity, ref Mover mine, ref MaterialColor color) =>
             {
                 var grey = new float4(0.5f, 0.5f, 0.5f, 1.0f);
                 var red = new float4(1.0f, 0.0f, 0.0f, 1.0f);
                 var green = new float4(0.0f, 1.0f, 0.0f, 1.0f);

                 if (mine.speed < mine.baseSpeed)
                 {
                     color.Value = math.lerp(grey, red, math.min((mine.baseSpeed - mine.speed) * 0.07f, 1.0f));
                 }
                 else if (mine.speed > mine.baseSpeed)
                 {
                     color.Value = math.lerp(grey, green, math.min(math.abs(mine.baseSpeed - mine.speed) * 0.07f, 1.0f));
                 }
                 else
                 {
                     color.Value = grey;
                 }  
            
             }).Schedule(inputDependencies);    
        jobHandle.Complete();

        return jobHandle;
    }
}