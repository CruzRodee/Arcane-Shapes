using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class LS_EventsScript : MonoBehaviour
{
    //SIDEBAR BUTTON
    public Button btnMute;
    public Button btnHome;
    public Button btnTutorial;  //TODO: Screenshot + Edit what those mean
    public Button btnGrimoire;

    //ROOMS BUTTON
    public Button btnLO;  //TODO: Screenshot + Edit what those mean
    public Button btnCompound;

    //Mute Related
    //private bool muted = false; // Use save data instead
    private Image btnMuteImg;
    public Sprite btnMutedSprite;
    public Sprite btnUnmutedSprite;
    private AudioSource bgmSrc;

    // Other
    public Text TextHUD;
    public Text loLevel, loTitle, loShapeTxt, hoLevel, hoTitle;

    //UI active
    public GameObject panelHallway;
    public GameObject panelDialogue;

    //NOTIFY SCREENS
    public GameObject pConfirmHome;

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

        //Load Save data stuff onto GlobalVariables
        savedGame = saverLoader.loadGame(savePath);
        if (savedGame == null)          // prevent NREs downstream, quit to main menu
        {
            //SceneChange to main menu
            screenFade.SetTrigger("sceneOut");  //NAG EERROR HERE
            Invoke(nameof(DelayedHomeLoad), TRANSITIONDELAY);
            return;
        }

        GlobalVariables.isMute = savedGame.prefMute;
        screenFade = GameObject.Find("ScreenFade").GetComponent<Animator>();

        btnMuteImg = btnMute.GetComponent<Image>(); //Get Image component
    }
    void Start()
    {
        screenFade.SetTrigger("sceneIn"); //Fade-in animation

        //hide notif screens
        pConfirmHome.SetActive(false);

        //Save Data after game
        if (GlobalVariables.gameFinished)
        {
            if (GlobalVariables.level == 0)
                GlobalVariables.level = 1; //Set level to 1 after playing
            if (GlobalVariables.playerWin && GlobalVariables.level < 6 && !GlobalVariables.isLOGame)
                GlobalVariables.level++; //Level up after win until 6 for HO
            else if (GlobalVariables.playerWin && GlobalVariables.level < 3)
                GlobalVariables.level++; //Level up after win until 3 for LO

            //Prestige/Loop again through HO Levels mechanic
            if (GlobalVariables.playerWin && GlobalVariables.level > 6 && !GlobalVariables.isLOGame)
            {
                GlobalVariables.level = 1;
                savedGame.compPres++;
            }

            //Save to GameData
            if (GlobalVariables.isLOGame) //Saving for LO game
            {
                savedGame.totalLOLevel++;
                int maxLOLevel = GlobalVariables.NUM_LO_LEVELS * 5 + 1; //3 levels, 5 shapes, 1 extra to ensure all done
                if(savedGame.totalLOLevel >= maxLOLevel)
                {
                    //Reset Levels
                    GlobalVariables.level = 0;
                    GlobalVariables.percent = 0f;
                    savedGame.totalLOLevel = 0;
                    savedGame.squareLvl = 0;
                    savedGame.rectLvl = 0;
                    savedGame.triLvl = 0;
                    savedGame.circleLvl = 0;
                    savedGame.scircleLvl = 0;

                    savedGame.loPres++; //Increase pres level
                }
                
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
                    default:
                        Debug.Log("LevelSelect: Error! Invalid shape!");
                        break;
                }
            }

            else if (!GlobalVariables.isLOGame) //Saving for HO Game
            {
                savedGame.compLvl = GlobalVariables.level;
            }

            //Reset trigger flags
            GlobalVariables.gameFinished = false;
            GlobalVariables.playerWin = false;
            GlobalVariables.isLOGame = false;

            // Save to JSON
            saverLoader.saveGame(savePath, savedGame);
        }

        //CHANGED: Debug.Log -> UnityEngine.Debug.Log
        UnityEngine.Debug.Log(savedGame.playerName);
        initLevels(savedGame);

        // 0 PASS UNLOCK ALL: ALL
        // 1 PASS LOWER: SIMPLE
        // 2 PASS HIGHER: COMPOUND
        if (savedGame.mode == 1)
        {
            btnCompound.interactable = false;
            //disable compound shapes button
        }
        else if (savedGame.mode == 2)
        {
            /*
            btnSemiCircle.interactable = false;
            btnCircle.interactable = false;
            btnSquare.interactable = false;
            btnTriangle.interactable = false;
            btnRectangle.interactable = false;
            */
        }


        playerNameText = GameObject.Find("DialogueCharNameText").GetComponent<Text>();

        //TODO: load the other levels here
        panelHallway = GameObject.Find("PanelHall");
        panelDialogue = GameObject.Find("PanelDialogue");

        TextHUD = GameObject.Find("DialogueCharNameText").GetComponent<Text>();




        bgmSrc = GameObject.Find("BGMAudioSource").GetComponent<AudioSource>();
        bgmSrc.Play();

        //Update mute button state and volume based on prefs
        if (!savedGame.prefMute)
        {
            if (btnUnmutedSprite != null)
                btnMuteImg.sprite = btnUnmutedSprite;
            bgmSrc.volume = GlobalVariables.defaultBGMVolume;
        }
        else
        {
            if (btnMutedSprite != null)
                btnMuteImg.sprite = btnMutedSprite;
            bgmSrc.volume = 0f;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void initLevels(GameData data)
    {
        GameObject.Find("DialogueCharNameText").GetComponent<Text>().text = data.playerName;

        // tempObject = GameObject.Find("TrianglePercent").GetComponent<Text>();
        // TriangleTitle
        //just populating the screen with these vars

        // Array of skill level texts
        string[] skillLevels = { "Walang Datos", "Baguhan", "Bihasa", "Dalubhasa" };

        //Determine what shape of LO right now
        int level = 0;
        if(savedGame.totalLOLevel < GlobalVariables.NUM_LO_LEVELS * 1 + 1) //Square
        {
            level = savedGame.squareLvl;
            loShapeTxt.text = "Square";
        }
        else if (savedGame.totalLOLevel < GlobalVariables.NUM_LO_LEVELS * 2 + 1) //Rect
        {
            level = savedGame.rectLvl;
            loShapeTxt.text = "Rectangle";
        }
        else if (savedGame.totalLOLevel < GlobalVariables.NUM_LO_LEVELS * 3 + 1) //Tri
        {
            level = savedGame.triLvl;
            loShapeTxt.text = "Triangle";
        }
        else if (savedGame.totalLOLevel < GlobalVariables.NUM_LO_LEVELS * 4 + 1) //Circ
        {
            level = savedGame.circleLvl;
            loShapeTxt.text = "Circle";
        }
        else if (savedGame.totalLOLevel < GlobalVariables.NUM_LO_LEVELS * 5 + 1) //Semicirc
        {
            level = savedGame.scircleLvl;
            loShapeTxt.text = "Semicircle";
        }

        loLevel.text = "Lvl " + level;
        //test
        switch (level)
        {
            case 0:
                loTitle.text = skillLevels[0] + $" - {savedGame.loPres}";
                break;
            case 1:
                loTitle.text = skillLevels[1] + $" - {savedGame.loPres}";
                break;
            case 2:
                loTitle.text = skillLevels[2] + $" - {savedGame.loPres}";
                break;
            case 3:
                loTitle.text = skillLevels[3] + $" - {savedGame.loPres}";
                break;
        }

        // if (GlobalVariables.IsHOUnlocked(savedGame)) // Unlock HO button     //NOTE: I removed the lock for the teacher's demo, pero we still need to lock it with scheduler once kids na
        // {
        btnCompound.GetComponent<Button>().interactable = true; // Activate button
        hoLevel.text = "Lvl " + data.compLvl;
        GameObject.Find("TextCompound").GetComponent<Text>().text = "Compound";
        switch (data.compLvl)
        {
            case 0:
                hoTitle.text = skillLevels[0] + $" - {savedGame.compPres}";
                break;
            case 1:
            case 2:
                hoTitle.text = skillLevels[1] + $" - {savedGame.compPres}";
                break;
            case 3:
            case 4:
                hoTitle.text = skillLevels[2] + $" - {savedGame.compPres}";
                break;
            case 5:
            case 6:
                hoTitle.text = skillLevels[3] + $" - {savedGame.compPres}";
                break;
        }
        // }
    }


    //////////// SIDE BAR BUTTONS

    public void toggleMute()
    {
        UnityEngine.Debug.Log("MUTE BUTTON PRESSED");
        if (savedGame == null)
        {
            savedGame = new GameData();        // initialise a fresh save or early-out
        }

        savedGame.prefMute = !savedGame.prefMute;     // invert state
        GlobalVariables.isMute = savedGame.prefMute;  // sync global flag

        UnityEngine.Debug.Log(savedGame.prefMute);

        if (!savedGame.prefMute)
        {
            if (btnUnmutedSprite != null)
                btnMuteImg.sprite = btnUnmutedSprite;
            bgmSrc.volume = GlobalVariables.defaultBGMVolume;
        }
        else
        {
            if (btnMutedSprite != null)
                btnMuteImg.sprite = btnMutedSprite;
            bgmSrc.volume = 0f;
        }

        saverLoader.saveGame(savePath, savedGame); // Save to remember mute state
    }

    public void closeConfirmPanelHome()
    {
        pConfirmHome.SetActive(false);
    }

    //button yes is clicked go back to main menu, no need to save progress
    public void loadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoMainMenu()
    {
        pConfirmHome.SetActive(true);
        Debug.Log("HOME BUTTON PRESSED, show ARE YOU SURE screen");
    }

    public void GoHome()
    {
        UnityEngine.Debug.Log("HOME BUTTON PRESSED, show ARE YOU SURE screen");

        //SceneChange to main menu
        screenFade.SetTrigger("sceneOut");
        saverLoader.saveGame(savePath, savedGame); // Save before quit
        Invoke(nameof(DelayedHomeLoad), TRANSITIONDELAY);
    }

    private void DelayedHomeLoad()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void showTutorial()
    {
        UnityEngine.Debug.Log("Show tutorial screenshot");
    }

    public void showGrimoire()
    {
        UnityEngine.Debug.Log("Show tutorial screenshot");
    }




    ////////////////////////////////////////
    ///// ENTERING ROOMS ///////////////////
    ////////////////////////////////////////

    private void DelayedRoomEnter()
    {
        GlobalVariables.nextLevel = "GameLevelScene_v1";
        SceneManager.LoadScene("LoadingScreen"); //Load Level scene
    }

    private void DelayedHORoomEnter()
    {
        GlobalVariables.nextLevel = "GameLevelScene_v3";
        SceneManager.LoadScene("LoadingScreen"); //Load Level scene
    }
    
    public void enterLowOrder()
    {
        if (savedGame.totalLOLevel < GlobalVariables.NUM_LO_LEVELS * 1 + 1) //Square
        {
            enterSquare();
        }
        else if (savedGame.totalLOLevel < GlobalVariables.NUM_LO_LEVELS * 2 + 1) //Rect
        {
            enterRectangle();
        }
        else if (savedGame.totalLOLevel < GlobalVariables.NUM_LO_LEVELS * 3 + 1) //Tri
        {
            enterTriangle();
        }
        else if (savedGame.totalLOLevel < GlobalVariables.NUM_LO_LEVELS * 4 + 1) //Circ
        {
            enterCircle();
        }
        else if (savedGame.totalLOLevel < GlobalVariables.NUM_LO_LEVELS * 5 + 1) //Semicirc
        {
            enterSemiCircle();
        }
    }

    public void enterRectangle()
    {
        UnityEngine.Debug.Log("Rectangle Room");

        //Load data
        GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.RECTANGLE;
        GlobalVariables.level = savedGame.rectLvl; //LOAD LEVEL DATA
        saverLoader.updateRoom(Path.Combine(Application.persistentDataPath, "saveData.json"), savedGame, "RECTANGLE");
        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
    }

    public void enterCircle()
    {
        UnityEngine.Debug.Log("Circle Room");

        //Load data
        GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.CIRCLE;
        GlobalVariables.level = savedGame.circleLvl; //LOAD LEVEL DATA
        saverLoader.updateRoom(Path.Combine(Application.persistentDataPath, "saveData.json"), savedGame, "CIRCLE");
        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
    }

    public void enterSquare()
    {
        UnityEngine.Debug.Log("Square Room");

        //Load data
        GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.SQUARE;
        GlobalVariables.level = savedGame.squareLvl; //LOAD LEVEL DATA
        saverLoader.updateRoom(Path.Combine(Application.persistentDataPath, "saveData.json"), savedGame, "SQUARE");
        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
    }
    public void enterTriangle()
    {
        UnityEngine.Debug.Log("Triangle Room");
        //Load data
        GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.TRIANGLE;
        GlobalVariables.level = savedGame.triLvl; //LOAD LEVEL DATA
        //SAVE THS TO JSON -> just a marker for next scene
        saverLoader.updateRoom(Path.Combine(Application.persistentDataPath, "saveData.json"), savedGame, "TRIANGLE");
        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
    }

    public void enterSemiCircle()
    {
        UnityEngine.Debug.Log("Semi-Circle Room");

        //Load data
        GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.SEMI_CIRCLE;
        GlobalVariables.level = savedGame.scircleLvl; //LOAD LEVEL DATA
        saverLoader.updateRoom(Path.Combine(Application.persistentDataPath, "saveData.json"), savedGame, "SEMI_CIRCLE");
        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
    }

    public void enterCompound()
    {
        UnityEngine.Debug.Log("Compound Floor, needs a lock for when lalaruin na nung mga kids");

        GlobalVariables.level = savedGame.compLvl; //LOAD LEVEL DATA
        //Set text as compound room
        saverLoader.updateRoom(Path.Combine(Application.persistentDataPath, "saveData.json"), savedGame, "COMPOUND");

        screenFade.SetTrigger("sceneOut");
        Invoke(nameof(DelayedHORoomEnter), TRANSITIONDELAY);
    }

    /*

        public void enterRectangle(){
            UnityEngine.Debug.Log("Rectangle Room");

            //Load data
            GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.RECTANGLE;
            GlobalVariables.level = savedGame.rectLvl; //LOAD LEVEL DATA

            screenFade.SetTrigger("sceneOut");
            Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
        }

        public void enterCircle(){
            UnityEngine.Debug.Log("Circle Room");

            //Load data
            GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.CIRCLE;
            GlobalVariables.level = savedGame.circleLvl; //LOAD LEVEL DATA

            screenFade.SetTrigger("sceneOut");
            Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
        }

        public void enterSquare(){
            UnityEngine.Debug.Log("Square Room");

            //Load data
            GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.SQUARE;
            GlobalVariables.level = savedGame.squareLvl; //LOAD LEVEL DATA

            screenFade.SetTrigger("sceneOut");
            Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
        }
        /*
        public void enterTriangle(){
            UnityEngine.Debug.Log("Triangle Room");
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
            UnityEngine.Debug.Log("Semi-Circle Room");

            //Load data
            GlobalVariables.loSelectedShape = GameBehaviour.SHAPES.SEMI_CIRCLE;
            GlobalVariables.level = savedGame.scircleLvl; //LOAD LEVEL DATA

            screenFade.SetTrigger("sceneOut");
            Invoke(nameof(DelayedRoomEnter), TRANSITIONDELAY);
        }

        public void enterCompound(){
            UnityEngine.Debug.Log("Compound Floor, check if complete all at least once");

            GlobalVariables.level = savedGame.compLvl; //LOAD LEVEL DATA

            screenFade.SetTrigger("sceneOut");
            Invoke(nameof(DelayedHORoomEnter), TRANSITIONDELAY);
        }
        */

    public void calcEquation()
    {
        //Randomizer based on range of easiness

    }

}
