using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.IO;



public class GameBehaviour : MonoBehaviour
{
    const int UNUSED = -1;

  
    SHAPES currentShape;
    public SpellCastEvent spellCastEvent;
    TMP_Dropdown dropdown;
    public TMP_Text text;
    TMP_Text manaMeasure;
    TMP_Text correctionPerc;
    public Slider slider;

    Button yesButton;
    Button noButton;
    Button confirmMeasurement;
    Button restart;
    Button undo;
    Button quit;
    int currentOptionSelected;

    public ShapeGenerator shapeGenerator;
    public ShapeFiller shapeFiller;

    public LineSnapper lineSnapper;

    //NEW ADDITIONS: DELETE IN CASE EVERYTHING BREAKS

    private Button changeCameraButton;
    private bool isUICamera = true;
    private bool y, n, m, s, cm, cp, ls, u, t, d, r, q; //Activity states of UI components
    private GameObject mainCamera, classroomCamera;
    private Material uiMaterial, classroomMaterial;
    private Animator screenFadeAnimator;
    private const float TRANSITIONTIME = 0.4f, FILLTIMEAPROX = 1f, STARTDELAY = 3.0f;
    private float ENDDELAY = 5.0f, SPELLDELAY = 2.0f;

    private AnimScript animScript;
    private bool STARTUP = true;
    public float error = 100f;
    private bool isQuit = false;
    private const float TRANSITIONDELAY = 0.5f;
    private List<TMP_Dropdown.OptionData> defaultOptions; //Use for reset
    private const string correctShapePropmt = "Tama na ba ang shape na pinili?";

    //----------------------------------------------
    //////////Copied from old repo
    //my changes in case mag boom boom lahat
    private GameData savedGame;
    private SaveLoadController saverLoader = new SaveLoadController();
    private string savePath;

    private RectTransform rtDialogue;
    private RectTransform rtSlider;

    private bool isDoneMeasuring;
    //panels na toggable, containers lang
    public GameObject hud;
    public GameObject quickMenu;
    public GameObject panelMagicScroll;
    public GameObject pConfirm;
    public GameObject pLowerScroll;
    public GameObject pNotify;
    private GameObject pDialogue;
    private GameObject pDiaButtons;
    private GameObject pSlider;

    private Text pNotifyText;
    private Text textAns;
    private Text textAnsSC;
    private Text  textAnsRect;
    private Text   textAnsCir;
    private Text   textAnsSqr;
    private Text   textAnsTri;
    private Text manaReq;
    private Text characterSay;
    private Text textFinish;

    public Text confirmText;
    public Text textHUD;
    public GameObject pEquationTriangle;
    public GameObject pEquationSquare;
    public GameObject pEquationRectangle;
    public GameObject pEquationSCircle;
    public GameObject pEquationCircle;

    

    private string currentShapeToSolve;
    private string chosenShape;


    private const string castBtnText1 = "Cast Spell";
    private const string castBtnText2 = "Redo Input";
    private const float DIALOGUESLIDETIME = 0.25f;
    private const float OCRSLIDETIME = 0.35f;
    private DrawingAndOCRManagerScript ocrScript;
    private FormulaAnalyzer fa;
    private Vector2 origDiaRT;
    // References to OCR input that will replace the slider
    public GameObject ocrInput;
    public GameObject formulaDisplay;
    public GameObject rightStartTransObj, rightEndTransObj;
    public GameObject formulaAnalyzerObj;
    private float inputAnswer = 0f; //Float field for entering answer via InputAnswer()

    //----------------------------------------------


    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        savedGame = saverLoader.loadGame(savePath);

        currentShape = SHAPES.NONE;
        screenFadeAnimator = GameObject.Find("ScreenFade").GetComponent<Animator>();
        animScript = GameObject.Find("AnimHolder").GetComponent<AnimScript>();
        // quit = GameObject.Find("Quit").GetComponent<Button>();
        defaultOptions = new List<TMP_Dropdown.OptionData>();

        //Get OCR script
        ocrScript = ocrInput.transform.Find("DrawingAndOCRManager").GetComponent<DrawingAndOCRManagerScript>();

