using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }
    public FitType fitType;

    public int rows;
    public int columns;
    public Vector2 cellSize;
    public Vector2 spacing;

    public bool fitX;
    public bool fitY;

    
    public override void CalculateLayoutInputVertical()
    {
        base.CalculateLayoutInputHorizontal();

        if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
        {
            fitX = true;
            fitY = true;
            // Find number of rows and columns
            float sqrRt = Mathf.Sqrt(transform.childCount);
            rows = Mathf.CeilToInt(sqrRt);
            columns = Mathf.CeilToInt(sqrRt);
        }
        // Adjust based on fitType
        if (fitType == FitType.Width || fitType == FitType.FixedColumns)
            rows = Mathf.CeilToInt(transform.childCount / (float)columns);

        if (fitType == FitType.Height || fitType == FitType.FixedRows)
            columns = Mathf.CeilToInt(transform.childCount / (float)rows);

        // Get width and height of container
        float parentWidth = rectTransform.sizeDelta.x;
        float parentHeight = rectTransform.sizeDelta.y;

        if (parentHeight == 0) // TODO: This is cancer, figure out why sizeDelta returns 0 (TORE APPROVED (kinda))
            parentHeight = 980;

        // Determine the child size
        float cellWidth = (parentWidth / (float)columns)  - (padding.left / (float)columns) - (padding.right / (float)columns);
        float cellHeight = (parentHeight / (float)rows) - (spacing.y / (float)rows) - (padding.top / (float)rows) - (padding.bottom / (float) rows);

        cellSize.x = (fitX ? cellWidth : cellSize.x) - spacing.x;
        cellSize.y = fitY ? cellHeight : cellSize.y;

        int columnCount = 0;
        int rowCount = 0;

        for(int i = 0; i < rectChildren.Count; i++)
        {
            // Find current row and column index
            rowCount = i / columns;
            columnCount = i % columns;

            // Refrence to child object
            var item = rectChildren[i];

            // Determine the positions of the child
            var xPos = (cellSize.x * columnCount) + (spacing.x * columnCount) + padding.left;
            var yPos = (cellSize.y * rowCount) + (spacing.y * rowCount) + padding.top;

            SetChildAlongAxis(item, 0, xPos, cellSize.x);
            SetChildAlongAxis(item, 1, yPos, cellSize.y);
        }
    }

    public override void CalculateLayoutInputHorizontal()
    {
        CalculateLayoutInputVertical();
    }
    
    public override void SetLayoutHorizontal()
    {
        
    }

    public override void SetLayoutVertical()
    {

    }

    public void update()
    {
        
    }
}
