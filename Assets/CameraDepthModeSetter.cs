using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDepthModeSetter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Camera camera = GetComponent<Camera>();
        if (camera != null)
        {
            camera.depthTextureMode = DepthTextureMode.DepthNormals;
        }
    }

}
