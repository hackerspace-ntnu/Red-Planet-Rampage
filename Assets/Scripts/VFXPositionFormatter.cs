using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXTextureFormatter
{
    private Texture2D texture;

    // The float data buffer that is passed to VRAM when apply is performed
    private float[] data;

    // Size of the texture, typically the maximum number of particles or the number of patricles in each strip
    private int size;

    // The resulting texture
    public Texture2D Texture { get => texture; }

    public void ApplyChanges()
    {
        texture.SetPixelData(data, 0, 0);
        texture.Apply();
    }

    public VFXTextureFormatter(int size)
    {
        this.size = size;
        this.data = new float[size*4];
        this.texture = new Texture2D(size, 1, TextureFormat.RGBAFloat, false);
    }


    // Sets single index RGB, not alpha, as this can be set manually to perform other tasks
    public void setValue(int index, Vector3 value)
    {
        data[index * 4] = value.x;
        data[index * 4 + 1] = value.y;
        data[index * 4 + 2] = value.z;
    }

    // Same as above, but sets entire array of values at once
    public void setValues(Vector3[] values)
    {
        for (int i = 0; i < size; i++)
        {
            data[i * 4] = values[i].x;
            data[i * 4 + 1] = values[i].y;
            data[i * 4 + 2] = values[i].z;
        }
    }

    // Same as other setvalue, but sets RGBA, could for einstance be used with Quaternion xyzw
    public void setValue(int index, Vector4 value)
    {
        data[index * 4] = value.x;
        data[index * 4 + 1] = value.y;
        data[index * 4 + 2] = value.z;
        data[index * 4 + 3] = value.w;
    }
    // Sets all RGBAs at once
    public void setValues(Vector4[] values)
    {
        for (int i = 0; i < size; i++)
        {
            data[i * 4] = values[i].x;
            data[i * 4 + 1] = values[i].y;
            data[i * 4 + 2] = values[i].z;
            data[i * 4 + 3] = values[i].w;
        }
    }
    // Sets a specific alpha, usefull for alive/dead state of single particles
    public void setAlpha(int index, float alpha)
    {
        data[index * 4 + 3] = alpha;
    }

    // Sets all alphas
    public void setAlphas(float[] alphas)
    {
        for (int i = 0; i<size; i++)
        {
            data[i * 4 + 3] = alphas[i];
        }
    }
}
