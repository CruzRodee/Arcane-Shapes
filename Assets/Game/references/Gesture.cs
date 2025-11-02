using UnityEngine;

public class LineSnapper : MonoBehaviour
{
    private LineRenderer currentLine;

    // NEW: Made public for GameBehaviour validation access
    [HideInInspector] public LineRenderer firstLine;
    [HideInInspector] public LineRenderer secondLine;

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
    private float result;

    //VFX material
    public Material lineMaterial;

    //Audio/SFX Stuff
    public AudioClip[] sfxSet;
    private AudioSource sfxSource;
    private float volumeFactor = 1.0f;

    // FIXED: Add initialization state tracking
    private bool isInitialized = false;
    private bool inputEnabled = false;

    // NEW: Store the actual drawn positions for validation
    private Vector3[] drawnLinePositions = new Vector3[4]; // Start/End for 2 lines max

    // DEBUG: Add counters for debugging
    private int debugFrameCount = 0;
    private float lastInputCheck = 0f;

    void Awake()
    {
        Debug.Log("=== LineSnapper Awake() ===");

        //Create and attach AudioSource
        sfxSource = GetComponent<AudioSource>();
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        //Settings
        sfxSource.playOnAwake = false;
        if (GlobalVariables.isMute)
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
            if (!sfxSource.isPlaying)
                sfxSource.Play();
    }

    public int GetMaxLinesForShape()
    {
        if (hoMain == null)
        {
            if (main == null || main.spellCastEvent == null)
            {
                return 2;
            }

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
            {
                return 2;
            }

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
        Debug.Log("=== LineSnapper Start() ===");

        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("LineSnapper: Main camera not found!");
        }
        else
        {
            Debug.Log($"LineSnapper: Found main camera: {mainCamera.name}");
        }

        if (gridSystem == null)
            gridSystem = FindObjectOfType<GridSystem>();

        main = FindObjectOfType<GameBehaviour>();
        hoMain = FindObjectOfType<HOGameScript>();

        Debug.Log($"LineSnapper Start complete - GridSystem: {gridSystem != null}, Main: {main != null}, HOMain: {hoMain != null}");
    }

    void OnEnable()
    {
        Debug.Log("=== LineSnapper OnEnable() called ===");
        Debug.Log($"IsInitialized: {isInitialized}, InputEnabled: {inputEnabled}");
    }

    void OnDisable()
    {
        Debug.Log("=== LineSnapper OnDisable() called ===");
    }

    public void ForceInitialize()
    {
        Debug.Log("=== LineSnapper ForceInitialize() called ===");

        // Make sure we have all dependencies
        if (gridSystem == null)
        {
            gridSystem = FindObjectOfType<GridSystem>();
            Debug.Log($"Found GridSystem: {gridSystem != null}");
        }

        if (main == null)
        {
            main = FindObjectOfType<GameBehaviour>();
            Debug.Log($"Found GameBehaviour: {main != null}");
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.Log($"Found Camera: {mainCamera != null}");
        }

        // Create first line if not already created
        if (firstLine == null)
        {
            firstLine = CreateNewLineRenderer();
            Debug.Log("Created first line renderer");
        }

        // Reset state
        lineCount = 0;
        value1 = "???";
        value2 = "???";
        isDrawing = false;

        // Clear position tracking
        for (int i = 0; i < drawnLinePositions.Length; i++)
        {
            drawnLinePositions[i] = Vector3.zero;
        }

        isInitialized = true;
        inputEnabled = true;

        Debug.Log("=== LineSnapper ForceInitialize complete - INPUT ENABLED ===");
        Debug.Log($"GameObject.activeInHierarchy: {gameObject.activeInHierarchy}");
        Debug.Log($"Component.enabled: {enabled}");
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
        textMesh.text = value.ToString("F2");
        textMesh.characterSize = 0.4f;
        textMesh.anchor = TextAnchor.MiddleCenter;

        //Save values
        if (lineCount >= 1)
            value2 = value.ToString("F2");
        if (lineCount < 1)
            value1 = value.ToString("F2");

        return textObj;
    }

    // REPLACE the entire Update method in Gesture.cs

