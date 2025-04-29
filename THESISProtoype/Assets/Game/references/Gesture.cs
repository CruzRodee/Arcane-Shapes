using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using Unity.VisualScripting;

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
    private int lineCount = 0;
    public GridSystem gridSystem;
    private GameBehaviour main;

    //NEW
    public AnimScript animScript;

    private int GetMaxLinesForShape()
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

        return textObj;
    }

    void Update()
    {
        int maxLines = GetMaxLinesForShape();
        if (lineCount >= maxLines)
        {
            main.SetCastingState(true); return;
        }
        else { main.SetCastingState(false); }

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
        float result = 0f;
        if (Vector3.Distance(start, end) > 0.01f)
        {
            isDrawing = false;
            if (lineCount == 0)
            {
                firstLine = currentLine;
                float value = CalculateLineValue(firstLine);
                firstLineText = CreateValueText(end, value);

                string currentText = main.text.text;
                switch (main.spellCastEvent.problem.problemShape)
                {
                    case GameBehaviour.SHAPES.TRIANGLE:
                        currentText = currentText.Replace("[B]", value.ToString("F2"));
                        break;
                    case GameBehaviour.SHAPES.RECTANGLE:
                        currentText = currentText.Replace("[L]", value.ToString("F2"));
                        break;
                    case GameBehaviour.SHAPES.SQUARE:
                        currentText = currentText.Replace("[S]", value.ToString("F2"));

                        result = (float)Math.Pow(value, 2);
                        result = (float)Math.Round(result * 10) / 10.0f;

                        currentText = currentText.Replace("[A]", result.ToString("F2"));
                        break;
                    case GameBehaviour.SHAPES.CIRCLE:
                        currentText = currentText.Replace("[R]", value.ToString("F2"));

                        result = (float)(Math.PI * Math.Pow(value, 2));
                        result = (float)Math.Round(result * 10) / 10.0f;

                        currentText = currentText.Replace("[A]", result.ToString("F2"));
                        break;
                    case GameBehaviour.SHAPES.SEMI_CIRCLE:
                        currentText = currentText.Replace("[R]", value.ToString("F2"));

                        result = (float)(0.5 * (Math.PI * Math.Pow(value, 2)));
                        result = (float)Math.Round(result * 10) / 10.0f;

                        currentText = currentText.Replace("[A]", result.ToString("F2"));
                        break;
                }
                main.text.text = currentText;
            }
            else if (lineCount == 1)
            {
                secondLine = currentLine;
                float value = CalculateLineValue(secondLine);
                secondLineText = CreateValueText(end, value);

                string currentText = main.text.text;
                switch (main.spellCastEvent.problem.problemShape)
                {
                    case GameBehaviour.SHAPES.TRIANGLE:
                        currentText = currentText.Replace("[H]", value.ToString("F2"));

                        result = (0.5f * CalculateLineValue(firstLine) * value);
                        result = (float)Math.Round(result * 10) / 10.0f;

                        currentText = currentText.Replace("[A]", result.ToString("F2"));
                        break;
                    case GameBehaviour.SHAPES.RECTANGLE:
                        currentText = currentText.Replace("[W]", value.ToString("F2"));

                        result = (CalculateLineValue(firstLine) * value);
                        result = (float)Math.Round(result * 10) / 10.0f;

                        currentText = currentText.Replace("[A]", result.ToString("F2"));
                        break;
                }
                main.text.text = currentText;
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
        if (lineCount <= 0) return;

        lineCount--; // Reduce lines by one if there is > 0 lines

        // Reset text to base form
        main.text.text = GlobalVariables.ShapeFormulaText(main.spellCastEvent.problem.problemShape);

        //Reset shape fill
        main.shapeFiller.fillMaxValue = 0f;
        main.shapeFiller.isFillingActive = true;
        //Reset slider
        main.slider.value = 0f;

        // Redo text replacements : Partially Copy pasted from above
        if (lineCount > 0) // If there is one line remaining
        {
            //Destroy secondline
            Destroy(secondLine.gameObject);
            if (secondLineText != null) Destroy(secondLineText);
            secondLine = null;
            secondLineText = null;

            float value = CalculateLineValue(firstLine);

            string currentText = main.text.text;
            switch (main.spellCastEvent.problem.problemShape)
            {
                case GameBehaviour.SHAPES.TRIANGLE:
                    currentText = currentText.Replace("[B]", value.ToString("F2"));
                    break;
                case GameBehaviour.SHAPES.RECTANGLE:
                    currentText = currentText.Replace("[L]", value.ToString("F2"));
                    break;
                // No need for square, circle or semicircle since they only have one line
            }
            main.text.text = currentText;
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

        /*
        public void OnUndoPressed()
        {
            if (lineCount <= 0) return;

            //Remove Area value on any undo
            main.text.text = main.text.text.Replace(main.text.text.Split("=")[1], " [A]");
            //Reset shape fill
            main.shapeFiller.fillMaxValue = 0f;
            main.shapeFiller.isFillingActive = true;
            //Reset slider
            main.slider.value = 0f;

            void RestorePlaceholder(GameObject textObj, string placeholder)
            {
                if (textObj != null)
                {
                    string valueToReplace = textObj.GetComponent<TextMesh>().text;
                    if (!string.IsNullOrEmpty(valueToReplace) && float.TryParse(valueToReplace, out float value))
                    {
                        string standardizedValue = value.ToString("F2");
                        main.text.text = main.text.text.Replace(standardizedValue, placeholder);
                    }
                }
            }

            if (lineCount == 2 && secondLine != null)
            {
                switch (main.spellCastEvent.problem.problemShape)
                {
                    case GameBehaviour.SHAPES.TRIANGLE:
                        RestorePlaceholder(secondLineText, "[H]");
                        break;
                    case GameBehaviour.SHAPES.RECTANGLE:
                        RestorePlaceholder(secondLineText, "[W]");
                        break;
                }
                Destroy(secondLine.gameObject);
                if (secondLineText != null) Destroy(secondLineText);
                secondLine = null;
                secondLineText = null;
            }
            else if (lineCount == 1 && firstLine != null)
            {
                switch (main.spellCastEvent.problem.problemShape)
                {
                    case GameBehaviour.SHAPES.TRIANGLE:
                        RestorePlaceholder(firstLineText, "[B]");
                        break;
                    case GameBehaviour.SHAPES.RECTANGLE:
                        RestorePlaceholder(firstLineText, "[L]");
                        break;
                    case GameBehaviour.SHAPES.SQUARE:
                        RestorePlaceholder(firstLineText, "[S]");
                        break;
                    case GameBehaviour.SHAPES.CIRCLE:
                    case GameBehaviour.SHAPES.SEMI_CIRCLE:
                        RestorePlaceholder(firstLineText, "[R]");
                        break;
                }
                firstLine.SetPosition(0, Vector3.zero);
                firstLine.SetPosition(1, Vector3.zero);
                if (firstLineText != null) Destroy(firstLineText);
                firstLineText = null;
            }

            lineCount--;
            if (lineCount < 0) lineCount = 0;

            isDrawing = false;
            currentLine = (lineCount == 1) ? secondLine : firstLine;
        }
        */

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