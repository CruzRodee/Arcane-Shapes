using System.Collections.Generic;
using UnityEngine;

public class ShapeFiller : MonoBehaviour
{
    private GameObject fillShape;
    private MeshFilter fillMeshFilter;
    private MeshRenderer fillRenderer;
    private float fillAmount = 0f;
    private float fillSpeed = 0.5f;
    private Vector3[] originalVertices;
    private Mesh fillMesh;
    private GameObject targetShape; // Store reference to original shape
    private MeshRenderer targetRenderer;

    public Material fillMaterial;
    public Material voidMaterial;
    public Material intersectMaterial;
    public float fillMaxValue = 0.0f;
    public bool isFillingActive = false;
    public bool isPerfectMatch = false;

    public void InitializeFill(GameObject toFillShape, Color fillColor, float speed, float fillMaxValue)
    {
        // --- START: MODIFIED SECTION ---

        // 1. Check if a "FillShape" already exists as a child of the target.
        Transform existingFill = toFillShape.transform.Find("FillShape");

        // 2. If it exists, destroy it before creating a new one.
        if (existingFill != null)
        {
            // Destroy the old GameObject to prevent duplicates.
            Destroy(existingFill.gameObject);
        }
        // --- END: MODIFIED SECTION ---

        // Now, proceed with creating the new fill shape as before.
        targetShape = toFillShape;

        // Create fill shape and match position/rotation
        fillShape = new GameObject("FillShape");
        fillShape.transform.position = targetShape.transform.position;
        fillShape.transform.rotation = targetShape.transform.rotation;
        fillShape.transform.SetParent(targetShape.transform, true);

        // Copy original mesh data
        MeshFilter originalMeshFilter = toFillShape.GetComponent<MeshFilter>();
        if (originalMeshFilter == null || originalMeshFilter.mesh == null)
        {
            Debug.LogError("The target shape does not have a MeshFilter or a mesh.", toFillShape);
            Destroy(fillShape); // Clean up the empty object we just made
            return; // Exit the function
        }
        originalVertices = originalMeshFilter.mesh.vertices;

        // Setup fill mesh
        fillMeshFilter = fillShape.AddComponent<MeshFilter>();
        fillMesh = new Mesh();
        fillMesh.vertices = originalVertices; // You need to start with the same vertices
        fillMeshFilter.mesh = fillMesh;

        // Setup renderer
        fillRenderer = fillShape.AddComponent<MeshRenderer>();

        // It's better to assign a material instance than to find a shader by string.
        // Ensure 'fillMaterial' is assigned in the Inspector or created elsewhere.
        fillRenderer.material = fillMaterial;
        fillRenderer.material.color = fillColor;

        fillSpeed = speed;

        // Ensure fill renders on top
        targetRenderer = toFillShape.GetComponent<MeshRenderer>();
        if (targetRenderer != null)
        {
            fillRenderer.sortingOrder = targetRenderer.sortingOrder + 1;
        }

        fillAmount = 0f;
        this.fillMaxValue = fillMaxValue;
    }

    void Update()
    {
        if (isFillingActive && fillAmount == this.fillMaxValue)
        {
            isFillingActive = false;
        }
        if (isFillingActive && (fillAmount < this.fillMaxValue)) //Filling
        {
            //Hide tofillshape
            if (targetRenderer != null)
                if (targetRenderer.enabled)
                    targetRenderer.enabled = false;

            fillAmount += fillSpeed * Time.deltaTime;
            if (fillAmount > this.fillMaxValue) fillAmount = this.fillMaxValue;
            fillAmount = Mathf.Clamp01(fillAmount);
            UpdateFillMesh();
        }
    }

    private void UpdateFillMesh()
    {
        if (originalVertices == null || originalVertices.Length == 0) return;

        // Find min/max Y values
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        foreach (Vector3 vertex in originalVertices)
        {
            minY = Mathf.Min(minY, vertex.y);
            maxY = Mathf.Max(maxY, vertex.y);
        }

        float fillHeight = Mathf.Lerp(minY, maxY, fillAmount);

        List<Vector3> fillVertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Special case for triangles
        if (originalVertices.Length == 3)
        {
            // Always add the bottom vertices first
            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 vertex = originalVertices[i];
                if (vertex.y <= fillHeight)
                {
                    fillVertices.Add(vertex);
                    uvs.Add(new Vector2(vertex.x, vertex.y));
                }
            }

            // Find intersections
            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 current = originalVertices[i];
                Vector3 next = originalVertices[(i + 1) % originalVertices.Length];

                if ((current.y > fillHeight && next.y <= fillHeight) ||
                    (current.y <= fillHeight && next.y > fillHeight))
                {
                    float t = (fillHeight - current.y) / (next.y - current.y);
                    Vector3 intersection = Vector3.Lerp(current, next, t);
                    fillVertices.Add(intersection);
                    uvs.Add(new Vector2(intersection.x, intersection.y));
                }
            }

