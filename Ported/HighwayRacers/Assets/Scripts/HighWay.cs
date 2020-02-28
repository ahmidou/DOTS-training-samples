using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public class HighWay : MonoBehaviour
{
    public GameObject Prefab;
    public int laneCount = 4;
    public float length = 200;
    public int carCount = 20;
    public float minSpeed = 1.0f/10.0f;
    public float maxSpeed = 5.0f / 10.0f;
    public float minDistanceToSlowDown = 3.0f;
    public float laneDistanceMultiplier = 1.03f;

    public class Spot
    {
        public int lane;
        public float position;
    };

    // Start is called before the first frame update
    void Start()
    {

        // Create entity prefab from the game object hierarchy once
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, settings);
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var singletonEntity = entityManager.CreateEntity(typeof(Track));
        var singletonGroup = entityManager.CreateEntityQuery(typeof(Track));
        singletonGroup.SetSingleton<Track>(
            new Track {
                laneCount = laneCount,
                length = length,
                minDistanceToSlowDown = minDistanceToSlowDown,
                laneDistanceMultiplier = laneDistanceMultiplier
            });

        var spots = new List<Spot>();
        for (int iLane = 0; iLane < laneCount; iLane++)
        {
            for(float x=0;x<length;x+= minDistanceToSlowDown)
            {
                spots.Add(new Spot { lane = iLane, position = x});
            }
        }
        if(carCount> spots.Count)
        {
            Debug.Log("Overcrowded !!!");
            return;
        }
        
        for (var x = 0; x < carCount; x++)
        {

            // Efficiently instantiate a bunch of entities from the already converted entity prefab
            var instance = entityManager.Instantiate(prefab);

            // Place the instantiated entity in a grid with some noise
            int currentLane = UnityEngine.Random.Range(0, laneCount);

            Vector3 position;

            int spotIndex = UnityEngine.Random.Range(0, spots.Count);

            currentLane = spots[spotIndex].lane;
            position = new Vector3(spots[spotIndex].position, 0, 0);

            spots.Remove(spots[spotIndex]);

            //Debug.Log(position);
            var speed = UnityEngine.Random.Range(minSpeed, maxSpeed); 
            entityManager.AddComponent<Mover>(instance);
            entityManager.AddComponent<MaterialColor>(instance);
            entityManager.SetComponentData(instance, new Mover { speed = speed, distanceOnLane=position.x, currentLane = currentLane, futureLane = currentLane, baseSpeed=speed, laneChangeSpeed=1, laneChangeRatio=0, behaviorStayOnLane=x%2==1 });
            entityManager.SetComponentData(instance, new Translation { Value = position });
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
