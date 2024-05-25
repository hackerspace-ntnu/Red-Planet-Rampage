using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDepthModeSetter : MonoBehaviour
{
    void Start()
    {
        Camera camera = GetComponent<Camera>();
        if (camera != null)
        {
            camera.depthTextureMode = DepthTextureMode.DepthNormals;
        }
    }

}
