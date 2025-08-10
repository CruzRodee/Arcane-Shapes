using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;



public class GameBehaviour : MonoBehaviour
{
    const int UNUSED = -1;


    SHAPES currentShape;
    public SpellCastEvent spellCastEvent;
    TMP_Text correctionPerc;

    public ShapeGenerator shapeGenerator;
    public ShapeFiller shapeFiller;

    public LineSnapper lineSnapper;

    //NEW ADDITIONS: DELETE IN CASE EVERYTHING BREAKS

    private bool isUICamera = true;
    private bool y, n, m, s, cm, cp, ls, u, t, d, r, q; //Activity states of UI components
    private GameObject mainCamera, classroomCamera;
    private Material uiMaterial, classroomMaterial;
    private Animator screenFadeAnimator;
    private const float TRANSITIONTIME = 0.4f, FILLTIMEAPROX = 1.5f, STARTDELAY = 3.0f;
    private float ENDDELAY = 5.0f, SPELLDELAY = 2.0f;

    private AnimScript animScript;
    private bool STARTUP = true;
    public float error = 100f;
    private bool isQuit = false;
    private const float TRANSITIONDELAY = 0.5f;
    private const string correctShapePropmt = "Tama na ba ang shape na pinili?";

    //----------------------------------------------
    //////////Copied from old repo
    //my changes in case mag boom boom lahat
    private GameData savedGame;
    private SaveLoadController saverLoader = new SaveLoadController();
    private string savePath;

    private RectTransform rtDialogue;
    private RectTransform rtDiaButtons;
    // private RectTransform rtblackboard;

    private bool isDoneMeasuring;
    //panels na toggable, containers lang
    // public GameObject blackboard;

    public GameObject hud;
    public GameObject quickMenu;
    public GameObject panelMagicScroll;
    public GameObject pConfirm;
    public GameObject pLowerScroll;
    public GameObject pNotify;
    private GameObject pDialogue;
    private GameObject pDiaButtons;
    public GameObject notifyTextObj;
    public Text textTemp;
    public Text textEME;

    public GameObject textHint;
    public GameObject textHintSpell;
    public GameObject textHintUndo;
    public GameObject textHintCalcu;

    public GameObject spriteHint;
    public GameObject spriteHintUndo;
    public GameObject spriteHintSpell;
    public GameObject spriteHintCalcu;

    public GameObject pEquationTriangle;
    public GameObject pEquationSquare;
    public GameObject pEquationRectangle;
    public GameObject pEquationSCircle;
    public GameObject pEquationCircle;


    public Text pConfirmText;
    private Text pNotifyText;
    private Text textAns;
    private Text textAnsSC;
    private Text textAnsRect;
    private Text textAnsCir;
    private Text textAnsSqr;
    private Text textAnsTri;
    // private Text manaReq;
    private Text characterSay;
    private Text textFinish;

    public Text confirmText;
    public Text textHUD;


    public Image spriteHintImg;
    public Image spriteHintImgUndo;
    public Image spriteHintImgSpell;
    public Image spriteHintImgCalcu;

    public Button bYesHome;   //alternate buttons
    public Button bYes;
    public Button btnConfirmSpell;
    public Button btnMeasure;

    private string currentShapeToSolve;
    private string chosenShape;


    private const string castBtnText1 = "Cast Spell";
    private const string castBtnText2 = "Erase";
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

    //----------------------------------------------

    //References to the GUI display for line Lengths
    public GameObject sqVarDisp1, rectVarDisp1, rectVarDisp2, triVarDisp1, triVarDisp2, cirVarDisp1, semiVarDisp1;
    private GameObject var1Display, var2Display; //Variables for determining which ones will be modified

    //Sound related stuff
    public GameObject soundPlayerObj;
    private GameLevelSoundPlayer soundPlayer;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        savedGame = saverLoader.loadGame(savePath);

        currentShape = SHAPES.NONE;
        screenFadeAnimator = GameObject.Find("ScreenFade").GetComponent<Animator>();
        animScript = GameObject.Find("AnimHolder").GetComponent<AnimScript>();

