using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;


public class LS_EventsScript : MonoBehaviour
{
    //SIDEBAR BUTTON
    public Button btnMute;
    public Button btnHome;
    public Button btnTutorial;  //TODO: Screenshot + Edit what those mean
    public Button btnGrimoire;
    
    //ROOMS BUTTON
    public Button btnTriangle;  //TODO: Screenshot + Edit what those mean
    public Button btnSquare;
    public Button btnSemiCircle;
    public Button btnCircle;
    public Button btnRectangle;
    public Button btnCompound;

    //Mute Related
    //private bool muted = false; // Use save data instead
    private const float defaultVolume = 0.5f;
    private Image btnMuteImg;
    private AudioSource bgmSrc;

    // Other
    public Text TextHUD;


    //UI active
    public GameObject panelHallway;
    public GameObject panelDialogue;
    public GameObject panelMagicScroll;
    public GameObject panelCasting;


    //dialogue boxes for the spells that contain the equations
    private GameObject panelTriangle;
    private GameObject panelRectangle;
    private GameObject panelSquare;
    private GameObject panelCircle;
    private GameObject panelSCircle;
    private GameObject tempObject;

    // Saving
    private SaveLoadController saverLoader = new SaveLoadController();
    private GameData savedGame;
    private Text playerNameText;
    private string savePath;

    // Transition stuff
    private Animator screenFade;
    private const float TRANSITIONDELAY = 0.5f;

