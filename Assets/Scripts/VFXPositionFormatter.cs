using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXTextureFormatter : MonoBehaviour
{
    private GraphicsBuffer buffer;
    // The float data buffer that is passed to VRAM when apply is performed
    private float[] data;

    // Size of the texture, typically the maximum number of particles or the number of patricles in each strip
    private int size;

    // The resulting texture
    public GraphicsBuffer Buffer { get => buffer; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="size">Size of the texture, typically the maximum number of particles or the number of patricles in each strip</param>
    public void Initialize(int size)
    {
        data = new float[size * 4];
        buffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, size, sizeof(float) * 4);
        this.size = size;
    }

    public void ApplyChanges()
    {
        buffer.SetData(data);
    }

    /// <summary>
    /// Sets single index RGB, not alpha, as this can be set manually to perform other tasks
    /// </summary>
    /// <param name="index">Target index for set data</param>
    /// <param name="value">Data to set</param>
    public void setValue(int index, Vector3 value)
    {
        data[index * 4] = value.x;
        data[index * 4 + 1] = value.y;
        data[index * 4 + 2] = value.z;
    }

    /// <summary>
    /// Set all, only set first 3 floats for each index.
    /// </summary>
    /// <param name="values"></param>
    public void setValues(Vector3[] values)
    {
        for (int i = 0; i < size; i++)
        {
            data[i * 4] = values[i].x;
            data[i * 4 + 1] = values[i].y;
            data[i * 4 + 2] = values[i].z;
        }
    }

    /// <summary>
    /// Set index RGBA
    /// </summary>
    /// <param name="index">Target index for set data</param>
    /// <param name="value">Data to set</param>
    public void setValue(int index, Vector4 value)
    {
        data[index * 4] = value.x;
        data[index * 4 + 1] = value.y;
        data[index * 4 + 2] = value.z;
        data[index * 4 + 3] = value.w;
    }

    /// <summary>
    /// Set all.
    /// </summary>
    /// <param name="values"></param>
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

    /// <summary>
    /// Sets a specific alpha, useful for alive/dead state of single particles
    /// </summary>
    /// <param name="index"></param>
    /// <param name="alpha"></param>
    public void setAlpha(int index, float alpha)
    {
        data[index * 4 + 3] = alpha;
    }

    /// <summary>
    /// Sets all alphas
    /// </summary>
    /// <param name="alphas"></param>
    public void setAlphas(float[] alphas)
    {
        for (int i = 0; i<size; i++)
        {
            data[i * 4 + 3] = alphas[i];
        }
    }

    private void OnDestroy()
    {
        buffer?.Release();
    }
}
