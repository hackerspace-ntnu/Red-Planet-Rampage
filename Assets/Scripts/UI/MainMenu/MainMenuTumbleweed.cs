using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class MainMenuTumbleweed : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    [SerializeField]
    private GameObject model;
    [SerializeField]
    private Terrain terrain;
    private float terrainHeight;

    private void Start()
    {
        // Rotates model 360 degrees 
        model.transform.LeanRotateAroundLocal(model.transform.forward,360,1).setLoopCount(-1);   


    }

    private void LateUpdate()
    {
        // Get the height of the terrain at the position of TumbleweedParent
        terrainHeight = terrain.SampleHeight(transform.position);
        // Change TumbleweedParent's height to be the height of the terrain + 2
        transform.position = new Vector3(transform.position.x, terrain.GetPosition().y + terrainHeight + 2, transform.position.z);
        // Keeps parent y-axis pointed at target
        transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z), transform.up);
    }
}
