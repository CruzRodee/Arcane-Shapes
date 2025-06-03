using UnityEngine;

public class LineSnapper : MonoBehaviour
{
    private LineRenderer currentLine;
    private LineRenderer firstLine;
    private LineRenderer secondLine;
    private GameObject firstLineText;
    private GameObject secondLineText;
    private Vector2 startPos;
    private bool isDrawing = false;
    private Camera mainCamera;
    private const float SNAP_INTERVAL = 0.25f;
    public int lineCount = 0;
    public GridSystem gridSystem;
    private GameBehaviour main;
    public string value1 = "???", value2 = "???";

    //NEW
    public AnimScript animScript;

    public int GetMaxLinesForShape()
    {
        if (main.spellCastEvent == null)
            return 0;

        switch (main.spellCastEvent.problem.problemShape)
        {
            case GameBehaviour.SHAPES.TRIANGLE:
            case GameBehaviour.SHAPES.RECTANGLE:
                return 2;
            case GameBehaviour.SHAPES.SQUARE:
            case GameBehaviour.SHAPES.CIRCLE:
            case GameBehaviour.SHAPES.SEMI_CIRCLE:
                return 1;
            default:
                return 0;
        }
    }

    public void ToggleLineText()
    {
        if (firstLineText != null)
            firstLineText.SetActive(!firstLineText.activeInHierarchy);
        if (secondLineText != null)
            secondLineText.SetActive(!secondLineText.activeInHierarchy);
    }

    void Start()
    {
        mainCamera = Camera.main;
        firstLine = CreateNewLineRenderer();
        gridSystem = FindObjectOfType<GridSystem>();
        main = FindObjectOfType<GameBehaviour>();
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
        GameObject textObj = new GameObject("LineValue");
        textObj.transform.parent = this.transform;

        textObj.transform.position = position + new Vector3(0.2f, 0.2f, 0);

        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = value.ToString("F2");  // Standardized to 2 decimal places
        textMesh.characterSize = 0.4f;
        textMesh.anchor = TextAnchor.MiddleCenter;

        //Save values
        if (lineCount >= 1)
            value2 = value.ToString("F2");
        if (lineCount < 1)
            value1 = value.ToString("F2");

        return textObj;
    }