        //Get FA script
        fa = formulaAnalyzerObj.GetComponent<FormulaAnalyzer>();
    }

    /*
     * 
     * PivotToCenter = Pivot + SizeX/2 + SizeY/2
     * 
     * 
     */

    // _init() _ready()

    void Reset()
    {
        //Cleanup possible temporary vfx and clones
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Temporary"))
        {
            Destroy(go);
        }

        // dropdown.gameObject.SetActive(true);
        // dropdown.onValueChanged.RemoveAllListeners(); // Stop listening
        // CopyOptions(dropdown.options, defaultOptions); // Reset options
        // dropdown.value = 0;
        text.text = "";
        manaMeasure.text = "0";
        // dropdown.RefreshShownValue();
        // dropdown.onValueChanged.AddListener(OnOptionSelect); //Start Listening
        // UnityEngine.Debug.Log("Dropdown Len: " + dropdown.options.Count);

        currentShape = SHAPES.NONE;

        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        manaMeasure.gameObject.SetActive(false);
        slider.gameObject.SetActive(false); slider.value = 0f;
        confirmMeasurement.gameObject.SetActive(false);
        correctionPerc.gameObject.SetActive(false);
        if(!STARTUP) // If not first run
            Destroy(this.spellCastEvent.problem.problemObjectShape);
        lineSnapper.gameObject.SetActive(false);
        undo.gameObject.SetActive(false);

        lineSnapper.OnUndoPressed();
        lineSnapper.OnUndoPressed();

        //RUN SCREEN FADE IN FOR RESTART OF LEVEL
        if(!STARTUP)
            screenFadeAnimator.SetTrigger("fadeIn");

        //Cleanup previous spell objects
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Spell"))
        {
            Destroy(go);
        }

        //Start with watching the spawn anim again in reset
        ToClass();
        DeactivateChangeCamera();
        Invoke(nameof(StartLevelAnim), STARTDELAY);

        // TODO: ADD SOMETHING THAT ALLOWS SWITCHING BETWEEN RANDOM PROBLEM AND MANUAL PROBLEM MAYBE
        Invoke(nameof(InitProblem), 0.1f); // Add delay to prevent object from getting nuked by cleanup
        //this.SetManualProblem(SHAPES.SEMI_CIRCLE, 6, 6, 100);

        //Init fill shape
        Invoke(nameof(InitFillShape), 0.2f);
    }

    private void InitFillShape()
    {
        shapeFiller.InitializeFill(spellCastEvent.problem.problemObjectShape, Color.green, 0.5f, 0f);
    }

    private void InitProblem()
    {
        // TODO: Measurements should read from array of possible answers per level, circles and semis need modified list for solvable problems
        float measure1 = 0f, measure2 = 0f;
        int numMeasures = 0;
        switch (GlobalVariables.level)
        {
            case 0:
            case 1:
                if(GlobalVariables.loSelectedShape == SHAPES.CIRCLE || GlobalVariables.loSelectedShape == SHAPES.SEMI_CIRCLE)
                {
                    // If Circle/SemiCircle, use different measures array
                    numMeasures = GlobalVariables.loCircleMeasures1.Length;
                    measure1 = GlobalVariables.loCircleMeasures1[UnityEngine.Random.Range(0, numMeasures)];
                    measure2 = GlobalVariables.loCircleMeasures1[UnityEngine.Random.Range(0, numMeasures)];
                    break;
                }
                numMeasures = GlobalVariables.loMeasures1.Length;
                measure1 = GlobalVariables.loMeasures1[UnityEngine.Random.Range(0, numMeasures)];
                measure2 = GlobalVariables.loMeasures1[UnityEngine.Random.Range(0, numMeasures)];
                break;
            case 2:
                if (GlobalVariables.loSelectedShape == SHAPES.CIRCLE || GlobalVariables.loSelectedShape == SHAPES.SEMI_CIRCLE)
                {
                    // If Circle/SemiCircle, use different measures array
                    numMeasures = GlobalVariables.loCircleMeasures2.Length;
                    measure1 = GlobalVariables.loCircleMeasures2[UnityEngine.Random.Range(0, numMeasures)];
                    measure2 = GlobalVariables.loCircleMeasures2[UnityEngine.Random.Range(0, numMeasures)];
                    break;
                }
                numMeasures = GlobalVariables.loMeasures2.Length;
                measure1 = GlobalVariables.loMeasures2[UnityEngine.Random.Range(0, numMeasures)];
                measure2 = GlobalVariables.loMeasures2[UnityEngine.Random.Range(0, numMeasures)];
                break;
            case 3:
                if (GlobalVariables.loSelectedShape == SHAPES.CIRCLE || GlobalVariables.loSelectedShape == SHAPES.SEMI_CIRCLE)
                {
                    // If Circle/SemiCircle, use different measures array
                    numMeasures = GlobalVariables.loCircleMeasures3.Length;
                    measure1 = GlobalVariables.loCircleMeasures3[UnityEngine.Random.Range(0, numMeasures)];
                    measure2 = GlobalVariables.loCircleMeasures3[UnityEngine.Random.Range(0, numMeasures)];
                    break;
                }
                numMeasures = GlobalVariables.loMeasures3.Length;
                measure1 = GlobalVariables.loMeasures3[UnityEngine.Random.Range(0, numMeasures)];
                measure2 = GlobalVariables.loMeasures3[UnityEngine.Random.Range(0, numMeasures)];
                break;
        }

        //Make sure mearure2 not same as 1 if rectangle, simplify into subracting 1 from either 1 or 2 at random
        if (GlobalVariables.loSelectedShape == SHAPES.RECTANGLE && measure1 == measure2)
        {
            int coinFlip = UnityEngine.Random.Range(0, 1);
            if (coinFlip == 0)
                measure1--;
            else if (coinFlip == 1)
                measure2--;
        }

        UnityEngine.Debug.Log("Measure1: " + measure1 + " | Measure2: " + measure2);
        SetManualProblem(GlobalVariables.loSelectedShape, measure1, measure2);
    }

    // Temp fix for options
    private void CopyOptions(List<TMP_Dropdown.OptionData> target, List<TMP_Dropdown.OptionData> source)
    {
        target.Clear();
        foreach (TMP_Dropdown.OptionData sourceOpt in source)
        {
            target.Add(sourceOpt);
        }
    }

