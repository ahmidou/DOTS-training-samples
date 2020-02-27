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
    public float minDistanceToSlowDown = 2.0f;
    public float laneDistanceMultiplier = 1.03f;

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

        for (var x = 0; x < carCount; x++)
        {

            // Efficiently instantiate a bunch of entities from the already converted entity prefab
            var instance = entityManager.Instantiate(prefab);   

            // Place the instantiated entity in a grid with some noise
            var position = new Vector3((float)UnityEngine.Random.Range(0, length), 0 ,0);
            //Debug.Log(position);
            var speed = UnityEngine.Random.Range(minSpeed, maxSpeed); 
            entityManager.AddComponent<Mover>(instance);
            entityManager.SetComponentData(instance, new Mover { speed = speed, distanceOnLane=position.x, currentLane= UnityEngine.Random.Range(0, 4) });
            entityManager.SetComponentData(instance, new Translation { Value = position });
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
