using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LessJallaVFXPositionEncoder : MonoBehaviour
{

    [SerializeField]
    private int MaxLines = 10;

    private Vector3[] startEndPositions;

    private GraphicsBuffer startEndPositionsBuffer;

    private int currentId = 0;

    public GraphicsBuffer StartEndPositionsBuffer {  get { return startEndPositionsBuffer; } }
    public int CurrentId { get { return currentId; } }

    void Awake()
    {
        startEndPositions = new Vector3[MaxLines * 2];

        startEndPositionsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, startEndPositions.Length, sizeof(float) * 3);
    }

    public void AddLine(Vector3 start, Vector3 end)
    {
        startEndPositions[currentId++] = start;
        startEndPositions[currentId++] = end;
        currentId = currentId % (MaxLines * 2);
    }

    public void PopulateBuffer()
    {
        startEndPositionsBuffer.SetData(startEndPositions);
    }

    public void resetBuffer()
    {
        Array.Fill(startEndPositions, Vector3.zero);
        currentId = 0;
    }
}