//--------------------------------------------------------
/////////////////added from old repo
// just button events
    public void onRestart(){
        screenFadeAnimator.SetTrigger("sceneOut");
        Invoke(nameof(LoadSceneDelay), TRANSITIONDELAY);
    }
    private void LoadSceneDelay()
    {
        SceneManager.LoadScene("GameLevelScene_v1"); // Reload scene to avoid problems (Lazy and slightly slow but eh...)
    }

    public void onQuit()
    {
        error = 100f; //Prevent accidental saving due to 0f error
        screenFadeAnimator.SetTrigger("sceneOut");
        Invoke(nameof(EndGameFunctions), TRANSITIONDELAY); //Quit to LS
    }

    public void onUndo()
    {
        lineSnapper.OnUndoPressed();
    }

    public void toggleConfirmScreen(string spell){
        if (pConfirm != null){
            pConfirm.SetActive(!pConfirm.activeInHierarchy);
        }

        if (pConfirm.activeInHierarchy){
            confirmText.text = "[ "+spell+" ]";
        }
    }

    public void toggleMagicScroll(){
        if (pLowerScroll != null){
            pLowerScroll.SetActive(!pLowerScroll.activeInHierarchy);
        }
    }
    public void chooseSquare(){
        chosenShape = "SQUARE";
        toggleConfirmScreen(chosenShape);
    }

    public void chooseSemiCircle(){
        chosenShape = "SEMI_CIRCLE";
        toggleConfirmScreen(chosenShape);
    }

    public void chooseCircle(){
        chosenShape = "CIRCLE";
        toggleConfirmScreen(chosenShape);
    }

    public void chooseRectangle(){
        chosenShape = "RECTANGLE";
        toggleConfirmScreen(chosenShape);
    }

    public void chooseTriangle(){
        chosenShape = "TRIANGLE";
        toggleConfirmScreen(chosenShape);
    }

    public void btnNo()
    {
        toggleConfirmScreen("");
        // pConfirm.SetActive(false);
    }


    public void toggleSlider(){
        //just move to right
        rtSlider.anchoredPosition = new Vector2(-525f,rtSlider.anchoredPosition.y);
    }

    public void toggleDialogueBox()
    {
        Vector2 RTAWAYTRANS = new (600f, -100f);
        Vector2 PDIAAWAYTRANS = new (239, 150);
        Vector2 RTONTRANS = new (600f, 100f);
        Vector2 PDIAONTRANS = new (239, 123);

        if (rtDialogue.anchoredPosition.y == 100)
        {
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, RTAWAYTRANS));
            StartCoroutine(RectTransformOverTime(pDiaButtons.GetComponent<RectTransform>(), DIALOGUESLIDETIME, PDIAAWAYTRANS));
            // pDialogue.y = -59;
        }
        else{
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, RTONTRANS));
            StartCoroutine(RectTransformOverTime(pDiaButtons.GetComponent<RectTransform>(), DIALOGUESLIDETIME, PDIAONTRANS));
            // pDialogue.y = 100; //tago
        }
    }
    private IEnumerator RectTransformOverTime(RectTransform rt, float duration, Vector2 endTransform)
    {
        var startTransform = rt.anchoredPosition;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            rt.anchoredPosition = Vector2.Lerp(startTransform, endTransform, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rt.anchoredPosition = endTransform;
    }

    public void btnYes(){
            
            //IF SHAPE IS CORRECT:
            // SHAPES.SQUARE 
            // if ((int)this.spellCastEvent.problem.problemShape == currentOptionSelected + 1)
            // {
            //     //TODO: just read from the JSON file to figure out the current shape needed due to the room you entered - done eme
            UnityEngine.Debug.Log("chosenshape -> "+ chosenShape);
            UnityEngine.Debug.Log("savedGame.currRoom -> "+ savedGame.currRoom);


            //hide the spellbook after choosing
            
            if (savedGame.currRoom == chosenShape){
                panelMagicScroll.SetActive(false);
                //show na den undo and cast buttons
                pDiaButtons.SetActive(true);
                toggleDialogueBox();
                characterSay.text = "Kailangan ko naman ngayong sukatin ang hugis gamit ang aking daliri!";
                manaReq.text = "Katumbas na Mana";
                manaMeasure.gameObject.SetActive(true);
                //slider.gameObject.SetActive(true);
                //confirmMeasurement.gameObject.SetActive(true);
                //correctionPerc.gameObject.SetActive(true);
                slider.gameObject.SetActive(false); //test

                // dropdown.gameObject.SetActive(false);
                lineSnapper.gameObject.SetActive(true);
                undo.gameObject.SetActive(true);


                //show correct casting equation
                //not entering correctly
                if (chosenShape == "TRIANGLE"){
                    pEquationTriangle.SetActive(true);
                    
                }
                else if (chosenShape == "SQUARE"){
                    pEquationSquare.SetActive(true);
                    // textAns = GameObject.Find("textAnsTri").GetComponent<Text>();
                    // textAns = GameObject.Find("textAnsSqr").GetComponent<Text>();
                    // textAns = GameObject.Find("textAnsRect").GetComponent<Text>();
                    // textAns = GameObject.Find("textAnsCir").GetComponent<Text>();
                    // textAns = GameObject.Find("textAnsSC").GetComponent<Text>();

                }
                else if (chosenShape == "RECTANGLE"){
                    pEquationRectangle.SetActive(true);
                }
                else if (chosenShape == "CIRCLE"){
                    pEquationCircle.SetActive(true);
                }
                else if (chosenShape == "SEMI_CIRCLE"){
                    pEquationSCircle.SetActive(true);
                }
                //am too tired to figure out lol why not working switch
                // switch(chosenShape){
                //     case "TRIANGLE":
                //         UnityEngine.Debug.Log("SHOWING TRIANGLE");
                //         pEquationTriangle.SetActive(true);
                //         break;
                //     case "SQUARE":
                //         UnityEngine.Debug.Log("SHOWING SQUARE");
                //         pEquationSquare.SetActive(true);
                //         break;
                //     case "CIRCLE":
                //         pEquationCircle.SetActive(true);
                //         break;
                //     case "SEMI_CIRCLE":
                //         pEquationSCircle.SetActive(true);
                //         break;
                //     case "RECTANGLE":
                //         pEquationRectangle.SetActive(true);
                //         break;
                // }
                //ensure only 1 active exists
            }
            else    //IF SHAPE CHOSEN IS WRONGG:
            {
                notifyWrongShape();
                // text.gameObject.SetActive(true); //Reactivate if not active
                // text.text = "Ang shape na pinili ay mali. Subukan ulit.";

                //Play shake head anim
                animScript.playerScript.BadTrace();
            }
            toggleConfirmScreen("");
    }

    public void notifyWrongShape()
    {
        if (pNotify != null){
            pNotify.SetActive(!pNotify.activeInHierarchy);
        }
    }

    public void onCast()
    {
        //added NTS
        
        if (!isDoneMeasuring)    //pag mag sslider plang
        {
            // rTransform.anchoredPosition = new Vector2(rTransform.anchoredPosition.x, 100);  //it broke idk y

            //Check if Lines is maxed out first
            if(lineSnapper.lineCount != lineSnapper.GetMaxLinesForShape())
            {
                // TODO: DISPLAY POPUP OF NOT DONE MEASURING YET

                //QUIT BECAUSE BUTTON DOES NOTHING
                return;
            }

            //ALSO TODO: FIX LINE LENGTHS DISPLAY

            DoneMeasure();
        }
        else{ //Reset the OCRInput board
            fa.ResetAnalyzer();
        }
    }

    public void InputAnswer(float ans = 0f) //Sends final answer
    {
        inputAnswer = ans;
        
        toggleDialogueBox(); //hide                  

        //DisableNewUI
        HideNewUI();

        //Disable OCR board and formulaDisplay
        StartCoroutine(SlideOCRBoard(false));

        CalcError();

        correctionPerc.text = "Incorrectness: " + Math.Abs(error) + "%";
        //shapeFiller.InitializeFill(spellCastEvent.problem.problemObjectShape, Color.green, 0.5f, spellCastEvent.GetFillPercentage());

        correctionPerc.gameObject.SetActive(true); //Show error

        Invoke(nameof(CallCastAnimation), FILLTIMEAPROX + OCRSLIDETIME);
    }

    public void DoneMeasure()
    {
        isDoneMeasuring = true;
        textFinish.text = castBtnText2;

        //OLD SLIDER CODE
        //toggleSlider();
        //slider.gameObject.SetActive(true);

        //NEW OCR SHOW CODE
        StartCoroutine(SlideOCRBoard(true));
        lineSnapper.ToggleLineText(); //Toggle off
    }
    public void UndoMeasure()
    {
        //Hide OCR Board
        if (isDoneMeasuring)
        {
            StartCoroutine(SlideOCRBoard(false));
            Invoke(nameof(ToggleLineDelay), OCRSLIDETIME); //Toggle on
        }

        textFinish.text = castBtnText1;
        isDoneMeasuring = false;
    }
    private void ToggleLineDelay()
    {
        lineSnapper.ToggleLineText();
    }

    private IEnumerator SlideOCRBoard(bool show) //Boolean determins if showing or hiding
    {
        if (show)
        {
            toggleDialogueBox(); //Show

            yield return new WaitForSeconds(DIALOGUESLIDETIME); //Wait for Dialogue Toggle
            
            ocrInput.SetActive(true); //Activate the board
            StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, rightEndTransObj.transform.position));

            //Slide and Scale Dialogue Box
            StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, new(160f, 100f)));
            StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, new(0.7f, 0.7f, 0.7f)));
        }
        else if (!show)
        {
            ocrScript.processing = true; //Stop accepting input

            formulaDisplay.SetActive(false); //Hide OCR input Display

            StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, rightStartTransObj.transform.position));

            //Slide Dialogue Box
            StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, origDiaRT));
            StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, new(1f, 1f, 1f))); //Scale to Normal
        }

        yield return new WaitForSeconds(OCRSLIDETIME); //Wait until OCR board stops moving

        if (show)
        {
            //Reset Board
            ocrScript.ResetColor();
            ocrScript.ResetVFX();

            ocrScript.processing = false; //Start accepting input
            formulaDisplay.SetActive(true); //Show OCR input Display
        }
        else if (!show)
        {
            ocrInput.SetActive(false); //Deactivate the board once off screen
            toggleDialogueBox(); //Hide
        }
    }

    private IEnumerator MoveOverTime(GameObject obj, float duration, Vector3 endPosition)
    {
        var startPosition = obj.transform.position;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            obj.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPosition;
    }

    private IEnumerator LocalScaleOverTime(GameObject obj, float duration, Vector3 endScale)
    {
        var startScale = obj.transform.localScale;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            var t = elapsed / duration;
            obj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = endScale;
    }

    ///------------------------- edn

    void Start()
    {
        //copied from old repo

        characterSay = GameObject.Find("characterSay")?.GetComponent<Text>();
        manaReq = GameObject.Find("ManaRequired").GetComponent<Text>();
        textFinish= GameObject.Find("textFinish").GetComponent<Text>();
        textFinish.text = castBtnText1;

        isDoneMeasuring = false;

        pDiaButtons = GameObject.Find("pDiaButtons");
        pSlider = GameObject.Find("pSlider");

        textAnsSC = GameObject.Find("textAnsSC")?.GetComponent<Text>();
        textAnsRect = GameObject.Find("textAnsRect")?.GetComponent<Text>();
        textAnsCir = GameObject.Find("textAnsCir")?.GetComponent<Text>();
        textAnsSqr = GameObject.Find("textAnsSqr")?.GetComponent<Text>();
        textAnsTri = GameObject.Find("textAnsTri")?.GetComponent<Text>();

        pDialogue = GameObject.Find("PanelCasting");
        rtDialogue = pDialogue.GetComponent<RectTransform>();
        origDiaRT = rtDialogue.anchoredPosition; //Save original pos
        rtSlider = pSlider.GetComponent<RectTransform>();


        savedGame =  saverLoader.loadGame(Path.Combine(Application.persistentDataPath, "saveData.json"));
        currentShapeToSolve = savedGame.currRoom;
        textHUD.text = savedGame.currRoom + " ROOM";


        pDialogue.SetActive(true);
        pConfirm.SetActive(false);  //hide first
        pLowerScroll.SetActive(true); //show muna para less empty space
        pNotify.SetActive(false);
    
        pEquationTriangle.SetActive(false);
        pEquationSquare.SetActive(false);
        pEquationRectangle.SetActive(false);
        pEquationSCircle.SetActive(false);
        pEquationCircle.SetActive(false);

        //hide lang muna
        pDiaButtons.SetActive(false);
        manaReq.text = "";//clear


        // end

        //////////////////////////////////////


        if (STARTUP)
        {
            Instantiate(animScript.transitionVFX, GameObject.Find("SpellOrigin").transform); //Spawn early for smoothness on start
            screenFadeAnimator.SetTrigger("fadeIn");

            //Hide new UI at start
            HideNewUI();

            //Except Hud
            hud.SetActive(true);
        }

        //UnityEngine.Debug.Log(dropdown);


        //removed listeners
        // restart = GameObject.Find("Restart").GetComponent<Button>();
        // restart.onClick.AddListener(() =>
        // {
        //     SceneManager.LoadScene("GameLevelScene_v1"); // Reload scene to avoid problems (Lazy and slightly slow but eh...)
        // });
        // quit.onClick.AddListener(() => {
        //     error = 100f; //Prevent accidental saving due to 0f error
        //     screenFadeAnimator.SetTrigger("sceneOut");
        //     Invoke(nameof(EndGameFunctions), TRANSITIONDELAY); //Quit to LS
        // });

        // dropdown = GameObject.Find("Dropdown").GetComponent<TMP_Dropdown>();
        // CopyOptions(defaultOptions, dropdown.options); //Store these for reset
        // dropdown.value = 0;
        // dropdown.RefreshShownValue();
        // dropdown.onValueChanged.AddListener(OnOptionSelect);

        text = GameObject.Find("DialoguePrompt").GetComponent<TMP_Text>();
        text.text = "";
        manaMeasure = GameObject.Find("ManaValue").GetComponent<TMP_Text>();
        manaMeasure.text = "0";

        slider = GameObject.Find("Slider").GetComponent<Slider>();

        yesButton = GameObject.Find("ConfirmYes").GetComponent<Button>();
        noButton = GameObject.Find("ConfirmNo").GetComponent<Button>();
        confirmMeasurement = GameObject.Find("ConfirmMeasurement").GetComponent<Button>();
        correctionPerc = GameObject.Find("ManaFillCorrectPerc").GetComponent<TMP_Text>();
        lineSnapper = GameObject.Find("Gesture").GetComponent<LineSnapper>();
        // undo = GameObject.Find("Undo").GetComponent<Button>();
        undo = GameObject.Find("btnUndo").GetComponent<Button>();
        //CHANGED: Undo-> btnUndo

        //NEW ADDITIONS: DELETE IN CASE EVERYTHING BREAKS

        changeCameraButton = GameObject.Find("ChangeCamera").GetComponent<Button>();

        mainCamera = GameObject.Find("Main Camera");
        mainCamera.SetActive(false);
        classroomCamera = GameObject.Find("ClassroomCamera");
        classroomCamera.SetActive(true);

        lineSnapper.animScript = this.animScript;
        //Set grid sizes based on difficulty ?? Not really working
        switch (GlobalVariables.level)
        {
            case 0:
            case 1: //Needs to be changed to whole numbers later maybe
                lineSnapper.gridSystem.majorGridSize = 2.0f;
                lineSnapper.gridSystem.minorGridSize = 1.0f;
                break;
            case 2:
                lineSnapper.gridSystem.majorGridSize = 2.0f;
                lineSnapper.gridSystem.minorGridSize = 1.0f;
                break;
            case 3:
                lineSnapper.gridSystem.majorGridSize = 1.0f;
                lineSnapper.gridSystem.minorGridSize = 0.5f;
                break;
            default:
                UnityEngine.Debug.Log("ERROR: Invalid level(Gamebehaviour)");
                break;
        }
        UnityEngine.Debug.Log("Level: " + GlobalVariables.level);

        uiMaterial = Resources.Load<Material>("Materials/UI_Material");
        classroomMaterial = Resources.Load<Material>("Materials/ClassroomScreenMaterial");

        //----------------------------------------------

        StartCoroutine(WaitForComponent());
       

       /* UnityEngine.Debug.Log(yesButton);
        UnityEngine.Debug.Log(noButton);*/

        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        manaMeasure.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);
        confirmMeasurement.gameObject.SetActive(false);
        correctionPerc.gameObject.SetActive(false);
        lineSnapper.gameObject.SetActive(false);
        undo.gameObject.SetActive(false);

        undo.onClick.AddListener(() =>
        {
            lineSnapper.OnUndoPressed();
        });

        slider.onValueChanged.AddListener((value) =>
        {
            manaMeasure.text = value.ToString();
            if (chosenShape == "TRIANGLE"){
                textAnsTri.text = manaMeasure.text;
            }
            else if (chosenShape == "SQUARE"){
                textAnsSqr.text = manaMeasure.text;
            }
            else if (chosenShape == "RECTANGLE"){
                textAnsRect.text = manaMeasure.text;
            }
            else if (chosenShape == "CIRCLE"){
                textAnsCir.text = manaMeasure.text;
            }
            else if (chosenShape == "SEMI_CIRCLE"){
                textAnsSC.text = manaMeasure.text;
            }

            // TODO: make slider rounding change based on level?
            slider.value = Mathf.Round(value * 10f) / 10f;  // Rounds to nearest 0.1

            //Change fill value
            shapeFiller.fillMaxValue = spellCastEvent.GetFillPercentage();

            //Check if area match
            CalcError();
            if (error == 0f)
                shapeFiller.isPerfectMatch = true;
            else 
                shapeFiller.isPerfectMatch = false;

            shapeFiller.isFillingActive = true;
        });



    //CHANGES: removed listeners
        // yesButton.onClick.AddListener(() =>
        // {
        //     yesButton.gameObject.SetActive(false);
        //     noButton.gameObject.SetActive(false);
            
        //     if ((int)this.spellCastEvent.problem.problemShape == currentOptionSelected + 1)
        //     {
        //         text.text = GlobalVariables.ShapeFormulaText(this.spellCastEvent.problem.problemShape);

        //         manaMeasure.gameObject.SetActive(true);
        //         //slider.gameObject.SetActive(true);
        //         //confirmMeasurement.gameObject.SetActive(true);
        //         //correctionPerc.gameObject.SetActive(true);
        //         // dropdown.gameObject.SetActive(false);
        //         lineSnapper.gameObject.SetActive(true);
        //         undo.gameObject.SetActive(true);
        //         text.gameObject.SetActive(true); //Reactivate if not active
        //     }
        //     else
        //     {
        //         text.gameObject.SetActive(true); //Reactivate if not active
        //         text.text = "Ang shape na pinili ay mali. Subukan ulit.";

        //         //Play shake head anim
        //         animScript.playerScript.BadTrace();
        //     }
        // });

        // confirmMeasurement.onClick.AddListener(() =>
        // {
        //     CalcError();
        //     correctionPerc.text = "Incorrectness: " +  Math.Abs(error) + "%";
        //     //shapeFiller.InitializeFill(spellCastEvent.problem.problemObjectShape, Color.green, 0.5f, spellCastEvent.GetFillPercentage());

        //     correctionPerc.gameObject.SetActive(true); //Show error

        //     Invoke(nameof(CallCastAnimation), FILLTIMEAPROX);
        // });

        // noButton.onClick.AddListener(() =>
        // {
        //     yesButton.gameObject.SetActive(false);
        //     noButton.gameObject.SetActive(false);
        // });

        // //NEW ADDITIONS: DELETE IN CASE EVERYTHING BREAKS

        changeCameraButton.onClick.AddListener(() =>
        {
            //Deactivate all UI if in ui, store previous states first though, then switch to classroom cam
            if (isUICamera)
            {
                screenFadeAnimator.SetTrigger("fade");
                Invoke(nameof(ToClass), TRANSITIONTIME);
            }
            else if (!isUICamera) // Reactivate UI if not in UI, switch to main cam
            {
                screenFadeAnimator.SetTrigger("fade");
                Invoke(nameof(ToUI), TRANSITIONTIME);
            }
        });

        //----------------------------------------------

    }


    //NEW ADDITIONS: DELETE IN CASE EVERYTHING BREAKS
    private void CalcError()
    {
        float clamped = spellCastEvent.GetFillPercentage();
        shapeFiller.fillMaxValue = clamped; //Fill Shape when input
        shapeFiller.isFillingActive = true; //Start filling
        if (clamped > 2.0f)
            clamped = 2.0f;

        //TODO: Adjust error tolerance based on level?
        error = ((1 - clamped) * 100); //Get error float
    }

    public void SetCastingState(bool state)
    {
        slider.gameObject.SetActive(state);
        confirmMeasurement.gameObject.SetActive(state);
    }

    private void ToClass()
    {
        y = yesButton.IsActive();
        n = noButton.IsActive();
        m = manaMeasure.IsActive();
        s = slider.IsActive();
        cm = confirmMeasurement.IsActive();
        cp = correctionPerc.IsActive();
        ls = lineSnapper.gameObject.activeSelf;
        u = undo.IsActive();
        t = text.IsActive();
        // d = dropdown.IsActive();
        // r = restart.IsActive();
        // q = quit.IsActive();

        yesButton.gameObject.SetActive(false);
        noButton.gameObject.SetActive(false);
        manaMeasure.gameObject.SetActive(false);
        slider.gameObject.SetActive(false);
        confirmMeasurement.gameObject.SetActive(false);
        correctionPerc.gameObject.SetActive(false);
        lineSnapper.gameObject.SetActive(false);
        undo.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        // dropdown.gameObject.SetActive(false);
        // restart.gameObject.SetActive(false);
        // quit.gameObject.SetActive(false);

        mainCamera.SetActive(false);
        classroomCamera.SetActive(true);
        changeCameraButton.GetComponent<Image>().material = uiMaterial; //Change material to UI mat

        isUICamera = false;
    }

    private void ToUI()
    {
        yesButton.gameObject.SetActive(y);
        noButton.gameObject.SetActive(n);
        manaMeasure.gameObject.SetActive(m);
        slider.gameObject.SetActive(s);
        confirmMeasurement.gameObject.SetActive(cm);
        correctionPerc.gameObject.SetActive(cp);
        lineSnapper.gameObject.SetActive(ls);
        undo.gameObject.SetActive(u);
        text.gameObject.SetActive(t);
        // dropdown.gameObject.SetActive(d);
        // restart.gameObject.SetActive(r);
        // quit.gameObject.SetActive(q);

        mainCamera.SetActive(true);
        classroomCamera.SetActive(false);
        changeCameraButton.GetComponent<Image>().material = classroomMaterial; //Change material to classroom mat

        isUICamera = true;
    }

    private int SendShapeToPlayer(SHAPES s)
    {
        switch (s)
        {
            case SHAPES.SQUARE:
                return 0;
            case SHAPES.RECTANGLE:
                return 1;
            case SHAPES.TRIANGLE:
                return 2;
            case SHAPES.CIRCLE:
                return 3;
            case SHAPES.SEMI_CIRCLE:
                return 4;
            default:
                return -1;
        }
    }

    private void DelayedCastAnimation()
    {
        //Hide UI elems
        HideNewUI();
        
        if (error == 0f)
        {
            animScript.playerScript.GoodCast(SendShapeToPlayer(currentShape));
            Invoke(nameof(DelayedSpellAnimation), SPELLDELAY);
            //Call function to display a level complete/retry screen

            //TODO: ADD END SCREENN, Base delay from sd + ENDDELAY - 3.5f maybe?
            UnityEngine.Debug.Log("LEVEL COMPLETE!!!");
            float sd = animScript.currentScript.GetSpellDuration();
            Invoke(nameof(FadeDelay), sd + ENDDELAY - 2f);

            Invoke(nameof(EndGameFunctions), sd + ENDDELAY);
            return; // Early return
        }          
        if (error < 0f)
            animScript.playerScript.OverCast();
        if (error > 0f)
            animScript.playerScript.UnderCast();

        //TODO: ADD END SCREEN, Base delay from ENDDELAY - 2.5f maybe?
        UnityEngine.Debug.Log("LEVEL FAILED!!!");
        Invoke(nameof(FadeDelay), ENDDELAY - 1f);

        //Call function to do end of game stuff
        Invoke(nameof(EndGameFunctions), ENDDELAY); // Shorter delay due to no anim
    }

    private void FadeDelay()
    {
        screenFadeAnimator.SetTrigger("fadeOut");
    }

    private void DelayedSpellAnimation()
    {
        animScript.CastSpell();
    }

    private void EndGameFunctions() //Function for saving data to save maybe? Also transitioning back to level select
    {
        // Save requisite data
        if(error == 0f)
        {
            GlobalVariables.playerWin = true;
            if(GlobalVariables.level < 3) //Reset on level up
                GlobalVariables.percent = 0f;
            else //Show 100%
                GlobalVariables.percent = 1f;
        }  
        else
        {
            GlobalVariables.playerWin = false;
            GlobalVariables.percent = 1-error;
        }

        if (!isQuit) // Only activate flags if not quitting
        {
            GlobalVariables.gameFinished = true; //Set flag to save data
            GlobalVariables.isLOGame = true; //Flag game as LO game
        }
        
        //TRANSITION TO LEVEL SELECT SCREEN AGAIN
        SceneManager.LoadScene("LevelSelect");
    }

    private void DeactivateChangeCamera()
    {
        //Require reset dialogue or script to reactivate
        changeCameraButton.enabled = false;
        changeCameraButton.GetComponent<Image>().enabled = false;
    }

    private void ActivateChangeCamera()
    {
        changeCameraButton.enabled = true;
        changeCameraButton.GetComponent<Image>().enabled = true;
    }

    //TODO: Maybe make a method that will be called to activate an end screen???

    private void CallCastAnimation()
    {
        screenFadeAnimator.SetTrigger("fade");

        Invoke(nameof(DeactivateChangeCamera), TRANSITIONTIME);
        Invoke(nameof(ToClass), TRANSITIONTIME);
        Invoke(nameof(DelayedCastAnimation), TRANSITIONTIME + 0.1f);
    }

    //----------------------------------------------


    // no need for a variable select to hold

    public enum SHAPES {

        NONE,
        TRIANGLE,
        SQUARE,
        RECTANGLE,
        CIRCLE,
        SEMI_CIRCLE,
    }

  


    // Update is called once per frame
    void Update()
    {

        //UnityEngine.Debug.Log("Line Snapper Value: " + lineSnapper.getMeasuredValue());

    }


    public void OnOptionSelect(int option)
    {
    //     if(dropdown.options.Count > 5) // When first selecting before removing default
    //    {
    //        dropdown.onValueChanged.RemoveAllListeners(); // Stop listening
    //        dropdown.options.RemoveAt(0); // Remove "Select Shape" Default Option
    //        dropdown.value -= 1; // move dropdown option to correct one
    //        option -= 1; //Reduce index by 1 to compensate for removed option
    //        dropdown.onValueChanged.AddListener(OnOptionSelect); // Start Listening
    //     }

        text.gameObject.SetActive(true); //Reactivate if not active
        // text.text = correctShapePropmt;

        /* UnityEngine.Debug.Log(option);*/
        currentOptionSelected = option;
        UnityEngine.Debug.Log("CurrentOption: " + currentOptionSelected);
        //currentShape
        /*       switch (option)
               {
                   case 0:
                      break;
                   case 1:
                      break;
                   case 2:
                       break;
                       case 3:
                       break;
                       case 4:
                       break;

               }*/

        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);
    }



    public class Problem
    {
        //Random actual value; Fixed Shape

        public SHAPES problemShape;
        public float p_measure = UNUSED;
        public float s_measure = UNUSED;
        private float offX = 0, offY = 0;
        private const float LVL3XOFF = 1.75f;
        private const float LVL3YOFF = 1.75f;

        int minLimitXY = 3;
        int limitXY = 8;
        

        public GameBehaviour main;
        public GameObject problemObjectShape;


        public Problem(SHAPES shape, GameBehaviour main, float x = -1, float y = -1)
        {
            this.main = main;
            //Next(limitXY);
            System.Random rand = new System.Random((int)DateTime.Now.Ticks);

            this.problemShape = shape;

            if (x == -1 && y == -1)
            {

                switch (this.problemShape)
                {
                    case SHAPES.SQUARE:
                        p_measure = rand.Next(minLimitXY, limitXY);
                        problemObjectShape = this.main.shapeGenerator.CreateSquare(new Vector2(0, 0), p_measure);
                        break;
                    case SHAPES.TRIANGLE:
                        p_measure = rand.Next(minLimitXY, limitXY);
                        s_measure = rand.Next(minLimitXY, limitXY);
                        problemObjectShape = this.main.shapeGenerator.CreateTriangle(new Vector2(0, 0), p_measure, s_measure);
                        break;
                    case SHAPES.CIRCLE:
                        p_measure = rand.Next(minLimitXY, limitXY);
                        problemObjectShape = this.main.shapeGenerator.CreateCircle(new Vector2(0, 0), p_measure, false);
                        break;
                    case SHAPES.RECTANGLE:
                        while (p_measure == s_measure)
                        {
                            p_measure = rand.Next(minLimitXY, limitXY);
                            s_measure = rand.Next(minLimitXY, limitXY);
                        }
                        problemObjectShape = this.main.shapeGenerator.CreateRectangle(new Vector2(0, 0), p_measure, s_measure);
                        break;
                    case SHAPES.SEMI_CIRCLE:
                        p_measure = rand.Next(minLimitXY, limitXY);
                        problemObjectShape = this.main.shapeGenerator.CreateCircle(new Vector2(0, 0), p_measure, true);
                        break;
                    default:
                        break;
                        //throw this shit 
                }
            }

            else
            {
                p_measure = x;
                s_measure = y;

                if (GlobalVariables.level >= 3) // Offset for lvl3 LO
                {
                    offX = -(p_measure / LVL3XOFF);
                    offY = -(s_measure / LVL3YOFF);
                }

                switch (this.problemShape)
                {
                    case SHAPES.SQUARE:
                        problemObjectShape = this.main.shapeGenerator.CreateSquare(new Vector2(offX, offY), p_measure);
                        break;
                    case SHAPES.TRIANGLE:
                        problemObjectShape = this.main.shapeGenerator.CreateTriangle(new Vector2(offX, offY), p_measure, s_measure);
                        break;
                    case SHAPES.CIRCLE:
                        problemObjectShape = this.main.shapeGenerator.CreateCircle(new Vector2(0, 0), p_measure, false);
                        break;
                    case SHAPES.RECTANGLE:
                        problemObjectShape = this.main.shapeGenerator.CreateRectangle(new Vector2(offX, offY), p_measure, s_measure);
                        break;
                    case SHAPES.SEMI_CIRCLE:
                        problemObjectShape = this.main.shapeGenerator.CreateCircle(new Vector2(0, 0), p_measure, true);
                        break;
                    default:
                        break;
                }

            }

            
        }
    }

    private void ActivateSpell(SHAPES s, GameObject instanced = null)
    {
        System.Random rand = new System.Random((int)DateTime.Now.Ticks);
        int limit = 0;

        if (instanced.IsUnityNull())
        {
            switch (s)
            {
                case SHAPES.SQUARE:
                    limit = animScript.square_Levels.Length;
                    instanced = animScript.square_Levels[rand.Next(0, limit)];
                    break;
                case SHAPES.RECTANGLE:
                    limit = animScript.rectangle_levels.Length;
                    instanced = animScript.rectangle_levels[rand.Next(0, limit)];
                    break;
                case SHAPES.TRIANGLE:
                    limit = animScript.triangle_levels.Length;
                    instanced = animScript.triangle_levels[rand.Next(0, limit)];
                    break;
                case SHAPES.CIRCLE:
                    limit = animScript.circle_levels.Length;
                    instanced = animScript.circle_levels[rand.Next(0, limit)];
                    break;
                case SHAPES.SEMI_CIRCLE:
                    limit = animScript.semicircle_levels.Length;
                    instanced = animScript.semicircle_levels[rand.Next(0, limit)];
                    break;
                default:
                    UnityEngine.Debug.Log("Whoa, you're not supposed to be here (Spell Instance error: Invalid shape)");
                    //TEMPLATE
                    limit = animScript.semicircle_levels.Length;
                    instanced = animScript.semicircle_levels[4];
                    break;
            }
        }

        //VFX
        if(!STARTUP)
            Instantiate(animScript.transitionVFX, GameObject.Find("SpellOrigin").transform);

        //SPELL
        Instantiate(instanced, GameObject.Find("SpellOrigin").transform);
        animScript.AcquireSpell(); // To prevent nullreference error
    }

    public class SpellCastEvent
    {
        public GameBehaviour main;
        public Problem problem; //level designer is the one responsible

        double p_measure = UNUSED;
        double s_measure = UNUSED;


        public SpellCastEvent(GameBehaviour behavior, Problem prob)
        {
            this.main = behavior;
            this.problem = prob;
            p_measure = this.problem.p_measure;
            s_measure = this.problem.s_measure;
        }
        /*Responsible to measure how much mana the player wants to USE.
        X value represents how many bars are displayed BEFORE a number is shown  
         Example: factor = 3
         -
         -
        3-
         -
         -
        6-
         */


        public float GetFillPercentage()
        {
            double result;

            switch(this.problem.problemShape)
            {
                case SHAPES.TRIANGLE:
                    result = (0.5 * this.p_measure * this.s_measure);
                    break;
                case SHAPES.CIRCLE:
                    result = (Math.PI * Math.Pow(p_measure/2, 2));
                    break;
                case SHAPES.RECTANGLE:
                    result = (p_measure * this.s_measure);
                    break;
                case SHAPES.SQUARE:
                    result = Math.Pow(p_measure, 2);
                    break;
                case SHAPES.SEMI_CIRCLE:
                    result = (0.5 * Math.PI * Math.Pow(p_measure/2, 2));
                    break;
                default:
                    throw new Exception("Invalid shape");
                    //throw this shit 
            }

            //12.4565735753735 => 1245
            float compX = (float)Math.Round(result*10)/10.0f;
            float compY = main.inputAnswer;//int.Parse(this.main.manaMeasure.text);
            /**/
            /*UnityEngine.Debug.Log("X Measure = " + compX);
            //UnityEngine.Debug.Log("S Measure = " + s_measure);
            UnityEngine.Debug.Log("Mana Measure = " + this.main.manaMeasure.text);*/

            return compY/compX;
             
        }



    }

    // NOTE: THIS IS BASICALLY WHERE ANY STARTUP STUFF NEEDS TO BE ADDED BESIDES THE START() FUNCTION
    IEnumerator WaitForComponent()
    {
        while (shapeGenerator == null)
        {
            UnityEngine.Debug.Log("Hi here...");
            shapeGenerator = GameObject.Find("ShapeGenerator").GetComponent<ShapeGenerator>();
           
            yield return new WaitForEndOfFrame();
        }

        shapeFiller = GameObject.Find("ShapeGenerator").GetComponent<ShapeFiller>();

        Reset(); // Same stuff as starting anyways
        STARTUP = false;
        // TODO: ADD SOMETHING THAT ALLOWS SWITCHING BETWEEN RANDOM PROBLEM AND MANUAL PROBLEM
        //generateProblem();
        //SetManualProblem(SHAPES.SEMI_CIRCLE, 6, 6, 100);

        //TODO: RUN SCREEN FADE IN FOR START OF LEVEL

        //Start with watching the spawn anim
        //ToClass();
        //DeactivateChangeCamera();
        //Invoke(nameof(StartLevelAnim), STARTDELAY);
    }

    private void StartLevelAnim()
    {
        screenFadeAnimator.SetTrigger("fade");
        Invoke(nameof(ToUI), TRANSITIONTIME);
        Invoke(nameof(ActivateChangeCamera), TRANSITIONTIME);
        Invoke(nameof(ShowNewUI), TRANSITIONTIME);
        Invoke(nameof(RemoveRoomText), TRANSITIONTIME);
    }

    private void HideNewUI()
    {
        hud.SetActive(false);
        pDialogue.SetActive(false);
        panelMagicScroll.SetActive(false);
        quickMenu.SetActive(false);
    }
    private void ShowNewUI()
    {
        hud.SetActive(true);
        pDialogue.SetActive(true);
        panelMagicScroll.SetActive(true);
        quickMenu.SetActive(true);
    }
    private void RemoveRoomText()
    {
        hud.SetActive(false);
    }

    /**Call to reset/set the level problem*/
    public void SetManualProblem(SHAPES shape, float x, float y = 1, int setSeed = -1)
    {
        currentShape = shape;
        
        Problem problem = new Problem(shape, this, x, y);
        System.Random random;
        if (setSeed != -1)
        {
            random = new System.Random(setSeed);
        }
        else
        {
            random = new System.Random((int)DateTime.Now.Ticks);
        }
            
        double result;

        switch (problem.problemShape)
        {
            case SHAPES.TRIANGLE:
                result = (0.5 * problem.p_measure * problem.s_measure);
                break;
            case SHAPES.CIRCLE:
                result = (Math.PI * Math.Pow(problem.p_measure / 2, 2));
                break;
            case SHAPES.RECTANGLE:
                result = (problem.p_measure * problem.s_measure);
                break;
            case SHAPES.SQUARE:
                result = Math.Pow(problem.p_measure, 2);
                break;
            case SHAPES.SEMI_CIRCLE:
                result = (0.5 * Math.PI * Math.Pow(problem.p_measure / 2, 2));
                break;
            default:
                throw new Exception("Invalid shape");
                //throw this shit 
        }

        slider.maxValue = (int)Math.Round(result * (1.5 + 0.5 * random.NextDouble()));
        this.spellCastEvent = new SpellCastEvent(this, problem);

        //Instantiate Spell Animation
        ActivateSpell(currentShape);
    }

    public void generateProblem()
    {
        System.Random random = new System.Random((int)DateTime.Now.Ticks);
        SHAPES randomShape = (SHAPES)(random.Next(1, Enum.GetValues(typeof(SHAPES)).Length));

        currentShape = randomShape;

        //UnityEngine.Debug.Log(randomShape);

        Problem problem = new Problem(randomShape, this);

        double result;

        switch (problem.problemShape)
        {
            case SHAPES.TRIANGLE:
                result = (0.5 * problem.p_measure * problem.s_measure);
                break;
            case SHAPES.CIRCLE:
                result = (Math.PI * Math.Pow(problem.p_measure / 2, 2));
                break;
            case SHAPES.RECTANGLE:
                result = (problem.p_measure * problem.s_measure);
                break;
            case SHAPES.SQUARE:
                result = Math.Pow(problem.p_measure, 2);
                break;
            case SHAPES.SEMI_CIRCLE:
                result = (0.5 * Math.PI * Math.Pow(problem.p_measure / 2, 2));
                break;
            default:
                throw new Exception("Invalid shape");
                //throw this shit 
        }

        slider.maxValue = (int)Math.Round(result * (1.5 + 0.5 * random.NextDouble()));

        this.spellCastEvent = new SpellCastEvent(this, problem);

        //Instantiate Spell Animation
        ActivateSpell(currentShape);
    }
}