    void Update()
    {
        debugFrameCount++;

        // DEBUG: Log input state every 60 frames (about once per second)
        if (debugFrameCount % 60 == 0)
        {
            //Debug.Log($"[Frame {debugFrameCount}] LineSnapper Update - InputEnabled: {inputEnabled}, Initialized: {isInitialized}, GameObject.active: {gameObject.activeInHierarchy}, Component.enabled: {enabled}");

            if (inputEnabled && isInitialized)
            {
                //Debug.Log($"Input should work - GridSystem: {gridSystem != null}, Camera: {mainCamera != null}, LineCount: {lineCount}, MaxLines: {GetMaxLinesForShape()}");
            }
        }

        // Check if input is enabled
        if (!inputEnabled)
        {
            if (debugFrameCount % 60 == 0)
                Debug.Log("Input blocked: inputEnabled is false");
            return;
        }

        if (!isInitialized)
        {
            if (debugFrameCount % 60 == 0)
                Debug.Log("Input blocked: not initialized");
            return;
        }

        if (gridSystem == null)
        {
            gridSystem = FindObjectOfType<GridSystem>();
            if (gridSystem == null)
            {
                if (debugFrameCount % 60 == 0)
                    Debug.Log("Input blocked: GridSystem is null");
                return;
            }
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                if (debugFrameCount % 60 == 0)
                    Debug.Log("Input blocked: Main camera is null");
                return;
            }
        }

        int maxLines = GetMaxLinesForShape();
        if (maxLines > 0 && lineCount >= maxLines)
        {
            if (debugFrameCount % 60 == 0)
                Debug.Log($"Input blocked: Line limit reached ({lineCount}/{maxLines})");
            return;
        }

        // Setup current line
        if (lineCount == 0)
        {
            if (firstLine == null)
                firstLine = CreateNewLineRenderer();
            currentLine = firstLine;
        }
        else if (lineCount == 1)
        {
            if (secondLine == null)
                secondLine = CreateNewLineRenderer();
            currentLine = secondLine;
        }

        if (currentLine == null)
        {
            if (debugFrameCount % 60 == 0)
                Debug.Log("Input blocked: currentLine is null");
            return;
        }