            // Ensure we have enough vertices to form triangles
            if (fillVertices.Count >= 3)
            {
                // Create triangles fan-style from first vertex
                for (int i = 1; i < fillVertices.Count - 1; i++)
                {
                    triangles.Add(0);  // First vertex
                    triangles.Add(i);  // Second vertex
                    triangles.Add(i + 1);  // Third vertex
                }
            }
            else if (fillVertices.Count == 2)
            {
                // If we only have two points, add a small triangle
                Vector3 third = (fillVertices[0] + fillVertices[1]) * 0.5f;
                third.y = fillHeight - 0.01f;
                fillVertices.Add(third);
                uvs.Add(new Vector2(third.x, third.y));

                triangles.Add(0);
                triangles.Add(1);
                triangles.Add(2);
            }
        }
        else
        {
            // Original implementation for other shapes
            for (int i = 0; i < originalVertices.Length; i++)
            {
                Vector3 current = originalVertices[i];
                Vector3 next = originalVertices[(i + 1) % originalVertices.Length];

                if (current.y <= fillHeight)
                {
                    fillVertices.Add(current);
                    uvs.Add(new Vector2(current.x, current.y));
                }

                if ((current.y > fillHeight && next.y <= fillHeight) ||
                    (current.y <= fillHeight && next.y > fillHeight))
                {
                    float t = (fillHeight - current.y) / (next.y - current.y);
                    Vector3 intersection = Vector3.Lerp(current, next, t);
                    fillVertices.Add(intersection);
                    uvs.Add(new Vector2(intersection.x, intersection.y));
                }
            }

            if (fillVertices.Count >= 3)
            {
                Vector3 center = Vector3.zero;
                foreach (Vector3 vertex in fillVertices)
                {
                    center += vertex;
                }
                center /= fillVertices.Count;

                fillVertices.Add(center);
                uvs.Add(new Vector2(center.x, center.y));

                int centerIndex = fillVertices.Count - 1;
                for (int i = 0; i < fillVertices.Count - 1; i++)
                {
                    triangles.Add(centerIndex);
                    triangles.Add(i);
                    triangles.Add((i + 1) % (fillVertices.Count - 1));
                }
            }
        }

        fillMesh.Clear();
        fillMesh.vertices = fillVertices.ToArray();
        fillMesh.triangles = triangles.ToArray();
        fillMesh.uv = uvs.ToArray();
        fillMesh.RecalculateNormals();
    }
    /*private void UpdateFillMesh()
    {
        if (originalVertices == null || originalVertices.Length == 0) return;

        // Find min/max Y values
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        foreach (Vector3 vertex in originalVertices)
        {
            minY = Mathf.Min(minY, vertex.y);
            maxY = Mathf.Max(maxY, vertex.y);
        }

        float fillHeight = Mathf.Lerp(minY, maxY, fillAmount);

        List<Vector3> fillVertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Add vertices at exact fill height for the top edge
        for (int i = 0; i < originalVertices.Length; i++)
        {
            Vector3 current = originalVertices[i];
            Vector3 next = originalVertices[(i + 1) % originalVertices.Length];

            // Add bottom vertices
            if (current.y <= fillHeight)
            {
                fillVertices.Add(current);
                uvs.Add(new Vector2(current.x, current.y));
            }

            // Calculate intersection points for the fill line
            if ((current.y > fillHeight && next.y <= fillHeight) ||
                (current.y <= fillHeight && next.y > fillHeight))
            {
                float t = (fillHeight - current.y) / (next.y - current.y);
                Vector3 intersection = Vector3.Lerp(current, next, t);
                fillVertices.Add(intersection);
                uvs.Add(new Vector2(intersection.x, intersection.y));
            }
        }

        // Create triangles
        if (fillVertices.Count >= 3)
        {
            // Find center point for triangulation
            Vector3 center = Vector3.zero;
            foreach (Vector3 vertex in fillVertices)
            {
                center += vertex;
            }
            center /= fillVertices.Count;

            // Add center point
            fillVertices.Add(center);
            uvs.Add(new Vector2(center.x, center.y));

            // Create triangles from center to each edge
            int centerIndex = fillVertices.Count - 1;
            for (int i = 0; i < fillVertices.Count - 1; i++)
            {
                triangles.Add(centerIndex);
                triangles.Add(i);
                triangles.Add((i + 1) % (fillVertices.Count - 1));
            }
        }

        fillMesh.Clear();
        fillMesh.vertices = fillVertices.ToArray();
        fillMesh.triangles = triangles.ToArray();
        fillMesh.uv = uvs.ToArray();
        fillMesh.RecalculateNormals();
    }*/
}