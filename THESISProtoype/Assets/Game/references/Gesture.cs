using UnityEngine;
using System;

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

    private HOGameScript hoMain;
    private float result; // Added this since it's used in your calculations

    //VFX material
    public Material lineMaterial;

    //Audio/SFX Stuff
    public AudioClip[] sfxSet;
    private AudioSource sfxSource;
    private float volumeFactor = 1.0f; //Multiplier of volume for mute / volume slider functions

    void Awake()
    {
        //Create and attach AudioSource
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        //Settings
        sfxSource.playOnAwake = false;
        if (GlobalVariables.isMute) //Mute function
            volumeFactor = 0f;
    }

    private void PlaySFX(int clipIndex, float pitch = 1f, float volume = 1f)
    {
        if (sfxSource != null)
            sfxSource.pitch = pitch;

        if (sfxSet.Length > 0 && sfxSet[clipIndex] != null)
            sfxSource.PlayOneShot(sfxSet[clipIndex], volume * volumeFactor);
    }

    private void PlaySFXOnFinish(int clipIndex, float pitch = 1f, float volume = 1f)
    {
        if (sfxSource != null)
        {
            sfxSource.pitch = pitch;
            sfxSource.volume = volume * volumeFactor;
            sfxSource.clip = sfxSet[clipIndex];
        }

        if (sfxSet.Length > 0 && sfxSet[clipIndex] != null)
            if(!sfxSource.isPlaying)
                sfxSource.Play();
    }

    public int GetMaxLinesForShape()
    {
        if (hoMain == null)
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

        else
        {
            if (hoMain.spellCastEvent == null)
                return 0;

            switch (hoMain.spellCastEvent.problem.problemShape)
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
        hoMain = FindObjectOfType<HOGameScript>();
    }

    private LineRenderer CreateNewLineRenderer()
    {
        GameObject lineObj = new GameObject("Line" + lineCount);
        lineObj.transform.parent = this.transform;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;

        lr.useWorldSpace = true;

        lr.material = lineMaterial;

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

        //Play drawing SFX when line updates
        PlaySFXOnFinish(0, 2f, 0.3f);
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

        //NEW Note: Discard this since this cannot be seen anyways
        //animScript.playerScript.GoodTrace(UnityEngine.Random.Range(0, 4)); //Random player animation

        //Play Finish SFX after stopping any current sfx
        sfxSource.Stop();
        PlaySFX(1, 1, 4);
    }

    public void OnUndoPressed()
    {
        if (lineCount <= 0)
        {
            lineCount = 0;
            return;
        }

        //Toggle Dialogue Box, reset button, flag and dialogue text
        if (hoMain == null)
            main.UndoMeasure();
        else
            hoMain.UndoMeasure();

        lineCount--; // Reduce lines by one if there is > 0 lines

        //Reset shape fill
        if (hoMain != null)
        {
            hoMain.shapeFiller.fillMaxValue = 0f;
            hoMain.shapeFiller.isFillingActive = true;
            //Reset slider
            hoMain.slider.value = 0f;
        }

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