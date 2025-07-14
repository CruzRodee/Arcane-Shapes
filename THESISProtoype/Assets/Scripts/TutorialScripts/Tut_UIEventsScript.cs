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
    // Start is called before the first frame update
    public Text textWho;
    public Text textWhat;
    public GameObject charImage;
    private bool skipped = false;
    private bool skipLine = false;
    private bool autoplay;
    public GameObject panelHall;
    public GameObject panelProceedYN;
    public GameObject pConfirmHome;

    public GameObject panelInputName;
    private InputField nameInputField;

    public GameObject btnMute;  //have to be gameobj
    private Image btnMuteImg;
    public Sprite btnMutedSprite;
    public Sprite btnUnmutedSprite;
    public Button btnHome;

    public Button btnSkip;
    public Text TextHUD;

    private string playerName;
    private string profName;


    private List<SayModel> messages;

    private bool talking = true;

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
        profName = "Prof. Oz";
        messages = initDialogue();

        // initial
        textWho.text = messages[0].charName;
        textWhat.text = messages[0].msg;

        // initDialogue();//initialize msgs first once
    }
    private List<SayModel> initDialogue()
    {
        List<SayModel> messages = new List<SayModel>();

        messages.Add(new SayModel { code = "say", charName = playerName, msg = "Grabe! Nandito na talaga ako sa Arcana Academy!" });
        messages.Add(new SayModel { code = "say", charName = playerName, msg = "Sabi nila dito mo matututunan kung paano lumikha ng mga mahiwagang bagay gamit lang ang anyo at mana." });
        messages.Add(new SayModel { code = "say", charName = playerName, msg = "Hindi pa rin ako makapaniwala… estudyante na ako dito!" });
        messages.Add(new SayModel { code = "say", charName = playerName, msg = "Hmm… Saan kaya ako magsisimula?" });

        messages.Add(new SayModel { code = "say", charName = profName, msg = "Oy! Lumayo ka muna diyan!" });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "Ay, sorry! Halos tamaan ka ng lebitating scroll ko. Minsan mahirap kontrolin 'yan, hehe." });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "Bago ka lang dito, 'no? Kita ko pa sa'yo yung kislap sa mata mo!" });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "Ako nga pala si Propesor Oz! Oo, wizard ako—pero wag kang mag-alala, hindi ako nangangagat!" });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "Ikaw? Anong pangalan mo?" });

        messages.Add(new SayModel { code = "say", charName = playerName, msg = "Ah… Ako si..." });
        messages.Add(new SayModel { code = "input", msg = "NAME" });
        messages.Add(new SayModel { code = "say", charName = playerName, msg = "Ako si " + playerName + "! Astig makilala ka, Propesor Oz!" });

        messages.Add(new SayModel { code = "say", charName = profName, msg = "Aba, magaling, " + playerName + "! Maligayang pagdating sa Arcana Academy!" });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "Handa ka na bang matutong gumamit ng anyo at mana para makalikha ng kahit anong maisip mo?" });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "Dito sa Academy, tuturuan ka naming magbuo ng mga bagay mula sa simpleng hugis hanggang sa mga mas komplikadong disenyo." });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "At bawat klase, may mga leksyon na matututuhan mo, kaya siguraduhin subukan mong lahat!" });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "O siya, pumili ka lang ng pintuan na gusto mong pasukin. Kahit ano diyan!" });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "Naghihintay na ang mga guro sa loob, kaya huwag ka nang mahiyang pumasok." });


        messages.Add(new SayModel { code = "say", charName = profName, msg = "Ah, teka lang! Isang paalala… 'wag ka munang dadaan sa huling pintuan ha? 'Yun yung advanced na klase, at kailangan mo munang matutunan ang basics bago ka pumasok diyan." });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "Tsaka, tiwala lang. Pag ready ka na, ako na ang magsasabi sa'yo!" });
        messages.Add(new SayModel { code = "say", charName = profName, msg = "Oh siya, ingat ka, " + playerName + ", at good luck sa unang aralin mo!" });

        return messages;
    }

    public void confirmName()
    {
        nameInputField = GameObject.Find("NameInputField").GetComponent<InputField>();
        playerName = nameInputField.text;
        //after this, reload the messages list to contain the new playerName

        //check if working huhu TODO OKAY IT WORKS NOW
        // saverLoader.saveGame(Path.Combine(Application.persistentDataPath, "saveData.json"), playerName, false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "NONE");

        saverLoader.updateName(Path.Combine(Application.persistentDataPath, "saveData.json"), playerName);

        savedGame = saverLoader.loadGame(Path.Combine(Application.persistentDataPath, "saveData.json"));
        Debug.Log(savedGame.playerName);


        messages = initDialogue();
        //this wont reset the index dw

        talking = true;   //continue talking
        panelInputName.SetActive(false);


    }

    private void Say(int index)
    {
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
            Debug.Log("Who: " + who + " : \"" + what + "\"");

            btnSkip.interactable = true;

        }
        else if (code == "input")
        {
            btnSkip.interactable = false;

            //ask for player input
            panelInputName.SetActive(true);
            talking = false;
        }

        //is there gonna be another option? idk for now eh, making this with room for expansion in mind
        //say = say what
        //choice = choose from button
        //input = get text input
        //FOR THE GAMES I'll just make a new system for it ig
    }

    //basically +1 sa msgs index, to magaganap if naclick ung dialogue box
    public void nextLine()
    {
        if (talking)
        {
            if (msgIndex < messages.Count - 1)
            {
                msgIndex += 1;
                Say(msgIndex);
            }
            else
            {
                //end of lines
                talking = false;
                panelHall.SetActive(true);
                textWhat.text = "Which Class should I attend this time?";
                textWho.text = playerName;
                loadLevelSelect();
            }
        }
        else
        {
            btnSkip.interactable = false;
        }


    }

    public void SkipDialogue()
    {
        //skips everything and goes to the main hall for choosing which class
        talking = false;
        panelProceedYN.SetActive(true);
    }

    /*
     * Old Text: Redo Stage? The variables won't reset. Your Stage progress will be unaffected.
     */

    public void skipYes()
    {
        skipped = true;
        talking = false;
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
    public void closeConfirmPanelHome(){
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

        if (talking)
        {//inactive hall meaning ndi pa naskip
            btnSkip.interactable = true;

        }
        else
        {
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
