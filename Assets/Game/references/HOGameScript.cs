using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main game controller for the Hidden Object geometry puzzle game.
/// Handles UI interactions, shape selection, measurement, and spell casting mechanics.
/// </summary>
public class HOGameScript : MonoBehaviour
{
    #region Constants
    private const int UNUSED = -1;
    private const int INITIAL_POOL_SIZE = 10;
    private const float TRANSITIONTIME = 0.4f;
    private const float FILLTIMEAPROX = 1.5f;
    private const float STARTDELAY = 3.4f;
    private const float TRANSITIONDELAY = 0.5f;
    private const float DIALOGUESLIDETIME = 0.25f;
    private const float OCRSLIDETIME = 0.35f;

    // UI Animation constants
    private const string castBtnText1 = "Done";
    private const string castBtnText2 = "Erase";
    private const string undoBtnText1 = "Undo";
    private const string undoBtnText2 = "Cast";

    // Dialogue constants - cached to avoid GC
    public const string charDialogue1 = "Kailangan ko pumili ng hugis na aking sasagutin";
    public const string charDialogue2 = "Sagutin natin ang Area gamit ng mga sukat na nakuha natin!";
    public const string charDialogue3 = "Kailangan ko piliin ang tugmang formula para sa hugis.";
    private const string areaDisplayText1 = "Area ng mga hugis:\n";
    #endregion

    #region Cached Static Data
    // Cached shape strings to avoid repeated ToString() calls
    private static readonly Dictionary<GameBehaviour.SHAPES, string> shapeStrings = new Dictionary<GameBehaviour.SHAPES, string>
    {
        { GameBehaviour.SHAPES.SQUARE, "SQUARE" },
        { GameBehaviour.SHAPES.RECTANGLE, "RECTANGLE" },
        { GameBehaviour.SHAPES.TRIANGLE, "TRIANGLE" },
        { GameBehaviour.SHAPES.CIRCLE, "CIRCLE" },
        { GameBehaviour.SHAPES.SEMI_CIRCLE, "SEMI_CIRCLE" },
        { GameBehaviour.SHAPES.NONE, "NONE" }
    };

    // Cached wait objects and common allocations
    private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private static WaitForSeconds[] cachedWaitForSeconds = new WaitForSeconds[10];

    // Cached Vector2 allocations for UI animations
    private static readonly Vector2 hideDiaPos = new Vector2(600f, -150f);
    private static readonly Vector2 showDiaPos = new Vector2(225f, 130f);
    private static readonly Vector2 dialogueUpPos = new Vector2(600f, 130f);
    private static readonly Vector2 dialogueDownPos = new Vector2(600f, -151f);
    private static readonly Vector2 buttonsLeftPos = new Vector2(-493f, -167f);
    private static readonly Vector2 buttonsUpPos = new Vector2(-493f, 138f);
    private static readonly Vector2 dialogueOCRPos = new Vector2(308f, 100f);

    // Cached Vector3 allocations
    private static readonly Vector3 dialogueSmallScale = new Vector3(0.9f, 0.9f, 0.9f);
    private static readonly Vector3 dialogueNormalScale = new Vector3(1f, 1f, 1f);
    #endregion

    #region Core Components & References
    [Header("Core Game Components")]
    public ShapeGenerator shapeGenerator;
    public ShapeFiller shapeFiller;
    public LineSnapper lineSnapper;
    public HOGameBeh hoGameBeh;
    public SpellCastEvent spellCastEvent;

    [Header("Audio")]
    public GameObject soundPlayerObj;
    public GameLevelSoundPlayer soundPlayer;

    [Header("Image")]
    public Image undoBtnImg;
    public Image undoBtnLogo;
    public Sprite undoLogoDefault;
    public Sprite undoLogoCast;

    // Cached component references for performance
    private Transform myTransform;
    private AnimScript animScript;
    private DrawingAndOCRManagerScript ocrScript;
    private FormulaAnalyzer fa;
    private Animator screenFadeAnimator;
    private TextMeshProUGUI areaDisplay;
    private TMP_Text correctionPerc;
    #endregion

    #region UI References
    [Header("UI Components - Buttons")]
    public Button btnMeasure;
    public Button btnRestart;
    public Button btnUndo;
    public Button btnQuit;
    public Button bYesHome;
    public Button bYes;
    public Button btnConfirmSpell;

    [Header("UI Components - Panels")]
    public GameObject hud;
    public GameObject quickMenu;
    public GameObject panelMagicScroll;
    public GameObject pConfirm;
    public GameObject pLowerScroll;
    public GameObject pNotify;
    public GameObject areaDisplayObj;

    [Header("UI Components - Equation Panels")]
    public GameObject pEquationTriangle;
    public GameObject pEquationSquare;
    public GameObject pEquationRectangle;
    public GameObject pEquationSCircle;
    public GameObject pEquationCircle;

    [Header("UI Components - Text")]
    public Text pConfirmText;
    public Text characterSay;
    public Text confirmText;
    public Text textHUD;
    public Text textFinish;
    public Text undoText;

    [Header("UI Components - OCR & Formula")]
    public GameObject ocrInput;
    public GameObject formulaDisplay;
    public Transform rightStartTransform, rightEndTransform, leftStartTransform, leftEndTransform;
    private Transform ocrStartTransform, ocrEndTransform;
    public GameObject formulaAnalyzerObj;
    public GameObject calcBtnObj;
    public GameObject backspaceButton;

    [Header("UI Components - Spawning")]
    public GameObject prefabSpawn;
    public Transform pSpawner;
    public GameObject notifyTextObj;

    //Reference to script for lefthand mode
    public LeftHandedMode canvasScript;

    // Cached UI references
    private GameObject pDialogue;
    private GameObject pDiaButtons;
    private RectTransform rtDialogue;
    private RectTransform rtDiaButtons;
    private Text pNotifyText;
    private Text calcBtnText;
    private Text txtFinalCompound;
    private Vector2 origDiaRT;
    #endregion

    #region Variable Display References
    [Header("Variable Display Objects")]
    public GameObject sqVarDisp1;
    public GameObject rectVarDisp1, rectVarDisp2;
    public GameObject triVarDisp1, triVarDisp2;
    public GameObject cirVarDisp1;
    public GameObject semiVarDisp1;

    private GameObject var1Display, var2Display;
    #endregion

