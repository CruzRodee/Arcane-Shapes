using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [Header("Grid Colors")]
    public Color mainGridColor = new Color(1f, 1f, 1f, 0.5f);
    public Color subGridColor = new Color(1f, 1f, 1f, 0.2f);

    [Header("Grid Spacing")]
    public float majorGridSize = 2f; // Size of major grid cells
    public float minorGridSize = 1f;  // Size of minor grid cells

    [Header("Line Width")]
    public float majorLineWidth = 0.1f;
    public float minorLineWidth = 0.05f;

    private Camera cameraComponent;
    private GameObject gridParent;

    void Awake()
    {
        cameraComponent = Camera.main;
    }

    void Start()
    {
        CreateInfiniteGrid();
    }

    void Update()
    {
        // Press 'R' to refresh the grid at runtime
        if (Input.GetKeyDown(KeyCode.R))
        {
            RefreshGrid();
        }
    }

    private void CreateInfiniteGrid()
    {
        if (gridParent != null)
        {
            Destroy(gridParent); // Destroy the old grid
        }

        gridParent = new GameObject("Infinite Grid");

        // Create the major grid lines
        CreateGridLines(majorGridSize, mainGridColor, "Major Grid", gridParent.transform, majorLineWidth);

        // Create the minor grid lines
        CreateGridLines(minorGridSize, subGridColor, "Minor Grid", gridParent.transform, minorLineWidth);
    }

    private void CreateGridLines(float spacing, Color color, string name, Transform parent, float lineWidth)
    {
        float height = 2f * cameraComponent.orthographicSize * 1.5f; // Slightly larger than camera view
        float width = height * cameraComponent.aspect * 1.5f;

        Vector3 camPos = cameraComponent.transform.position;

        float startX = Mathf.Floor(camPos.x / spacing) * spacing - width / 2;
        float startY = Mathf.Floor(camPos.y / spacing) * spacing - height / 2;
        float endX = Mathf.Ceil(camPos.x / spacing) * spacing + width / 2;
        float endY = Mathf.Ceil(camPos.y / spacing) * spacing + height / 2;

        // Create horizontal and vertical lines
        for (float y = startY; y <= endY; y += spacing)
        {
            //UnityEngine.Debug.Log($"Horizontal line at y = {y}");
            CreateSingleLine(new Vector3(startX, y, 0), new Vector3(endX, y, 0), color, parent, lineWidth);
        }

        for (float x = startX; x <= endX; x += spacing)
        {
            //UnityEngine.Debug.Log($"Horizontal line at x = {x}");
            CreateSingleLine(new Vector3(x, startY, 0), new Vector3(x, endY, 0), color, parent, lineWidth);
        }
    }

    private void CreateSingleLine(Vector3 start, Vector3 end, Color color, Transform parent, float lineWidth)
    {
        GameObject lineObject = new GameObject("Grid Line");
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        // Configure LineRenderer properties
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.useWorldSpace = true;

        // Set positions
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[] { start, end });

        lineObject.name = "[" + start.x + "," + start.y + "," + start.z + "]" + ":::[" + end.x + "," + end.y + "," + end.z + "]";

        // Attach to parent
        lineObject.transform.parent = parent;
    }

    public void RefreshGrid()
    {
        CreateInfiniteGrid();
    }

    public Vector3 GetWorldPositionFromGrid(Vector2 gridPosition)
    {
        // Calculate the world position based on the grid's origin and spacing
        Vector3 origin = cameraComponent.transform.position;
        float x = Mathf.Floor(origin.x / minorGridSize) * minorGridSize + gridPosition.x * minorGridSize;
        float y = Mathf.Floor(origin.y / minorGridSize) * minorGridSize + gridPosition.y * minorGridSize;

        return new Vector3(x, y, 0);
    }

    public Vector3 GetAlignedWorldPosition(Vector2 gridPosition)
    {
        Vector3 cameraOrigin = cameraComponent.transform.position;

        float x = Mathf.Floor(cameraOrigin.x / minorGridSize) * minorGridSize + gridPosition.x * minorGridSize - (minorGridSize / 2);
        float y = Mathf.Floor(cameraOrigin.y / minorGridSize) * minorGridSize + gridPosition.y * minorGridSize - (minorGridSize / 2);

        return new Vector3(x, y, 0);
    }
}
