using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public class ShapeGenerator : MonoBehaviour
{
    [Header("Shape Settings")]
    public Color shapeColor = new Color(0.9f, 0.1f, 0.1f, 0.6f);
    public GridSystem gridSystem;
    public float unitSize = 1f;

    private Material shapeMaterial;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    void Start()
    {
        shapeMaterial = new Material(Shader.Find("Sprites/Default"));
        shapeMaterial.color = shapeColor;

        if (gridSystem == null)
        {
            gridSystem = FindObjectOfType<GridSystem>();
        }

        if (gridSystem != null)
        {
            unitSize = gridSystem.minorGridSize;
        }
        else
        {
            UnityEngine.Debug.LogWarning("GridSystem not found! Ensure it's assigned.");
        }
    }

    public Vector3 SnapToGrid(Vector3 offset, float sizeWidth, float sizeHeight)
    {
        //Camera cam = Camera.main;


        float height = 2f * cam.orthographicSize * 1.5f;
        float width = height * cam.aspect * 1.5f;
        Vector3 camPos = cam.transform.position;
        float spacing = gridSystem.minorGridSize;

        // Calculate grid origin point
        float gridStartX = Mathf.Floor(camPos.x / spacing) * spacing - width / 2;
        float gridStartY = Mathf.Floor(camPos.y / spacing) * spacing - height / 2;

        Vector3 position = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));


        // Calculate how many spacing units away from the start point
        float deltaX = position.x - gridStartX;
        float deltaY = position.y - gridStartY;

        // Round to nearest intersection
        int gridIndexX = Mathf.RoundToInt(deltaX / spacing);
        int gridIndexY = Mathf.RoundToInt(deltaY / spacing);

        gridIndexX += (int)offset.x;
        gridIndexY += (int)offset.y;

        // Calculate final intersection position
        Vector3 snappedPos = new Vector3(
            gridStartX + (gridIndexX * spacing) - (int)((spacing * sizeWidth) / 2),
            gridStartY + (gridIndexY * spacing) - (int)((spacing * sizeHeight) / 2),
            0
        );

        return snappedPos;
    }
    /*    private Vector3 SnapToGrid(Vector2 position)
        {
            Camera cam = Camera.main;
            float height = 2f * cam.orthographicSize * 1.5f; // Slightly larger than camera view
            float width = height * cam.aspect * 1.5f;

            Vector3 camPos = cam.transform.position;

            float spacing = gridSystem.minorGridSize;


            float x = Mathf.Floor(camPos.x / spacing) * spacing - width / 2;
            float y = Mathf.Floor(camPos.y / spacing) * spacing - height / 2;

            float eX = Mathf.Ceil(camPos.x / spacing) * spacing + width / 2;
            float eY = Mathf.Ceil(camPos.y / spacing) * spacing + height / 2;
            eX = (eX + x) * 0.75f;
            eY = (eY + y) * 0.625f;

            Vector3 cameraCenter = cam.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            Vector3 roundedCenter = new Vector3(
                (int)cameraCenter.x,
                (int)cameraCenter.y,
                0
            );
            //new Vector3(x + (int)eX, y + (int)eY, 0);

            return roundedCenter;
        }*/

    private GameObject CreateShape(Vector2 gridPosition, Vector3[] vertices, int[] triangles, float width, float height)
    {
        Vector3 snappedPosition = SnapToGrid(gridPosition, width, height);

        GameObject shape = new GameObject("Shape");
        shape.transform.position = snappedPosition;

        MeshFilter meshFilter = shape.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = shape.AddComponent<MeshRenderer>();
        meshRenderer.material = shapeMaterial;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x * width, vertices[i].y * height, 0);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x / unitSize, vertices[i].y / unitSize);
        }
        mesh.uv = uvs;

        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        meshFilter.mesh = mesh;

        return shape;
    }

    public GameObject CreateSquare(Vector2 gridPosition, float size = 1f)
    {
        return CreateShape(gridPosition, new Vector3[]
        {
           new Vector3(0, 0, 0),
           new Vector3(1, 0, 0),
           new Vector3(1, 1, 0),
           new Vector3(0, 1, 0)
        }, new int[] { 0, 1, 2, 2, 3, 0 }, unitSize * size, unitSize * size);
    }

    public GameObject CreateRectangle(Vector2 gridPosition, float width, float height)
    {
        return CreateShape(gridPosition, new Vector3[]
        {
           new Vector3(0, 0, 0),
           new Vector3(1, 0, 0),
           new Vector3(1, 1, 0),
           new Vector3(0, 1, 0)
        }, new int[] { 0, 1, 2, 2, 3, 0 }, unitSize * width, unitSize * height);
    }

    public GameObject CreateTriangle(Vector2 gridPosition, float width = 1f, float height = 1f)
    {
        return CreateShape(gridPosition, new Vector3[]
        {
           new Vector3(0, 0, 0),
           new Vector3(1, 0, 0),
           new Vector3(0.5f, 1, 0)
        }, new int[] { 0, 1, 2 }, unitSize * width, unitSize * height);
    }

    public GameObject CreateCircle(Vector2 gridPosition, float size = 1f, bool isHalf = false, int numSegments = 32)
    {
        float radius = unitSize * size / 2f;
        Vector3 snappedPosition = SnapToGrid(gridPosition, size, size);


        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        vertices.Add(Vector3.zero);
        float maxAngle = isHalf ? Mathf.PI : Mathf.PI * 2;

        for (int i = 0; i <= numSegments; i++)
        {
            float angle = (i * maxAngle) / numSegments;
            vertices.Add(new Vector3(
                radius * Mathf.Cos(angle),
                radius * Mathf.Sin(angle),
                0
            ));
        }

        for (int i = 0; i < numSegments; i++)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i + 2);
        }

        return CreateShape(gridPosition, vertices.ToArray(), triangles.ToArray(), 1, 1);
    }

    public void OffsetPositionTo(GameObject shape, Vector3 gridOffset, float width = 1f, float height = 1f)
    {
        if (shape == null)
        {
            UnityEngine.Debug.LogError("Cannot offset null shape");
            return;
        }

        // Calculate current position in grid units
        Vector3 currentPos = shape.transform.position;
        Vector3 camPos = cam.transform.position;
        float spacing = gridSystem.minorGridSize;
        float height2D = 2f * cam.orthographicSize * 1.5f;
        float width2D = height2D * cam.aspect * 1.5f;

        // Calculate grid origin point
        float gridStartX = Mathf.Floor(camPos.x / spacing) * spacing - width2D / 2;
        float gridStartY = Mathf.Floor(camPos.y / spacing) * spacing - height2D / 2;

        // Calculate current grid indices
        float deltaX = currentPos.x - gridStartX + (int)((spacing * width) / 2);
        float deltaY = currentPos.y - gridStartY + (int)((spacing * height) / 2);
        int currentGridX = Mathf.RoundToInt(deltaX / spacing);
        int currentGridY = Mathf.RoundToInt(deltaY / spacing);

        // Apply offset
        int newGridX = currentGridX + (int)gridOffset.x;
        int newGridY = currentGridY + (int)gridOffset.y;

        // Calculate new world position
        Vector3 newPos = new Vector3(
            gridStartX + (newGridX * spacing) - (int)((spacing * width) / 2),
            gridStartY + (newGridY * spacing) - (int)((spacing * height) / 2),
            currentPos.z
        );

        // Apply the new position
        shape.transform.position = newPos;
    }

}