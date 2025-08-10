using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class HOGameScript : MonoBehaviour
{
    const int UNUSED = -1;

    GameBehaviour.SHAPES currentShape;
    public SpellCastEvent spellCastEvent;
    TMP_Text correctionPerc;

    public Button btnMeasure; //this is btnMeasure / confirmMeasurement
    public Button btnRestart;
    public Button btnUndo;
    public Button btnQuit;

    public ShapeGenerator shapeGenerator;
    public ShapeFiller shapeFiller;

    public LineSnapper lineSnapper;

    //NEW ADDITIONS: DELETE IN CASE EVERYTHING BREAKS
    private GameObject mainCamera, classroomCamera;
    private Material classroomMaterial;
    private Animator screenFadeAnimator;
    private const float TRANSITIONTIME = 0.4f, FILLTIMEAPROX = 1.5f, STARTDELAY = 3.0f;
    private float ENDDELAY = 5.0f;

    private AnimScript animScript;
    private bool STARTUP = true;
    public float error = 100f;
    private bool isQuit = false;
    private const float TRANSITIONDELAY = 0.5f;
    public const string charDialogue1 = "Kailangan ko pumili ng hugis na aking sasagutin",
                        charDialogue2 = "Sagutin natin ang Area gamit ng mga sukat na nakuha natin!",
                        charDialogue3 = "Kailangan ko piliin ang tugmang formula para sa hugis.";
    private const string areaDisplayText1 = "Area ng mga hugis:\n";
    public GameObject areaDisplayObj;
    private TextMeshProUGUI areaDisplay;

    //----------------------------------------------
    //////////Copied from old repo
    //my changes in case mag boom boom lahat
    private GameData savedGame;
    private SaveLoadController saverLoader = new SaveLoadController();
    private string savePath;

    private RectTransform rtDialogue;
    private RectTransform rtDiaButtons;

    private bool isDoneMeasuring;

    public Text pConfirmText;
    //panels na toggable, containers lang
    public GameObject hud;
    public GameObject quickMenu;
    public GameObject panelMagicScroll;
    public GameObject pConfirm;
    public GameObject pLowerScroll;
    public GameObject pNotify;
    private GameObject pDialogue;
    private GameObject pDiaButtons;

    public GameObject prefabSpawn;
    public Transform pSpawner;  //contaings prefab for spawning
                                //needs to be Transformm cuz if GameObject cannot convert from scene sa Instantiate kineme


    public Text textTemp;

    public GameObject notifyTextObj;

    private Text pNotifyText;
    public Text characterSay;
    private Text textFinish;
    private Text txtFinalCompound; //once used lng to
    public Text confirmText;
    public Text textHUD;
    public GameObject pEquationTriangle;
    public GameObject pEquationSquare;
    public GameObject pEquationRectangle;
    public GameObject pEquationSCircle;
    public GameObject pEquationCircle;

    public Button bYesHome;   //alternate buttons
    public Button bYes;
    public Button btnConfirmSpell;

    private Dictionary<HOGameBeh.ShapeObject, float> recordedAnswer = new Dictionary<HOGameBeh.ShapeObject, float>();
    private GameObject currentlySolvedShape = null;
    private HOGameBeh.ShapeObject currentShapeObject = null;

    private string chosenShape;

    private const string castBtnText1 = "Done";
    private const string castBtnText2 = "Erase";
    private const string castBtnText3 = "Finish Spell";
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
    public GameObject calcBtnObj;
    private Text calcBtnText;
    public GameObject backspaceButton;

    public bool isFinalAnswer = false;
    private bool isAllSolved = false;
    private bool justDoneSolving = false;

    //----------------------------------------------

    //References to the GUI display for line Lengths
    public GameObject sqVarDisp1, rectVarDisp1, rectVarDisp2, triVarDisp1, triVarDisp2, cirVarDisp1, semiVarDisp1;
    private GameObject var1Display, var2Display; //Variables for determining which ones will be modified

    //Sound related stuff
    public GameObject soundPlayerObj;
    public GameLevelSoundPlayer soundPlayer;

    public HOGameBeh hoGameBeh;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        savedGame = saverLoader.loadGame(savePath);

        currentShape = GameBehaviour.SHAPES.NONE;
        screenFadeAnimator = GameObject.Find("ScreenFade").GetComponent<Animator>();
        animScript = GameObject.Find("AnimHolder").GetComponent<AnimScript>();

        //Get OCR script
        ocrScript = ocrInput.transform.Find("DrawingAndOCRManager").GetComponent<DrawingAndOCRManagerScript>();

        //Get FA script
        fa = formulaAnalyzerObj.GetComponent<FormulaAnalyzer>();
        fa.hgb = this;


        //Get notif text
        pNotifyText = notifyTextObj.GetComponent<Text>();

        //Get calcBtn text
        calcBtnText = calcBtnObj.transform.Find("textFinish").gameObject.GetComponent<Text>();

        //Disable calcBtn
        calcBtnObj.SetActive(false);

        //Get area display text
        areaDisplay = areaDisplayObj.GetComponent<TextMeshProUGUI>();
        areaDisplayObj.SetActive(false);
    }

    //Function for getting the text objects for displaying the line lengths
    public void GetVarDisp(GameBehaviour.SHAPES shape)
    {
        //Get the text objects that will be used to display the line lengths of the shape
        switch (shape)
        {
            case GameBehaviour.SHAPES.SQUARE:
                var1Display = sqVarDisp1;
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
                break;
            case GameBehaviour.SHAPES.SEMI_CIRCLE:
                var1Display = semiVarDisp1;
                break;
        }
    }

    private void ResetVarDisp(GameBehaviour.SHAPES shape)
    {
        switch (shape)
        {
            case GameBehaviour.SHAPES.SQUARE:
                var1Display.GetComponent<Text>().text = "S";
                break;
            case GameBehaviour.SHAPES.RECTANGLE:
                var1Display.GetComponent<Text>().text = "L";
                var2Display.GetComponent<Text>().text = "W";
                break;
            case GameBehaviour.SHAPES.TRIANGLE:
                var1Display.GetComponent<Text>().text = "B";
                var2Display.GetComponent<Text>().text = "H";
                break;
            case GameBehaviour.SHAPES.CIRCLE:
                var1Display.GetComponent<Text>().text = "R";
                break;
            case GameBehaviour.SHAPES.SEMI_CIRCLE:
                var1Display.GetComponent<Text>().text = "R";
                break;
        }
    }

    void Reset()
    {
        //Cleanup possible temporary vfx and clones
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Temporary"))
        {
            Destroy(go);
        }

        currentShape = GameBehaviour.SHAPES.NONE;

        btnMeasure.gameObject.SetActive(false);
        correctionPerc.gameObject.SetActive(false);
        if (!STARTUP) // If not first run
            Destroy(this.spellCastEvent.problem.problemObjectShape);
        lineSnapper.gameObject.SetActive(false);
        btnUndo.gameObject.SetActive(false);

        lineSnapper.OnUndoPressed();
        lineSnapper.OnUndoPressed();

        //RUN SCREEN FADE IN FOR RESTART OF LEVEL
        if (!STARTUP)
            screenFadeAnimator.SetTrigger("fadeIn");

        //Cleanup previous spell objects
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Spell"))
        {
            Destroy(go);
        }

        //Start with watching the spawn anim again in reset
        ToClass();
        Invoke(nameof(StartLevelAnim), STARTDELAY);

        Invoke(nameof(InitProblem), 0.1f); // Add delay to prevent object from getting nuked by cleanup
    }

    private void InitProblem()
    {
        hoGameBeh.Initiate();
    }

    //--------------------------------------------------------
    /////////////////added from old repo
    // just button events
    public void onRestart()
    {
        formulaDisplay.SetActive(false); //Disable this since its visible above the screenfade for some reason
        screenFadeAnimator.SetTrigger("sceneOut");
        Invoke(nameof(LoadSceneDelay), TRANSITIONDELAY);
    }
    private void LoadSceneDelay()
    {

        SceneManager.LoadScene("GameLevelScene_v3"); // Reload scene to avoid problems (Lazy and slightly slow but eh...)
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

    public void toggleConfirmScreen(string what)
    {

        if (what == "shape")    //chaned some stuff lang
        {
            what = chosenShape;
        }

        bool temp = !pConfirm.activeInHierarchy;

        if (pConfirm != null)
        {
            pConfirm.SetActive(temp);    //hideshow

        }
        bYesHome.gameObject.SetActive(false);
        bYes.gameObject.SetActive(false);
        if (what == "confirmHome")
        {
            pConfirmText.text = "Nais mo bang bumalik sa labas na pagpipilian?";
            confirmText.text = "Hindi masa-Save ang progreso.";
            bYes.gameObject.SetActive(false);
            bYesHome.gameObject.SetActive(true);
        }
        else if (pConfirm.activeInHierarchy)
        {
            pConfirmText.text = "Tama ba ang napili:";
            bYes.gameObject.SetActive(true);
            bYesHome.gameObject.SetActive(false);
            confirmText.text = "[ " + what + " ]?";

        }
    }

    public void toggleMagicScroll()
    {
        if (pLowerScroll != null)
        {
            pLowerScroll.SetActive(!pLowerScroll.activeInHierarchy);
        }
    }


    public void hideAllEquation()
    {
        pEquationSquare.SetActive(false);
        pEquationSCircle.SetActive(false);
        pEquationCircle.SetActive(false);
        pEquationRectangle.SetActive(false);
        pEquationTriangle.SetActive(false);

    }


    public void chooseSquare()
    {
        chosenShape = "SQUARE";

        // toggleConfirmScreen(chosenShape);
        hideAllEquation();
        pEquationSquare.SetActive(true);
        // textTemp.text = "SQUARE";
    }

    public void chooseSemiCircle()
    {
        chosenShape = "SEMI_CIRCLE";
        // toggleConfirmScreen(chosenShape);
        hideAllEquation();
        pEquationSCircle.SetActive(true);
        // textTemp.text = "SEMI_CIRCLE";
    }

    public void chooseCircle()
    {
        chosenShape = "CIRCLE";
        // toggleConfirmScreen(chosenShape);
        hideAllEquation();
        pEquationCircle.SetActive(true);
        // textTemp.text = "CIRCLE";
    }

    public void chooseRectangle()
    {
        chosenShape = "RECTANGLE";
        // toggleConfirmScreen(chosenShape);
        hideAllEquation();
        pEquationRectangle.SetActive(true);
        // textTemp.text = "RECTANGLE";
    }

    public void chooseTriangle()
    {
        chosenShape = "TRIANGLE";
        // toggleConfirmScreen(chosenShape);
        hideAllEquation();
        pEquationTriangle.SetActive(true);
        // textTemp.text = "TRIANGLE";
    }

    public void btnNo()
    {
        toggleConfirmScreen("");
        // pConfirm.SetActive(false);
    }

    public void hideDiaBoxWhileMeasuring()
    { //use only pag nag mmeasure, iba to sa mahahalf yung screen ah
        StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new(600f, -150f)));
        backspaceButton.gameObject.SetActive(false);//f4rom lower
        Debug.Log("It's here 450, button should not be visible...");
        //uhh idk bakit nawawala lahat ng buttons???
        if (isAllSolved)
        {
            //last na, hide redo button
            btnMeasure.gameObject.SetActive(false);
        }
        else
        {
            btnMeasure.gameObject.SetActive(true);
        }
    }

    public void showDiaBoxAfterMeasuring()
    { //use only pag nag mmeasure, iba to sa mahahalf yung screen ah
        StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new(225f, 130f)));
        Debug.Log("Line 457, should have all the buttons showing after measuring for all times");
        calcBtnObj.gameObject.SetActive(true);
    }

    //cleaned this up a bit, same thing naman ung ganapings
    public void toggleDialogueBox()
    {

        // 600, -121.46 Y to hide the dialogue while measuring (since the shape cannot be moved)

        if (rtDialogue.anchoredPosition.y == 100)
        {
            // StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, RTAWAYTRANS));
            // StartCoroutine(RectTransformOverTime(pDiaButtons.GetComponent<RectTransform>(), DIALOGUESLIDETIME, PDIAAWAYTRANS));

            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new(600f, 130f)));
            // StartCoroutine(RectTransformOverTime(rtblackboard, OCRSLIDETIME, new(940f, 285f)));


            StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, new(-493f, -167f)));   //left mode

            // StartCoroutine(RectTransformOverTime(pDiaButtons.GetComponent<RectTransform>(), DIALOGUESLIDETIME, new(-493f, 223f)));   //experiemtn positions
            // pDialogue.y = -59;
        }
        else
        {

            // StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, origDiaRT));

            //jusrDoneSolving is true when mana filling is triggered so we really know it got activated
            //^ false when onCasting na? todo: test if this works
            if (justDoneSolving && !isAllSolved)
            {
                Debug.Log("Line 500 Im kms if this fails");
                StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new(600f, 130f)));    //when youy undo enough it should return here (nasa baba)
                // justDoneSolving = true;  //NAH KEEP THIS ON UNTIL MEASUREMENT
            }
            else
            {   //OK SO IT WORKS NOW, THE WHOLE PROB WAS THAT THE THING IS TURNING FALSE, SHOULNDT BE UNTIL AFTER CASTING IG
                StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new(600f, -151f)));    //when youy undo enough it should return here (nasa baba)


                // StartCoroutine(RectTransformOverTime(rtblackboard, OCRSLIDETIME, new(940f, -40f)));
            }

            if (isDoneMeasuring == false)
            {
                StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, new(-493f, -167f)));
                btnMeasure.gameObject.SetActive(true);
                backspaceButton.SetActive(false);
                //when you undo enough basically
                Debug.Log("kine 504");
            }
            else
            {
                btnMeasure.gameObject.SetActive(false);//show uli pag bumalik sa measuring kakaundo
                backspaceButton.SetActive(true);
                StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, new(-493f, 138f)));
                Debug.Log("Line 510");
            }

            if (isAllSolved) //final portion
            {
                StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, new(-493f, 138f)));
                // btnMeasure.gameObject.SetActive(false); //hide measure//OKAY DONT DO THAT
                // Debug.Log("line 543 naway mawala na ung HO button na nasa likod ng redo after neto pls lang");
            }
            //StartCoroutine(RectTransformOverTime(pDiaButtons.GetComponent<RectTransform>(),DIALOGUESLIDETIME, new(-493f, -175f)));
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

    public void btnYes()
    {
        UnityEngine.Debug.Log("chosenshape -> " + chosenShape);
        UnityEngine.Debug.Log("current shape -> " + this.spellCastEvent.problem.problemShape.ToString());


        //hide the spellbook after choosing

        if (this.spellCastEvent.problem.problemShape.ToString() == chosenShape)
        {
            hideDiaBoxWhileMeasuring(); //this is new for only measurement

            btnConfirmSpell.gameObject.SetActive(false);
            panelMagicScroll.SetActive(false);
            //show na den undo and cast buttons
            pDiaButtons.SetActive(true);
            // HideDialogue();

            characterSay.text = charDialogue2;

            lineSnapper.gameObject.SetActive(true);
            btnUndo.gameObject.SetActive(true);

            //show correct casting equation
            //not entering correctly
            if (chosenShape == "TRIANGLE")
            {
                pEquationTriangle.SetActive(true);
            }
            else if (chosenShape == "SQUARE")
            {
                pEquationSquare.SetActive(true);
            }
            else if (chosenShape == "RECTANGLE")
            {
                pEquationRectangle.SetActive(true);
            }
            else if (chosenShape == "CIRCLE")
            {
                pEquationCircle.SetActive(true);
            }
            else if (chosenShape == "SEMI_CIRCLE")
            {
                pEquationSCircle.SetActive(true);
            }
        }
        else    //IF SHAPE CHOSEN IS WRONGG:
        {
            notifyWrongShape();
        }
        toggleConfirmScreen("");
    }

    public void CloseNotification()
    {
        if (pNotify != null)
        {
            pNotify.SetActive(false);
        }

        if (isDoneMeasuring) //For resuming OCR after notification
            Invoke(nameof(ResumeOCR), 0.1f);
    }

    public void notifyWrongShape()
    {
        if (pNotify != null)
        {
            pNotify.SetActive(true);
        }
    }

    private void ResumeOCR()
    {
        ocrScript.processing = false;
    }

    public void NotifyInvalidFormula()
    {
        ocrScript.processing = true; //Prevent input from occuring

        if (pNotify != null)
        {
            pNotify.SetActive(true);
            pNotifyText.text = "Hindi wasto ang ibinigay na formula.";
        }
    }
    public void NotifyMismatchedAnswer()
    {
        ocrScript.processing = true; //Prevent input from occuring

        if (pNotify != null)
        {
            pNotify.SetActive(true);
            pNotifyText.text = "Hindi tugma sa formula ang ibinigay na sagot.";
        }
    }

    public void ToggleCalcMode() //For calculator button
    {
        //Reset Board
        ocrScript.ResetColor();
        ocrScript.ResetVFX();

        if (fa.calcMode)
        {
            calcBtnText.text = "Calculator";
            fa.ExitCalc();
        }
        else if (!fa.calcMode)
        {
            calcBtnText.text = "Formula Input";
            fa.EnterCalc();
        }
    }

    public void onCast()
    {
        inputAnswer = 0f; // Reset input answer
        btnMeasure.gameObject.SetActive(false); //Deactivate "Done" button

        // Modified function if all shapes solved
        characterSay.text = "";
        justDoneSolving = false;
        if (isAllSolved || !isDoneMeasuring)
        {
            DoneMeasure();
        }
        else
        { //Reset the OCRInput board
            fa.ResetCalcDisp();
            fa.ResetAnalyzer();

            //Reset Board
            ocrScript.ResetColor();
            ocrScript.ResetVFX();
        }
    }

    public void OnBackspacePressed()
    {
        fa.BackspaceInput();

        //Reset Board
        ocrScript.ResetColor();
        ocrScript.ResetVFX();
    }

    private void ManaFilling()
    {
        //Attach new shapeFiller to target shape and activate it
        ShapeFiller currFiller = currentlySolvedShape.AddComponent<ShapeFiller>();

        //Fill shape with mana, fill excess with void
        if (!currentShapeObject.isExcess)
            currFiller.fillMaterial = shapeFiller.fillMaterial;
        else if (currentShapeObject.isIntersect)
            currFiller.fillMaterial = shapeFiller.intersectMaterial;
        else if (currentShapeObject.isExcess)
            currFiller.fillMaterial = shapeFiller.voidMaterial;

        currFiller.InitializeFill(currentlySolvedShape, Color.green, 0.5f, spellCastEvent.GetFillPercentage());
        currFiller.isFillingActive = true;
    }

    public void InputAnswer(float ans = 0f) //Sends final answer
    {
        inputAnswer = ans;
        ResetVarDisp(currentShape);

        Debug.Log("Line 777: test DO YOU REACH THIS POINT");

        if (!isFinalAnswer) //Only record if not final answer mode
        {
            if (!currentShapeObject.isExcess) //Positive Area
                recordedAnswer.Add(currentShapeObject, inputAnswer);
            else if (currentShapeObject.isExcess) //Negative Area
                recordedAnswer.Add(currentShapeObject, -inputAnswer);
        }

        CalcError();

        //Disable OCR board and formulaDisplay
        StartCoroutine(SlideOCRBoard(false));

        //Make spell explode or fizzled out and end the game if input of subshape or whole shape is wrong
        if (Math.Round(Math.Abs(error), 2) != 0f)
        {
            //Show error if not 0%
            correctionPerc.text = "Error: " + Math.Round(Math.Abs(error), 2) + "%";

            correctionPerc.gameObject.SetActive(true);

            if (!isFinalAnswer)
                ManaFilling();

            //End game stuff

            toggleDialogueBox(); //hide  
            HideNewUI();

            if (!isFinalAnswer)
                Invoke(nameof(CallCastAnimation), FILLTIMEAPROX + OCRSLIDETIME);
            else
                Invoke(nameof(CallCastAnimation), FILLTIMEAPROX - 1.0f + OCRSLIDETIME); //Reduced time due to no filling

            //Play Error SFX
            soundPlayer.PlaySFX(3, 1, 2f);

            return; //Just in case it decides to run the code after
        }

        calcBtnObj.SetActive(false);

        //More stuff to only do when not in final answer
        if (!isFinalAnswer)
        {
            //Play Fill SFX if not final answer
            soundPlayer.PlaySFX(1, 1, 0.75f);

            hoGameBeh.shapeClickManager.EnableShapeClicking();

            //Make solved shape unclickable
            currentlySolvedShape.GetComponent<MeshCollider>().enabled = false;

            justDoneSolving = true;
            Debug.Log("Pls work plplplplpls justDoneSolving: " + justDoneSolving);
            toggleDialogueBox();//show again if not final answer
            ManaFilling();

            Debug.Log("Line 825 see if this is the prob?");
            //just gonna try to do it the hardcode way :/
            hoGameBeh.spellCastEvent.setHiddenStateAllShapes(false);

            //TODO: Probably add the new UI per shape here
        }

        ModifiedToUIAgain();

        //If all simple shapes have been solved, show cast button and modify it to be a "Finish Spell" button for final input
        if (hoGameBeh.isAllAttemptedSolve())
        {
            // Show and Modify Cast button functions
            pDiaButtons.SetActive(true); //Enable the buttons

            //Disable all other buttons for now
            backspaceButton.SetActive(false);   //whys is this not erroring lmao
            btnUndo.gameObject.SetActive(false);
            btnMeasure.gameObject.SetActive(false);
            Debug.Log("Line 881 if it errors lam na");

            // Set flag that confirms all shapes solved
            isAllSolved = true;

            //change dialogue text to say that the final area needs to be calculated if final answer
            // characterSay.text = "Kailangan ko na ngayon mabuo ang Spell.";
            //No need to reactivate undo button since not used any longer
        }

        //TODO: Add checks for if the input is a final answer i.e. the input is the final area of the compound shape
        //      Then run the animation
        if (isFinalAnswer)
        {
            HideNewUI();

            Debug.Log("-------- before this point the buttons should all be hidden!!! -----");
            pSpawner.gameObject.SetActive(false); //hide before animation

            //hide all buttons at the end
            pDiaButtons.SetActive(false);
            //hide all buttons at the end
            pDiaButtons.SetActive(false);
            toggleDialogueBox(); //hide 
            txtFinalCompound.text = "";

            //Indicate that the spell casting is done with the error text field
            correctionPerc.text = "Spell Complete!";

            correctionPerc.gameObject.SetActive(true);

            //Play a new spell complete SFX since the mana filling is done?
            soundPlayer.PlaySFX(4, 1, 1.5f);

            Invoke(nameof(CallCastAnimation), FILLTIMEAPROX - 1.0f + OCRSLIDETIME); //Reduced time due to no filling
        }
    }

    public void DoneMeasure()
    {
        isDoneMeasuring = true;
        textFinish.text = castBtnText2;
        calcBtnObj.SetActive(true);

        //Update dialogue displays for line lengths
        Debug.Log("value1: " + lineSnapper.value1 + "| value2: " + lineSnapper.value2);
        if (var1Display != null)
            var1Display.GetComponent<Text>().text = lineSnapper.value1;
        if (var2Display != null)
            var2Display.GetComponent<Text>().text = lineSnapper.value2;

        //Set flag for fa to be in final answer mode (doesn't check if it is a valid shape formula just any valid formula)
        if (isAllSolved)
        {
            fa.isCompoundArea = true;
        }

        //NEW OCR SHOW CODE
        StartCoroutine(SlideOCRBoard(true));
        lineSnapper.ToggleLineText(); //Toggle off
    }

    private bool undoPressed = false;
    public void UndoMeasure()
    {
        undoPressed = true;
        //Reset line values based on linecount
        if (lineSnapper.lineCount >= 1)
            lineSnapper.value2 = "???";
        if (lineSnapper.lineCount < 1)
            lineSnapper.value1 = "???";

        //Hide OCR Board
        if (isDoneMeasuring)
        {
            StartCoroutine(SlideOCRBoard(false));
            Invoke(nameof(ToggleLineDelay), OCRSLIDETIME); //Toggle on
        }

        textFinish.text = castBtnText1;
        if (calcBtnObj.activeInHierarchy)
            calcBtnObj.SetActive(false); //Deactivate calculator button
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
            if (!isAllSolved)
                // ShowDialogue(); //Show
                toggleDialogueBox();

            yield return new WaitForSeconds(DIALOGUESLIDETIME); //Wait for Dialogue Toggle

            ocrInput.SetActive(true); //Activate the board
            StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, rightEndTransObj.transform.position));

            //Slide and Scale Dialogue Box
            StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, new(308f, 100f)));
            StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, new(0.9f, 0.9f, 0.9f)));
        }
        else if (!show)
        {
            ocrScript.processing = true; //Stop accepting input

            formulaDisplay.SetActive(false); //Hide OCR input Display

            StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, rightStartTransObj.transform.position));

            //Slide Dialogue Box
            StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, origDiaRT));
            StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, new(1f, 1f, 1f))); //Scale to Normal

            if (!isAllSolved)
                pDiaButtons.SetActive(false); //Disable buttons when dialogue is up until all solved
            backspaceButton.SetActive(false);

            if (inputAnswer > 0f) undoPressed = false;
            Debug.Log("undoPressed: " + undoPressed);
            if (undoPressed)
            {
                pDiaButtons.SetActive(true);
                backspaceButton.SetActive(true);
            }
        }

        yield return new WaitForSeconds(OCRSLIDETIME); //Wait until OCR board stops moving

        if (show)
        {
            //Reset Board
            ocrScript.ResetColor();
            ocrScript.ResetVFX();

            ocrScript.processing = false; //Start accepting input
            formulaDisplay.SetActive(true); //Show OCR input Display

            backspaceButton.SetActive(true);
        }
        else if (!show)
        {
            ocrInput.SetActive(false); //Deactivate the board once off screen

            if (isFinalAnswer || undoPressed)
            {
                // HideDialogue();
                toggleDialogueBox(); //Hide

                undoPressed = false;
            }
            else
            {
                // ShowDialogue();
                toggleDialogueBox(); //Hide
            }
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
        textFinish = GameObject.Find("textFinish").GetComponent<Text>();
        txtFinalCompound = GameObject.Find("shapeCompoundFinal").GetComponent<Text>();
        textFinish.text = castBtnText1;

        isDoneMeasuring = false;
        justDoneSolving = false;

        pDialogue = GameObject.Find("PanelCasting");
        rtDialogue = pDialogue.GetComponent<RectTransform>();
        origDiaRT = rtDialogue.anchoredPosition; //Save original pos
        pDiaButtons = GameObject.Find("pDiaButtons");
        rtDiaButtons = pDiaButtons.GetComponent<RectTransform>();

        savedGame = saverLoader.loadGame(Path.Combine(Application.persistentDataPath, "saveData.json"));
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
        // manaReq.text = "";//clear
        btnConfirmSpell.gameObject.SetActive(false);

        pConfirmText.text = "";

        bYesHome.gameObject.SetActive(false);
        bYes.gameObject.SetActive(false);

        // end

        //////////////////////////////////////


        if (STARTUP)
        {
            screenFadeAnimator.SetTrigger("fadeIn");

            //Hide new UI at start
            HideNewUI();

            //Except Hud
            //hud.SetActive(true);

            //Get sound player script, do on start since component init is at Awake()
            soundPlayer = soundPlayerObj.GetComponent<GameLevelSoundPlayer>();
        }

        correctionPerc = GameObject.Find("ManaFillCorrectPerc").GetComponent<TMP_Text>();
        lineSnapper = GameObject.Find("Gesture").GetComponent<LineSnapper>();

        //NEW ADDITIONS: DELETE IN CASE EVERYTHING BREAKS

        mainCamera = GameObject.Find("Main Camera");
        mainCamera.SetActive(false);
        classroomCamera = GameObject.Find("ClassroomCamera");
        classroomCamera.SetActive(true);

        lineSnapper.animScript = this.animScript;

        StartCoroutine(WaitForComponent());

        Debug.Log("Level: " + GlobalVariables.level);

        classroomMaterial = Resources.Load<Material>("Materials/ClassroomScreenMaterial");

        //----------------------------------------------
        btnMeasure.gameObject.SetActive(false);
        correctionPerc.gameObject.SetActive(false);
        lineSnapper.gameObject.SetActive(false);
        btnUndo.gameObject.SetActive(false);
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

        error = ((1 - clamped) * 100); //Get error float
    }

    private void ToClass()
    {
        animScript.VideoPlayerScript.Stop();

        correctionPerc.gameObject.SetActive(false);
        lineSnapper.gameObject.SetActive(false);

        mainCamera.SetActive(false);
        classroomCamera.SetActive(true);
    }

    private void ToUI()
    {
        animScript.VideoPlayerScript.Stop();
        animScript.VideoPlayerScript.PlayBGAnim();
        ModifiedToUI();
    }

    private void ModifiedToUIAgain()
    {
        currentlySolvedShape = null;
        lineSnapper.OnUndoPressed();
        lineSnapper.OnUndoPressed();
        lineSnapper.gameObject.SetActive(false);
        if (!isFinalAnswer)
            characterSay.text = charDialogue1;
        SetVisibilityNewUI(false, true, false, true);

        //Disable all formula displays
        pEquationSquare.SetActive(false);
        pEquationRectangle.SetActive(false);
        pEquationTriangle.SetActive(false);
        pEquationCircle.SetActive(false);
        pEquationSCircle.SetActive(false);

        if (hoGameBeh.isAllAttemptedSolve())
        {
            //Display the areas in SolvedAreaDisplay
            areaDisplayObj.SetActive(true);
            areaDisplay.text = areaDisplayText1;
            string currDisplay = "";

            int i = 0;
            const int n = 1;
            foreach (var dict in recordedAnswer)
            {
                i++;
                currDisplay = areaDisplay.text; //Get current text
                //ADD NEW OBJECTS HERE

                Debug.Log(recordedAnswer.Count + "COMPLETED SHAPE! Add here yung new UI for it: " + dict.Key.shape);
                txtFinalCompound.text = "Mga Component na bumubuo sa Compound Shape:";
                spawnCompoundComponents(i, dict.Key.shape, dict.Value);

                areaDisplay.text = currDisplay + $"[{dict.Key.shape}]: {dict.Value} "; //Add shape and area
                Debug.Log("Size: " + dict.Value);

                //New line every n shapes
                if (i % n == 0)
                {
                    currDisplay = areaDisplay.text; //Get current text
                    areaDisplay.text = currDisplay + '\n'; //Add line break
                }
            }
        }

    }

    private void spawnCompoundComponents(int i, GameBehaviour.SHAPES shapeName, float shapeVal)
    {
        //just spawn the component shapes (that are already solved)
        //n is list size
        Vector2 newPos = new Vector2(0f, 0f) + new Vector2(0f, -60f * i);  //60 is height ng box to spawn
        GameObject newSpawn = Instantiate(prefabSpawn, pSpawner);
        RectTransform rtSpawn = newSpawn.GetComponent<RectTransform>();
        if (rtSpawn != null)
        {
            rtSpawn.anchoredPosition = newPos;
        }
        else
        {
            newSpawn.transform.localPosition = newPos;
        }

        Text txtShape = newSpawn.transform.Find("shapeText").GetComponent<Text>();
        txtShape.text = shapeName.ToString() + " : " + shapeVal;  //current name change

        GameObject imgShapeSqr = newSpawn.transform.Find("shapeSqr").gameObject;
        GameObject imgShapeCir = newSpawn.transform.Find("shapeCir").gameObject;
        GameObject imgShapeSCir = newSpawn.transform.Find("shapeSCir").gameObject;
        GameObject imgShapeRect = newSpawn.transform.Find("shapeRect").gameObject;
        GameObject imgShapeTri = newSpawn.transform.Find("shapeTri").gameObject;
        imgShapeSqr.SetActive(false);
        imgShapeCir.SetActive(false);
        imgShapeSCir.SetActive(false);
        imgShapeRect.SetActive(false);
        imgShapeTri.SetActive(false);

        if (shapeName == GameBehaviour.SHAPES.SQUARE)
        {
            imgShapeSqr.SetActive(true); //hidk whjat i am doing

        }
        else if (shapeName == GameBehaviour.SHAPES.TRIANGLE)
        {
            GameObject imgShape = newSpawn.transform.Find("shapeTri").gameObject;
            imgShapeTri.SetActive(true); //hidk whjat i am doing
        }



    }

    private void ModifiedToUI()
    {
        lineSnapper.gameObject.SetActive(false);
        characterSay.text = charDialogue1;

        mainCamera.SetActive(true);
        classroomCamera.SetActive(false);
        //DisableNewUI
        SetVisibilityNewUI(false, true, false, true);
    }

    public void UIAfterShapeSelect(ShapeClickManager.ShapeClickData clickData)
    {
        correctionPerc.text = "";
        hoGameBeh.shapeClickManager.DisableShapeClicking();
        hoGameBeh.spellCastEvent.setHiddenStateAllShapes(true);
        GlobalVariables.loSelectedShape = clickData.shapeType;
        currentlySolvedShape = clickData.originalShapeObject.actualShapeObj;
        currentShapeObject = clickData.originalShapeObject;
        SetManualProblem(clickData);
        SetVisibilityNewUI(false, true, true, true);
    }

    private int SendShapeToPlayer(GameBehaviour.SHAPES s)
    {
        switch (s)
        {
            case GameBehaviour.SHAPES.SQUARE:
                return 0;
            case GameBehaviour.SHAPES.RECTANGLE:
                return 1;
            case GameBehaviour.SHAPES.TRIANGLE:
                return 2;
            case GameBehaviour.SHAPES.CIRCLE:
                return 3;
            case GameBehaviour.SHAPES.SEMI_CIRCLE:
                return 4;
            default:
                return -1;
        }
    }

    private void DelayedCastAnimation()
    {
        //Hide UI elems
        HideNewUI();

        if (Math.Round(Math.Abs(error), 2) == 0f)
        {
            int state = 2;
            if (error > 0)
                state = 0;
            else if (error < 0)
                state = 1;
            animScript.VideoPlayerScript.PlaySpellAnim(GameBehaviour.SHAPES.NONE, state);

            //TODO: ADD END SCREENN, Base delay from sd + ENDDELAY - 3.5f maybe?
            UnityEngine.Debug.Log("LEVEL COMPLETE!!!");
            float sd = animScript.VideoPlayerScript.GetVideoLength();
            Invoke(nameof(FadeDelay), sd + ENDDELAY - 1f);

            Invoke(nameof(EndGameFunctions), sd + ENDDELAY);
            return; // Early return
        }
        //if (error < 0f)
        //    ;
        //if (error > 0f)
        //    ;

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

    private void EndGameFunctions() //Function for saving data to save maybe? Also transitioning back to level select
    {
        // Save requisite data
        if (Math.Round(Math.Abs(error), 2) == 0f)
        {
            GlobalVariables.playerWin = true;
            if (GlobalVariables.level < 3) //Reset on level up
                GlobalVariables.percent = 0f;
            else //Show 100%
                GlobalVariables.percent = 1f;
        }
        else
        {
            GlobalVariables.playerWin = false;
            GlobalVariables.percent = 1 - error;
        }

        if (!isQuit) // Only activate flags if not quitting
        {
            GlobalVariables.gameFinished = true; //Set flag to save data
        }

        //TRANSITION TO LEVEL SELECT SCREEN AGAIN
        SceneManager.LoadScene("LevelSelect");
    }

    private void CallCastAnimation()
    {
        screenFadeAnimator.SetTrigger("fade");

        Invoke(nameof(ToClass), TRANSITIONTIME);
        Invoke(nameof(DelayedCastAnimation), TRANSITIONTIME + 0.1f);
    }

    public class Problem
    {
        //Random actual value; Fixed Shape

        public GameBehaviour.SHAPES problemShape;
        public float p_measure = UNUSED;
        public float s_measure = UNUSED;

        public HOGameScript main;
        public GameObject problemObjectShape;


        public Problem(GameBehaviour.SHAPES shape, HOGameScript main, GameObject shapeProblem, float x, float y)
        {
            this.main = main;
            //Next(limitXY);
            System.Random rand = new System.Random((int)DateTime.Now.Ticks);

            this.problemShape = shape;
            problemObjectShape = shapeProblem;
            this.p_measure = x;
            this.s_measure = y;
        }
    }

    public void InstanceSpellObject(GameObject instanced = null)
    {
        animScript.VideoPlayerScript.PlaySpellIntro(GameBehaviour.SHAPES.NONE);
    }

    public class SpellCastEvent
    {
        public HOGameScript main;
        public Problem problem; //level designer is the one responsible

        double p_measure = UNUSED;
        double s_measure = UNUSED;


        public SpellCastEvent(HOGameScript behavior, Problem prob)
        {
            this.main = behavior;
            this.problem = prob;
            p_measure = this.problem.p_measure;
            s_measure = this.problem.s_measure;
        }


        public float GetFillPercentage()
        {
            double result;

            if (main.isFinalAnswer) //Base answer in the sum of all values of recordedAnswer
            {
                float x = main.recordedAnswer.Values.Sum();
                float y = main.inputAnswer;

                Debug.Log("Answer: " + x);

                return y / x;
            }

            switch (this.problem.problemShape)
            {
                case GameBehaviour.SHAPES.TRIANGLE:
                    result = (0.5 * this.p_measure * this.s_measure);
                    break;
                case GameBehaviour.SHAPES.CIRCLE:
                    result = (Math.PI * Math.Pow(p_measure / 2, 2));
                    break;
                case GameBehaviour.SHAPES.RECTANGLE:
                    result = (p_measure * this.s_measure);
                    break;
                case GameBehaviour.SHAPES.SQUARE:
                    result = Math.Pow(p_measure, 2);
                    break;
                case GameBehaviour.SHAPES.SEMI_CIRCLE:
                    result = (0.5 * Math.PI * Math.Pow(p_measure / 2, 2));
                    break;
                default:
                    throw new Exception("Invalid shape");
                    //throw this shit 
            }

            //12.4565735753735 => 1245
            float compX = (float)Math.Round(result, 2);
            float compY = main.inputAnswer;//int.Parse(this.main.manaMeasure.text);
            /**/
            /*UnityEngine.Debug.Log("X Measure = " + compX);
            //UnityEngine.Debug.Log("S Measure = " + s_measure);
            UnityEngine.Debug.Log("Mana Measure = " + this.main.manaMeasure.text);*/

            return compY / compX;

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
        Invoke(nameof(RemoveRoomText), TRANSITIONTIME);
        Invoke(nameof(PlayMusic), TRANSITIONTIME);
    }

    private void PlayMusic()
    {
        soundPlayer.PlayBGM(0, 1, 0.4f);
    }

    private void HideNewUI()
    {
        hud.SetActive(false);
        pDialogue.SetActive(false);
        panelMagicScroll.SetActive(false);
        quickMenu.SetActive(false);

        //Deactivate AreaDisplay
        areaDisplayObj.SetActive(false);
    }
    private void SetVisibilityNewUI(bool a, bool b, bool c, bool d)
    {
        hud.SetActive(a);
        pDialogue.SetActive(b);
        panelMagicScroll.SetActive(c);
        btnConfirmSpell.gameObject.SetActive(c);//copy magic sroll
        quickMenu.SetActive(d);
        Debug.Log("Line 1657 see if kita ba ung buttons sa right = " + b);
    }

    private void RemoveRoomText()
    {
        hud.SetActive(false);
    }

    /**Call to reset/set the level problem*/
    /*public void SetManualProblem(GameBehaviour.SHAPES shape, float x, float y = 1, int setSeed = -1)*/
    public void SetManualProblem(ShapeClickManager.ShapeClickData data)
    {
        currentShape = data.shapeType;

        Problem problem = new Problem(currentShape, this, data.originalShapeObject.actualShapeObj, data.originalShapeObject.x, data.originalShapeObject.y);

        this.spellCastEvent = new SpellCastEvent(this, problem);
    }

}