    #region Game State Variables
    [Header("Game State")]
    public float error = 100f;
    public bool isFinalAnswer = false;

    private GameBehaviour.SHAPES currentShape;
    private string chosenShape;
    private float inputAnswer = 0f;
    private float ENDDELAY = 4.0f;

    // State flags
    private bool STARTUP = true;
    private bool isQuit = false;
    private bool isDoneMeasuring;
    private bool isAllSolved = false;
    private bool justDoneSolving = false;
    private bool undoPressed = false;

    // Game objects tracking
    private GameObject currentlySolvedShape = null;
    private HOGameBeh.ShapeObject currentShapeObject = null;
    private Dictionary<HOGameBeh.ShapeObject, float> recordedAnswer = new Dictionary<HOGameBeh.ShapeObject, float>();
    #endregion

    #region Camera & Save System
    private GameObject mainCamera, classroomCamera;
    private GameData savedGame;
    private SaveLoadController saverLoader = new SaveLoadController();
    private string savePath;
    #endregion

    #region Object Pooling
    // Object pools for temporary GameObjects to reduce instantiation
    private Queue<GameObject> tempObjectPool = new Queue<GameObject>();
    #endregion

    #region Initialization
    void Awake()
    {
        InitializeCachedReferences();
        InitializeObjectPool();
        InitializeCachedWaitTimes();
        InitializeSaveSystem();
        InitializeComponents();
        SetupInitialState();

        // Activate Left handed mode based on save data
        if (savedGame.isLeftHanded)
        {
            canvasScript.ToggleLeftHandedMode();

            //Set OCR transforms
            ocrStartTransform = leftStartTransform;
            ocrEndTransform = leftEndTransform;

            //Move OCR board to new start pos
            ocrInput.transform.position = ocrStartTransform.position;

            //OFfsets to correct positions
            formulaDisplay.GetComponent<RectTransform>().anchoredPosition = new Vector2(90, 10); //Offset to correct text pos
            ocrEndTransform.position -= new Vector3(3f, 0f, 0f); //Offset to correct board position
        }
        else //Default right positions
        {
            ocrStartTransform = rightStartTransform;
            ocrEndTransform = rightEndTransform;
        }
    }

    private void InitializeCachedReferences()
    {
        myTransform = transform;
    }

