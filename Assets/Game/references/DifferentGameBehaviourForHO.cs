// ModifiedGameBehaviour.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModifiedGameBehaviour : MonoBehaviour // This script will be on the gbHolder GameObject for FormulaAnalyzer
{
    // --- Constants ---
    public const int UNUSED = -1;
    private const float TRANSITIONTIME = 0.4f, FILLTIMEAPROX = 1.5f, STARTDELAY = 3.0f;
    private const float ENDDELAY = 5.0f; // Used for standalone LO mode end
    private const float SPELLDELAY = 2.0f; // Generic delay for animations if specific duration isn't available
    private const float TRANSITIONDELAY = 0.5f;
    private const string castBtnText1_Measure = "Measure";
    private const string castBtnText2_Submit = "Submit Answer";
    private const string castBtnText3_RedoOCR = "Redo Input";
    private const float DIALOGUESLIDETIME = 0.25f;
    private const float OCRSLIDETIME = 0.35f;

    public enum HOGameState
    {
        Initializing, SelectingHOShape, SolvingLOSubProblem, CalculatingFinalSum, HOLevelComplete
    }
    public HOGameState currentHOGameState = HOGameState.Initializing;

    // HO Problem Data
    public List<ShapeObject> hoProblemShapesConfig; // Defined in DefineHOProblemConfiguration
    private List<ShapeObject> activeHOShapes;
    private Dictionary<ShapeObject, bool> hoShapeSolvedStatus;
    private Dictionary<ShapeObject, float> hoSolvedShapeAreas;
    private ShapeObject currentShapeBeingSolvedLO; // The HO ShapeObject being solved by LO mechanics
    private bool allIndividualShapesSolved = false;

    // Core Component References
    public ShapeGenerator shapeGenerator;
    public ShapeFiller shapeFiller;
    public LineSnapper lineSnapper;         // Assign in Inspector
    public ShapeClickManager shapeClickManager; // Assign in Inspector or Find
    public DrawingAndOCRManagerScript ocrScript; // Assign in Inspector
    public FormulaAnalyzer fa;              // Assign in Inspector

    // UI Element References
    public TMP_Text textDialoguePrompt; // Renamed for clarity (was 'text')
    public TMP_Text textManaMeasureDisplay; // Was 'manaMeasure' - for slider, likely unused
    public TMP_Text textCorrectionPercentage; // Was 'correctionPerc'
    public Button buttonUndo; // Was 'undo'
    private Button buttonChangeCamera;
    public Text textCharacterSays; // Was 'characterSay'
    public Text textManaRequirement; // Was 'manaReq'
    public Text textSubmitButton; // Was 'textFinish' - on the main action button

    public GameObject hudPanel; // Was 'hud'
    public GameObject quickMenuPanel; // Was 'quickMenu'
    public GameObject confirmationPopup; // Was 'pConfirm'
    public GameObject lowerScrollPanel; // Was 'pLowerScroll'
    public GameObject notificationPopup; // Was 'pNotify'
    public GameObject loSolverPanel; // Was 'pDialogue' - Main panel for LO measurement/input
    public GameObject loActionButtonsPanel; // Was 'pDiaButtons' - Contains Undo/Submit for LO

    public GameObject notificationTextObject; // Was 'notifyTextObj'
    private Text textNotificationPopup; // Was 'pNotifyText'
    public Text textHUDDisplay; // Was 'textHUD'

    public GameObject equationPanel_Triangle, equationPanel_Square, equationPanel_Rectangle, equationPanel_SemiCircle, equationPanel_Circle;
    public GameObject ocrInputElement; // Was 'ocrInput'
    public GameObject formulaDisplayElement; // Was 'formulaDisplay'
    public GameObject ocrBoardSlideStartTarget, ocrBoardSlideEndTarget; // Was 'rightStartTransObj', 'rightEndTransObj'
    public GameObject calculatorButtonObject; // Was 'calcBtnObj'
    private Text textCalculatorButton; // Was 'calcBtnText'

    public GameObject lineLengthDisplay_Sq1, lineLengthDisplay_Rect1, lineLengthDisplay_Rect2, lineLengthDisplay_Tri1, lineLengthDisplay_Tri2, lineLengthDisplay_Cir1, lineLengthDisplay_Semi1;
    private GameObject currentLineDisplay1, currentLineDisplay2; // Was 'var1Display', 'var2Display'


    // Internal State
    private bool isUICamera = true;
    private GameObject mainSceneCamera, classroomViewCamera; // Was 'mainCamera', 'classroomCamera'
    private Material uiModeMaterial, classroomModeMaterial; // Was 'uiMaterial', 'classroomMaterial'
    private Animator screenFadeAnimator;
    private AnimScript gameAnimScript; // Was 'animScript'
    private bool isStartupSequence = true; // Was 'STARTUP'
    public float currentLOError = 100f; // Was 'error'
    private RectTransform rtLOSolverPanel; // Was 'rtDialogue'
    private Vector2 originalLOSolverPanelPosition; // Was 'origDiaRT'
    private bool isMeasurementDoneLO; // Was 'isDoneMeasuring'
    private float currentPlayerInputAnswer; // Was 'inputAnswer'


    // --- SHAPES Enum and ShapeObject Class (Self-contained) ---
    public enum SHAPES { NONE, TRIANGLE, SQUARE, RECTANGLE, CIRCLE, SEMI_CIRCLE }
    public class ShapeObject
    {
        public int x = UNUSED; public int y = UNUSED; public SHAPES shape;
        public GameObject actualShapeObj; public Vector3 offset = Vector3.zero;
        public bool isToBeFilled = false; public float angle = 0; public bool isExcess = false;
        public Guid id; // Unique identifier
        public ShapeObject(int x, int y, SHAPES shape) { this.x = x; this.y = y; this.shape = shape; this.id = Guid.NewGuid(); }
        public ShapeObject withOffset(Vector3 o) { offset = o; return this; }
        public ShapeObject setIsToBeFilled() { isToBeFilled = true; return this; }
        public ShapeObject tilt(float a) { angle = a; return this; }
        public override bool Equals(object obj) => obj is ShapeObject other && id == other.id;
        public override int GetHashCode() => id.GetHashCode();
    }
    private Problem currentLOProblemInstance; // Instance for the current LO sub-problem visual

    void Awake()
    {
        currentHOGameState = HOGameState.Initializing;
        screenFadeAnimator = GameObject.Find("ScreenFade")?.GetComponent<Animator>();
        gameAnimScript = GameObject.Find("AnimHolder")?.GetComponent<AnimScript>();

        // OCR Script is assigned via Inspector
        // Formula Analyzer is assigned via Inspector

        if (notificationTextObject != null) textNotificationPopup = notificationTextObject.GetComponent<Text>();
        if (calculatorButtonObject != null)
        {
            Transform calcBtnTextTransform = calculatorButtonObject.transform.Find("textFinish");
            if (calcBtnTextTransform != null) textCalculatorButton = calcBtnTextTransform.GetComponent<Text>();
            calculatorButtonObject.SetActive(false);
        }
        currentLineDisplay1 = lineLengthDisplay_Sq1; // Default
    }

    void Start()
    {
        if (GameObject.Find("characterSay") != null) textCharacterSays = GameObject.Find("characterSay").GetComponent<Text>();
        if (GameObject.Find("ManaRequired") != null) textManaRequirement = GameObject.Find("ManaRequired").GetComponent<Text>();

        GameObject submitButtonGO = GameObject.Find("btnCast"); // Standard name for main action button
        if (submitButtonGO != null)
        {
            Transform finishTextTransform = submitButtonGO.transform.Find("textFinish");
            if (finishTextTransform != null) textSubmitButton = finishTextTransform.GetComponent<Text>();
        }
        if (textSubmitButton == null) Debug.LogError("MGB: textSubmitButton (Text on Submit Button) not found! Check button 'btnCast' and its child 'textFinish'.");
        else textSubmitButton.text = castBtnText1_Measure;

        isMeasurementDoneLO = false;
        if (GameObject.Find("pDiaButtons") != null) loActionButtonsPanel = GameObject.Find("pDiaButtons");
        if (loSolverPanel != null)
        {
            rtLOSolverPanel = loSolverPanel.GetComponent<RectTransform>();
            if (rtLOSolverPanel != null) originalLOSolverPanelPosition = rtLOSolverPanel.anchoredPosition;
        }
        else
        {
            loSolverPanel = GameObject.Find("PanelCasting"); // Fallback find
            if (loSolverPanel != null)
            {
                rtLOSolverPanel = loSolverPanel.GetComponent<RectTransform>();
                if (rtLOSolverPanel != null) originalLOSolverPanelPosition = rtLOSolverPanel.anchoredPosition;
            }
        }


        if (confirmationPopup != null) confirmationPopup.SetActive(false);
        if (lowerScrollPanel != null) lowerScrollPanel.SetActive(true); // Assuming this is general UI
        if (notificationPopup != null) notificationPopup.SetActive(false);
        HideAllEquationPanels();
        HideLOInterface(); // Keep LO specific UI hidden initially

        if (isStartupSequence)
        {
            if (screenFadeAnimator != null) screenFadeAnimator.SetTrigger("fadeIn");
            if (hudPanel != null) hudPanel.SetActive(true);
        }

        if (GameObject.Find("DialoguePrompt") != null) textDialoguePrompt = GameObject.Find("DialoguePrompt").GetComponent<TMP_Text>();
        if (textDialoguePrompt != null) textDialoguePrompt.text = "";
        if (GameObject.Find("ManaValue") != null) textManaMeasureDisplay = GameObject.Find("ManaValue").GetComponent<TMP_Text>();
        if (textManaMeasureDisplay != null) textManaMeasureDisplay.gameObject.SetActive(false);

        //if (GameObject.Find("ConfirmMeasurement") != null) confirmMeasurement = GameObject.Find("ConfirmMeasurement").GetComponent<Button>(); // Likely unused for OCR
        //if (confirmMeasurement != null) confirmMeasurement.gameObject.SetActive(false);
        if (GameObject.Find("ManaFillCorrectPerc") != null) textCorrectionPercentage = GameObject.Find("ManaFillCorrectPerc").GetComponent<TMP_Text>();
        if (textCorrectionPercentage != null) textCorrectionPercentage.gameObject.SetActive(false);

        // LineSnapper is assigned via Inspector
        if (lineSnapper != null)
        {
            lineSnapper.gameObject.SetActive(false);
            if (gameAnimScript != null) lineSnapper.animScript = this.gameAnimScript; // Pass AnimScript to LineSnapper
        }
        // buttonUndo is assigned via Inspector
        if (buttonUndo != null)
        {
            buttonUndo.gameObject.SetActive(false);
            buttonUndo.onClick.AddListener(OnUndoClicked_LO);
        }

        if (GameObject.Find("ChangeCamera") != null) buttonChangeCamera = GameObject.Find("ChangeCamera").GetComponent<Button>();
        mainSceneCamera = GameObject.Find("Main Camera"); // Assuming this is the gameplay/UI camera
        classroomViewCamera = GameObject.Find("ClassroomCamera"); // Assuming this is the overview camera
        if (mainSceneCamera != null) mainSceneCamera.SetActive(false); // Default to overview for HO
        if (classroomViewCamera != null) classroomViewCamera.SetActive(true);
        if (buttonChangeCamera != null) buttonChangeCamera.onClick.AddListener(OnChangeCameraClicked);

        uiModeMaterial = Resources.Load<Material>("Materials/UI_Material");
        classroomModeMaterial = Resources.Load<Material>("Materials/ClassroomScreenMaterial");

        StartCoroutine(InitializeHOEnvironment());
    }

    IEnumerator InitializeHOEnvironment()
    {
        // Ensure ShapeGenerator is ready
        while (this.shapeGenerator == null) { yield return new WaitForEndOfFrame(); } // Assign in Inspector
        if (this.shapeGenerator != null) shapeFiller = shapeGenerator.GetComponent<ShapeFiller>();

        // Ensure ShapeClickManager is ready (Assign in Inspector or FindObjectOfType)
        if (this.shapeClickManager == null) this.shapeClickManager = FindObjectOfType<ShapeClickManager>();
        while (this.shapeClickManager == null) { Debug.Log("MGB: Waiting for ShapeClickManager..."); yield return new WaitForEndOfFrame(); }

        ShapeClickManager.OnShapeClicked -= OnHOShapeClicked; // Defensive unsubscribe
        ShapeClickManager.OnShapeClicked += OnHOShapeClicked;

        DefineHOProblemConfiguration();
        InitializeHOProblemMasterList(); // Generates GameObjects for HO problem

        if (this.shapeClickManager != null && activeHOShapes != null)
        {
            yield return new WaitForSeconds(0.2f); // Ensure GameObjects are fully initialized
            //this.shapeClickManager.SetTargetHOShapeList(activeHOShapes);
            //this.shapeClickManager.MakeHOShapesClickable();
        }

        isStartupSequence = false;
        UpdateHOGameState(HOGameState.SelectingHOShape);
    }

    void DefineHOProblemConfiguration()
    {
        hoProblemShapesConfig = new List<ShapeObject>();
        // Example: A "house"
        hoProblemShapesConfig.Add(new ShapeObject(4, UNUSED, SHAPES.SQUARE).setIsToBeFilled().withOffset(new Vector3(0, -1, 0)));
        hoProblemShapesConfig.Add(new ShapeObject(4, 3, SHAPES.TRIANGLE).setIsToBeFilled().withOffset(new Vector3(0, 1.5f, 0)));
        hoProblemShapesConfig.Add(new ShapeObject(1, UNUSED, SHAPES.CIRCLE).setIsToBeFilled().withOffset(new Vector3(-1.5f, -0.5f, 0)));
    }

    void InitializeHOProblemMasterList()
    {
        activeHOShapes = new List<ShapeObject>();
        hoShapeSolvedStatus = new Dictionary<ShapeObject, bool>();
        hoSolvedShapeAreas = new Dictionary<ShapeObject, float>();

        if (hoProblemShapesConfig == null || shapeGenerator == null) { Debug.LogError("MGB: HO Config or ShapeGenerator is null."); return; }

        foreach (ShapeObject config in hoProblemShapesConfig)
        {
            GameObject go = GenerateHOShapeObjectVisual(config);
            if (go != null)
            {
                config.actualShapeObj = go; // Link GameObject to the ShapeObject
                activeHOShapes.Add(config);
                hoShapeSolvedStatus[config] = false; // Use the ShapeObject instance itself as the key
            }
        }
    }

    GameObject GenerateHOShapeObjectVisual(ShapeObject hoConfig) // Renamed for clarity
    {
        if (hoConfig == null || this.shapeGenerator == null) return null;
        GameObject generatedObj = null;
        switch (hoConfig.shape)
        {
            case SHAPES.SQUARE: generatedObj = this.shapeGenerator.CreateSquare(hoConfig.offset, hoConfig.x); break;
            case SHAPES.TRIANGLE: generatedObj = this.shapeGenerator.CreateTriangle(hoConfig.offset, hoConfig.x, hoConfig.y); break;
            // ... other shapes ...
            case SHAPES.CIRCLE: generatedObj = this.shapeGenerator.CreateCircle(hoConfig.offset, hoConfig.x, false); break;
            case SHAPES.RECTANGLE: generatedObj = this.shapeGenerator.CreateRectangle(hoConfig.offset, hoConfig.x, hoConfig.y); break;
            case SHAPES.SEMI_CIRCLE: generatedObj = this.shapeGenerator.CreateCircle(hoConfig.offset, hoConfig.x, true); break;

        }
        if (generatedObj != null && hoConfig.angle != 0) { generatedObj.transform.Rotate(0, 0, -hoConfig.angle); }
        return generatedObj;
    }

    void UpdateHOGameState(HOGameState newState)
    {
        currentHOGameState = newState;
        Debug.Log($"MGB: HO Game State changed to: {currentHOGameState}");
        string hudMsg = "";
        string charMsg = "";

        switch (currentHOGameState)
        {
            case HOGameState.Initializing:
                HideLOInterface(); hudMsg = "Loading Problem...";
                break;
            case HOGameState.SelectingHOShape:
                HideLOInterface(); SetCameraView(false); // false for classroom/overview
                hudMsg = "Select a Shape to Solve"; charMsg = "Choose a part of the puzzle to work on.";
                if (activeHOShapes != null)
                { // Ensure HO shapes are visible
                    foreach (var shapeObj in activeHOShapes) { if (shapeObj.actualShapeObj != null) shapeObj.actualShapeObj.SetActive(true); }
                }
                break;
            case HOGameState.SolvingLOSubProblem:
                // LO Interface will be shown by StartSolvingLOSubProblem()
                hudMsg = currentShapeBeingSolvedLO != null ? $"Solving: {currentShapeBeingSolvedLO.shape}" : "Solving...";
                break;
            case HOGameState.CalculatingFinalSum:
                HideLOInterface(); StartFinalSumCalculationUI(); // Sets up UI for final sum
                hudMsg = "Calculate Total Area";
                break;
            case HOGameState.HOLevelComplete:
                HideLOInterface(); hudMsg = "Problem Solved!"; charMsg = "Excellent! The entire puzzle is complete!";
                // Play victory animations, show score, transition to next level/menu
                Invoke(nameof(TriggerEndOfGame), 3.0f);
                break;
        }
        if (textHUDDisplay != null && !string.IsNullOrEmpty(hudMsg)) textHUDDisplay.text = hudMsg;
        if (textCharacterSays != null && !string.IsNullOrEmpty(charMsg)) textCharacterSays.text = charMsg;
    }

    private void OnHOShapeClicked(ShapeClickManager.ShapeClickData clickData)
    {
        if (currentHOGameState != HOGameState.SelectingHOShape) return;
        if (clickData.originalShapeObject == null) { Debug.LogError("MGB: Click data has null originalShapeObject."); return; }

        ShapeObject clickedHOShape = null;//clickData.originalShapeObject; // This IS the instance from activeHOShapes

        if (hoShapeSolvedStatus.TryGetValue(clickedHOShape, out bool isSolved) && isSolved)
        {
            if (notificationPopup != null && textNotificationPopup != null)
            {
                textNotificationPopup.text = $"{clickedHOShape.shape} is already complete!";
                notificationPopup.SetActive(true);
            }
            return;
        }
        currentShapeBeingSolvedLO = clickedHOShape;
        StartSolvingLOSubProblem(clickedHOShape);
    }

    void StartSolvingLOSubProblem(ShapeObject hoShapeToSolve)
    {
        UpdateHOGameState(HOGameState.SolvingLOSubProblem);
        ResetUIForNewLOSubProblem();
        // Create a new temporary visual for the LO measurement phase
        currentLOProblemInstance = new Problem(hoShapeToSolve.shape, this, hoShapeToSolve.x, hoShapeToSolve.y);

        if (currentLOProblemInstance.problemObjectShape != null && shapeFiller != null)
        {
            shapeFiller.InitializeFill(currentLOProblemInstance.problemObjectShape, Color.cyan, 0.5f, 0f);
        }
        else { Debug.LogError("MGB: LO problem visual for fill is null or ShapeFiller missing."); }

        SetupAndShowLOInterfaceForShape(hoShapeToSolve.shape);
        SetCameraView(true); // true for UI/gameplay focused view
    }

    void ResetUIForNewLOSubProblem()
    { // Renamed from ResetLOProblemAndUI for clarity
        if (currentLOProblemInstance?.problemObjectShape != null) { Destroy(currentLOProblemInstance.problemObjectShape); currentLOProblemInstance = null; }
        // Clean up any temporary spell effects or visuals from previous LO attempt
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Temporary")) { Destroy(go); }
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Spell")) { Destroy(go); }

        if (textDialoguePrompt != null) textDialoguePrompt.text = "";
        if (textCorrectionPercentage != null) textCorrectionPercentage.gameObject.SetActive(false);
        HideAllEquationPanels();

        if (lineSnapper != null)
        {
            lineSnapper.gameObject.SetActive(false); // Hide first
            // Repeatedly call OnUndoPressed on LineSnapper to clear its lines if ResetLines doesn't exist
            // This assumes OnUndoPressed correctly handles removing one line at a time and resetting its state.
            // A direct ResetLines() method in LineSnapper would be cleaner.
            int maxPossibleLines = 2; // Max lines any shape might have
            for (int i = 0; i < maxPossibleLines + 1; ++i) { lineSnapper.OnUndoPressed(); }
            lineSnapper.lineCount = 0; // Explicitly reset lineCount
            lineSnapper.value1 = "???"; lineSnapper.value2 = "???";
        }
        if (buttonUndo != null) buttonUndo.gameObject.SetActive(false);

        isMeasurementDoneLO = false; currentPlayerInputAnswer = 0f;
        if (textSubmitButton != null) textSubmitButton.text = castBtnText1_Measure;
        if (calculatorButtonObject != null) calculatorButtonObject.SetActive(false); // Hide calc button

        if (ocrInputElement != null) ocrInputElement.SetActive(false);
        if (formulaDisplayElement != null) formulaDisplayElement.SetActive(false);
        if (fa != null) { fa.ResetCalcDisp(); fa.ResetAnalyzer(); }
        if (ocrScript != null) { ocrScript.ResetColor(); ocrScript.ResetVFX(); ocrScript.processing = true; } // processing = true to stop input

        HideLOInterface(); // Start with LO panel hidden, SetupAndShowLOInterfaceForShape will show it
    }

    void SetupAndShowLOInterfaceForShape(SHAPES shapeType)
    { // Renamed
        if (loSolverPanel != null) loSolverPanel.SetActive(true);
        if (loActionButtonsPanel != null) loActionButtonsPanel.SetActive(true);
        if (textCharacterSays != null) textCharacterSays.text = $"Measure the {shapeType}.";
        if (textManaRequirement != null) textManaRequirement.text = "Expected Area";
        if (textSubmitButton != null) textSubmitButton.text = castBtnText1_Measure;

        ShowEquationPanelForShape(shapeType);
        ConfigureLineLengthDisplaysForShape(shapeType);

        if (lineSnapper != null) lineSnapper.gameObject.SetActive(true);
        if (buttonUndo != null) buttonUndo.gameObject.SetActive(true);
    }

    private void OnUndoClicked_LO()
    {
        if (currentHOGameState != HOGameState.SolvingLOSubProblem && currentHOGameState != HOGameState.CalculatingFinalSum) return;

        if (currentHOGameState == HOGameState.SolvingLOSubProblem && !isMeasurementDoneLO)
        {
            if (lineSnapper != null) lineSnapper.OnUndoPressed(); // LineSnapper handles its own state and text updates
        }
        else
        { // Undoing OCR input (resetting OCR board)
            UndoOCRInput();
        }
    }

    public void ActionButton_Measure_Submit_Redo() // Renamed from OnLOCastOrSubmit
    {
        if (currentHOGameState != HOGameState.SolvingLOSubProblem && currentHOGameState != HOGameState.CalculatingFinalSum) return;

        if (currentHOGameState == HOGameState.SolvingLOSubProblem)
        {
            if (!isMeasurementDoneLO)
            { // "Measure" phase, button press means "Done Measuring"
                if (lineSnapper == null || currentShapeBeingSolvedLO == null || lineSnapper.lineCount != GetMaxLinesForCurrentLOShape())
                {
                    if (notificationPopup != null && textNotificationPopup != null) { textNotificationPopup.text = "Please measure all required sides first."; notificationPopup.SetActive(true); }
                    return;
                }
                ProceedFromMeasurementToOCR();
            }
            else
            { // isMeasurementDoneLO is true, "Submit Answer" or "Redo Input" phase
              // If text is "Submit Answer", it means FA should have already processed and called back.
              // If text is "Redo Input", then this button press resets OCR.
                if (textSubmitButton != null && textSubmitButton.text == castBtnText3_RedoOCR)
                {
                    UndoOCRInput(); // Reset OCR board
                }
                else
                {
                    Debug.Log("MGB: Submit Answer pressed. Waiting for FormulaAnalyzer callback if input was made.");
                    // FA should call SubmitLOSubProblemAnswer or SubmitFinalSumAnswer
                    // No direct action here other than possibly prompting FA if it needs manual trigger
                }
            }
        }
        else if (currentHOGameState == HOGameState.CalculatingFinalSum)
        {
            // Similar to above, FA should call back. Button might be "Redo Input".
            if (textSubmitButton != null && textSubmitButton.text == castBtnText3_RedoOCR)
            {
                UndoOCRInput();
            }
            else
            {
                Debug.Log("MGB: Submit Total pressed. Waiting for FormulaAnalyzer callback.");
            }
        }
    }

    int GetMaxLinesForCurrentLOShape()
    {
        if (currentShapeBeingSolvedLO == null) return 0;
        switch (currentShapeBeingSolvedLO.shape)
        {
            case SHAPES.TRIANGLE: case SHAPES.RECTANGLE: return 2;
            case SHAPES.SQUARE: case SHAPES.CIRCLE: case SHAPES.SEMI_CIRCLE: return 1;
            default: return 0;
        }
    }

    void ProceedFromMeasurementToOCR()
    { // Renamed from DoneLOMeasure
        isMeasurementDoneLO = true;
        if (textSubmitButton != null) textSubmitButton.text = castBtnText2_Submit;

        // Update displayed line lengths from LineSnapper's public string properties
        if (currentLineDisplay1?.GetComponent<Text>() != null && lineSnapper != null) currentLineDisplay1.GetComponent<Text>().text = lineSnapper.value1;
        if (currentLineDisplay2?.GetComponent<Text>() != null && lineSnapper != null) currentLineDisplay2.GetComponent<Text>().text = lineSnapper.value2;

        StartCoroutine(SlideOCRBoardIntoView(true)); // Renamed
        if (lineSnapper != null) lineSnapper.ToggleLineText(); // This method exists in LineSnapper
    }

    void UndoOCRInput()
    { // Renamed from UndoLOMeasureOrOCR and simplified
        Debug.Log("MGB: Undoing/Resetting OCR input.");
        StartCoroutine(SlideOCRBoardIntoView(false)); // Hide OCR board first
        if (fa != null) { fa.ResetCalcDisp(); fa.ResetAnalyzer(); } // These methods exist in FormulaAnalyzer
        if (ocrScript != null) { ocrScript.ResetColor(); ocrScript.ResetVFX(); /* ocrScript.processing will be handled by SlideOCR */ }

        // After hiding, re-show the OCR board clean
        Invoke(nameof(ReShowCleanOCRBoardAfterUndo), OCRSLIDETIME + 0.1f); // Renamed
        if (textSubmitButton != null) textSubmitButton.text = castBtnText2_Submit; // Back to "Submit Answer"
    }
    private void ReShowCleanOCRBoardAfterUndo() { StartCoroutine(SlideOCRBoardIntoView(true)); }


    // **** Methods called by FormulaAnalyzer (FA) ****
    // FA needs to be on the same GameObject as this script, or this script needs to be on "gbHolder"
    // AND FA's "gb" variable needs to be assigned THIS instance of ModifiedGameBehaviour.
    public void InputAnswer(float answerFromFA)
    { // Called by FA when it has an answer
        if (currentHOGameState == HOGameState.SolvingLOSubProblem)
        {
            SubmitLOSubProblemAnswer(answerFromFA);
        }
        else if (currentHOGameState == HOGameState.CalculatingFinalSum)
        {
            SubmitFinalSumAnswer(answerFromFA);
        }
    }
    public void NotifyInvalidFormula()
    { // Called by FA
        if (ocrScript != null) ocrScript.processing = true; // Stop further OCR input
        if (notificationPopup != null && textNotificationPopup != null)
        {
            notificationPopup.SetActive(true);
            textNotificationPopup.text = "Invalid formula. Please try again.";
        }
        if (textSubmitButton != null) textSubmitButton.text = castBtnText3_RedoOCR; // Allow redo
    }
    public void NotifyMismatchedAnswer()
    { // Called by FA
        if (ocrScript != null) ocrScript.processing = true;
        if (notificationPopup != null && textNotificationPopup != null)
        {
            notificationPopup.SetActive(true);
            textNotificationPopup.text = "Answer doesn't match formula. Check your calculation.";
        }
        if (textSubmitButton != null) textSubmitButton.text = castBtnText3_RedoOCR;
    }
    // **** End Methods called by FA ****


    void SubmitLOSubProblemAnswer(float ansFromOCR)
    {
        if (currentHOGameState != HOGameState.SolvingLOSubProblem) return;
        currentPlayerInputAnswer = ansFromOCR;
        StartCoroutine(SlideOCRBoardIntoView(false)); // Hide OCR
        CalculateErrorForCurrentLOSubProblem(); // Renamed
        if (textCorrectionPercentage != null)
        {
            textCorrectionPercentage.text = "Error: " + Math.Abs(this.currentLOError) + "%";
            textCorrectionPercentage.gameObject.SetActive(true);
        }
        // Use SPELLDELAY as a generic animation time before processing outcome
        Invoke(nameof(ProcessLOSubProblemOutcome), FILLTIMEAPROX + OCRSLIDETIME + SPELLDELAY);
    }

    void CalculateErrorForCurrentLOSubProblem()
    {
        if (currentLOProblemInstance == null) { this.currentLOError = 100f; return; }
        float targetArea = CalculateAreaForLOProblemVisual(currentLOProblemInstance); // Renamed
        if (targetArea == 0) this.currentLOError = (currentPlayerInputAnswer == 0) ? 0f : 100f;
        else this.currentLOError = (1 - (currentPlayerInputAnswer / targetArea)) * 100f;
        this.currentLOError = (float)Math.Round(this.currentLOError, 2);

        if (shapeFiller != null && currentLOProblemInstance.problemObjectShape != null)
        {
            float fillRatio = (targetArea == 0) ? (currentPlayerInputAnswer == 0 ? 1f : 2f) : currentPlayerInputAnswer / targetArea;
            if (fillRatio > 2.0f) fillRatio = 2.0f; if (fillRatio < 0f) fillRatio = 0f;
            shapeFiller.fillMaxValue = fillRatio;
            shapeFiller.isPerfectMatch = (this.currentLOError == 0f);
            shapeFiller.isFillingActive = true;
        }
    }

    float CalculateAreaForLOProblemVisual(Problem loProblem)
    { // Renamed
        if (loProblem == null) return 0f;
        // IMPORTANT: Use the dimensions of the *original HO shape piece* (currentShapeBeingSolvedLO)
        // NOT the potentially scaled dimensions of the temporary LO visual (loProblem.p_measure).
        if (currentShapeBeingSolvedLO == null) return 0f;

        double p = currentShapeBeingSolvedLO.x; // Use original dimensions
        double s = currentShapeBeingSolvedLO.y == UNUSED ? p : currentShapeBeingSolvedLO.y;
        double result = 0;
        switch (currentShapeBeingSolvedLO.shape)
        { // Use original shape type
            case SHAPES.TRIANGLE: result = (0.5 * p * s); break;
            case SHAPES.CIRCLE: result = (Math.PI * Math.Pow(p / 2, 2)); break;
            // ... other shapes ...
            case SHAPES.RECTANGLE: result = (p * s); break;
            case SHAPES.SQUARE: result = Math.Pow(p, 2); break;
            case SHAPES.SEMI_CIRCLE: result = (0.5 * Math.PI * Math.Pow(p / 2, 2)); break;
            default: return 0f;
        }
        return (float)Math.Round(result, 2);
    }

    void ProcessLOSubProblemOutcome()
    {
        if (this.currentLOError == 0f)
        {
            TriggerSuccessAnimsAndSound(currentShapeBeingSolvedLO.shape); // Renamed
            float actualArea = CalculateAreaForLOProblemVisual(currentLOProblemInstance); // Recalculate with original dims for storage
            hoShapeSolvedStatus[currentShapeBeingSolvedLO] = true;
            hoSolvedShapeAreas[currentShapeBeingSolvedLO] = actualArea;
            if (currentShapeBeingSolvedLO.actualShapeObj?.GetComponent<MeshRenderer>() != null)
                currentShapeBeingSolvedLO.actualShapeObj.GetComponent<MeshRenderer>().material.color = Color.green;
            allIndividualShapesSolved = hoShapeSolvedStatus.Values.All(status => status);

            if (currentLOProblemInstance?.problemObjectShape != null) Destroy(currentLOProblemInstance.problemObjectShape);
            currentLOProblemInstance = null;

            float animDuration = GetCurrentAnimationDurationSafe();
            if (allIndividualShapesSolved) Invoke(nameof(DelayedTransitionToFinalSumState), animDuration + 0.5f); //Renamed
            else Invoke(nameof(DelayedReturnToHOSelectionState), animDuration + 0.5f); //Renamed
        }
        else
        {
            TriggerFailureAnimsAndSound(); // Renamed
            Invoke(nameof(SetupUIForLORetry), SPELLDELAY); // Renamed
        }
    }

    float GetCurrentAnimationDurationSafe()
    { // Renamed
        return SPELLDELAY; // Fallback
    }

    void DelayedReturnToHOSelectionState() { UpdateHOGameState(HOGameState.SelectingHOShape); }
    void DelayedTransitionToFinalSumState() { UpdateHOGameState(HOGameState.CalculatingFinalSum); }

    void SetupUIForLORetry()
    { // Renamed from ReShowLOUIForRetry
        Debug.Log("MGB: Setting up UI for LO sub-problem retry.");
        if (isMeasurementDoneLO)
        { // If error was in OCR phase
            StartCoroutine(SlideOCRBoardIntoView(true));
            if (fa != null) { fa.ResetCalcDisp(); fa.ResetAnalyzer(); }
            if (ocrScript != null) { ocrScript.ResetColor(); ocrScript.ResetVFX(); /* ocrScript.processing handled by SlideOCR */ }
            if (textSubmitButton != null) textSubmitButton.text = castBtnText3_RedoOCR;
        }
        else
        { // If error was somehow during measurement (unlikely to reach here with current flow)
            if (lineSnapper != null) lineSnapper.gameObject.SetActive(true);
            if (textSubmitButton != null) textSubmitButton.text = castBtnText1_Measure;
        }
        if (loSolverPanel != null) loSolverPanel.SetActive(true);
        if (loActionButtonsPanel != null) loActionButtonsPanel.SetActive(true);
        if (textCorrectionPercentage != null) textCorrectionPercentage.gameObject.SetActive(true); // Keep error shown
    }

    void StartFinalSumCalculationUI()
    {
        ResetUIForNewLOSubProblem(); // Clears general LO stuff
        if (loSolverPanel != null) loSolverPanel.SetActive(true);
        if (loActionButtonsPanel != null) loActionButtonsPanel.SetActive(true); // For Submit/Redo OCR
        if (textCharacterSays != null) textCharacterSays.text = "All parts are complete! Now, what is the TOTAL area?";
        if (textManaRequirement != null) textManaRequirement.text = "Enter Total Area";
        if (textSubmitButton != null) textSubmitButton.text = castBtnText2_Submit;

        HideAllEquationPanels();
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(false); // No line drawing for sum
        if (buttonUndo != null) buttonUndo.gameObject.SetActive(true); // Allow OCR redo for sum

        StartCoroutine(SlideOCRBoardIntoView(true)); // Show OCR for number input
        SetCameraView(true); // UI focus
    }

    void SubmitFinalSumAnswer(float sumAnswerFromOCR)
    {
        if (currentHOGameState != HOGameState.CalculatingFinalSum) return;
        currentPlayerInputAnswer = sumAnswerFromOCR;
        StartCoroutine(SlideOCRBoardIntoView(false)); // Hide OCR
        CalculateErrorForFinalSum(); // Renamed
        if (textCorrectionPercentage != null)
        {
            textCorrectionPercentage.text = "Error: " + Math.Abs(this.currentLOError) + "%";
            textCorrectionPercentage.gameObject.SetActive(true);
        }
        Invoke(nameof(ProcessFinalSumOutcome), FILLTIMEAPROX + OCRSLIDETIME + SPELLDELAY);
    }

    void CalculateErrorForFinalSum()
    {
        float targetTotalArea = 0;
        if (hoSolvedShapeAreas != null && hoSolvedShapeAreas.Count > 0) targetTotalArea = hoSolvedShapeAreas.Values.Sum();
        targetTotalArea = (float)Math.Round(targetTotalArea, 2);

        if (targetTotalArea == 0) this.currentLOError = (currentPlayerInputAnswer == 0) ? 0f : 100f;
        else this.currentLOError = (1 - (currentPlayerInputAnswer / targetTotalArea)) * 100f;
        this.currentLOError = (float)Math.Round(this.currentLOError, 2);
        if (shapeFiller != null) shapeFiller.isFillingActive = false; // No visual fill for sum
    }

    void ProcessFinalSumOutcome()
    {
        if (this.currentLOError == 0f)
        {
            TriggerSuccessAnimsAndSound(SHAPES.NONE); // Generic success
            UpdateHOGameState(HOGameState.HOLevelComplete);
        }
        else
        {
            TriggerFailureAnimsAndSound();
            Invoke(nameof(SetupUIForFinalSumRetry), SPELLDELAY); // Renamed
        }
    }
    void SetupUIForFinalSumRetry()
    { // Renamed
        Debug.Log("MGB: Setting up UI for retry of final sum.");
        StartCoroutine(SlideOCRBoardIntoView(true)); // Show OCR board again
        if (fa != null) { fa.ResetCalcDisp(); fa.ResetAnalyzer(); }
        if (ocrScript != null) { ocrScript.ResetColor(); ocrScript.ResetVFX(); /* processing by SlideOCR */ }
        if (textSubmitButton != null) textSubmitButton.text = castBtnText3_RedoOCR;
        if (loSolverPanel != null) loSolverPanel.SetActive(true);
        if (loActionButtonsPanel != null) loActionButtonsPanel.SetActive(true);
        if (textCorrectionPercentage != null) textCorrectionPercentage.gameObject.SetActive(true);
    }

    // --- Animation and Sound Triggers (Placeholders) ---
    void TriggerSuccessAnimsAndSound(SHAPES shapeType)
    {
        Debug.Log($"Success anim/sound for {shapeType}");
        // Add sound play calls here
        InvokeDelayedSpellVisual();
    }
    void TriggerFailureAnimsAndSound()
    {
        Debug.Log("Failure anim/sound");
        // Add sound play calls here
    }
    void InvokeDelayedSpellVisual() { Invoke(nameof(TriggerSpellVisualEffect), SPELLDELAY); } // Renamed
    void TriggerSpellVisualEffect() { if (gameAnimScript != null) gameAnimScript.CastSpell(); } // Using existing call
    int SendShapeToPlayerAnim(SHAPES s) { return (s == SHAPES.NONE) ? -1 : (int)s - 1; } // Maps enum to int for anims

    // --- UI Panel Management ---
    private void HideLOInterface()
    {
        if (loSolverPanel != null) loSolverPanel.SetActive(false);
        if (ocrInputElement != null) ocrInputElement.SetActive(false);
        if (formulaDisplayElement != null) formulaDisplayElement.SetActive(false);
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(false);
        if (buttonUndo != null) buttonUndo.gameObject.SetActive(false);
        if (textCorrectionPercentage != null) textCorrectionPercentage.gameObject.SetActive(false);
        HideAllEquationPanels();
    }
    private void HideAllEquationPanels()
    {
        if (equationPanel_Triangle != null) equationPanel_Triangle.SetActive(false); if (equationPanel_Square != null) equationPanel_Square.SetActive(false); if (equationPanel_Rectangle != null) equationPanel_Rectangle.SetActive(false); if (equationPanel_SemiCircle != null) equationPanel_SemiCircle.SetActive(false); if (equationPanel_Circle != null) equationPanel_Circle.SetActive(false);
    }
    private void ShowEquationPanelForShape(SHAPES shape)
    {
        HideAllEquationPanels();
        switch (shape)
        {
            case SHAPES.TRIANGLE: if (equationPanel_Triangle != null) equationPanel_Triangle.SetActive(true); break;
            case SHAPES.SQUARE: if (equationPanel_Square != null) equationPanel_Square.SetActive(true); break;
            // ... other cases ...
            case SHAPES.RECTANGLE: if (equationPanel_Rectangle != null) equationPanel_Rectangle.SetActive(true); break;
            case SHAPES.CIRCLE: if (equationPanel_Circle != null) equationPanel_Circle.SetActive(true); break;
            case SHAPES.SEMI_CIRCLE: if (equationPanel_SemiCircle != null) equationPanel_SemiCircle.SetActive(true); break;
        }
    }
    private void ConfigureLineLengthDisplaysForShape(SHAPES shapeType)
    { // Renamed
        if (lineLengthDisplay_Sq1 != null) lineLengthDisplay_Sq1.SetActive(false); if (lineLengthDisplay_Rect1 != null) lineLengthDisplay_Rect1.SetActive(false); if (lineLengthDisplay_Rect2 != null) lineLengthDisplay_Rect2.SetActive(false); if (lineLengthDisplay_Tri1 != null) lineLengthDisplay_Tri1.SetActive(false); if (lineLengthDisplay_Tri2 != null) lineLengthDisplay_Tri2.SetActive(false); if (lineLengthDisplay_Cir1 != null) lineLengthDisplay_Cir1.SetActive(false); if (lineLengthDisplay_Semi1 != null) lineLengthDisplay_Semi1.SetActive(false);
        currentLineDisplay1 = null; currentLineDisplay2 = null;
        switch (shapeType)
        {
            case SHAPES.SQUARE: currentLineDisplay1 = lineLengthDisplay_Sq1; break;
            case SHAPES.RECTANGLE: currentLineDisplay1 = lineLengthDisplay_Rect1; currentLineDisplay2 = lineLengthDisplay_Rect2; break;
            // ... other cases ...
            case SHAPES.TRIANGLE: currentLineDisplay1 = lineLengthDisplay_Tri1; currentLineDisplay2 = lineLengthDisplay_Tri2; break;
            case SHAPES.CIRCLE: currentLineDisplay1 = lineLengthDisplay_Cir1; break;
            case SHAPES.SEMI_CIRCLE: currentLineDisplay1 = lineLengthDisplay_Semi1; break;
        }
        if (currentLineDisplay1 != null) currentLineDisplay1.SetActive(true);
        if (currentLineDisplay2 != null) currentLineDisplay2.SetActive(true);
        // Clear text initially
        if (currentLineDisplay1?.GetComponent<Text>() != null) currentLineDisplay1.GetComponent<Text>().text = "?.??";
        if (currentLineDisplay2?.GetComponent<Text>() != null) currentLineDisplay2.GetComponent<Text>().text = "?.??";
    }

    // --- Camera and General UI ---
    private void OnChangeCameraClicked()
    {
        if (screenFadeAnimator == null) return;
        screenFadeAnimator.SetTrigger("fade"); // Generic fade trigger
        SetCameraView(!isUICamera); // Toggle
    }
    private void SetCameraView(bool showUIFocusView)
    { // Renamed from ToUI/ToClass
        isUICamera = showUIFocusView;
        if (mainSceneCamera != null) mainSceneCamera.SetActive(isUICamera);
        if (classroomViewCamera != null) classroomViewCamera.SetActive(!isUICamera);
        if (buttonChangeCamera?.GetComponent<Image>() != null)
        {
            buttonChangeCamera.GetComponent<Image>().material = isUICamera ? classroomModeMaterial : uiModeMaterial;
        }
    }

    public void CloseNotificationPopup()
    { // Renamed
        if (notificationPopup != null) notificationPopup.SetActive(false);
        // If OCR was paused by notification, resume it
        if (ocrScript != null && (currentHOGameState == HOGameState.SolvingLOSubProblem && isMeasurementDoneLO || currentHOGameState == HOGameState.CalculatingFinalSum))
        {
            ocrScript.processing = false;
        }
    }

    // --- OCR Board Sliding Animation ---
    private IEnumerator SlideOCRBoardIntoView(bool show)
    { // Renamed
        if (show)
        {
            // Optional: Adjust dialogue box position if it overlaps with OCR board
            // if(rtLOSolverPanel != null && loSolverPanel != null && loSolverPanel.activeSelf) { /* Toggle or slide it */ }
            // yield return new WaitForSeconds(DIALOGUESLIDETIME * 0.5f);

            if (ocrInputElement != null)
            {
                ocrInputElement.SetActive(true);
                if (ocrBoardSlideEndTarget != null) StartCoroutine(MoveObjectOverTime(ocrInputElement, OCRSLIDETIME, ocrBoardSlideEndTarget.transform.position)); //Renamed
                else if (ocrInputElement.transform.parent != null) StartCoroutine(MoveObjectOverTime(ocrInputElement, OCRSLIDETIME, ocrInputElement.transform.parent.TransformPoint(Vector3.zero))); // Fallback to center of parent
            }
            // Optional: Scale dialogue box if it's part of the OCR view
            // if(rtLOSolverPanel != null && loSolverPanel != null) { /* ... */ }
        }
        else
        { // Hide
            if (ocrScript != null) ocrScript.processing = true; // Stop input before sliding away
            if (formulaDisplayElement != null) formulaDisplayElement.SetActive(false);
            if (ocrInputElement != null && ocrBoardSlideStartTarget != null) StartCoroutine(MoveObjectOverTime(ocrInputElement, OCRSLIDETIME, ocrBoardSlideStartTarget.transform.position));
            // Optional: Restore dialogue box scale/position
            // if(rtLOSolverPanel != null && loSolverPanel != null) { /* ... */ }
        }
        yield return new WaitForSeconds(OCRSLIDETIME); // Wait for slide animation
        if (show)
        {
            if (ocrScript != null) { ocrScript.ResetColor(); ocrScript.ResetVFX(); ocrScript.processing = false; } // Ready for input
            if (formulaDisplayElement != null) formulaDisplayElement.SetActive(true);
        }
        else
        {
            if (ocrInputElement != null) ocrInputElement.SetActive(false);
        }
    }
    private IEnumerator MoveObjectOverTime(GameObject obj, float duration, Vector3 endPosition)
    { /* Generic move coroutine */
        if (obj == null) yield break;
        Vector3 startPosition = obj.transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = endPosition;
    }
    private IEnumerator ScaleObjectOverTime(GameObject obj, float duration, Vector3 endScale)
    { /* Generic scale coroutine */
        if (obj == null) yield break;
        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        obj.transform.localScale = endScale;
    }


    void OnDestroy() { if (true && shapeClickManager != null) ShapeClickManager.OnShapeClicked -= OnHOShapeClicked; }
    private void TriggerEndOfGame() { SceneManager.LoadScene("LevelSelect"); } // Renamed

    // --- Nested Problem class for LO sub-problem visuals ---
    public class Problem
    {
        public SHAPES problemShape;
        public float p_measure = UNUSED, s_measure = UNUSED; // These are the *scaled* dimensions for the LO visual
        public ModifiedGameBehaviour main;
        public GameObject problemObjectShape; // The temporary visual for LO measurement

        public Problem(SHAPES shapeTypeForVisual, ModifiedGameBehaviour mainController, float originalDimX, float originalDimY_or_unused)
        {
            this.main = mainController;
            this.problemShape = shapeTypeForVisual;

            // Scale original dimensions for the temporary LO visual if needed
            float displayScaleFactor = 1.5f; // Make LO problem visual larger for clarity
            this.p_measure = originalDimX * displayScaleFactor;
            float temp_s_measure = (originalDimY_or_unused == UNUSED &&
                                   (shapeTypeForVisual == SHAPES.SQUARE || shapeTypeForVisual == SHAPES.CIRCLE || shapeTypeForVisual == SHAPES.SEMI_CIRCLE))
                                   ? originalDimX : originalDimY_or_unused;
            this.s_measure = (temp_s_measure == UNUSED) ? UNUSED : temp_s_measure * displayScaleFactor;


            if (main.shapeGenerator == null) { Debug.LogError("MGB.Problem: ShapeGenerator is null!"); return; }
            Vector2 centerOffset = Vector2.zero; // LO visual is always centered

            switch (this.problemShape)
            { // Use scaled p_measure and s_measure for visual creation
                case SHAPES.SQUARE: problemObjectShape = main.shapeGenerator.CreateSquare(centerOffset, this.p_measure); break;
                case SHAPES.TRIANGLE: problemObjectShape = main.shapeGenerator.CreateTriangle(centerOffset, this.p_measure, this.s_measure); break;
                case SHAPES.CIRCLE: problemObjectShape = main.shapeGenerator.CreateCircle(centerOffset, this.p_measure, false); break; // p_measure is diameter
                case SHAPES.RECTANGLE: problemObjectShape = main.shapeGenerator.CreateRectangle(centerOffset, this.p_measure, this.s_measure); break;
                case SHAPES.SEMI_CIRCLE: problemObjectShape = main.shapeGenerator.CreateCircle(centerOffset, this.p_measure, true); break; // p_measure is diameter
            }
            if (problemObjectShape != null) problemObjectShape.name = "LO_SubProblem_Visual";
        }
    }
}