        //Get OCR script
        ocrScript = ocrInput.transform.Find("DrawingAndOCRManager").GetComponent<DrawingAndOCRManagerScript>();

        //Get FA script
        fa = formulaAnalyzerObj.GetComponent<FormulaAnalyzer>();

        //Get notif text
        pNotifyText = notifyTextObj.GetComponent<Text>();

        //Get calcBtn text
        calcBtnText = calcBtnObj.transform.Find("textFinish").gameObject.GetComponent<Text>();

        //Disable calcBtn
        calcBtnObj.SetActive(false);

        //Get the text objects that will be used to display the line lengths of the shape
        switch (GlobalVariables.loSelectedShape)
        {
            case SHAPES.SQUARE:
                var1Display = sqVarDisp1;
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
                break;
            case SHAPES.SEMI_CIRCLE:
                var1Display = semiVarDisp1;
                break;
        }
    }

    /*
     * 
     * PivotToCenter = Pivot + SizeX/2 + SizeY/2
     * 
     * 
     */

    // _init() _ready()


    //debugging chenez
    // void Update(){
    //     textEME.text = "STATS\nisDoneMeasuring: " + isDoneMeasuring+"\nidkna";
    // }


    void Reset()
    {
        currentShape = SHAPES.NONE;

        correctionPerc.gameObject.SetActive(false);
        if (!STARTUP) // If not first run
            Destroy(this.spellCastEvent.problem.problemObjectShape);
        lineSnapper.gameObject.SetActive(false);

        lineSnapper.OnUndoPressed();
        lineSnapper.OnUndoPressed();

        //RUN SCREEN FADE IN FOR RESTART OF LEVEL
        if (!STARTUP)
            screenFadeAnimator.SetTrigger("fadeIn");

        //Start with watching the spawn anim again in reset
        ToClass();
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
                if (GlobalVariables.loSelectedShape == SHAPES.CIRCLE || GlobalVariables.loSelectedShape == SHAPES.SEMI_CIRCLE)
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
            case 3: //Same code as case 2 now
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
                /* //0.25 measures are broken so discard code
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
                */
        }

        //Make sure mearure2 not same as 1 if rectangle, simplify into subracting 1 from either 1 or 2 at random
        if (GlobalVariables.loSelectedShape == SHAPES.RECTANGLE && measure1 == measure2)
        {
            int coinFlip = UnityEngine.Random.Range(0, 2); // returns 0 or 1
            if (coinFlip == 0)
                measure1 = Mathf.Max(1, measure1 - 1);
            else if (coinFlip == 1)
                measure2 = Mathf.Max(1, measure2 - 1);
        }

        Debug.Log("Measure1: " + measure1 + " | Measure2: " + measure2);
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
    public void onRestart()
    {
        formulaDisplay.SetActive(false); //Disable this since its visible above the screenfade for some reason
        screenFadeAnimator.SetTrigger("sceneOut");


        Color color0 = spriteHintImgSpell.color;
        color0.a = 0f;
        spriteHintImgSpell.color = color0;

        Invoke(nameof(LoadSceneDelay), TRANSITIONDELAY);
    }
    private void LoadSceneDelay()
    {
        SceneManager.LoadScene("GameLevelScene_v1"); // Reload scene to avoid problems (Lazy and slightly slow but eh...)
    }

    public void onQuit()
    {
        error = 100f; //Prevent accidental saving due to 0f error
        //add "Do you want to return to main menu? Score won't be saved"
        // toggleConfirmScreen("confirmHome");

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
            if (temp == true) //nagtoggle ng active yung notif
            {
                HideMeasureHint();
            }
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
        backspaceButton.gameObject.SetActive(false);
        Debug.Log("It's here 460, button should not be visible...");
    }

    public void showDiaBoxAfterMeasuring()
    { //use only pag nag mmeasure, iba to sa mahahalf yung screen ah
        // StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new(600f, 130f)));
        StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new(225f, 130f)));
        Debug.Log("It's here 465, delete measure button here");
        // btnMeasure.gameObject.SetActive(false);
    }


    public void toggleDialogueBox()
    {
        // Vector2 RTAWAYTRANS = new(600f, -100f);
        // Vector2 PDIAAWAYTRANS = new(239, 150);
        // Vector2 RTONTRANS = new(600f, 100f);
        // Vector2 PDIAONTRANS = new(239, 123);

        // 600, -121.46 Y to hide the dialogue while measuring (since the shape cannot be moved)


        if (rtDialogue.anchoredPosition.y == 100)
        {
            // StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, RTAWAYTRANS));
            // StartCoroutine(RectTransformOverTime(pDiaButtons.GetComponent<RectTransform>(), DIALOGUESLIDETIME, PDIAAWAYTRANS));

            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new(600f, 130f)));
            // StartCoroutine(RectTransformOverTime(rtblackboard, OCRSLIDETIME, new(940f, 285f)));


            StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, new(-493f, -167f)));   //left mode
            Debug.Log("Line 491");

            // StartCoroutine(RectTransformOverTime(pDiaButtons.GetComponent<RectTransform>(), DIALOGUESLIDETIME, new(-493f, 223f)));   //experiemtn positions
            // pDialogue.y = -59;
        }
        else
        {

            // StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, origDiaRT));
            StartCoroutine(RectTransformOverTime(rtDialogue, DIALOGUESLIDETIME, new(600f, -151.46f)));    //when youy undo enough it should return here (nasa baba)
            // StartCoroutine(RectTransformOverTime(rtblackboard, OCRSLIDETIME, new(940f, -40f)));
            if (!isDoneMeasuring)
            {
                StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, new(-493f, -167f)));
                btnMeasure.gameObject.SetActive(true);
                backspaceButton.SetActive(false);
                //when you undo enough basically
                Debug.Log("kine 504"); // OMG IT'S HERE THE FKING PROBLEM IS HERE YALL
            }
            else
            {
                //IT"S HERE PART 2!!!!!! Im gonna kms
                btnMeasure.gameObject.SetActive(false);//show uli pag bumalik sa measuring kakaundo
                backspaceButton.SetActive(true);
                StartCoroutine(RectTransformOverTime(rtDiaButtons, DIALOGUESLIDETIME, new(-493f, 138f)));
                Debug.Log("Line 510");
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


    private void ShowHint(int step)
    {
        HideMeasureHint();
        // spriteHint.SetActive(false);
        // textHint.SetActive(false);
        // spriteHintUndo.SetActive(false);
        // textHintUndo.SetActive(false);
        // spriteHintSpell.SetActive(false);
        // textHintSpell.SetActive(false);
        // spriteHintCalcu.SetActive(false);
        // textHintCalcu.SetActive(false);

        //todo switch
        if (step == 0)
        {
            spriteHintSpell.SetActive(true);
            // textHintSpell.SetActive(true);
            StartCoroutine(BlinkSprite(step)); //blinmk
        }
        else if (step == 1)
        {
            spriteHint.SetActive(true);
            textHint.SetActive(true);
            StartCoroutine(BlinkSprite(step));  //sabay nalng
            spriteHintUndo.SetActive(true);
            textHintUndo.SetActive(true);
            StartCoroutine(BlinkSprite(step));
        }
        else if (step == 3)
        {
            textHintCalcu.SetActive(true);
            spriteHintCalcu.SetActive(true);
            StartCoroutine(BlinkSprite(step));

        }
        //dont think we need anything else pa naman
    }


    private void HideMeasureHint()
    {
        spriteHint.SetActive(false);
        textHint.SetActive(false);
        spriteHintUndo.SetActive(false);
        textHintUndo.SetActive(false);
        textHintCalcu.SetActive(false);
        spriteHintCalcu.SetActive(false);
        spriteHintSpell.SetActive(false);
        textHintSpell.SetActive(false);
    }

    private IEnumerator BlinkSprite(int step)
    {
        Color color1 = spriteHintImg.color;
        Color color2 = spriteHintImgUndo.color;
        Color color0 = spriteHintImgSpell.color;
        Color color3 = spriteHintImgCalcu.color;

        if (step == 0)
        {
            yield return new WaitForSeconds(3f);
            textHintSpell.SetActive(true);
            btnConfirmSpell.gameObject.SetActive(true); //make sure not to prematurely show up
        }
        float elapsed = 0f;
        while (true)
        {

            if (Input.GetMouseButtonDown(0))
            {
                //hide the sprite if screen s touched
                HideMeasureHint();
                yield break;
            }
            elapsed += Time.deltaTime;

            float alpha = Mathf.PingPong(elapsed, 1f); //smooth

            color0.a = alpha;
            spriteHintImgSpell.color = color0;
            color1.a = alpha;
            spriteHintImg.color = color1;
            color2.a = alpha;
            spriteHintImgUndo.color = color2;
            color3.a = alpha;
            spriteHintImgCalcu.color = color3;
            // if (step == 0 )
            // {
            //     // Color color=spriteHintImgSpell.color;  //this is yung click the spell book kineme
            //     color0.a = alpha;
            //     spriteHintImgSpell.color = color;
            // }
            // else if (step == 1)  //measure
            // {
            //     // Color color=spriteHintImg.color;
            //     color1.a = alpha;
            //     spriteHintImg.color = color;
            // }
            // else{
            //     // Color color=spriteHintImgUndo.color;
            //     color2.a = alpha;
            //     spriteHintImgUndo.color = color;
            // }

            yield return null;
        }
    }

    public void btnYes()
    {

        //IF SHAPE IS CORRECT:
        // SHAPES.SQUARE 
        // if ((int)this.spellCastEvent.problem.problemShape == currentOptionSelected + 1)
        // {
        //     //TODO: just read from the JSON file to figure out the current shape needed due to the room you entered - done eme
        UnityEngine.Debug.Log("chosenshape -> " + chosenShape);
        UnityEngine.Debug.Log("savedGame.currRoom -> " + savedGame.currRoom);

        //hide the spellbook after choosing

        if (savedGame.currRoom == chosenShape)
        {
            hideDiaBoxWhileMeasuring(); //this is new for only measurement
            ShowHint(1); // step 1 = 1 and 2
            btnConfirmSpell.gameObject.SetActive(false);
            panelMagicScroll.SetActive(false);
            //show na den undo and cast buttons
            pDiaButtons.SetActive(true);
            // toggleDialogueBox();




            // dropdown.gameObject.SetActive(false);
            lineSnapper.gameObject.SetActive(true);


            //show correct casting equation
            //not entering correctly

            if (chosenShape == "TRIANGLE")
            {
                pEquationTriangle.SetActive(true); //dont need this anymore
                //TODO: insert the tutorial here
                // characterSay.text = "Kailangan ko naman ngayong sukatin ang hugis gamit ang aking daliri!";
                // manaReq.text = "Katumbas na Mana";

            }
            else if (chosenShape == "SQUARE")
            {
                pEquationSquare.SetActive(true);
                // textAns = GameObject.Find("textAnsTri").GetComponent<Text>();
                // textAns = GameObject.Find("textAnsSqr").GetComponent<Text>();
                // textAns = GameObject.Find("textAnsRect").GetComponent<Text>();
                // textAns = GameObject.Find("textAnsCir").GetComponent<Text>();
                // textAns = GameObject.Find("textAnsSC").GetComponent<Text>();

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
        //added NTS

        //mali dat sa open ng oc to makikita



        if (!isDoneMeasuring)
        {
            // rTransform.anchoredPosition = new Vector2(rTransform.anchoredPosition.x, 100);  //it broke idk y



            //Check if Lines is maxed out first
            if (lineSnapper.lineCount != lineSnapper.GetMaxLinesForShape())
            {
                // TODO: DISPLAY POPUP OF NOT DONE MEASURING YET

                //QUIT BECAUSE BUTTON DOES NOTHING
                return;
            }

            //ALSO TODO: FIX LINE LENGTHS DISPLAY

            DoneMeasure();

        }
        else
        { //Reset the OCRInput board


            // btnMeasure.gameObject.SetActive(false);//hide for board after casting
            Debug.Log("Line 818, onCast");

            fa.ResetCalcDisp();
            fa.ResetAnalyzer();
            showDiaBoxAfterMeasuring(); //this is to bring up the dialogue box after measuring

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

    public void InputAnswer(float ans = 0f) //Sends final answer
    {
        inputAnswer = ans;

        toggleDialogueBox(); //hide                  

        //DisableNewUI
        HideNewUI();

        //hide all buttons at the end
        pDiaButtons.SetActive(false);



        //Disable OCR board and formulaDisplay
        StartCoroutine(SlideOCRBoard(false));

        CalcError();

        correctionPerc.text = "Error: " + Math.Round(Math.Abs(error), 2) + "%";
        //shapeFiller.InitializeFill(spellCastEvent.problem.problemObjectShape, Color.green, 0.5f, spellCastEvent.GetFillPercentage());

        correctionPerc.gameObject.SetActive(true); //Show error

        Invoke(nameof(CallCastAnimation), FILLTIMEAPROX + OCRSLIDETIME);
    }

    public void DoneMeasure()
    {

        isDoneMeasuring = true;
        textFinish.text = castBtnText2;
        //hide measure button
        // btnMeasure.gameObject.SetActive(false);
        Debug.Log("Line 869, DoneMeasuring");


        if (GlobalVariables.level < 3)
            calcBtnObj.SetActive(true); //Activate calculator button if less than level 3 during LO

        //Update dialogue displays for line lengths
        Debug.Log("value1: " + lineSnapper.value1 + "| value2: " + lineSnapper.value2);
        if (var1Display != null)
            var1Display.GetComponent<Text>().text = lineSnapper.value1; //IT's getting an error here
        if (var2Display != null)
            var2Display.GetComponent<Text>().text = lineSnapper.value2;

        //NEW OCR SHOW CODE
        StartCoroutine(SlideOCRBoard(true));
        lineSnapper.ToggleLineText(); //Toggle off
    }

    public void UndoMeasure()
    {
        //Reset line values based on linecount
        if (lineSnapper.lineCount >= 1)
            lineSnapper.value2 = "???";
        if (lineSnapper.lineCount < 1)
            lineSnapper.value1 = "???";

        //Hide OCR Board
        if (isDoneMeasuring)
        {
            // Debug.Log("Line 897, UndoMeasure");
            // btnMeasure.gameObject.SetActive(false);  //not this

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
            toggleDialogueBox(); //Show

            yield return new WaitForSeconds(DIALOGUESLIDETIME); //Wait for Dialogue Toggle

            ocrInput.SetActive(true); //Activate the board
            StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, rightEndTransObj.transform.position));

            //Slide and Scale Dialogue Box
            // StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, new(160f, 100f)));
            //adjusted
            StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, new(308f, 100f)));
            // StartCoroutine(RectTransformOverTime(rtblackboard, OCRSLIDETIME, new(285f, -40f)));

            StartCoroutine(LocalScaleOverTime(pDialogue, OCRSLIDETIME, new(0.9f, 0.9f, 0.9f)));

            Debug.Log("It's here 933");
        }
        else if (!show)
        {
            Debug.Log("It's here instead 937");
            ocrScript.processing = true; //Stop accepting input

            formulaDisplay.SetActive(false); //Hide OCR input Display

            StartCoroutine(MoveOverTime(ocrInput, OCRSLIDETIME, rightStartTransObj.transform.position));

            //Slide Dialogue Box
            StartCoroutine(RectTransformOverTime(rtDialogue, OCRSLIDETIME, new(225f, 130f)));

            // StartCoroutine(RectTransformOverTime(rtblackboard, OCRSLIDETIME, new(940f, -40f)));
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
            ShowHint(3);//done measuring na so show Hint for undo button for the first time
            characterSay.text = ""; //reset answer
            // backspaceButton.SetActive(true);
        }
        else if (!show)
        {
            ocrInput.SetActive(false); //Deactivate the board once off screen
            toggleDialogueBox(); //Hide

            // backspaceButton.SetActive(false);
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
        // manaReq = GameObject.Find("ManaRequired").GetComponent<Text>(); 
        textFinish = GameObject.Find("textFinish").GetComponent<Text>();
        textFinish.text = castBtnText1;

        isDoneMeasuring = false;
        textAnsSC = GameObject.Find("textAnsSC")?.GetComponent<Text>();
        textAnsRect = GameObject.Find("textAnsRect")?.GetComponent<Text>();
        textAnsCir = GameObject.Find("textAnsCir")?.GetComponent<Text>();
        textAnsSqr = GameObject.Find("textAnsSqr")?.GetComponent<Text>();
        textAnsTri = GameObject.Find("textAnsTri")?.GetComponent<Text>();

        pDialogue = GameObject.Find("PanelCasting");
        rtDialogue = pDialogue.GetComponent<RectTransform>();
        origDiaRT = rtDialogue.anchoredPosition; //Save original pos
        pDiaButtons = GameObject.Find("pDiaButtons");
        rtDiaButtons = pDiaButtons.GetComponent<RectTransform>();
        // rtblackboard = blackboard.GetComponent<RectTransform>();

        savedGame = saverLoader.loadGame(Path.Combine(Application.persistentDataPath, "saveData.json"));
        currentShapeToSolve = savedGame.currRoom;
        textHUD.text = savedGame.currRoom + " ROOM";



        currentShapeToSolve = savedGame.currRoom;



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

        spriteHint.SetActive(false);
        spriteHintSpell.SetActive(false);
        spriteHintCalcu.SetActive(false);

        textHint.SetActive(false);
        spriteHintUndo.SetActive(false);
        textHintUndo.SetActive(false);
        textHintCalcu.SetActive(false);
        textHintSpell.SetActive(false);

        btnConfirmSpell.gameObject.SetActive(false);
        btnMeasure.gameObject.SetActive(true);

        pConfirmText.text = "";

        bYesHome.gameObject.SetActive(false);
        bYes.gameObject.SetActive(false);

        ShowHint(0); //show hint after delay

        Color color0 = spriteHintImgSpell.color;
        color0.a = 0f;
        spriteHintImgSpell.color = color0; //transparent default kasi baka makita lol
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

        mainCamera = GameObject.Find("Main Camera");
        mainCamera.SetActive(false);
        classroomCamera = GameObject.Find("ClassroomCamera");
        classroomCamera.SetActive(true);

        lineSnapper.animScript = this.animScript;

        StartCoroutine(WaitForComponent());

        Debug.Log("Level: " + GlobalVariables.level);

        uiMaterial = Resources.Load<Material>("Materials/UI_Material");
        classroomMaterial = Resources.Load<Material>("Materials/ClassroomScreenMaterial");

        //----------------------------------------------

        correctionPerc.gameObject.SetActive(false);
        lineSnapper.gameObject.SetActive(false);

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

        //Play Fill SFX if good answer else play wrong sound
        if (error == 0f)
            soundPlayer.PlaySFX(2, 1, 1);
        else
            soundPlayer.PlaySFX(1, 1, 2f);
    }

    //TODO: Maybe use this to activate and deactivate cast button?, Reactivate and implement code if so
    //public void SetCastingState(bool state) 
    //{
    //    
    //}

    private void ToClass()
    {
        cp = correctionPerc.IsActive();
        ls = lineSnapper.gameObject.activeSelf;

        correctionPerc.gameObject.SetActive(false);
        lineSnapper.gameObject.SetActive(false);

        mainCamera.SetActive(false);
        classroomCamera.SetActive(true);

        isUICamera = false;
    }

    private void ToUI()
    {
        correctionPerc.gameObject.SetActive(cp);
        lineSnapper.gameObject.SetActive(ls);

        mainCamera.SetActive(true);
        classroomCamera.SetActive(false);

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
            Invoke(nameof(DelayedSpellAnimation), SPELLDELAY);
            //Call function to display a level complete/retry screen

            //TODO: ADD END SCREENN, Base delay from sd + ENDDELAY - 3.5f maybe?
            UnityEngine.Debug.Log("LEVEL COMPLETE!!!");
            float sd = animScript.spellDuration;
            Invoke(nameof(FadeDelay), sd + ENDDELAY - 2f);

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

    private void DelayedSpellAnimation()
    {
        animScript.CastSpell();
    }

    private void EndGameFunctions() //Function for saving data to save maybe? Also transitioning back to level select
    {
        // Save requisite data
        if (error == 0f)
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
            GlobalVariables.percent = Mathf.Clamp01(1 - Mathf.Abs(error) / 100f);
        }

        if (!isQuit) // Only activate flags if not quitting
        {
            GlobalVariables.gameFinished = true; //Set flag to save data
            GlobalVariables.isLOGame = true; //Flag game as LO game
        }

        //TRANSITION TO LEVEL SELECT SCREEN AGAIN
        SceneManager.LoadScene("LevelSelect");
    }

    //TODO: Maybe make a method that will be called to activate an end screen???

    private void CallCastAnimation()
    {
        screenFadeAnimator.SetTrigger("fade");

        Invoke(nameof(ToClass), TRANSITIONTIME);
        Invoke(nameof(DelayedCastAnimation), TRANSITIONTIME + 0.1f);
    }

    //----------------------------------------------


    // no need for a variable select to hold

    public enum SHAPES
    {

        NONE,
        TRIANGLE,
        SQUARE,
        RECTANGLE,
        CIRCLE,
        SEMI_CIRCLE,
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
                Debug.Log("Random Problem!");
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
                Debug.Log("Manual Problem!");

                p_measure = x;
                s_measure = y;

                Debug.Log("p_measure: " + p_measure + " | s_measure: " + s_measure);

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

    private void ActivateSpell(SHAPES s)
    {
        return;
        
        System.Random rand = new System.Random((int)DateTime.Now.Ticks);
        int limit = 0;

            switch (s)
            {
                case SHAPES.SQUARE:
                    limit = animScript.square_Levels.Length;
                    break;
                case SHAPES.RECTANGLE:
                    limit = animScript.rectangle_levels.Length;
                    break;
                case SHAPES.TRIANGLE:
                    limit = animScript.triangle_levels.Length;
                    break;
                case SHAPES.CIRCLE:
                    limit = animScript.circle_levels.Length;
                    break;
                case SHAPES.SEMI_CIRCLE:
                    limit = animScript.semicircle_levels.Length;
                    break;
                default:
                    UnityEngine.Debug.Log("Whoa, you're not supposed to be here (Spell Instance error: Invalid shape)");
                    //TEMPLATE
                    limit = animScript.semicircle_levels.Length;
                    break;
            }

        //SPELL
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

            switch (this.problem.problemShape)
            {
                case SHAPES.TRIANGLE:
                    result = (0.5 * this.p_measure * this.s_measure);
                    break;
                case SHAPES.CIRCLE:
                    result = (Math.PI * Math.Pow(p_measure / 2, 2));
                    break;
                case SHAPES.RECTANGLE:
                    result = (p_measure * this.s_measure);
                    break;
                case SHAPES.SQUARE:
                    result = Math.Pow(p_measure, 2);
                    break;
                case SHAPES.SEMI_CIRCLE:
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
            var go = GameObject.Find("ShapeGenerator");
            if (go != null)
                shapeGenerator = go.GetComponent<ShapeGenerator>();

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
        Invoke(nameof(ShowNewUI), TRANSITIONTIME);
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

        //DEBUG
        Debug.Log("Result: " + Math.Round(result, 2));

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

        this.spellCastEvent = new SpellCastEvent(this, problem);

        //Instantiate Spell Animation
        ActivateSpell(currentShape);
    }
}
