using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class HOLineSnapper : MonoBehaviour
{
    private LineRenderer currentLine;
    private List<LineRenderer> lines = new List<LineRenderer>();
    private List<GameObject> lineTexts = new List<GameObject>();
    private Vector2 startPos;
    private bool isDrawing = false;
    private Camera mainCamera;
    private const float SNAP_INTERVAL = 0.25f;
    private int lineCount = 0;
    private GridSystem gridSystem;
    private HOGameBeh main;

    void Start()
    {
        mainCamera = Camera.main;
        lines.Add(CreateNewLineRenderer());
        gridSystem = FindObjectOfType<GridSystem>();
        main = FindObjectOfType<HOGameBeh>();
    }

    private LineRenderer CreateNewLineRenderer()
    {
        GameObject lineObj = new GameObject("Line" + lineCount);
        lineObj.transform.parent = this.transform;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;

        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.useWorldSpace = true;

        Color lineColor = Color.white;
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        return lr;
    }

    private GameObject CreateValueText(Vector3 position, float value)
    {
        GameObject textObj = new GameObject("LineValue" + lineCount);
        textObj.transform.parent = this.transform;
        textObj.transform.position = position + new Vector3(0.2f, 0.2f, 0);

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = value.ToString("F2");
        textMesh.characterSize = 0.4f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.white;

        return textObj;
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Always set the current line to the most recent one
        currentLine = lines[lineCount];

        // Touch input handling
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = touch.position;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 10f));
            Vector3 intersectionSnappedPos = SnapToGridIntersection(worldPos);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    StartDrawing(intersectionSnappedPos);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (isDrawing) UpdateLine(SnapToGrid(worldPos));
                    break;
                case TouchPhase.Ended:
                    if (isDrawing) FinishLine();
                    break;
            }
        }
        // Mouse input handling
        else
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
            Vector3 snappedPos = SnapToGrid(worldPos);

            if (Input.GetMouseButtonDown(0))
            {
                StartDrawing(SnapToGridIntersection(worldPos));
            }
            else if (Input.GetMouseButton(0) && isDrawing)
            {
                UpdateLine(snappedPos);
            }
            else if (Input.GetMouseButtonUp(0) && isDrawing)
            {
                FinishLine();
            }
        }
    }

    private Vector3 SnapToGridIntersection(Vector3 position)
    {
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize * 1.5f;
        float width = height * cam.aspect * 1.5f;
        Vector3 camPos = cam.transform.position;
        float spacing = gridSystem.minorGridSize;

        // Calculate grid origin point
        float gridStartX = Mathf.Floor(camPos.x / spacing) * spacing - width / 2;
        float gridStartY = Mathf.Floor(camPos.y / spacing) * spacing - height / 2;

        // Calculate how many spacing units away from the start point
        float deltaX = position.x - gridStartX;
        float deltaY = position.y - gridStartY;

        // Round to nearest intersection
        int gridIndexX = Mathf.RoundToInt(deltaX / spacing);
        int gridIndexY = Mathf.RoundToInt(deltaY / spacing);

        // Calculate final intersection position
        return new Vector3(
            gridStartX + (gridIndexX * spacing),
            gridStartY + (gridIndexY * spacing),
            position.z
        );
    }

    public Vector3 SnapToGrid(Vector3 position, bool debug = false)
    {
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize * 1.5f;
        float width = height * cam.aspect * 1.5f;
        Vector3 camPos = cam.transform.position;
        float spacing = gridSystem.minorGridSize / 2.0f;

        // Calculate grid origin point
        float gridStartX = Mathf.Floor(camPos.x / spacing) * spacing - width / 2;
        float gridStartY = Mathf.Floor(camPos.y / spacing) * spacing - height / 2;

        // Calculate how many spacing units away from the start point
        float deltaX = position.x - gridStartX;
        float deltaY = position.y - gridStartY;

        // Find the nearest grid line index
        int gridIndexX = Mathf.RoundToInt(deltaX / spacing);
        int gridIndexY = Mathf.RoundToInt(deltaY / spacing);

        // Calculate final snapped position
        return new Vector3(
            gridStartX + (gridIndexX * spacing),
            gridStartY + (gridIndexY * spacing),
            position.z
        );
    }

    void StartDrawing(Vector3 worldPos)
    {
        isDrawing = true;
        startPos = worldPos;
        currentLine.SetPosition(0, startPos);
        currentLine.SetPosition(1, startPos);
    }

    void UpdateLine(Vector3 currentPos)
    {
        Vector3 direction = currentPos - (Vector3)startPos;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            currentPos = new Vector3(currentPos.x, startPos.y, currentPos.z);
        else
            currentPos = new Vector3(startPos.x, currentPos.y, currentPos.z);

        currentLine.SetPosition(0, startPos);
        currentLine.SetPosition(1, currentPos);
    }

    private float CalculateLineValue(LineRenderer line)
    {
        if (line != null)
        {
            Vector3 start = line.GetPosition(0);
            Vector3 end = line.GetPosition(1);
            float xDiff = Mathf.Abs(end.x - start.x);
            float yDiff = Mathf.Abs(end.y - start.y);
            float distance = Mathf.Max(xDiff, yDiff);
            return distance / SNAP_INTERVAL / 4f;
        }
        return 0f;
    }

    void FinishLine()
    {
        Vector3 start = currentLine.GetPosition(0);
        Vector3 end = currentLine.GetPosition(1);

        if (Vector3.Distance(start, end) > 0.01f)
        {
            isDrawing = false;

            // Calculate and display the line's value
            float value = CalculateLineValue(currentLine);
            GameObject textObject = CreateValueText(end, value);
            lineTexts.Add(textObject);

            // Prepare for the next line
            lineCount++;
            lines.Add(CreateNewLineRenderer());
        }
        else
        {
            // Line is too short, remove it
            if (lineCount == 0)
            {
                currentLine.SetPosition(0, Vector3.zero);
                currentLine.SetPosition(1, Vector3.zero);
            }
            else
            {
                Destroy(currentLine.gameObject);
                lines.RemoveAt(lineCount);
                lines.Add(CreateNewLineRenderer());
            }
        }

        isDrawing = false;
    }

    public bool OnUndoPressed()
    {
        if (lineCount <= 0)
            return false;

        // Remove the last completed line
        Destroy(lines[lineCount].gameObject);
        lines.RemoveAt(lineCount);

        // Remove the associated text
        if (lineTexts.Count > 0)
        {
            Destroy(lineTexts[lineTexts.Count - 1]);
            lineTexts.RemoveAt(lineTexts.Count - 1);
        }

        lineCount--;

        // Ensure we always have a line ready for drawing
        if (lines.Count <= lineCount + 1)
        {
            lines.Add(CreateNewLineRenderer());
        }

        isDrawing = false;
        currentLine = lines[lineCount];

        return true;
    }

    public float GetMeasuredValue(int lineIndex = -1)
    {
        // If lineIndex is not specified, use the most recently completed line
        int index = lineIndex >= 0 ? lineIndex : Mathf.Max(0, lineCount - 1);

        if (index < lines.Count)
        {
            LineRenderer lineToMeasure = lines[index];
            Vector3 start = lineToMeasure.GetPosition(0);
            Vector3 end = lineToMeasure.GetPosition(1);
            float xDiff = Mathf.Abs(end.x - start.x);
            float yDiff = Mathf.Abs(end.y - start.y);
            float distance = Mathf.Max(xDiff, yDiff);
            return distance / SNAP_INTERVAL / 4f;
        }
        return 0f;
    }
}