    private void Awake()
    {
        //LOAD THE JSON FILE HERE AND GET ALL INFO LIKE NAME ETC
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        savedGame = saverLoader.loadGame(savePath);

        screenFade = GameObject.Find("ScreenFade").GetComponent<Animator>();
    }
    void Start()
    {
        screenFade.SetTrigger("sceneIn"); //Fade-in animation

        panelTriangle = GameObject.Find("EquationTriangle");
        panelSquare = GameObject.Find("EquationSquare");
        panelCircle = GameObject.Find("EquationCircle");
        panelRectangle = GameObject.Find("EquationRect");
        panelSCircle = GameObject.Find("EquationSCircle");

        panelTriangle.SetActive(false);
        panelSquare.SetActive(false);
        panelRectangle.SetActive(false);
        panelCircle.SetActive(false);
        panelSCircle.SetActive(false);

        //Save Data after game
        if (GlobalVariables.gameFinished)
        {
            if (GlobalVariables.level == 0)
                GlobalVariables.level = 1; //Set level to 1 after playing
            if (GlobalVariables.playerWin && GlobalVariables.level < 3)
                GlobalVariables.level++; //Level up after win until 3

            

            //Save to GameData
            if(GlobalVariables.isLOGame) //Saving for LO game
                switch (GlobalVariables.loSelectedShape)
                {
                    case GameBehaviour.SHAPES.SQUARE:
                        savedGame.squareLvl = GlobalVariables.level;
                        savedGame.squarePercent = GlobalVariables.percent;
                        break;
                    case GameBehaviour.SHAPES.TRIANGLE:
                        savedGame.triLvl = GlobalVariables.level;
                        savedGame.triPercent = GlobalVariables.percent;
                        break;
                    case GameBehaviour.SHAPES.RECTANGLE:
                        savedGame.rectLvl = GlobalVariables.level;
                        savedGame.rectPercent = GlobalVariables.percent;
                        break;
                    case GameBehaviour.SHAPES.CIRCLE:
                        savedGame.circleLvl = GlobalVariables.level;
                        savedGame.circlePercent = GlobalVariables.percent;
                        break;
                    case GameBehaviour.SHAPES.SEMI_CIRCLE:
                        savedGame.scircleLvl = GlobalVariables.level;
                        savedGame.scirclePercent = GlobalVariables.percent;
                        break;
                }

            //Reset trigger flags
            GlobalVariables.gameFinished = false;
            GlobalVariables.playerWin = false;
            GlobalVariables.isLOGame = false;

            // Save to JSON
            saverLoader.saveGame(savePath, savedGame);
        }

        Debug.Log(savedGame.playerName);
        initLevels(savedGame);


        playerNameText = GameObject.Find("DialogueCharNameText").GetComponent<Text>();

        //TODO: load the other levels here
        panelHallway = GameObject.Find("PanelHall");
        panelDialogue = GameObject.Find("PanelDialogue");
        panelCasting = GameObject.Find("PanelCasting");
        panelMagicScroll = GameObject.Find("PanelMagicScroll");

        panelCasting.SetActive(false); //hide lang muna
        panelMagicScroll.SetActive(false); //hide lang muna

        TextHUD = GameObject.Find("DialogueCharNameText").GetComponent<Text>();




        bgmSrc = GameObject.Find("BGMAudioSource").GetComponent<AudioSource>();
        bgmSrc.Play();
        if (!savedGame.prefMute) // If not muted, play sound
            bgmSrc.volume = defaultVolume;
        else
            bgmSrc.volume = 0.0f;
        btnMuteImg = btnMute.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void initLevels(GameData data){
        GameObject.Find("DialogueCharNameText").GetComponent<Text>().text = data.playerName;

        // tempObject = GameObject.Find("TrianglePercent").GetComponent<Text>();
        // TriangleTitle
        //just populating the screen with these vars

        // Array of skill level texts
        string[] skillLevels = { "Walang Datos", "Baguhan", "Bihasa", "Dalubhasa" };
        
        GameObject.Find("TrianglePercent").GetComponent<Text>().text = "Lvl "+data.triLvl;
        //test
        switch(data.triLvl)
        {
            case 0:
                GameObject.Find("TriangleTitle").GetComponent<Text>().text = skillLevels[0];
                break;
            case 1:
                GameObject.Find("TriangleTitle").GetComponent<Text>().text = skillLevels[1];
                break;
            case 2:
                GameObject.Find("TriangleTitle").GetComponent<Text>().text = skillLevels[2];
                break;
            case 3:
                GameObject.Find("TriangleTitle").GetComponent<Text>().text = skillLevels[3];
                break;
        }

        GameObject.Find("SquarePercent").GetComponent<Text>().text = "Lvl " + data.squareLvl;
        //test
        switch (data.squareLvl)
        {
            case 0:
                GameObject.Find("SquareTitle").GetComponent<Text>().text = skillLevels[0];
                break;
            case 1:
                GameObject.Find("SquareTitle").GetComponent<Text>().text = skillLevels[1];
                break;
            case 2:
                GameObject.Find("SquareTitle").GetComponent<Text>().text = skillLevels[2];
                break;
            case 3:
                GameObject.Find("SquareTitle").GetComponent<Text>().text = skillLevels[3];
                break;
        }

        GameObject.Find("RectanglePercent").GetComponent<Text>().text = "Lvl " + data.rectLvl;
        //test
        switch (data.rectLvl)
        {
            case 0:
                GameObject.Find("RectangleTitle").GetComponent<Text>().text = skillLevels[0];
                break;
            case 1:
                GameObject.Find("RectangleTitle").GetComponent<Text>().text = skillLevels[1];
                break;
            case 2:
                GameObject.Find("RectangleTitle").GetComponent<Text>().text = skillLevels[2];
                break;
            case 3:
                GameObject.Find("RectangleTitle").GetComponent<Text>().text = skillLevels[3];
                break;
        }

        GameObject.Find("CirclePercent").GetComponent<Text>().text = "Lvl " + data.circleLvl;
        //test
        switch (data.circleLvl)
        {
            case 0:
                GameObject.Find("CircleTitle").GetComponent<Text>().text = skillLevels[0];
                break;
            case 1:
                GameObject.Find("CircleTitle").GetComponent<Text>().text = skillLevels[1];
                break;
            case 2:
                GameObject.Find("CircleTitle").GetComponent<Text>().text = skillLevels[2];
                break;
            case 3:
                GameObject.Find("CircleTitle").GetComponent<Text>().text = skillLevels[3];
                break;
        }

        GameObject.Find("SemiCirclePercent").GetComponent<Text>().text = "Lvl " + data.scircleLvl;
        //test
        switch (data.scircleLvl)
        {
            case 0:
                GameObject.Find("SemiCircleTitle").GetComponent<Text>().text = skillLevels[0];
                break;
            case 1:
                GameObject.Find("SemiCircleTitle").GetComponent<Text>().text = skillLevels[1];
                break;
            case 2:
                GameObject.Find("SemiCircleTitle").GetComponent<Text>().text = skillLevels[2];
                break;
            case 3:
                GameObject.Find("SemiCircleTitle").GetComponent<Text>().text = skillLevels[3];
                break;
        }

        if (GlobalVariables.IsHOUnlocked(savedGame)) // Unlock HO button
        {
            btnCompound.GetComponent<Button>().interactable = true; // Activate button
            GameObject.Find("CompoundLvl").GetComponent<Text>().text = "Lvl " + data.compLvl;
            GameObject.Find("TextCompound").GetComponent<Text>().text = "Compound";
            switch (data.compLvl)
            {
                case 0:
                    GameObject.Find("CompoundTitle").GetComponent<Text>().text = skillLevels[0];
                    break;
                case 1:
                    GameObject.Find("CompoundTitle").GetComponent<Text>().text = skillLevels[1];
                    break;
                case 2:
                    GameObject.Find("CompoundTitle").GetComponent<Text>().text = skillLevels[2];
                    break;
                case 3:
                    GameObject.Find("CompoundTitle").GetComponent<Text>().text = skillLevels[3];
                    break;
            }
        }
    }
 

//////////// SIDE BAR BUTTONS

    public void toggleMute(){
        Debug.Log("MUTE BUTTON PRESSED");
        savedGame.prefMute = !savedGame.prefMute; // Invert mute state

        Debug.Log(savedGame.prefMute);

        if (!savedGame.prefMute)
        {
            //idk where it is
            bgmSrc.volume = defaultVolume;
        }
        else
        {
            //blank sprite idkk
            bgmSrc.volume = 0f;
        }

        saverLoader.saveGame(savePath, savedGame); // Save to remember mute state
    }
    public void GoHome(){
        Debug.Log("HOME BUTTON PRESSED, show ARE YOU SURE screen");

        //SceneChange to main menu
        screenFade.SetTrigger("sceneOut");
        saverLoader.saveGame(savePath, savedGame); // Save before quit
        Invoke(nameof(DelayedHomeLoad), TRANSITIONDELAY);
    }

    private void DelayedHomeLoad()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void showTutorial(){
        Debug.Log("Show tutorial screenshot");
    }

    public void showGrimoire(){
        Debug.Log("Show tutorial screenshot");
    }




////////////////////////////////////////
///// ENTERING ROOMS ///////////////////
////////////////////////////////////////

    private void DelayedRoomEnter()
    {
        SceneManager.LoadScene("GameLevelScene_v1"); //Load Level scene
    }

    private void DelayedHORoomEnter()
    {
        SceneManager.LoadScene("HOGame"); //Load Level scene
    }

    public void enterRectangle(){
        Debug.Log("Rectangle Room");

        //Load data
        GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.RECTANGLE;
        GlobalVariables.level = savedGame.rectLvl; //LOAD LEVEL DATA

        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
    }

    public void enterCircle(){
        Debug.Log("Circle Room");

        //Load data
        GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.CIRCLE;
        GlobalVariables.level = savedGame.circleLvl; //LOAD LEVEL DATA

        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
    }

    public void enterSquare(){
        Debug.Log("Square Room");

        //Load data
        GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.SQUARE;
        GlobalVariables.level = savedGame.squareLvl; //LOAD LEVEL DATA

        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
    }
    public void enterTriangle(){
        Debug.Log("Triangle Room");
        //example muna naten to since eto namanna den ung nakagawa na
        //panelHallway.SetActive(false);
        //panelDialogue.SetActive(false);
        //panelCasting.SetActive(true);
        //panelMagicScroll.SetActive(true);
        //TextHUD.text = "Triangle Lv1"; //not yet loaded TODO
        //panelTriangle.SetActive(true);

        //TODO, add the animation snippet here first
        //for testig purposes muna to complete 1 level

        //Load data
        GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.TRIANGLE;
        GlobalVariables.level = savedGame.triLvl; //LOAD LEVEL DATA

        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
    }

    public void enterSemiCircle(){
        Debug.Log("Semi-Circle Room");

        //Load data
        GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.SEMI_CIRCLE;
        GlobalVariables.level = savedGame.scircleLvl; //LOAD LEVEL DATA

        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
    }

    public void enterCompound(){
        Debug.Log("Compound Floor, check if complete all at least once");

        GlobalVariables.level = savedGame.compLvl; //LOAD LEVEL DATA

        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedHORoomEnter), TRANSITIONDELAY);
    }

    public void calcEquation(){
        //Randomizer based on range of easiness
        
    }

}
