using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;   //Text

[System.Serializable]
public class SayModel
{
    public string code;
    public string charName;
    public string exp;
    public string msg;
};
public class Tut_UIEventsScript : MonoBehaviour
{
    //Dialogue Thing
    [SerializeField] private DialogueSystem Msger;

    // Start is called before the first frame update
    public Text textWho;
    public Text textWhat;
    public GameObject charImage;
    private bool skipped = false;
    public GameObject panelHall;
    public GameObject panelProceedYN;
    public GameObject pConfirmHome;

    public GameObject panelInputName;

    public GameObject btnMute;  //have to be gameobj
    private Image btnMuteImg;
    public Sprite btnMutedSprite;
    public Sprite btnUnmutedSprite;
    public Button btnHome;

    public Button btnSkip;
    public Text TextHUD;

    private string playerName;

    private AudioSource bgmSrc;

    private SaveLoadController saverLoader = new SaveLoadController();
    private GameData savedGame;

    private int msgIndex = 0;

    private Animator screenFade;
    private float TRANSITIONDELAY = 3.0f;
    private string savePath;

    private void Awake()
    {
        screenFade = GameObject.Find("ScreenFade").GetComponent<Animator>();
        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
    }
    void Start()
    {
        screenFade.SetTrigger("sceneIn"); //Fade-in animation

        bgmSrc = GameObject.Find("BGMAudioSource").GetComponent<AudioSource>();
        bgmSrc.Play();
        bgmSrc.volume = GlobalVariables.defaultBGMVolume;

        btnMuteImg = btnMute.GetComponent<Image>(); //Get component for manipulation later

        //load saved game if meron (automatic na meron if after main menu)
        // savedGame =  saverLoader.loadGame(Path.Combine(Application.persistentDataPath, "saveData.json"));
        savedGame = saverLoader.loadGame(savePath);


        TextHUD.text = "Arcana Hallway I";
        // GameObject pHall = GameObject.Find("PanelHall").GetComponent<GameObject>();

        panelHall.SetActive(false); //hude all buttons from the hall if not skipping
        panelProceedYN.SetActive(false);
        pConfirmHome.SetActive(false);
        panelInputName.SetActive(false);
        playerName = savedGame.playerName;

        //NEW Dialogue Thingy
        Msger.StartDialogue(4);
    }




    public void SkipDialogue()
    {
        //skips everything and goes to the main hall for choosing which class
        panelProceedYN.SetActive(true);
    }

    /*
     * Old Text: Redo Stage? The variables won't reset. Your Stage progress will be unaffected.
     */

    public void skipYes()
    {
        skipped = true;
        panelHall.SetActive(true);

        textWhat.text = "Which Class should I attend this time?";
        textWho.text = playerName;  //TODO: load the playerName from txt file
        GameObject expBG = GameObject.Find("chatBubble");   //alrdy a gameobject / panel
        // expBG.sprite = Resources.Load<Sprite>("Sprites/UI Assets/chatHeart");


        panelProceedYN.SetActive(false);
        loadLevelSelect();
    }

    public void skipNo()
    {
        panelProceedYN.SetActive(false);
    }

    public void loadLevelSelect()
    {
        //TODO: SAVE playerName to JSON
        //save mute and other settings din
        if (skipped)
            TRANSITIONDELAY -= 2.4f; //Load faster if skipped
        Invoke(nameof(DelayedSceneOut), TRANSITIONDELAY - 0.5f);
        Invoke(nameof(DelayedLS), TRANSITIONDELAY);
        //for this to keep working tho need ko na matapos ung load save from json cuz thats how theyre gonna interact eme
    }

    private void DelayedLS()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    private void DelayedSceneOut()
    {
        screenFade.SetTrigger("sceneOut");
    }

    //////////// OTHER BUTTONS

    public void toggleMute()
    {
        Debug.Log("MUTE BUTTON PRESSED");

        if (savedGame == null)
        {
            savedGame = new GameData();        // initialise a fresh save or early-out
        }

        savedGame.prefMute = !savedGame.prefMute;     // invert state
        GlobalVariables.isMute = savedGame.prefMute;  // sync global flag

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

    //button no is clicked, stau on scene
    public void closeConfirmPanelHome()
    {
        pConfirmHome.SetActive(false);
    }

    //button yes is clicked go back to main menu, no need to save progress
    public void loadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void GoHome()
    {
        pConfirmHome.SetActive(true);
        Debug.Log("HOME BUTTON PRESSED, show ARE YOU SURE screen");
    }

    public void showTutorial()
    {
        Debug.Log("Show tutorial screenshot");
    }

    public void showGrimoire()
    {
        Debug.Log("Show tutorial screenshot");
    }

    public void UpdatePlayerNameInSave(string newName)
    {
        if (savedGame == null)
            savedGame = new GameData();

        savedGame.playerName = newName;
        saverLoader.saveGame(savePath, savedGame);
    }


    // Update is called once per frame
    void Update()
    {

    }
}
