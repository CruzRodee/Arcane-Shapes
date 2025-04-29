using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   //Text
using System.IO;
using UnityEngine.SceneManagement;

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
    // Start is called before the first frame update
    public Text textWho;
    public Text textWhat;
    public GameObject charImage;
    private bool skipped = false;
    private bool skipLine = false;
    private bool autoplay;
    public GameObject panelHall;
    public GameObject panelProceedYN;

    public GameObject panelInputName;
    private InputField nameInputField;

    public Button btnMute;
    public Button btnHome;
    
    public Button btnSkip;
    public Text TextHUD;

    private string playerName;
    private string profName;


    private List<SayModel> messages;
    
    private bool talking = true; 

    private bool muted = false; 
    private Image btnMuteImg;
    private AudioSource bgmSrc;

    private SaveLoadController saverLoader = new SaveLoadController();
    private GameData savedGame;

    private int msgIndex = 0;

    private Animator screenFade;
    private float TRANSITIONDELAY = 3.0f;

    private void Awake()
    {
        screenFade = GameObject.Find("ScreenFade").GetComponent<Animator>();
    }
    void Start()
    {
        screenFade.SetTrigger("sceneIn"); //Fade-in animation

        bgmSrc = GameObject.Find("BGMAudioSource").GetComponent<AudioSource>();
        bgmSrc.Play();
        btnMuteImg = btnMute.GetComponent<Image>();

        //load saved game if meron (automatic na meron if after main menu)
        // savedGame =  saverLoader.loadGame(Path.Combine(Application.persistentDataPath, "saveData.json"));
        savedGame =  saverLoader.loadGame(Path.Combine(Application.persistentDataPath, "saveData.json"));


        TextHUD.text = "Arcana Hallway I";
        // GameObject pHall = GameObject.Find("PanelHall").GetComponent<GameObject>();
        
        panelHall.SetActive(false); //hude all buttons from the hall if not skipping

        panelProceedYN.SetActive(false);
        panelInputName.SetActive(false);
        playerName = savedGame.playerName;
        profName = "Prof. Oz";
        messages = initDialogue();

        // initial
        textWho.text = messages[0].charName;
        textWhat.text = messages[0].msg;

        // initDialogue();//initialize msgs first once
    }
    private List<SayModel> initDialogue(){
        List<SayModel> messages = new List<SayModel>();
        messages.Add(new SayModel{code="say", charName=playerName, exp = "shock", msg="Wow! I'm finally here at Arcana Academy..."});
        messages.Add(new SayModel{code="say", charName=playerName, exp = "happy", msg="I wonder which class I'll be in?"});
        messages.Add(new SayModel{code="say", charName=profName, exp = "shock", msg="Duck!"});
        //cue magic sounds lol
        messages.Add(new SayModel{code="say", charName=profName, exp = "shock", msg="Hey there, student! Didn't see ya there..."});
        messages.Add(new SayModel{code="say", charName=profName, exp = "question", msg="Oh you must be new here, I can still see the wonder in your eyes!"});
        messages.Add(new SayModel{code="say", charName=profName, exp = "happy", msg="I'm Professor Oz, the Wizard, yes hehe that's me."});
        messages.Add(new SayModel{code="say", charName=profName, exp = "happy", msg="What's your name?"});

        messages.Add(new SayModel{code="say", charName=playerName, exp = "happy", msg="My name is..."});
        //cue input name
        messages.Add(new SayModel{code="input", msg="NAME"});
        messages.Add(new SayModel{code="say", charName=playerName, exp = "happy", msg="My name is..." + playerName+"!"});
        messages.Add(new SayModel{code="say", charName=profName, exp = "happy", msg="Okay, "+playerName+" Welcome to Arcana Academy! You can click on which Lesson you wanna enter. I'll see you in class!"});
        

        return messages;
    }

    public void confirmName()
    {
        nameInputField = GameObject.Find("NameInputField").GetComponent<InputField>();
        playerName = nameInputField.text;
        //after this, reload the messages list to contain the new playerName

        //check if working huhu TODO OKAY IT WORKS NOW
        saverLoader.saveGame(Path.Combine(Application.persistentDataPath, "saveData.json"), playerName, false,0,0,0,0,0,0,0,0,0,0,0);
        savedGame = saverLoader.loadGame(Path.Combine(Application.persistentDataPath, "saveData.json"));
        Debug.Log(savedGame.playerName);


        messages = initDialogue();
        //this wont reset the index dw

        talking=true;   //continue talking
        panelInputName.SetActive(false);


    }

    private void Say(int index){      
        //this func show the changes in the textboxes
        string who = messages[index].charName;
        string what = messages[index].msg;
        string code = messages[index].code;
        string exp = messages[index].exp;

        if (code == "say")
        {
            textWho.text = who;
            textWhat.text = what;
        //todo edit the chat sprite to change exps to exp+.png
            Debug.Log("Who: "+who+" : \""+what+"\"");

            btnSkip.interactable = true;

        }
        else if(code == "input")
        {
            btnSkip.interactable = false;

            //ask for player input
            panelInputName.SetActive(true);
            talking=false;
        }

        //is there gonna be another option? idk for now eh, making this with room for expansion in mind
        //say = say what
        //choice = choose from button
        //input = get text input
        //FOR THE GAMES I'll just make a new system for it ig
    }

    //basically +1 sa msgs index, to magaganap if naclick ung dialogue box
    public void nextLine(){
        if(talking)
        {
            if(msgIndex<messages.Count-1){
                msgIndex+=1;
                Say(msgIndex);
            }
            else{
                //end of lines
                talking = false;
                panelHall.SetActive(true);
                textWhat.text = "Which Class should I attend this time?";
                textWho.text = playerName;
                loadLevelSelect();
            }
        }
        else{
            btnSkip.interactable = false;
        }

        
    }

    public void SkipDialogue(){
        //skips everything and goes to the main hall for choosing which class
        talking=false;
        panelProceedYN.SetActive(true);
    }

    /*
     * Old Text: Redo Stage? The variables won't reset. Your Stage progress will be unaffected.
     */

    public void skipYes(){
        skipped=true;
        talking = false;
        panelHall.SetActive(true);

        textWhat.text = "Which Class should I attend this time?";
        textWho.text = playerName;  //TODO: load the playerName from txt file
        GameObject expBG = GameObject.Find("chatBubble");   //alrdy a gameobject / panel
        // expBG.sprite = Resources.Load<Sprite>("Sprites/UI Assets/chatHeart");


        panelProceedYN.SetActive(false);
        loadLevelSelect();
    }

    public void skipNo(){
        panelProceedYN.SetActive(false);
        talking = true;
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

    public void toggleMute(){
        Debug.Log("MUTE BUTTON PRESSED");
        muted = !muted;
        if (muted)
        {

            btnMuteImg.sprite = Resources.Load<Sprite>("/Sprites/UI Assets/ICONS/SoundsOFF.png");
            bgmSrc.volume = 1f;
        }
        else
        {
            btnMuteImg.sprite = Resources.Load<Sprite>("/Sprites/UI Assets/ICONS/SoundsON.png");
            bgmSrc.volume = 0f;
        }
    }
    public void GoHome(){
        Debug.Log("HOME BUTTON PRESSED, show ARE YOU SURE screen");
    }

    public void showTutorial(){
        Debug.Log("Show tutorial screenshot");
    }

    public void showGrimoire(){
        Debug.Log("Show tutorial screenshot");
    }


    // Update is called once per frame
    void Update()
    {
        
        // if(!talking) //check if panelHall is inactive, meaning the player didnt skipped the dialogue button yet ">>"
        // {
        //     talking = true;//checker, also para isa lang ung instance
        //     startScene_1(playerName, initDialogue());

        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         skipLine = true; //skipLine is when user clicks the screeb to skip 1 line.
        //         //in order to figure out if the player has let go
        //         counter+=1;
        //         Debug.Log("Debugging rn if this is reached x"+counter);
        //     }
        // }

        if(talking)
        {//inactive hall meaning ndi pa naskip
            btnSkip.interactable = true;

        }
        else{
            btnSkip.interactable = false;
        }
            // if(skipLine)
            // {
            //     skipLine=!skipLine; //player clicked last tick, avoid double click
            // }
            // else{
            //     if (Input.GetKeyDown(KeyCode.Space))
            //     {
            //         skipLine = true;
            //         yield return new WaitForSeconds(0.05f); //avoid accidental double click
            //     }
            // }
    }
}