    private void InitializeObjectPool()
    {
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            var poolObj = new GameObject("PooledObject");
            poolObj.SetActive(false);
            tempObjectPool.Enqueue(poolObj);
        }
    }

    private void InitializeCachedWaitTimes()
    {
        for (int i = 0; i < cachedWaitForSeconds.Length; i++)
        {
            cachedWaitForSeconds[i] = new WaitForSeconds(i * 0.1f);
        }
    }

    private void InitializeSaveSystem()
    {
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        savedGame = saverLoader.loadGame(savePath);
    }

    private void InitializeComponents()
    {
        currentShape = GameBehaviour.SHAPES.NONE;

        // Cache frequently accessed components with null checks
        screenFadeAnimator = GameObject.Find("ScreenFade")?.GetComponent<Animator>();
        animScript = GameObject.Find("AnimHolder")?.GetComponent<AnimScript>();

        // Initialize OCR script
        var ocrTransform = ocrInput?.transform?.Find("DrawingAndOCRManager");
        if (ocrTransform != null)
            ocrScript = ocrTransform.GetComponent<DrawingAndOCRManagerScript>();

        // Initialize Formula Analyzer
        if (formulaAnalyzerObj != null)
        {
            fa = formulaAnalyzerObj.GetComponent<FormulaAnalyzer>();
            if (fa != null) fa.hgb = this;
        }

        // Cache UI text components
        if (notifyTextObj != null)
            pNotifyText = notifyTextObj.GetComponent<Text>();

        if (calcBtnObj != null)
            calcBtnText = calcBtnObj.transform.Find("textFinish")?.GetComponent<Text>();

        if (areaDisplayObj != null)
            areaDisplay = areaDisplayObj.GetComponent<TextMeshProUGUI>();
    }

    private void SetupInitialState()
    {
        if (calcBtnObj != null) calcBtnObj.SetActive(false);
        if (areaDisplayObj != null) areaDisplayObj.SetActive(false);
    }

    void Start()
    {
        CacheUIComponents();
        InitializeGameState();
        LoadSavedData();
        SetupCameras();
        SetupInitialUI();

        //Former Reset Function
        CleanupTempObjects();
        ResetGameState();
        ResetUI();
        ResetComponents();
        InitiateNewLevel();

        STARTUP = false;
    }
    void Update()
    {
        if (fa.GetIsEquMode() && undoBtnImg.color != Color.cyan) //Change to cyan on final answer
        {
            undoText.text = undoBtnText2;
            undoBtnImg.color = Color.cyan;
            undoBtnLogo.sprite = undoLogoCast;
            if(isAllSolved)
                undoBtnLogo.gameObject.transform.parent.gameObject.SetActive(true);
        }
        else if (!fa.GetIsEquMode() && undoBtnImg.color != Color.red) //Change to Red on not final answer
        {
            undoText.text = undoBtnText1;
            undoBtnImg.color = Color.red;
            undoBtnLogo.sprite = undoLogoDefault;
            if (isAllSolved)
                undoBtnLogo.gameObject.transform.parent.gameObject.SetActive(false);
        }
    }

    private void CacheUIComponents()
    {
        // Cache frequently accessed UI components
        characterSay = GameObject.Find("characterSay")?.GetComponent<Text>();
        txtFinalCompound = GameObject.Find("shapeCompoundFinal")?.GetComponent<Text>();
        correctionPerc = GameObject.Find("ManaFillCorrectPerc")?.GetComponent<TMP_Text>();

        // Cache dialogue panel references
        pDialogue = GameObject.Find("PanelCasting");
        if (pDialogue != null)
        {
            rtDialogue = pDialogue.GetComponent<RectTransform>();
            if (rtDialogue != null)
                origDiaRT = rtDialogue.anchoredPosition;
        }

        pDiaButtons = GameObject.Find("pDiaButtons");
        if (pDiaButtons != null)
            rtDiaButtons = pDiaButtons.GetComponent<RectTransform>();

        if (textFinish != null)
            textFinish.text = castBtnText1;
    }

    private void InitializeGameState()
    {
        isDoneMeasuring = false;
        justDoneSolving = false;
    }

    private void LoadSavedData()
    {
        if (textHUD != null && savedGame != null)
            textHUD.text = savedGame.currRoom + " ROOM";
    }

    private void SetupCameras()
    {
        mainCamera = GameObject.Find("Main Camera");
        classroomCamera = GameObject.Find("ClassroomCamera");

        if (mainCamera != null) mainCamera.SetActive(false);
        if (classroomCamera != null) classroomCamera.SetActive(true);
    }

    private void SetupInitialUI()
    {
        InitializeUI();

        if (STARTUP)
        {
            if (screenFadeAnimator != null)
                screenFadeAnimator.SetTrigger("fadeIn");
            HideNewUI();
            if (soundPlayerObj != null)
                soundPlayer = soundPlayerObj.GetComponent<GameLevelSoundPlayer>();
        }

        if (lineSnapper != null && animScript != null)
            lineSnapper.animScript = animScript;

        SetInitialUIState();
    }

    private void InitializeUI()
    {
        // Batch UI initialization
        if (pDialogue != null) pDialogue.SetActive(true);
        if (pConfirm != null) pConfirm.SetActive(false);
        if (pLowerScroll != null) pLowerScroll.SetActive(true);
        if (pNotify != null) pNotify.SetActive(false);

        // Batch equation panel setup
        if (pEquationTriangle != null) pEquationTriangle.SetActive(false);
        if (pEquationSquare != null) pEquationSquare.SetActive(false);
        if (pEquationRectangle != null) pEquationRectangle.SetActive(false);
        if (pEquationSCircle != null) pEquationSCircle.SetActive(false);
        if (pEquationCircle != null) pEquationCircle.SetActive(false);

        if (pDiaButtons != null) pDiaButtons.SetActive(false);
        if (btnConfirmSpell != null) btnConfirmSpell.gameObject.SetActive(false);

        // Clear text fields
        if (pConfirmText != null) pConfirmText.text = "";

        // Button setup
        if (bYesHome != null) bYesHome.gameObject.SetActive(false);
        if (bYes != null) bYes.gameObject.SetActive(false);
    }

    private void SetInitialUIState()
    {
        if (btnMeasure != null) btnMeasure.gameObject.SetActive(false);
        if (correctionPerc != null) correctionPerc.gameObject.SetActive(false);
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(false);
        if (btnUndo != null) btnUndo.gameObject.SetActive(false);
    }
    #endregion

    #region Object Pool Management
    private GameObject GetPooledObject()
    {
        if (tempObjectPool.Count > 0)
        {
            var obj = tempObjectPool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return new GameObject("TempObject");
    }

    private void ReturnToPool(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);
            obj.transform.SetParent(null);
            tempObjectPool.Enqueue(obj);
        }
    }

    private void CleanupTempObjects()
    {
        var tempObjects = GameObject.FindGameObjectsWithTag("Temporary");
        foreach (var tempObj in tempObjects)
        {
            ReturnToPool(tempObj);
        }
    }

    private void CleanupSpellObjects()
    {
        var spellObjects = GameObject.FindGameObjectsWithTag("Spell");
        foreach (var spellObj in spellObjects)
        {
            if (spellObj != null)
                Destroy(spellObj);
        }
    }
    #endregion

    #region Variable Display Management
    public void GetVarDisp(GameBehaviour.SHAPES shape)
    {
        switch (shape)
        {
            case GameBehaviour.SHAPES.SQUARE:
                var1Display = sqVarDisp1;
                var2Display = null;
                break;
            case GameBehaviour.SHAPES.RECTANGLE:
                var1Display = rectVarDisp1;
                var2Display = rectVarDisp2;
                break;
            case GameBehaviour.SHAPES.TRIANGLE:
                var1Display = triVarDisp1;
                var2Display = triVarDisp2;
                break;
            case GameBehaviour.SHAPES.CIRCLE:
                var1Display = cirVarDisp1;
                var2Display = null;
                break;
            case GameBehaviour.SHAPES.SEMI_CIRCLE:
                var1Display = semiVarDisp1;
                var2Display = null;
                break;
            default:
                var1Display = null;
                var2Display = null;
                break;
        }
    }

    private void ResetVarDisp(GameBehaviour.SHAPES shape)
    {
        Text var1Text = var1Display?.GetComponent<Text>();
        Text var2Text = var2Display?.GetComponent<Text>();

        switch (shape)
        {
            case GameBehaviour.SHAPES.SQUARE:
                if (var1Text) var1Text.text = "S";
                break;
            case GameBehaviour.SHAPES.RECTANGLE:
                if (var1Text) var1Text.text = "L";
                if (var2Text) var2Text.text = "W";
                break;
            case GameBehaviour.SHAPES.TRIANGLE:
                if (var1Text) var1Text.text = "B";
                if (var2Text) var2Text.text = "H";
                break;
            case GameBehaviour.SHAPES.CIRCLE:
                if (var1Text) var1Text.text = "R";
                break;
            case GameBehaviour.SHAPES.SEMI_CIRCLE:
                if (var1Text) var1Text.text = "R";
                break;
        }
    }
    #endregion

    #region Game Reset & Initialization

    private void ResetGameState()
    {
        currentShape = GameBehaviour.SHAPES.NONE;
    }

    private void ResetUI()
    {
        if (btnMeasure != null) btnMeasure.gameObject.SetActive(false);
        if (correctionPerc != null) correctionPerc.gameObject.SetActive(false);
    }

    private void ResetComponents()
    {
        if (spellCastEvent?.problem?.problemObjectShape != null && !STARTUP)
            Destroy(spellCastEvent.problem.problemObjectShape);

        if (lineSnapper != null)
        {
            lineSnapper.gameObject.SetActive(false);
            lineSnapper.OnUndoPressed();
            lineSnapper.OnUndoPressed();
        }

        if (btnUndo != null) btnUndo.gameObject.SetActive(false);
    }

    private void InitiateNewLevel()
    {
        if (!STARTUP && screenFadeAnimator != null)
            screenFadeAnimator.SetTrigger("fadeIn");

        CleanupSpellObjects();
        ToClass();

        CancelInvoke();
        Invoke(nameof(StartLevelAnim), STARTDELAY);
        Invoke(nameof(InitProblem), 0.1f);
    }

    private void InitProblem()
    {
        if (hoGameBeh != null)
            hoGameBeh.Initiate();
    }
    #endregion

    #region UI Event Handlers
    public void onRestart()
    {
        if (formulaDisplay != null) formulaDisplay.SetActive(false);
        if (screenFadeAnimator != null) screenFadeAnimator.SetTrigger("sceneOut");
        Invoke(nameof(LoadSceneDelay), TRANSITIONDELAY);
    }

    private void LoadSceneDelay()
    {
        SceneManager.LoadScene("GameLevelScene_v3");
    }

    public void onQuit()
    {
        error = 100f;
        isQuit = true;
        if (screenFadeAnimator != null) screenFadeAnimator.SetTrigger("sceneOut");
        Invoke(nameof(EndGameFunctions), TRANSITIONDELAY);
    }

    public void onUndo()
    {
        if (fa.GetIsEquMode())
        {
            fa.InputString("equ");
            return;
        }

        if (lineSnapper != null)
            lineSnapper.OnUndoPressed();
    }

    public void toggleMagicScroll()
    {
        if (pLowerScroll != null)
            pLowerScroll.SetActive(!pLowerScroll.activeInHierarchy);
    }

    public void btnNo()
    {
        toggleConfirmScreen("");
    }

    public void CloseNotification()
    {
        if (pNotify != null)
            pNotify.SetActive(false);

        if (isDoneMeasuring)
            Invoke(nameof(ResumeOCR), 0.1f);
    }

    private void ResumeOCR()
    {
        if (ocrScript != null)
            ocrScript.processing = false;
    }
    #endregion

    #region Shape Selection
    public void chooseSquare()
    {
        chosenShape = shapeStrings[GameBehaviour.SHAPES.SQUARE];
        ShowEquationPanel(pEquationSquare);
    }

    public void chooseSemiCircle()
    {
        chosenShape = shapeStrings[GameBehaviour.SHAPES.SEMI_CIRCLE];
        ShowEquationPanel(pEquationSCircle);
    }

    public void chooseCircle()
    {
        chosenShape = shapeStrings[GameBehaviour.SHAPES.CIRCLE];
        ShowEquationPanel(pEquationCircle);
    }

    public void chooseRectangle()
    {
        chosenShape = shapeStrings[GameBehaviour.SHAPES.RECTANGLE];
        ShowEquationPanel(pEquationRectangle);
    }

    public void chooseTriangle()
    {
        chosenShape = shapeStrings[GameBehaviour.SHAPES.TRIANGLE];
        ShowEquationPanel(pEquationTriangle);
    }

    private void ShowEquationPanel(GameObject targetPanel)
    {
        hideAllEquation();
        if (targetPanel != null)
            targetPanel.SetActive(true);
    }

    public void hideAllEquation()
    {
        if (pEquationSquare != null) pEquationSquare.SetActive(false);
        if (pEquationSCircle != null) pEquationSCircle.SetActive(false);
        if (pEquationCircle != null) pEquationCircle.SetActive(false);
        if (pEquationRectangle != null) pEquationRectangle.SetActive(false);
        if (pEquationTriangle != null) pEquationTriangle.SetActive(false);
    }
    #endregion

    #region Confirmation System
    public void toggleConfirmScreen(string what)
    {
        if (what == "shape")
            what = chosenShape;

        bool shouldShow = !pConfirm.activeInHierarchy;
        if (pConfirm != null) pConfirm.SetActive(shouldShow);

        // Reset button states
        if (bYesHome != null) bYesHome.gameObject.SetActive(false);
        if (bYes != null) bYes.gameObject.SetActive(false);

        if (what == "confirmHome")
        {
            SetConfirmationText("Nais mo bang bumalik sa labas na pagpipilian?", "Hindi masa-Save ang progreso.");
            if (bYesHome != null) bYesHome.gameObject.SetActive(true);
        }
        else if (shouldShow)
        {
            SetConfirmationText("Tama ba ang napili:", "[ " + what + " ]?");
            if (bYes != null) bYes.gameObject.SetActive(true);
        }
    }

    private void SetConfirmationText(string confirmText, string additionalText)
    {
        if (pConfirmText != null) pConfirmText.text = confirmText;
        if (this.confirmText != null) this.confirmText.text = additionalText;
    }

    public void btnYes()
    {
        if (spellCastEvent?.problem == null) return;

        string currentShapeString = shapeStrings[spellCastEvent.problem.problemShape];

        if (currentShapeString == chosenShape)
        {
            ProcessCorrectShapeSelection();
        }
        else
        {
            notifyWrongShape();
        }

        toggleConfirmScreen("");
    }

    private void ProcessCorrectShapeSelection()
    {
        hideDiaBoxWhileMeasuring();

        // Batch UI updates
        if (btnConfirmSpell != null) btnConfirmSpell.gameObject.SetActive(false);
        if (panelMagicScroll != null) panelMagicScroll.SetActive(false);
        if (pDiaButtons != null) pDiaButtons.SetActive(true);
        if (characterSay != null) characterSay.text = charDialogue2;

        // Enable measurement tools
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(true);
        if (btnUndo != null) btnUndo.gameObject.SetActive(true);

        ShowEquationForShape(chosenShape);
    }

    private void ShowEquationForShape(string shape)
    {
        switch (shape)
        {
            case "TRIANGLE":
                if (pEquationTriangle != null) pEquationTriangle.SetActive(true);
                break;
            case "SQUARE":
                if (pEquationSquare != null) pEquationSquare.SetActive(true);
                break;
            case "RECTANGLE":
                if (pEquationRectangle != null) pEquationRectangle.SetActive(true);
                break;
            case "CIRCLE":
                if (pEquationCircle != null) pEquationCircle.SetActive(true);
                break;
            case "SEMI_CIRCLE":
                if (pEquationSCircle != null) pEquationSCircle.SetActive(true);
                break;
        }
    }
    #endregion

    #region Notification System
    public void notifyWrongShape()
    {
        if (pNotify != null)
            pNotify.SetActive(true);
    }

    public void NotifyInvalidFormula()
    {
        if (ocrScript != null) ocrScript.processing = true;
        ShowNotification("Hindi wasto ang ibinigay na formula.");
    }

    public void NotifyMismatchedAnswer()
    {
        if (ocrScript != null) ocrScript.processing = true;
        ShowNotification("Hindi tugma sa formula ang ibinigay na sagot.");
    }

    private void ShowNotification(string message)
    {
        if (pNotify != null) pNotify.SetActive(true);
        if (pNotifyText != null) pNotifyText.text = message;
    }
    #endregion

    #region Dialogue Box Animation
    public void hideDiaBoxWhileMeasuring()
    {
        if (rtDialogue != null)
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, hideDiaPos));

        if (backspaceButton != null) backspaceButton.SetActive(false);

        if (isAllSolved)
        {
            if (btnMeasure != null) btnMeasure.gameObject.SetActive(false);
        }
        else
        {
            if (btnMeasure != null) btnMeasure.gameObject.SetActive(true);
        }
    }

    public void showDiaBoxAfterMeasuring()
    {
        if (rtDialogue != null)
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, showDiaPos));

        if (calcBtnObj != null) calcBtnObj.SetActive(true);
    }

    public void toggleDialogueBox()
    {
        if (rtDialogue == null) return;

        if (rtDialogue.anchoredPosition.y == 100)
        {
            // Hide dialogue
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, dialogueUpPos));
            if (rtDiaButtons != null)
                StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, buttonsLeftPos));
        }
        else
        {
            // Show dialogue
            Vector2 targetPos = (justDoneSolving && !isAllSolved) ? dialogueUpPos : dialogueDownPos;
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, targetPos));

            HandleDialogueButtonsPosition();
        }
    }

    private void HandleDialogueButtonsPosition()
    {
        if (rtDiaButtons == null) return;

        if (!isDoneMeasuring)
        {
            StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, buttonsLeftPos));
            if (btnMeasure != null) btnMeasure.gameObject.SetActive(true);
            if (backspaceButton != null) backspaceButton.SetActive(false);
        }
        else
        {
            if (btnMeasure != null) btnMeasure.gameObject.SetActive(false);
            if (backspaceButton != null) backspaceButton.SetActive(true);
            StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, buttonsUpPos));
        }

        if (isAllSolved)
        {
            StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, buttonsUpPos));
        }
    }
    #endregion

    #region Animation Coroutines
    private IEnumerator RectTransformOverTime(RectTransform rt, float duration, Vector2 endTransform)
    {
        if (rt == null) yield break;

        var startTransform = rt.anchoredPosition;
        var elapsed = 0f;
        var invDuration = 1f / duration;

        while (elapsed < duration)
        {
            var t = elapsed * invDuration;
            rt.anchoredPosition = Vector2.Lerp(startTransform, endTransform, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = endTransform;
    }

    private IEnumerator MoveOverTime(GameObject obj, float duration, Vector3 endPosition)
    {
        if (obj == null) yield break;

        var startPosition = obj.transform.position;
        var elapsed = 0f;
        var invDuration = 1f / duration;

        while (elapsed < duration)
        {
            var t = elapsed * invDuration;
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPosition;
    }

    private IEnumerator LocalScaleOverTime(GameObject obj, float duration, Vector3 endScale)
    {
        if (obj == null) yield break;

        var startScale = obj.transform.localScale;
        var elapsed = 0f;
        var invDuration = 1f / duration;

        while (elapsed < duration)
        {
            var t = elapsed * invDuration;
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = endScale;
    }
    #endregion

    #region Calculator & OCR Management
    public void ToggleCalcMode()
    {
        if (ocrScript == null || fa == null) return;

        // Batch OCR operations
        ocrScript.ResetColor();
        ocrScript.ResetVFX();

        if (fa.calcMode)
        {
            if (calcBtnText != null) calcBtnText.text = "Calculator";
            fa.ExitCalc();
        }
        else
        {
            if (calcBtnText != null) calcBtnText.text = "Formula Input";
            fa.EnterCalc();
        }
    }

    public void OnBackspacePressed()
    {
        if (fa != null) fa.BackspaceInput();

        if (ocrScript != null)
        {
            ocrScript.ResetColor();
            ocrScript.ResetVFX();
        }
    }

    public void onCast()
    {
        inputAnswer = 0f;
        if (btnMeasure != null) btnMeasure.gameObject.SetActive(false);
        if (characterSay != null) characterSay.text = "";

        justDoneSolving = false;

        if (isAllSolved || !isDoneMeasuring)
        {
            DoneMeasure();
        }
        else
        {
            ResetFormulaAnalyzer();
            ResetOCRScript();
        }
    }

    private void ResetFormulaAnalyzer()
    {
        if (fa != null)
        {
            fa.ResetCalcDisp();
            fa.ResetAnalyzer();
        }
    }

    private void ResetOCRScript()
    {
        if (ocrScript != null)
        {
            ocrScript.ResetColor();
            ocrScript.ResetVFX();
        }
    }
    #endregion

    #region Measurement System
    public void DoneMeasure()
    {
        isDoneMeasuring = true;

        if (textFinish != null) textFinish.text = castBtnText2;
        if (calcBtnObj != null) calcBtnObj.SetActive(true);

        UpdateVariableDisplays();

        if (isAllSolved && fa != null)
            fa.isCompoundArea = true;

        StartCoroutine(SlideOCRBoard(true));

        if (lineSnapper != null)
            lineSnapper.ToggleLineText();
    }

    private void UpdateVariableDisplays()
    {
        if (lineSnapper == null) return;

        if (var1Display != null)
        {
            var text1 = var1Display.GetComponent<Text>();
            if (text1 != null) text1.text = lineSnapper.value1;
        }

        if (var2Display != null)
        {
            var text2 = var2Display.GetComponent<Text>();
            if (text2 != null) text2.text = lineSnapper.value2;
        }
    }

    public void UndoMeasure()
    {
        undoPressed = true;

        if (lineSnapper != null)
        {
            if (lineSnapper.lineCount >= 1)
                lineSnapper.value2 = "???";
            if (lineSnapper.lineCount < 1)
                lineSnapper.value1 = "???";
        }

        if (isDoneMeasuring)
        {
            StartCoroutine(SlideOCRBoard(false));
            Invoke(nameof(ToggleLineDelay), OCRSLIDETIME);
        }

        if (textFinish != null) textFinish.text = castBtnText1;
        if (calcBtnObj != null && calcBtnObj.activeInHierarchy)
            calcBtnObj.SetActive(false);

        isDoneMeasuring = false;
    }

    private void ToggleLineDelay()
    {
        if (lineSnapper != null)
            lineSnapper.ToggleLineText();
    }
    #endregion

    #region OCR Board Management
    private IEnumerator SlideOCRBoard(bool show)
    {
        if (show)
        {
            yield return StartCoroutine(ShowOCRBoard());
        }
        else
        {
            yield return StartCoroutine(HideOCRBoard());
        }
    }

    private IEnumerator ShowOCRBoard()
    {
        if (!isAllSolved)
            toggleDialogueBox();

        yield return GetCachedWaitForSeconds(DIALOGUESLIDETIME);

        if (ocrInput != null)
        {
            ocrInput.SetActive(true);
            if (ocrEndTransform != null)
                StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, ocrEndTransform.position));
        }

        if (rtDialogue != null)
            StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, dialogueOCRPos));

        if (pDialogue != null)
            StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, dialogueSmallScale));

        yield return GetCachedWaitForSeconds(OCRSLIDETIME);

        if (ocrScript != null)
        {
            ocrScript.ResetColor();
            ocrScript.ResetVFX();
            ocrScript.processing = false;
        }

        if (formulaDisplay != null) formulaDisplay.SetActive(true);
        if (backspaceButton != null) backspaceButton.SetActive(true);
    }

    private IEnumerator HideOCRBoard()
    {
        if (ocrScript != null) ocrScript.processing = true;
        if (formulaDisplay != null) formulaDisplay.SetActive(false);

        if (ocrInput != null && ocrStartTransform != null)
            StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, ocrStartTransform.position));

        if (rtDialogue != null)
            StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, origDiaRT));

        if (pDialogue != null)
            StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, dialogueNormalScale));

        if (!isAllSolved && pDiaButtons != null)
            pDiaButtons.SetActive(false);

        if (backspaceButton != null) backspaceButton.SetActive(false);

        if (inputAnswer > 0f) undoPressed = false;
        if (undoPressed)
        {
            if (pDiaButtons != null) pDiaButtons.SetActive(true);
            if (backspaceButton != null) backspaceButton.SetActive(true);
        }

        yield return GetCachedWaitForSeconds(OCRSLIDETIME);

        if (ocrInput != null) ocrInput.SetActive(false);

        if (isFinalAnswer || undoPressed)
        {
            toggleDialogueBox();
            undoPressed = false;
        }
        else
        {
            toggleDialogueBox();
        }
    }
    #endregion

    #region Answer Processing
    public void InputAnswer(float ans = 0f)
    {
        inputAnswer = ans;
        ResetVarDisp(currentShape);

        if (!isFinalAnswer)
        {
            float recordValue = (currentShapeObject != null && currentShapeObject.isExcess) ? -inputAnswer : inputAnswer;
            if (currentShapeObject != null)
                recordedAnswer.Add(currentShapeObject, recordValue);
        }

        CalcError();
        StartCoroutine(SlideOCRBoard(false));

        float absError = Mathf.Abs(error);
        float roundedError = Mathf.Round(absError * 100f) / 100f;

        if (roundedError != 0f)
        {
            ProcessErrorResult(roundedError);
        }
        else
        {
            ProcessSuccessResult();
        }
    }

    private void ProcessErrorResult(float roundedError)
    {
        if (correctionPerc != null)
        {
            correctionPerc.text = $"Error: {roundedError}%";
            correctionPerc.gameObject.SetActive(true);
        }

        if (!isFinalAnswer)
            ManaFilling();

        toggleDialogueBox();
        HideNewUI();

        float animDelay = isFinalAnswer ? FILLTIMEAPROX - 1.0f + OCRSLIDETIME : FILLTIMEAPROX + OCRSLIDETIME;
        Invoke(nameof(CallCastAnimation), animDelay);

        if (soundPlayer != null)
            soundPlayer.PlaySFX(3, 1, 2f);
    }

    private void ProcessSuccessResult()
    {
        if (calcBtnObj != null) calcBtnObj.SetActive(false);

        if (!isFinalAnswer)
        {
            HandleNonFinalSuccess();
        }

        ModifiedToUIAgain();

        if (hoGameBeh != null && hoGameBeh.isAllAttemptedSolve())
        {
            ProcessAllShapesSolved();
        }

        if (isFinalAnswer)
        {
            ProcessFinalAnswer();
        }
    }

    private void HandleNonFinalSuccess()
    {
        if (soundPlayer != null) soundPlayer.PlaySFX(1, 1, 0.75f);

        if (hoGameBeh?.shapeClickManager != null)
            hoGameBeh.shapeClickManager.EnableShapeClicking();

        if (currentlySolvedShape != null && currentlySolvedShape.TryGetComponent<MeshCollider>(out var meshCollider))
            meshCollider.enabled = false;

        justDoneSolving = true;
        toggleDialogueBox();
        ManaFilling();

        if (hoGameBeh?.spellCastEvent != null)
            hoGameBeh.spellCastEvent.setHiddenStateAllShapes(false);
    }

    private void ProcessAllShapesSolved()
    {
        if (pDiaButtons != null) pDiaButtons.SetActive(true);
        if (backspaceButton != null) backspaceButton.SetActive(false);
        if (btnUndo != null) btnUndo.gameObject.SetActive(false);
        if (btnMeasure != null) btnMeasure.gameObject.SetActive(false);

        isAllSolved = true;
    }

    private void ProcessFinalAnswer()
    {
        HideNewUI();

        if (pSpawner != null) pSpawner.gameObject.SetActive(false);
        if (pDiaButtons != null) pDiaButtons.SetActive(false);

        toggleDialogueBox();

        if (txtFinalCompound != null) txtFinalCompound.text = "";
        if (correctionPerc != null)
        {
            correctionPerc.text = "Spell Complete!";
            correctionPerc.gameObject.SetActive(true);
        }

        if (soundPlayer != null) soundPlayer.PlaySFX(4, 1, 1.5f);

        Invoke(nameof(CallCastAnimation), FILLTIMEAPROX - 1.0f + OCRSLIDETIME);
    }

    private void CalcError()
    {
        if (spellCastEvent == null) return;

        float clamped = spellCastEvent.GetFillPercentage();

        if (shapeFiller != null)
        {
            shapeFiller.fillMaxValue = clamped;
            shapeFiller.isFillingActive = true;
        }

        if (clamped > 2.0f)
            clamped = 2.0f;

        error = (1 - clamped) * 100f;
    }

    private void ManaFilling()
    {
        if (currentlySolvedShape == null || currentShapeObject == null || shapeFiller == null) return;

        ShapeFiller currFiller = currentlySolvedShape.AddComponent<ShapeFiller>();

        if (!currentShapeObject.isExcess)
            currFiller.fillMaterial = shapeFiller.fillMaterial;
        else if (currentShapeObject.isIntersect)
            currFiller.fillMaterial = shapeFiller.intersectMaterial;
        else
            currFiller.fillMaterial = shapeFiller.voidMaterial;

        currFiller.InitializeFill(currentlySolvedShape, Color.green, 0.5f, spellCastEvent.GetFillPercentage());
        currFiller.isFillingActive = true;
    }
    #endregion

    #region Camera & Scene Management
    private void ToClass()
    {
        if (animScript?.VideoPlayerScript != null)
            animScript.VideoPlayerScript.Stop();

        if (correctionPerc != null) correctionPerc.gameObject.SetActive(false);
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.SetActive(false);
        if (classroomCamera != null) classroomCamera.SetActive(true);
    }

    private void ToUI()
    {
        if (animScript?.VideoPlayerScript != null)
        {
            animScript.VideoPlayerScript.Stop();
            animScript.VideoPlayerScript.PlayBGAnim();
        }
        ModifiedToUI();
    }

    private void ModifiedToUI()
    {
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(false);
        if (characterSay != null) characterSay.text = charDialogue1;
        if (mainCamera != null) mainCamera.SetActive(true);
        if (classroomCamera != null) classroomCamera.SetActive(false);

        SetVisibilityNewUI(false, true, false, true);
    }

    private void ModifiedToUIAgain()
    {
        currentlySolvedShape = null;

        if (lineSnapper != null)
        {
            lineSnapper.OnUndoPressed();
            lineSnapper.OnUndoPressed();
            lineSnapper.gameObject.SetActive(false);
        }

        if (!isFinalAnswer && characterSay != null)
            characterSay.text = charDialogue1;

        SetVisibilityNewUI(false, true, false, true);
        hideAllEquation();

        if (hoGameBeh != null && hoGameBeh.isAllAttemptedSolve())
        {
            ProcessAllShapesComplete();
        }
    }

    private void HideNewUI()
    {
        if (hud != null) hud.SetActive(false);
        if (pDialogue != null) pDialogue.SetActive(false);
        if (panelMagicScroll != null) panelMagicScroll.SetActive(false);
        if (quickMenu != null) quickMenu.SetActive(false);
        if (areaDisplayObj != null) areaDisplayObj.SetActive(false);
    }

    private void SetVisibilityNewUI(bool a, bool b, bool c, bool d)
    {
        if (hud != null) hud.SetActive(a);
        if (pDialogue != null) pDialogue.SetActive(b);
        if (panelMagicScroll != null) panelMagicScroll.SetActive(c);
        if (btnConfirmSpell != null) btnConfirmSpell.gameObject.SetActive(c);
        if (quickMenu != null) quickMenu.SetActive(d);
    }

    private void RemoveRoomText()
    {
        if (hud != null) hud.SetActive(false);
    }
    #endregion

    #region Compound Shape Processing
    private void ProcessAllShapesComplete()
    {
        if (areaDisplayObj != null)
        {
            areaDisplayObj.SetActive(true);
            if (areaDisplay != null)
                areaDisplay.text = areaDisplayText1;
        }

        int i = 0;
        const int n = 1;
        foreach (var dict in recordedAnswer)
        {
            i++;
            ProcessCompoundShape(i, dict.Key.shape, dict.Value, n);
        }
    }

    private void ProcessCompoundShape(int index, GameBehaviour.SHAPES shape, float value, int n)
    {
        if (txtFinalCompound != null)
            txtFinalCompound.text = "Mga Component na bumubuo sa Compound Shape:";

        spawnCompoundComponents(index, shape, value);

        if (areaDisplay != null)
        {
            string currentText = areaDisplay.text;
            areaDisplay.text = $"{currentText}[{shape}]: {value} ";

            if (index % n == 0)
                areaDisplay.text += '\n';
        }
    }

    private void spawnCompoundComponents(int i, GameBehaviour.SHAPES shapeName, float shapeVal)
    {
        if (prefabSpawn == null || pSpawner == null) return;

        Vector2 newPos = new Vector2(0f, -60f * i);
        GameObject newSpawn = Instantiate(prefabSpawn, pSpawner);

        if (newSpawn.TryGetComponent<RectTransform>(out var rtSpawn))
        {
            rtSpawn.anchoredPosition = newPos;
        }
        else
        {
            newSpawn.transform.localPosition = newPos;
        }

        var txtShape = newSpawn.transform.Find("shapeText")?.GetComponent<Text>();
        if (txtShape != null)
            txtShape.text = $"{shapeName} : {shapeVal}";

        SetupShapeImages(newSpawn, shapeName);
    }

    private void SetupShapeImages(GameObject newSpawn, GameBehaviour.SHAPES shapeName)
    {
        var shapeImages = new Dictionary<string, GameObject>
        {
            { "shapeSqr", newSpawn.transform.Find("shapeSqr")?.gameObject },
            { "shapeCir", newSpawn.transform.Find("shapeCir")?.gameObject },
            { "shapeSCir", newSpawn.transform.Find("shapeSCir")?.gameObject },
            { "shapeRect", newSpawn.transform.Find("shapeRect")?.gameObject },
            { "shapeTri", newSpawn.transform.Find("shapeTri")?.gameObject }
        };

        // Deactivate all images first
        foreach (var image in shapeImages.Values)
        {
            image?.SetActive(false);
        }

        // Activate the correct image based on shape
        string targetImage = shapeName switch
        {
            GameBehaviour.SHAPES.SQUARE => "shapeSqr",
            GameBehaviour.SHAPES.TRIANGLE => "shapeTri",
            GameBehaviour.SHAPES.RECTANGLE => "shapeRect",
            GameBehaviour.SHAPES.CIRCLE => "shapeCir",
            GameBehaviour.SHAPES.SEMI_CIRCLE => "shapeSCir",
            _ => null
        };

        if (targetImage != null && shapeImages.ContainsKey(targetImage))
        {
            shapeImages[targetImage]?.SetActive(true);
        }
    }
    #endregion

    #region Shape Click Management
    public void UIAfterShapeSelect(ShapeClickManager.ShapeClickData clickData)
    {
        if (correctionPerc != null) correctionPerc.text = "";

        if (hoGameBeh?.shapeClickManager != null)
            hoGameBeh.shapeClickManager.DisableShapeClicking();

        if (hoGameBeh?.spellCastEvent != null)
            hoGameBeh.spellCastEvent.setHiddenStateAllShapes(true);

        GlobalVariables.loSelectedShape = clickData.shapeType;
        currentlySolvedShape = clickData.originalShapeObject.actualShapeObj;
        currentShapeObject = clickData.originalShapeObject;

        SetManualProblem(clickData);
        SetVisibilityNewUI(false, true, true, true);
    }

    public void SetManualProblem(ShapeClickManager.ShapeClickData data)
    {
        currentShape = data.shapeType;

        Problem problem = new Problem(currentShape, this, data.originalShapeObject.actualShapeObj,
                                     data.originalShapeObject.x, data.originalShapeObject.y);

        this.spellCastEvent = new SpellCastEvent(this, problem);
    }

    private int SendShapeToPlayer(GameBehaviour.SHAPES s)
    {
        return s switch
        {
            GameBehaviour.SHAPES.SQUARE => 0,
            GameBehaviour.SHAPES.RECTANGLE => 1,
            GameBehaviour.SHAPES.TRIANGLE => 2,
            GameBehaviour.SHAPES.CIRCLE => 3,
            GameBehaviour.SHAPES.SEMI_CIRCLE => 4,
            _ => -1
        };
    }
    #endregion

    #region Animation & End Game
    private void StartLevelAnim()
    {
        if (screenFadeAnimator != null) screenFadeAnimator.SetTrigger("fade");
        Invoke(nameof(ToUI), TRANSITIONTIME);
        Invoke(nameof(RemoveRoomText), TRANSITIONTIME);
        Invoke(nameof(PlayMusic), TRANSITIONTIME);
    }

    private void PlayMusic()
    {
        if (soundPlayer != null)
            soundPlayer.PlayBGM(0, 1, 0.4f);
    }

    private void CallCastAnimation()
    {
        if (screenFadeAnimator != null) screenFadeAnimator.SetTrigger("fade");

        Invoke(nameof(ToClass), TRANSITIONTIME);
        StartCoroutine(DelayedCastAnimation());
    }

    private IEnumerator DelayedCastAnimation()
    {
        yield return new WaitForSeconds(TRANSITIONTIME + 0.1f);

        HideNewUI();

        int state = (error > 0) ? 0 : (error < 0) ? 1 : 2;
        animScript.VideoPlayerScript.PlaySpellAnim(GameBehaviour.SHAPES.NONE, state);

        yield return new WaitUntil(() => animScript.VideoPlayerScript.videoPlayer.isPrepared);

        float len = animScript.VideoPlayerScript.GetVideoLength();
        float sd = state == 2 ? len : 0f;

        yield return new WaitForSeconds(sd + ENDDELAY);

        FadeDelay();

        yield return new WaitForSeconds(1f);

        EndGameFunctions();
    }

    private void FadeDelay()
    {
        if (screenFadeAnimator != null)
            screenFadeAnimator.SetTrigger("fadeOut");
    }

    private void EndGameFunctions()
    {
        float roundedError = Mathf.Round(Mathf.Abs(error) * 100f) / 100f;

        if (roundedError == 0f)
        {
            GlobalVariables.playerWin = true;
            GlobalVariables.percent = GlobalVariables.level < 3 ? 0f : 1f;
        }
        else
        {
            GlobalVariables.playerWin = false;
            GlobalVariables.percent = 1 - error;
        }

        if (!isQuit)
            GlobalVariables.gameFinished = true;

        SceneManager.LoadScene("LevelSelect");
    }

    public void InstanceSpellObject(GameObject instanced = null)
    {
        if (animScript?.VideoPlayerScript != null)
            animScript.VideoPlayerScript.PlaySpellIntro(GameBehaviour.SHAPES.NONE);
    }
    #endregion

    #region Utility Methods
    private WaitForSeconds GetCachedWaitForSeconds(float time)
    {
        int index = Mathf.Clamp(Mathf.RoundToInt(time * 10f), 0, cachedWaitForSeconds.Length - 1);
        return cachedWaitForSeconds[index];
    }
    #endregion

    #region Nested Classes
    /// <summary>
    /// Represents a game problem with shape and measurements
    /// </summary>
    public class Problem
    {
        public GameBehaviour.SHAPES problemShape;
        public float p_measure = UNUSED;
        public float s_measure = UNUSED;
        public HOGameScript main;
        public GameObject problemObjectShape;

        public Problem(GameBehaviour.SHAPES shape, HOGameScript main, GameObject shapeProblem, float x, float y)
        {
            this.main = main;
            this.problemShape = shape;
            this.problemObjectShape = shapeProblem;
            this.p_measure = x;
            this.s_measure = y;
        }
    }

    /// <summary>
    /// Handles spell casting events and area calculations
    /// </summary>
    public class SpellCastEvent
    {
        public HOGameScript main;
        public Problem problem;
        private double p_measure = UNUSED;
        private double s_measure = UNUSED;

        private const double HALF_PI = Math.PI * 0.5;

        public SpellCastEvent(HOGameScript behavior, Problem prob)
        {
            this.main = behavior;
            this.problem = prob;
            this.p_measure = prob.p_measure;
            this.s_measure = prob.s_measure;
        }

        public float GetFillPercentage()
        {
            if (main.isFinalAnswer)
            {
                float x = main.recordedAnswer.Values.Sum();
                float y = main.inputAnswer;
                return y / x;
            }

            double result = problem.problemShape switch
            {
                GameBehaviour.SHAPES.TRIANGLE => 0.5 * p_measure * s_measure,
                GameBehaviour.SHAPES.CIRCLE => Math.PI * (p_measure * 0.5) * (p_measure * 0.5),
                GameBehaviour.SHAPES.RECTANGLE => p_measure * s_measure,
                GameBehaviour.SHAPES.SQUARE => p_measure * p_measure,
                GameBehaviour.SHAPES.SEMI_CIRCLE => HALF_PI * (p_measure * 0.5) * (p_measure * 0.5),
                _ => throw new Exception("Invalid shape")
            };

            float compX = (float)Math.Round(result, 2);
            return main.inputAnswer / compX;
        }
    }
    #endregion
}