    void Update()
    {
        if (lineCount >= GetMaxLinesForShape())
        {
            //main.SetCastingState(true);
            return;
        }
        //else { main.SetCastingState(false); }

        if (lineCount == 0)
            currentLine = firstLine;
        else if (lineCount == 1 && secondLine == null)
            secondLine = CreateNewLineRenderer();

        if (lineCount == 1)
            currentLine = secondLine;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 touchPos = touch.position;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 10f));
            // Vector3 snappedPos = SnapToGrid(worldPos);
            Vector3 intersectionSnappedPos = SnapToGridIntersection(worldPos);
            //UnityEngine.Debug.Log("" + snappedPos);

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
        else
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
            Vector3 snappedPos = SnapToGrid(worldPos);


            if (Input.GetMouseButtonDown(0))
            {
                StartDrawing(SnapToGridIntersection(worldPos));
            }

            else if (Input.GetMouseButton(0) && isDrawing) UpdateLine(snappedPos);
            else if (Input.GetMouseButtonUp(0) && isDrawing)
            {
                SnapToGrid(worldPos, true);
                UnityEngine.Debug.Log("" + currentLine.GetPosition(0));
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
        Vector3 snappedPos = new Vector3(
            gridStartX + (gridIndexX * spacing),
            gridStartY + (gridIndexY * spacing),
            position.z
        );

        return snappedPos;
    }

    public Vector3 SnapToGrid(Vector3 position, bool debug = false)
    {
        Camera cam = Camera.main;
        float height = 2f * cam.orthographicSize * 1.5f;
        float width = height * cam.aspect * 1.5f;
        Vector3 camPos = cam.transform.position;
        float spacing = gridSystem.minorGridSize / 2.0f;

        // Calculate grid origin point (like in CreateGridLines)
        float gridStartX = Mathf.Floor(camPos.x / spacing) * spacing - width / 2;
        float gridStartY = Mathf.Floor(camPos.y / spacing) * spacing - height / 2;

        // Calculate how many spacing units away from the start point
        float deltaX = position.x - gridStartX;
        float deltaY = position.y - gridStartY;

        // Find the nearest grid line index
        int gridIndexX = Mathf.RoundToInt(deltaX / spacing);
        int gridIndexY = Mathf.RoundToInt(deltaY / spacing);

        // Calculate final snapped position
        Vector3 snappedPos = new Vector3(
            gridStartX + (gridIndexX * spacing),
            gridStartY + (gridIndexY * spacing),
            position.z
        );

        return snappedPos;
    }

    /*    public Vector3 SnapToGrid(Vector3 position, bool debug = false)
        {
            Camera cam = Camera.main;
            float height = 2f * cam.orthographicSize * 1.5f;
            float width = height * cam.aspect * 1.5f;

            Vector3 camPos = cam.transform.position;
            float spacing = gridSystem.minorGridSize / 4.0f;

            //float x = Mathf.Floor(camPos.x / spacing) * spacing - width / 2;
            //float y = Mathf.Floor(camPos.y / spacing) * spacing - height / 2;

              Vector3 snappedPos = new Vector3(
                  Mathf.Round(position.x / spacing) * spacing,
                  Mathf.Round(position.y / spacing) * spacing,
                  position.z
              );

           *//* Vector3 snappedPos = new Vector3(
                  Mathf.Floor(position.x / spacing) * spacing - width / 2,
                  Mathf.Floor(position.y / spacing) * spacing - height / 2,
                  position.z
              );*//*



            if (debug)
            {
                UnityEngine.Debug.Log("Height: " + height);
                UnityEngine.Debug.Log("Width: " + width);
                UnityEngine.Debug.Log("Cam Pos: " + camPos);
                //UnityEngine.Debug.Log("X: " + x);
                //UnityEngine.Debug.Log("Y: " + y);
                UnityEngine.Debug.Log("sP_X: " + snappedPos.x);
                UnityEngine.Debug.Log("sP_Y: " + snappedPos.y);
            }



            return snappedPos;
        }*/

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
            if (lineCount == 0)
            {
                firstLine = currentLine;
                float value = CalculateLineValue(firstLine);
                firstLineText = CreateValueText(end, value);
            }
            else if (lineCount == 1)
            {
                secondLine = currentLine;
                float value = CalculateLineValue(secondLine);
                secondLineText = CreateValueText(end, value);
            }
            lineCount++;
        }
        else
        {
            if (lineCount == 0)
            {
                firstLine.SetPosition(0, Vector3.zero);
                firstLine.SetPosition(1, Vector3.zero);
            }
            else
            {
                Destroy(currentLine.gameObject);
            }
        }
        isDrawing = false;

        //NEW
        animScript.playerScript.GoodTrace(UnityEngine.Random.Range(0, 4)); //Random player animation
    }

    public void OnUndoPressed()
    {
        if (lineCount <= 0)
        {
            lineCount = 0;
            return;
        }

        //Toggle Dialogue Box, reset button, flag and dialogue text
        main.UndoMeasure();

        lineCount--; // Reduce lines by one if there is > 0 lines

        //Reset shape fill
        main.shapeFiller.fillMaxValue = 0f;
        main.shapeFiller.isFillingActive = true;

        // Redo text replacements : Partially Copy pasted from above
        if (lineCount > 0) // If there is one line remaining
        {
            //Destroy secondline
            Destroy(secondLine.gameObject);
            if (secondLineText != null) Destroy(secondLineText);
            secondLine = null;
            secondLineText = null;

            float value = CalculateLineValue(firstLine);
        }
        else if (lineCount <= 0) // Nuke first line if linecount <= 0
        {
            firstLine.SetPosition(0, Vector3.zero);
            firstLine.SetPosition(1, Vector3.zero);
            if (firstLineText != null) Destroy(firstLineText);
            firstLineText = null;
        }
        // No need for 2 lines since the only possible line values are 1 and 0

        if (lineCount < 0) lineCount = 0;

        isDrawing = false;
        currentLine = (lineCount == 1) ? secondLine : firstLine;
    }

    public float getMeasuredValue()
    {
        LineRenderer lineToMeasure = (lineCount == 2) ? secondLine : firstLine;

        if (lineToMeasure != null)
        {
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