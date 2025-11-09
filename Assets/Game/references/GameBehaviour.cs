using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameBehaviour : MonoBehaviour
{
    #region Constants
    private const int UNUSED = -1;

    private const float TRANSITIONTIME = 0.4f;
    private const float FILLTIMEAPROX = 1.5f;
    private float STARTDELAY = GlobalVariables.introLen;
    private const float TRANSITIONDELAY = 0.5f;

    // OPTIMIZATION: Cached strings to avoid allocations
    private const string correctShapePropmt = "Tama na ba ang shape na pinili?";
    private const string castBtnText1 = "Done";
    private const string castBtnText2 = "Erase";
    private const string undoBtnText1 = "Undo";
    private const string undoBtnText2 = "Cast";
    private const string undoBtnText3 = "Back";
    private const string wrongShapeMsg = "Ang shape na pinili ay mali. Subukan ulit.";
    private const string invalidFormulaMsg = "Hindi wasto ang ibinigay na formula.";
    private const string mismatchedAnswerMsg = "Hindi tugma sa formula ang ibinigay na sagot.";
    private const string homeConfirmMsg = "Nais mo bang bumalik sa labas na pagpipilian?";
    private const string progressNotSavedMsg = "Hindi masa-Save ang progreso.";
    private const string correctChoiceMsg = "Tama ba ang napili:";

    //----------------------------------------------
    private const float DIALOGUESLIDETIME = 0.25f;
    private const float OCRSLIDETIME = 0.35f;
    #endregion

    #region Enums
    public enum SHAPES
    {
        NONE,
        TRIANGLE,
        SQUARE,
        RECTANGLE,
        CIRCLE,
        SEMI_CIRCLE,
    }
    #endregion

    #region Public Fields (Inspector)
    public SpellCastEvent spellCastEvent;
    public ShapeGenerator shapeGenerator;
    public ShapeFiller shapeFiller;
    public LineSnapper lineSnapper;

    // UI
    public GameObject hud;
    public GameObject quickMenu;
    public GameObject panelMagicScroll;
    public GameObject pConfirm;
    public GameObject pLowerScroll;
    public GameObject pNotify;
    private GameObject pDialogue;
    private GameObject pDiaButtons;
    public GameObject notifyTextObj;
    public LeftHandedMode canvasScript;

    // Text refs
    public Text textTemp;
    public Text textEME;
    public Text pConfirmText;
    private Text pNotifyText;
    private Text characterSay;
    public Text textFinish;
    public Text confirmText;
    public Text textHUD;
    public Text undoText;
    public GameObject measureCounter;

    [Header("Hint System")]
    public GameObject textHint;
    public GameObject textHintSpell;
    public GameObject textHintUndo;
    public GameObject textHintCalcu;
    public GameObject spriteHint;
    public GameObject spriteHintUndo;
    public GameObject spriteHintSpell;
    public GameObject spriteHintCalcu;
    public Image spriteHintImg;
    public Image spriteHintImgUndo;
    public Image spriteHintImgSpell;
    public Image spriteHintImgCalcu;

    [Header("Equation Panels")]
    public GameObject pEquationTriangle;
    public GameObject pEquationSquare;
    public GameObject pEquationRectangle;
    public GameObject pEquationSCircle;
    public GameObject pEquationCircle;

    [Header("Buttons")]
    public Button bYesHome;
    public Button bYes;
    public Button btnConfirmSpell;
    public Button btnMeasure;

    // OCR and formula
    [Header("OCR & Formula")]
    public GameObject ocrInput;
    public GameObject formulaDisplay;
    public Transform rightStartTransform, rightEndTransform, leftStartTransform, leftEndTransform;
    private Transform ocrStartTransform, ocrEndTransform;
    public GameObject formulaAnalyzerObj;
    public GameObject calcBtnObj;
    public GameObject backspaceButton;

    [Header("Variable Displays")]
    public GameObject sqVarDisp1, sqVarDisp2, rectVarDisp1, rectVarDisp2, triVarDisp1, triVarDisp2, cirVarDisp1, cirVarDisp2, semiVarDisp1, semiVarDisp2;

    [Header("Sound")]
    public GameObject soundPlayerObj;

    [Header("Image")]
    public Image undoBtnImg;
    public Image undoBtnLogo;
    public Sprite undoLogoDefault;
    public Sprite undoLogoCast;
    #endregion

    #region Private / Cached Fields
    private SHAPES currentShape;

    // legacy flags (kept for compatibility)
    private bool cp, ls; //Activity states of UI components
    private GameObject mainCamera, classroomCamera;
    private Animator screenFadeAnimator;

    private AnimScript animScript;
    private bool STARTUP = true;
    public float error = 100f;

    // Save/load
    private GameData savedGame;
    private SaveLoadController saverLoader = new SaveLoadController();
    private string savePath;

    // Cached RectTransform references
    private RectTransform rtDialogue;
    private RectTransform rtDiaButtons;

    private bool isDoneMeasuring;

    // OCR and formula
    private DrawingAndOCRManagerScript ocrScript;
    private FormulaAnalyzer fa;
    private Vector2 origDiaRT;

    private float inputAnswer = 0f;

    private Text calcBtnText;

    // Variable display refs
    private GameObject var1Display, var2Display;

    // Sound player
    private GameLevelSoundPlayer soundPlayer;

    // Cached vectors/constants
    private static readonly Vector2 hiddenDialoguePos = new Vector2(600f, -150f);
    private static readonly Vector2 shownDialoguePos = new Vector2(225f, 130f);
    private static readonly Vector2 measuringDialoguePos = new Vector2(600f, 130f);
    private static readonly Vector2 ocrDialoguePos = new Vector2(308f, 100f);
    private static readonly Vector2 leftButtonPos = new Vector2(-493f, -167f);
    private static readonly Vector2 rightButtonPos = new Vector2(-493f, 138f);
    private static readonly Vector3 normalScale = new Vector3(1f, 1f, 1f);
    private static readonly Vector3 smallScale = new Vector3(0.9f, 0.9f, 0.9f);

    private static  WaitForSeconds blinkDelay = new WaitForSeconds(GlobalVariables.introLen + 0.4f);
    private static readonly WaitForSeconds dialogueWait = new WaitForSeconds(DIALOGUESLIDETIME);
    private static readonly WaitForSeconds ocrWait = new WaitForSeconds(OCRSLIDETIME);

    // Measures
    private float[] currentMeasureArray;
    private float[] currentCircleMeasureArray;

    // Other cached data
    private System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder(64);

    // New: previously missing declarations
    private TMP_Text correctionPerc;
    private string chosenShape;
    #endregion

    #region Awake/Start/Update
    void Awake()
    {
        // Pre-calculate save path once
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        savedGame = saverLoader.loadGame(savePath);

        currentShape = SHAPES.NONE;

        // Cache frequently accessed GameObjects early
        CacheGameObjectReferences();

        // Pre-cache measure arrays based on level
        CacheMeasureArrays();

        // Initialize variable displays based on selected shape
        InitializeVariableDisplays();

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

    void Start()
    {
        // Batch GameObject.Find calls and cache results
        InitializeTextComponents();
        InitializeUIComponents();
        InitializeGameState();

        if (STARTUP)
        {
            screenFadeAnimator?.SetTrigger("fadeIn");
            HideNewUI();
            soundPlayer = soundPlayerObj?.GetComponent<GameLevelSoundPlayer>();
        }

        correctionPerc = GameObject.Find("ManaFillCorrectPerc")?.GetComponent<TMP_Text>();

        mainCamera = GameObject.Find("Main Camera");
        mainCamera?.SetActive(false);
        classroomCamera = GameObject.Find("ClassroomCamera");
        classroomCamera?.SetActive(true);

        if (lineSnapper != null) lineSnapper.animScript = this.animScript;

        // ---- Former Reset logic ----

        currentShape = SHAPES.NONE;
        correctionPerc?.gameObject.SetActive(false);

        // If there was a previous problem shape, clean it up (only meaningful on restarts)
        if (!STARTUP)
        {
            if (spellCastEvent?.problem?.problemObjectShape != null)
                Destroy(spellCastEvent.problem.problemObjectShape);
        }

        if (lineSnapper != null)
        {
            lineSnapper.gameObject.SetActive(false);
            // Call undo twice (kept from original logic)
            lineSnapper.OnUndoPressed();
            lineSnapper.OnUndoPressed();
        }

        if (!STARTUP)
            screenFadeAnimator?.SetTrigger("fadeIn");

        ToClass();
        Invoke(nameof(StartLevelAnim), STARTDELAY);
        Invoke(nameof(InitProblem), 0.1f);
        Invoke(nameof(InitFillShape), 0.2f);
        STARTUP = false;

        // ----------------------------------------------

        correctionPerc?.gameObject.SetActive(false);
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(false);
    }
    void Update()
    {
        if(fa.GetIsEquMode() && undoBtnImg.color != Color.cyan) //Change to cyan on final answer
        {
            undoText.text = undoBtnText2;
            undoBtnImg.color = Color.cyan;
            undoBtnLogo.sprite = undoLogoCast;
        }
        else if (!fa.GetIsEquMode() && undoBtnImg.color != Color.red) //Change to Red on not final answer
        {
            undoText.text = undoBtnText3;
            undoBtnImg.color = Color.red;
            undoBtnLogo.sprite = undoLogoDefault;
        }
    }
    #endregion

    #region Cache & Init helpers
    private void CacheGameObjectReferences()
    {
        var sf = GameObject.Find("ScreenFade");
        if (sf != null) screenFadeAnimator = sf.GetComponent<Animator>();

        var ah = GameObject.Find("AnimHolder");
        if (ah != null) animScript = ah.GetComponent<AnimScript>();

        if (ocrInput != null)
        {
            var d = ocrInput.transform.Find("DrawingAndOCRManager");
            if (d != null) ocrScript = d.GetComponent<DrawingAndOCRManagerScript>();
        }

        if (formulaAnalyzerObj != null) fa = formulaAnalyzerObj.GetComponent<FormulaAnalyzer>();

        if (notifyTextObj != null) pNotifyText = notifyTextObj.GetComponent<Text>();

        if (calcBtnObj != null)
        {
            var cb = calcBtnObj.transform.Find("textFinish");
            if (cb != null) calcBtnText = cb.gameObject.GetComponent<Text>();
            calcBtnObj.SetActive(false);
        }
    }

    private void CacheMeasureArrays()
    {
        switch (GlobalVariables.level)
        {
            case 0:
            case 1:
                currentMeasureArray = GlobalVariables.loMeasures1;
                currentCircleMeasureArray = GlobalVariables.loCircleMeasures1;
                break;
            case 2:
            case 3:
                currentMeasureArray = GlobalVariables.loMeasures2;
                currentCircleMeasureArray = GlobalVariables.loCircleMeasures2;
                break;
        }
    }

    private void InitializeVariableDisplays()
    {
        switch (GlobalVariables.loSelectedShape)
        {
            case SHAPES.SQUARE:
                var1Display = sqVarDisp1;
                var2Display = sqVarDisp2;
                break;
            case SHAPES.RECTANGLE:
                var1Display = rectVarDisp1;
                var2Display = rectVarDisp2;
                break;
            case SHAPES.TRIANGLE:
                var1Display = triVarDisp1;
                var2Display = triVarDisp2;
                break;
            case SHAPES.CIRCLE:
                var1Display = cirVarDisp1;
                var2Display = cirVarDisp2;
                break;
            case SHAPES.SEMI_CIRCLE:
                var1Display = semiVarDisp1;
                var2Display = semiVarDisp2;
                break;
        }
    }

    private void InitializeTextComponents()
    {
        characterSay = GameObject.Find("characterSay")?.GetComponent<Text>();
        if (textFinish != null) textFinish.text = castBtnText1;
    }

    private void InitializeUIComponents()
    {
        pDialogue = GameObject.Find("PanelCasting");
        if (pDialogue != null) rtDialogue = pDialogue.GetComponent<RectTransform>();
        origDiaRT = rtDialogue != null ? rtDialogue.anchoredPosition : Vector2.zero;
        pDiaButtons = GameObject.Find("pDiaButtons");
        if (pDiaButtons != null) rtDiaButtons = pDiaButtons.GetComponent<RectTransform>();

        stringBuilder.Clear();
        stringBuilder.Append(savedGame != null ? savedGame.currRoom : string.Empty);
        stringBuilder.Append(" ROOM");
        if (textHUD != null) textHUD.text = stringBuilder.ToString();
    }

    private void InitializeGameState()
    {
        isDoneMeasuring = false;

        if (pDialogue != null) pDialogue.SetActive(true);
        if (pConfirm != null) pConfirm.SetActive(false);
        if (pLowerScroll != null) pLowerScroll.SetActive(true);
        if (pNotify != null) pNotify.SetActive(false);

        hideAllEquation();

        if (pDiaButtons != null) pDiaButtons.SetActive(false);

        InitializeHintSystem();

        if (btnConfirmSpell != null) btnConfirmSpell.gameObject.SetActive(false);
        if (btnMeasure != null) btnMeasure.gameObject.SetActive(true);

        if (pConfirmText != null) pConfirmText.text = "";
        if (bYesHome != null) bYesHome.gameObject.SetActive(false);
        if (bYes != null) bYes.gameObject.SetActive(false);

        ShowHint(0);

        // set transparent spell hint color
        if (spriteHintImgSpell != null)
        {
            var spellColor = spriteHintImgSpell.color;
            spellColor.a = 0f;
            spriteHintImgSpell.color = spellColor;
        }
    }

    private void InitializeHintSystem()
    {
        GameObject[] hintsToHide = {
            spriteHint, spriteHintSpell, spriteHintCalcu, textHint,
            spriteHintUndo, textHintUndo, textHintCalcu, textHintSpell
        };

        for (int i = 0; i < hintsToHide.Length; i++)
            if (hintsToHide[i] != null)
                hintsToHide[i].SetActive(false);
    }
    #endregion

    #region Button handlers & UI methods
    public void onRestart()
    {
        if (formulaDisplay != null) formulaDisplay.SetActive(false);
        screenFadeAnimator?.SetTrigger("sceneOut");

        // set spell hint transparent
        if (spriteHintImgSpell != null)
        {
            var spellColor = spriteHintImgSpell.color;
            spellColor.a = 0f;
            spriteHintImgSpell.color = spellColor;
        }

        Invoke(nameof(LoadSceneDelay), TRANSITIONDELAY);
    }

    private void LoadSceneDelay()
    {
        SceneManager.LoadScene("LoadingScreen");
    }

    public void onQuit()
    {
        error = 100f;
        screenFadeAnimator?.SetTrigger("sceneOut");
        Invoke(nameof(EndGameFunctions), TRANSITIONDELAY);
    }

    public void onUndo()
    {
        if (fa.GetIsEquMode())
        {
            fa.InputString("equ");
            return;
        }
        
        if (isDoneMeasuring) textFinish.text = castBtnText1;
        
        lineSnapper?.OnUndoPressed();
    }

    public void toggleConfirmScreen(string what)
    {
        if (what == "shape")
            what = chosenShape;

        if (pConfirm == null) return;

        bool isActive = !pConfirm.activeInHierarchy;
        pConfirm.SetActive(isActive);

        if (isActive)
            HideMeasureHint();

        bool isHomeConfirm = what == "confirmHome";
        bYesHome?.gameObject.SetActive(isHomeConfirm);
        bYes?.gameObject.SetActive(!isHomeConfirm && pConfirm.activeInHierarchy);

        if (isHomeConfirm)
        {
            if (pConfirmText != null) pConfirmText.text = homeConfirmMsg;
            if (confirmText != null) confirmText.text = progressNotSavedMsg;
        }
        else if (pConfirm.activeInHierarchy)
        {
            if (pConfirmText != null) pConfirmText.text = correctChoiceMsg;
            stringBuilder.Clear();
            stringBuilder.Append("[ ");
            stringBuilder.Append(what);
            stringBuilder.Append(" ]?");
            if (confirmText != null) confirmText.text = stringBuilder.ToString();
        }
    }

    public void toggleMagicScroll()
    {
        if (pLowerScroll != null)
            pLowerScroll.SetActive(!pLowerScroll.activeInHierarchy);
    }

    public void hideAllEquation()
    {
        pEquationSquare?.SetActive(false);
        pEquationSCircle?.SetActive(false);
        pEquationCircle?.SetActive(false);
        pEquationRectangle?.SetActive(false);
        pEquationTriangle?.SetActive(false);
    }

    public void chooseSquare() => ChooseShape("SQUARE", pEquationSquare);
    public void chooseSemiCircle() => ChooseShape("SEMI_CIRCLE", pEquationSCircle);
    public void chooseCircle() => ChooseShape("CIRCLE", pEquationCircle);
    public void chooseRectangle() => ChooseShape("RECTANGLE", pEquationRectangle);
    public void chooseTriangle() => ChooseShape("TRIANGLE", pEquationTriangle);

    private void ChooseShape(string shape, GameObject equation)
    {
        chosenShape = shape;
        hideAllEquation();
        equation?.SetActive(true);
    }

    public void btnNo()
    {
        toggleConfirmScreen("");
    }

    public void hideDiaBoxWhileMeasuring()
    {
        if (rtDialogue != null)
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, hiddenDialoguePos));
        backspaceButton?.SetActive(false);
    }

    public void showDiaBoxAfterMeasuring()
    {
        if (rtDialogue != null)
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, shownDialoguePos));
    }

    public void toggleDialogueBox()
    {
        if (rtDialogue == null || rtDiaButtons == null) return;

        if (Math.Abs(rtDialogue.anchoredPosition.y - 100f) < 1f)
        {
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, measuringDialoguePos));
            StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, leftButtonPos));
        }
        else
        {
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new Vector2(600f, -151.46f)));

            if (!isDoneMeasuring)
            {
                StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, leftButtonPos));
                btnMeasure?.gameObject.SetActive(true);
                backspaceButton?.SetActive(false);
            }
            else
            {
                btnMeasure?.gameObject.SetActive(false);
                backspaceButton?.SetActive(true);
                StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, rightButtonPos));
            }
        }
    }
    #endregion

    #region Hints
    private void ShowHint(int step)
    {
        HideMeasureHint();

        switch (step)
        {
            case 0:
                spriteHintSpell?.SetActive(true);
                StartCoroutine(BlinkSprite(step));
                break;
            case 1:
                spriteHint?.SetActive(true);
                textHint?.SetActive(true);
                spriteHintUndo?.SetActive(true);
                textHintUndo?.SetActive(true);
                StartCoroutine(BlinkSprite(step));
                break;
            case 3:
                textHintCalcu?.SetActive(true);
                spriteHintCalcu?.SetActive(true);
                StartCoroutine(BlinkSprite(step));
                break;
        }
    }

    private void HideMeasureHint()
    {
        GameObject[] hintsToHide = {
            spriteHint, textHint, spriteHintUndo, textHintUndo,
            textHintCalcu, spriteHintCalcu, spriteHintSpell, textHintSpell
        };

        for (int i = 0; i < hintsToHide.Length; i++)
            if (hintsToHide[i] != null)
                hintsToHide[i].SetActive(false);
    }

    private IEnumerator BlinkSprite(int step)
    {
        Color color0 = spriteHintImgSpell != null ? spriteHintImgSpell.color : Color.white;
        Color color1 = spriteHintImg != null ? spriteHintImg.color : Color.white;
        Color color2 = spriteHintImgUndo != null ? spriteHintImgUndo.color : Color.white;
        Color color3 = spriteHintImgCalcu != null ? spriteHintImgCalcu.color : Color.white;

        if (step == 0)
        {
            yield return blinkDelay;
            textHintSpell?.SetActive(true);
            btnConfirmSpell?.gameObject.SetActive(true);
        }

        float elapsed = 0f;

        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                HideMeasureHint();
                yield break;
            }

            elapsed += Time.deltaTime;
            float alpha = Mathf.PingPong(elapsed, 1f);

            color0.a = alpha;
            color1.a = alpha;
            color2.a = alpha;
            color3.a = alpha;

            if (spriteHintImgSpell != null) spriteHintImgSpell.color = color0;
            if (spriteHintImg != null) spriteHintImg.color = color1;
            if (spriteHintImgUndo != null) spriteHintImgUndo.color = color2;
            if (spriteHintImgCalcu != null) spriteHintImgCalcu.color = color3;

            yield return null;
        }
    }
    #endregion

    #region Selection / Confirmation
    public void btnYes()
    {
        if (savedGame != null && savedGame.currRoom == chosenShape)
        {
            hideDiaBoxWhileMeasuring();
            ShowHint(1);
            btnConfirmSpell?.gameObject.SetActive(false);
            panelMagicScroll?.SetActive(false);
            pDiaButtons?.SetActive(true);
            lineSnapper?.gameObject.SetActive(true);

            //Display measure counter
            measureCounter.SetActive(true);

            //Update Measurements
            if (measureCounter.activeInHierarchy)
                measureCounter.GetComponent<TextMeshProUGUI>().text = lineSnapper.GetMeasuresLeftText();

            ActivateEquationForShape(chosenShape);
        }
        else
        {
            notifyWrongShape();
        }

        toggleConfirmScreen("");
    }

    private void ActivateEquationForShape(string shape)
    {
        switch (shape)
        {
            case "TRIANGLE": pEquationTriangle?.SetActive(true); break;
            case "SQUARE": pEquationSquare?.SetActive(true); break;
            case "RECTANGLE": pEquationRectangle?.SetActive(true); break;
            case "CIRCLE": pEquationCircle?.SetActive(true); break;
            case "SEMI_CIRCLE": pEquationSCircle?.SetActive(true); break;
        }
    }
    #endregion

    #region Notifications / OCR
    public void CloseNotification()
    {
        if (pNotify != null) pNotify.SetActive(false);
        if (isDoneMeasuring) Invoke(nameof(ResumeOCR), 0.1f);
    }

    public void notifyWrongShape()
    {
        if (pNotify != null) pNotify.SetActive(true);
    }

    private void ResumeOCR()
    {
        if (ocrScript != null) ocrScript.processing = false;
    }

    public void NotifyInvalidFormula()
    {
        if (ocrScript != null) ocrScript.processing = true;
        if (pNotify != null && pNotifyText != null)
        {
            pNotify.SetActive(true);
            pNotifyText.text = invalidFormulaMsg;
        }
    }

    public void NotifyMismatchedAnswer()
    {
        if (ocrScript != null) ocrScript.processing = true;
        if (pNotify != null && pNotifyText != null)
        {
            pNotify.SetActive(true);
            pNotifyText.text = mismatchedAnswerMsg;
        }
    }
    #endregion

    #region Calculator / OCR toggle
    public void ToggleCalcMode()
    {
        ocrScript?.ResetColor();
        ocrScript?.ResetVFX();

        if (fa == null) return;

        if (fa.calcMode)
        {
            if (calcBtnText != null) calcBtnText.text = "Calculate";
            fa.ExitCalc();
        }
        else
        {
            if (calcBtnText != null) calcBtnText.text = "Input Formula";
            fa.EnterCalc();
        }
    }
    #endregion

    #region Measurement & Casting Flow
    public void onCast()
    {
        if (!isDoneMeasuring)
        {
            if (lineSnapper == null) return;
            if (lineSnapper.lineCount != lineSnapper.GetMaxLinesForShape())
                return;

            DoneMeasure();
        }
        else
        {
            fa?.ResetCalcDisp();
            fa?.ResetAnalyzer();
            showDiaBoxAfterMeasuring();
            ocrScript?.ResetColor();
            ocrScript?.ResetVFX();
        }
    }

    public void OnBackspacePressed()
    {
        fa?.BackspaceInput();
        ocrScript?.ResetColor();
        ocrScript?.ResetVFX();
    }

    public void InputAnswer(float ans = 0f)
    {
        inputAnswer = ans;
        toggleDialogueBox();
        HideNewUI();
        pDiaButtons?.SetActive(false);
        StartCoroutine(SlideOCRBoard(false));
        CalcError();

        if (correctionPerc != null)
        {
            stringBuilder.Clear();
            stringBuilder.Append("Level Failed! \n Error: ");
            stringBuilder.Append(Math.Round(Math.Abs(error), 2));
            stringBuilder.Append("%");
            correctionPerc.text = stringBuilder.ToString();
            correctionPerc.gameObject.SetActive(true);
        }

        Invoke(nameof(CallCastAnimation), FILLTIMEAPROX + OCRSLIDETIME);
    }

    public void DoneMeasure()
    {
        isDoneMeasuring = true;

        //Change to back to avoid confusion
        undoText.text = undoBtnText3;

        //Turn off measure Counter
        measureCounter.SetActive(false);

        if (textFinish != null) textFinish.text = castBtnText2;

        if (GlobalVariables.level < 3)
            calcBtnObj?.SetActive(true);

        if (var1Display != null)
            var1Display.GetComponent<Text>().text = lineSnapper?.value1;
        if (var2Display != null)
            var2Display.GetComponent<Text>().text = lineSnapper?.value2;

        switch (GlobalVariables.loSelectedShape) //Match value 1 for display 2 if single line
        {
            case SHAPES.SQUARE:
            case SHAPES.CIRCLE:
            case SHAPES.SEMI_CIRCLE:
                var2Display.GetComponent<Text>().text = lineSnapper?.value1;
                break;
        }

        StartCoroutine(SlideOCRBoard(true));
        lineSnapper?.ToggleLineText();
    }

    public void UndoMeasure()
    {
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

            //Revert to Undo
            undoText.text = undoBtnText1;

            //Turn on measure Counter
            measureCounter.SetActive(true);
        }

        if (textFinish != null) textFinish.text = castBtnText1;
        if (calcBtnObj != null && calcBtnObj.activeInHierarchy)
            calcBtnObj.SetActive(false);
        isDoneMeasuring = false;
    }

    private void ToggleLineDelay()
    {
        lineSnapper?.ToggleLineText();
    }
    #endregion

    #region Error calculation and end-game
    private void CalcError()
    {
        if (spellCastEvent == null || shapeFiller == null || soundPlayer == null) return;

        float clamped = spellCastEvent.GetFillPercentage();
        shapeFiller.fillMaxValue = clamped;
        shapeFiller.isFillingActive = true;

        if (clamped > 2.0f)
            clamped = 2.0f;

        error = (1 - clamped) * 100f;

        int sfxIndex = (error == 0f) ? 2 : 1;
        float pitch = (error == 0f) ? 1f : 2f;
        soundPlayer.PlaySFX(sfxIndex, 1, pitch);
    }

    private void ToClass()
    {
        animScript?.VideoPlayerScript?.Stop();

        cp = correctionPerc != null && correctionPerc.IsActive();
        ls = lineSnapper != null && lineSnapper.gameObject.activeSelf;

        if (correctionPerc != null) correctionPerc.gameObject.SetActive(false);
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(false);

        mainCamera?.SetActive(false);
        classroomCamera?.SetActive(true);
    }

    private void ToUI()
    {
        animScript?.VideoPlayerScript?.Stop();
        animScript?.VideoPlayerScript?.PlayBGAnim();

        if (correctionPerc != null) correctionPerc.gameObject.SetActive(cp);
        if (lineSnapper != null) lineSnapper.gameObject.SetActive(ls);

        mainCamera?.SetActive(true);
        classroomCamera?.SetActive(false);
    }

    private IEnumerator DelayedCastAnimation()
    {
        //Reduce the BGM volume
        soundPlayer.setBGMVolume(soundPlayer.GetBGMVolume() / 2);

        yield return new WaitForSeconds(TRANSITIONTIME + 0.1f);

        HideNewUI();

        int state = (error > 0) ? 0 : (error < 0) ? 1 : 2;
        animScript.VideoPlayerScript.PlaySpellAnim(currentShape, state);

        yield return new WaitUntil(() => animScript.VideoPlayerScript.videoPlayer.isPrepared);

        if(error != 0f) //All default error anims are 5f len
            GlobalVariables.outroLen = 5f;

        yield return new WaitForSeconds(GlobalVariables.outroLen);

        FadeDelay();

        yield return new WaitForSeconds(1f);

        EndGameFunctions();
    }

    private void FadeDelay()
    {
        screenFadeAnimator?.SetTrigger("fadeOut");
    }

    private void EndGameFunctions()
    {
        bool isWin = error == 0f;
        GlobalVariables.playerWin = isWin;

        if (isWin)
        {
            GlobalVariables.percent = (GlobalVariables.level < 3) ? 0f : 1f;
        }
        else
        {
            GlobalVariables.percent = Mathf.Clamp01(1f - Mathf.Abs(error) * 0.01f);
        }

        GlobalVariables.gameFinished = true;
        GlobalVariables.isLOGame = true;

        SceneManager.LoadScene("LevelSelect");
    }

    private void CallCastAnimation()
    {
        screenFadeAnimator?.SetTrigger("fade");
        Invoke(nameof(ToClass), TRANSITIONTIME);
        StartCoroutine(DelayedCastAnimation());
    }
    #endregion

    #region Coroutines: movement & UI transitions
    private IEnumerator RectTransformOverTime(RectTransform rt, float duration, Vector2 endTransform)
    {
        if (rt == null) yield break;

        Vector2 startTransform = rt.anchoredPosition;
        float elapsed = 0f;
        float invDuration = 1f / duration;

        while (elapsed < duration)
        {
            float t = elapsed * invDuration;
            rt.anchoredPosition = Vector2.Lerp(startTransform, endTransform, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = endTransform;
    }

    private IEnumerator MoveOverTime(GameObject obj, float duration, Vector3 endPosition)
    {
        if (obj == null) yield break;

        Vector3 startPosition = obj.transform.position;
        float elapsed = 0f;
        float invDuration = 1f / duration;

        while (elapsed < duration)
        {
            float t = elapsed * invDuration;
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPosition;
    }

    private IEnumerator LocalScaleOverTime(GameObject obj, float duration, Vector3 endScale)
    {
        if (obj == null) yield break;

        Vector3 startScale = obj.transform.localScale;
        float elapsed = 0f;
        float invDuration = 1f / duration;

        while (elapsed < duration)
        {
            float t = elapsed * invDuration;
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = endScale;
    }

    private IEnumerator SlideOCRBoard(bool show)
    {
        if (show)
        {
            toggleDialogueBox();
            yield return dialogueWait;

            ocrInput?.SetActive(true);
            if (ocrEndTransform != null) StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, ocrEndTransform.position));
            if (rtDialogue != null) StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, ocrDialoguePos));
            if (pDialogue != null) StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, smallScale));
        }
        else
        {
            if (ocrScript != null) ocrScript.processing = true;
            if (formulaDisplay != null) formulaDisplay.SetActive(false);

            if (ocrStartTransform != null) StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, ocrStartTransform.position));
            if (rtDialogue != null) StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, origDiaRT));
            if (pDialogue != null) StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, normalScale));
        }

        yield return ocrWait;

        if (show)
        {
            if (ocrScript != null) { ocrScript.ResetColor(); ocrScript.ResetVFX(); ocrScript.processing = false; }
            if (formulaDisplay != null) formulaDisplay.SetActive(true);
            ShowHint(3);
            if (characterSay != null) characterSay.text = "";
        }
        else
        {
            if (ocrInput != null) ocrInput.SetActive(false);
            toggleDialogueBox();
        }
    }
    #endregion

    #region Problem creation & Spell classes
    public class Problem
    {
        public SHAPES problemShape;
        public float p_measure = UNUSED;
        public float s_measure = UNUSED;
        private float offX = 0, offY = 0;
        private const float LVL3XOFF = 1.75f;
        private const float LVL3YOFF = 1.75f;

        private const int minLimitXY = 3;
        private const int limitXY = 8;

        public GameBehaviour main;
        public GameObject problemObjectShape;

        private static readonly System.Random staticRand = new System.Random((int)DateTime.Now.Ticks);

        public Problem(SHAPES shape, GameBehaviour main, float x = -1, float y = -1)
        {
            this.main = main;
            this.problemShape = shape;

            if (x == -1 && y == -1)
            {
                // Intentionally minimal logging after cleanup
                GenerateRandomProblem();
            }
            else
            {
                GenerateManualProblem(x, y);
            }
        }

        private void GenerateRandomProblem()
        {
            switch (problemShape)
            {
                case SHAPES.SQUARE:
                    p_measure = staticRand.Next(minLimitXY, limitXY);
                    problemObjectShape = main.shapeGenerator.CreateSquare(Vector2.zero, p_measure);
                    break;
                case SHAPES.TRIANGLE:
                    p_measure = staticRand.Next(minLimitXY, limitXY);
                    s_measure = staticRand.Next(minLimitXY, limitXY);
                    problemObjectShape = main.shapeGenerator.CreateTriangle(Vector2.zero, p_measure, s_measure);
                    break;
                case SHAPES.CIRCLE:
                    p_measure = staticRand.Next(minLimitXY, limitXY);
                    problemObjectShape = main.shapeGenerator.CreateCircle(Vector2.zero, p_measure, false);
                    break;
                case SHAPES.RECTANGLE:
                    do
                    {
                        p_measure = staticRand.Next(minLimitXY, limitXY);
                        s_measure = staticRand.Next(minLimitXY, limitXY);
                    } while (p_measure == s_measure);
                    problemObjectShape = main.shapeGenerator.CreateRectangle(Vector2.zero, p_measure, s_measure);
                    break;
                case SHAPES.SEMI_CIRCLE:
                    p_measure = staticRand.Next(minLimitXY, limitXY);
                    problemObjectShape = main.shapeGenerator.CreateCircle(Vector2.zero, p_measure, true);
                    break;
            }
        }

        private void GenerateManualProblem(float x, float y)
        {
            p_measure = x;
            s_measure = y;

            Vector2 offset = new Vector2(offX, offY);

            switch (problemShape)
            {
                case SHAPES.SQUARE:
                    problemObjectShape = main.shapeGenerator.CreateSquare(offset, p_measure);
                    break;
                case SHAPES.TRIANGLE:
                    problemObjectShape = main.shapeGenerator.CreateTriangle(offset, p_measure, s_measure);
                    break;
                case SHAPES.CIRCLE:
                    problemObjectShape = main.shapeGenerator.CreateCircle(Vector2.zero, p_measure, false);
                    break;
                case SHAPES.RECTANGLE:
                    problemObjectShape = main.shapeGenerator.CreateRectangle(offset, p_measure, s_measure);
                    break;
                case SHAPES.SEMI_CIRCLE:
                    problemObjectShape = main.shapeGenerator.CreateCircle(Vector2.zero, p_measure, true);
                    break;
            }
        }
    }

    private void ActivateSpell(SHAPES s)
    {
        animScript?.VideoPlayerScript?.PlaySpellIntro(s);
    }

    public class SpellCastEvent
    {
        public GameBehaviour main;
        public Problem problem;

        private double p_measure = UNUSED;
        private double s_measure = UNUSED;

        public SpellCastEvent(GameBehaviour behavior, Problem prob)
        {
            main = behavior;
            problem = prob;
            p_measure = problem.p_measure;
            s_measure = problem.s_measure;
        }

        private const double HalfPI = Math.PI * 0.5;

        public float GetFillPercentage()
        {
            double result = CalculateArea();
            float compX = (float)Math.Round(result, 2);
            float compY = main.inputAnswer;

            return compY / compX;
        }

        private double CalculateArea()
        {
            switch (problem.problemShape)
            {
                case SHAPES.TRIANGLE:
                    return 0.5 * p_measure * s_measure;
                case SHAPES.CIRCLE:
                    double radius = p_measure * 0.5;
                    return Math.PI * radius * radius;
                case SHAPES.RECTANGLE:
                    return p_measure * s_measure;
                case SHAPES.SQUARE:
                    return p_measure * p_measure;
                case SHAPES.SEMI_CIRCLE:
                    double semiRadius = p_measure * 0.5;
                    return HalfPI * semiRadius * semiRadius;
                default:
                    throw new Exception("Invalid shape");
            }
        }
    }
    #endregion

    #region Wait & Problem init (merged Reset flow called from Start)
    private void StartLevelAnim()
    {
        screenFadeAnimator?.SetTrigger("fade");
        Invoke(nameof(ToUI), TRANSITIONTIME);
        Invoke(nameof(ShowNewUI), TRANSITIONTIME);
        Invoke(nameof(RemoveRoomText), TRANSITIONTIME);
        Invoke(nameof(PlayMusic), TRANSITIONTIME);
    }

    private void PlayMusic()
    {
        soundPlayer?.PlayBGM(0, 1, 0.4f);
    }

    private void HideNewUI()
    {
        GameObject[] uiElements = { hud, pDialogue, panelMagicScroll, quickMenu };
        for (int i = 0; i < uiElements.Length; i++)
            if (uiElements[i] != null) uiElements[i].SetActive(false);
    }

    private void ShowNewUI()
    {
        GameObject[] uiElements = { hud, pDialogue, panelMagicScroll, quickMenu };
        for (int i = 0; i < uiElements.Length; i++)
            if (uiElements[i] != null) uiElements[i].SetActive(true);
    }

    private void RemoveRoomText()
    {
        hud?.SetActive(false);
    }

    private void SetManualProblem(SHAPES shape, float x, float y = 1, int setSeed = -1)
    {
        currentShape = shape;
        Problem problem = new Problem(shape, this, x, y);

        double result = CalculateShapeArea(problem);
        Debug.Log($"Result: {Math.Round(result, 2)}");

        spellCastEvent = new SpellCastEvent(this, problem);
        ActivateSpell(currentShape);
    }

    private double CalculateShapeArea(Problem problem)
    {
        switch (problem.problemShape)
        {
            case SHAPES.TRIANGLE:
                return 0.5 * problem.p_measure * problem.s_measure;
            case SHAPES.CIRCLE:
                double radius = problem.p_measure * 0.5;
                return Math.PI * radius * radius;
            case SHAPES.RECTANGLE:
                return problem.p_measure * problem.s_measure;
            case SHAPES.SQUARE:
                return problem.p_measure * problem.p_measure;
            case SHAPES.SEMI_CIRCLE:
                double semiRadius = problem.p_measure * 0.5;
                return 0.5 * Math.PI * semiRadius * semiRadius;
            default:
                throw new Exception("Invalid shape");
        }
    }

    public void generateProblem()
    {
        System.Random random = new System.Random((int)DateTime.Now.Ticks);
        SHAPES randomShape = (SHAPES)random.Next(1, Enum.GetValues(typeof(SHAPES)).Length);

        currentShape = randomShape;
        Problem problem = new Problem(randomShape, this);

        double result = CalculateShapeArea(problem);
        spellCastEvent = new SpellCastEvent(this, problem);
        ActivateSpell(currentShape);
    }

    private void InitFillShape()
    {
        if (shapeFiller != null && spellCastEvent?.problem?.problemObjectShape != null)
            shapeFiller.InitializeFill(spellCastEvent.problem.problemObjectShape, Color.green, 0.5f, 0f);
    }

    private void InitProblem()
    {
        float[] measures = GlobalVariables.GetLOProblemMeasures();

        SetManualProblem(GlobalVariables.loSelectedShape, measures[0], measures[1]);
    }
    #endregion
}
