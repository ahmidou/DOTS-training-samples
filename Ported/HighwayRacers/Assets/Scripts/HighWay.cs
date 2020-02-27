﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public class HighWay : MonoBehaviour
{
    public GameObject Prefab;
    // Start is called before the first frame update
    void Start()
    {

        // Create entity prefab from the game object hierarchy once
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(Prefab, settings);
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        for (var x = 0; x < 20; x++)
        {

            // Efficiently instantiate a bunch of entities from the already converted entity prefab
            var instance = entityManager.Instantiate(prefab);   

            // Place the instantiated entity in a grid with some noise
            var position = new Vector3((float)UnityEngine.Random.Range(0, 200), 0 ,0);
            //Debug.Log(position);
            var speed = UnityEngine.Random.Range(10.0f, 20.0f); 
            entityManager.AddComponent<Mover>(instance);
            entityManager.AddComponent<SpeedLimit>(instance);
            entityManager.AddComponent<LaneProbe>(instance);
            entityManager.SetComponentData(instance, new Mover { speed = speed, distanceOnLane=position.x });
            entityManager.SetComponentData(instance, new Translation { Value = position });
            entityManager.SetComponentData(instance, new LaneProbe());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