        // SIMPLIFIED EventSystem check - be more permissive
        bool uiBlocked = false;
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            uiBlocked = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
            if (uiBlocked && debugFrameCount % 60 == 0)
            {
                Debug.Log("Input blocked: Pointer over UI GameObject");
            }
        }

        // Check for input every frame and log when detected
        bool hasInput = false;

        // Touch input
        if (Input.touchCount > 0)
        {
            hasInput = true;
            if (Time.time - lastInputCheck > 0.5f) // Log every half second
            {
                Debug.Log($"TOUCH INPUT DETECTED - Count: {Input.touchCount}, UI Blocked: {uiBlocked}");
                lastInputCheck = Time.time;
            }

            if (!uiBlocked)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 touchPos = touch.position;
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 10f));
                Vector3 intersectionSnappedPos = SnapToGridIntersection(worldPos);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        Debug.Log("Touch began - Starting drawing");
                        StartDrawing(intersectionSnappedPos);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (isDrawing)
                        {
                            Vector3 snappedEnd = SnapToGrid(worldPos);

                            // Constrain the line to be horizontal or vertical
                            Vector3 direction = snappedEnd - (Vector3)startPos;
                            Vector3 constrainedEnd;
                            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                                constrainedEnd = new Vector3(snappedEnd.x, startPos.y, snappedEnd.z);
                            else
                                constrainedEnd = new Vector3(startPos.x, snappedEnd.y, snappedEnd.z);

                            UpdateLine(constrainedEnd);

                            // Live preview update with correct value calculation and position data
                            float distance = Vector3.Distance(startPos, constrainedEnd);
                            float currentValue = distance / SNAP_INTERVAL / 4f;
                            main.variableDisplayManager.OnMeasurementPreview(lineCount, currentValue, startPos, constrainedEnd);
                        }
                        break;
                    case TouchPhase.Ended:
                        Debug.Log("Touch ended - Finishing line");
                        if (isDrawing) FinishLine();
                        break;
                }
            }
        }

        // Mouse input
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
            hasInput = true;
            if (Time.time - lastInputCheck > 0.5f) // Log every half second
            {
                Debug.Log($"MOUSE INPUT DETECTED - Down: {Input.GetMouseButtonDown(0)}, Hold: {Input.GetMouseButton(0)}, Up: {Input.GetMouseButtonUp(0)}, UI Blocked: {uiBlocked}");
                lastInputCheck = Time.time;
            }

            if (!uiBlocked)
            {
                Vector3 mousePos = Input.mousePosition;
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 10f));
                Vector3 snappedPos = SnapToGrid(worldPos);

                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Mouse down - Starting drawing");
                    StartDrawing(SnapToGridIntersection(worldPos));
                }
                else if (Input.GetMouseButton(0) && isDrawing)
                {
                    // Constrain the line to be horizontal or vertical
                    Vector3 direction = snappedPos - (Vector3)startPos;
                    Vector3 constrainedEnd;
                    if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                        constrainedEnd = new Vector3(snappedPos.x, startPos.y, snappedPos.z);
                    else
                        constrainedEnd = new Vector3(startPos.x, snappedPos.y, snappedPos.z);

                    UpdateLine(constrainedEnd);

                    // Live preview update with correct value calculation and position data
                    float distance = Vector3.Distance(startPos, constrainedEnd);
                    float currentValue = distance / SNAP_INTERVAL / 4f;
                    main.variableDisplayManager.OnMeasurementPreview(lineCount, currentValue, startPos, constrainedEnd);
                }
                else if (Input.GetMouseButtonUp(0) && isDrawing)
                {
                    Debug.Log("Mouse up - Finishing line");
                    SnapToGrid(worldPos, true);
                    FinishLine();
                }
            }
        }
    }
    
    private Vector3 SnapToGridIntersection(Vector3 position)
    {
        if (gridSystem == null)
        {
            Debug.LogWarning("GridSystem is null in SnapToGridIntersection");
            return position;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Camera.main is null in SnapToGridIntersection");
            return position;
        }

        float height = 2f * cam.orthographicSize * 1.5f;
        float width = height * cam.aspect * 1.5f;
        Vector3 camPos = cam.transform.position;
        float spacing = gridSystem.minorGridSize;

        float gridStartX = Mathf.Floor(camPos.x / spacing) * spacing - width / 2;
        float gridStartY = Mathf.Floor(camPos.y / spacing) * spacing - height / 2;

        float deltaX = position.x - gridStartX;
        float deltaY = position.y - gridStartY;

        int gridIndexX = Mathf.RoundToInt(deltaX / spacing);
        int gridIndexY = Mathf.RoundToInt(deltaY / spacing);

        Vector3 snappedPos = new Vector3(
            gridStartX + (gridIndexX * spacing),
            gridStartY + (gridIndexY * spacing),
            position.z
        );

        return snappedPos;
    }

    public Vector3 SnapToGrid(Vector3 position, bool debug = false)
    {
        if (gridSystem == null)
        {
            Debug.LogWarning("GridSystem is null in SnapToGrid");
            return position;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Camera.main is null in SnapToGrid");
            return position;
        }

        float height = 2f * cam.orthographicSize * 1.5f;
        float width = height * cam.aspect * 1.5f;
        Vector3 camPos = cam.transform.position;
        float spacing = gridSystem.minorGridSize / 2.0f;

        float gridStartX = Mathf.Floor(camPos.x / spacing) * spacing - width / 2;
        float gridStartY = Mathf.Floor(camPos.y / spacing) * spacing - height / 2;

        float deltaX = position.x - gridStartX;
        float deltaY = position.y - gridStartY;

        int gridIndexX = Mathf.RoundToInt(deltaX / spacing);
        int gridIndexY = Mathf.RoundToInt(deltaY / spacing);

        Vector3 snappedPos = new Vector3(
            gridStartX + (gridIndexX * spacing),
            gridStartY + (gridIndexY * spacing),
            position.z
        );

        return snappedPos;
    }

    void StartDrawing(Vector3 worldPos)
    {
        if (currentLine == null) return;

        isDrawing = true;
        startPos = worldPos;
        currentLine.SetPosition(0, startPos);
        currentLine.SetPosition(1, startPos);

        Debug.Log($"*** STARTED DRAWING LINE {lineCount} at {worldPos} ***");
    }

    void UpdateLine(Vector3 currentPos)
    {
        if (currentLine == null) return;

        Vector3 direction = currentPos - (Vector3)startPos;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            currentPos = new Vector3(currentPos.x, startPos.y, currentPos.z);
        else
            currentPos = new Vector3(startPos.x, currentPos.y, currentPos.z);

        currentLine.SetPosition(0, startPos);
        currentLine.SetPosition(1, currentPos);

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
        if (currentLine == null) return;

        Vector3 start = currentLine.GetPosition(0);
        Vector3 end = currentLine.GetPosition(1);

        if (Vector3.Distance(start, end) > 0.01f)
        {
            isDrawing = false;
            float value = 0f;

            // Store the actual drawn positions for validation
            if (lineCount == 0)
            {
                firstLine = currentLine;
                value = CalculateLineValue(firstLine);
                firstLineText = CreateValueText(end, value);

                // NEW: Store drawn line positions
                drawnLinePositions[0] = start; // First line start
                drawnLinePositions[1] = end;   // First line end
            }
            else if (lineCount == 1)
            {
                secondLine = currentLine;
                value = CalculateLineValue(secondLine);
                secondLineText = CreateValueText(end, value);

                // NEW: Store drawn line positions
                drawnLinePositions[2] = start; // Second line start
                drawnLinePositions[3] = end;   // Second line end
            }

            lineCount++;

            Debug.Log($"*** FINISHED LINE {lineCount - 1} with value: {value} ***");
            Debug.Log($"*** Line drawn from {start} to {end} ***");

            // *** PHASE 1 CALLBACK WITH POSITION DATA ***
            if (main != null)
            {
                int measurementIndex = lineCount - 1;
                // NEW: Pass the actual drawn line positions to GameBehaviour
                main.OnMeasurementCompleted(measurementIndex, value, start, end);
                Debug.Log($"Notified GameBehaviour: measurement {measurementIndex} = {value} from {start} to {end}");
            }
        }
        else
        {
            Debug.Log("Line too short, discarding");
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

        sfxSource.Stop();
        PlaySFX(1, 1, 4);
    }

    // NEW: Public method to get drawn line positions for a specific measurement
    public bool GetDrawnLinePositions(int measurementIndex, out Vector3 start, out Vector3 end)
    {
        start = Vector3.zero;
        end = Vector3.zero;

        if (measurementIndex == 0 && lineCount > 0)
        {
            start = drawnLinePositions[0];
            end = drawnLinePositions[1];
            return true;
        }
        else if (measurementIndex == 1 && lineCount > 1)
        {
            start = drawnLinePositions[2];
            end = drawnLinePositions[3];
            return true;
        }

        return false;
    }

    public void OnUndoPressed()
    {
        Debug.Log($"OnUndoPressed called - lineCount: {lineCount}");

        if (lineCount <= 0)
        {
            lineCount = 0;
            return;
        }

        if (hoMain != null)
            hoMain.UndoMeasure();
        //if (hoMain == null)
        //   main.h
        //main.inputHandler.HandleUndo();
        //else
        //   hoMain.UndoMeasure();

        lineCount--;

        if (hoMain != null)
        {
            hoMain.shapeFiller.fillMaxValue = 0f;
            hoMain.shapeFiller.isFillingActive = true;
        }

        if (lineCount > 0)
        {
            if (secondLine != null)
            {
                Destroy(secondLine.gameObject);
                secondLine = null;
            }
            if (secondLineText != null)
            {
                Destroy(secondLineText);
                secondLineText = null;
            }

            // NEW: Clear second line position data
            drawnLinePositions[2] = Vector3.zero;
            drawnLinePositions[3] = Vector3.zero;

            float value = CalculateLineValue(firstLine);
        }
        else if (lineCount <= 0)
        {
            if (firstLine != null)
            {
                firstLine.SetPosition(0, Vector3.zero);
                firstLine.SetPosition(1, Vector3.zero);
            }
            if (firstLineText != null)
            {
                Destroy(firstLineText);
                firstLineText = null;
            }

            // NEW: Clear first line position data
            drawnLinePositions[0] = Vector3.zero;
            drawnLinePositions[1] = Vector3.zero;
        }

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