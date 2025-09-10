using UnityEngine;
using TMPro; //Text TMP Pro
using System.Collections; //for IEnumerator
using System.Collections.Generic;//List
using UnityEngine.UI;   //Text


//NOTE_DIA: Don't forget to drop the prefab called "DialogueSystem" inside the game
public class DialogueSystem : MonoBehaviour
{
    // //remove sa thesis game
    // [SerializeField] private MonoBehaviour cameraController;

    private float WAITSECONDS;
    private List<SayModel> messages;
    private List<ChoiceModel> choices;

    //NOTE_DIA: When you're using the sys don;t forget to drop the prefabs with the style inside the inspector window
    //NOTE_DIA: ALSO SEMI-IMPORTANT; use TMP_Text for the text inside the prefab kek
    [SerializeField] private GameObject prefabChoice;
    [SerializeField] private Transform pParentChoiceBTNS;
    private string tempChoice;

    private bool talking = true; 
    private bool muted = false; //no voiceover
    private int msgIndex = 0; //pointer of msg list
    private string whoPlayer;
    private string whoCaller;

    private string branchName;

//  TODO, instead of refering to each one, just say all children here cuz it's for show and hide lang
//   unlesss? May gagawing animations lol
    [SerializeField] private GameObject dialogueBG;
    [SerializeField] private GameObject dialogueContainer;

    // [SerializeField] private GameObject pParent;
    [SerializeField] private TMP_Text textWho; //gameobject for the name of the character speaking
    [SerializeField] private TMP_Text textWhat; //game obvjec for what the char is saying

    [System.Serializable]
    public class SayModel
    {
        public string code;
        public string charName;
        public string exp;
        public string msg;
        public List<ChoiceModel> choices;
    };

    [System.Serializable]
    public class ChoiceModel
    {
        public string choiceText;   //button text
        public string choiceCode;   //what the choice/branch is called for easier reference
    };


    public List<ChoiceModel> POPULATE_CHOICES_A(){
        List<ChoiceModel> choices = new List<ChoiceModel>();
        branchName = branchName+"A";

        choices.Add(new ChoiceModel{choiceText="Jack Vessalius did nothing wrong.", choiceCode="A"});
        choices.Add(new ChoiceModel{choiceText="--- should not have been born.", choiceCode="B"});
        choices.Add(new ChoiceModel{choiceText="--- is an anomaly.", choiceCode="C"});
        choices.Add(new ChoiceModel{choiceText="This cruel beautiful world...", choiceCode="D"});

        return choices;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WAITSECONDS = 3f;
        whoPlayer = "Player naem";
        whoCaller = "Caller";
        //describe the textbox to be instatiated here
    }

    void SHOW_DIALOGUE(){
        dialogueBG.SetActive(true);
        dialogueContainer.SetActive(true);
        //instatiate the dialogue box here.
        //there should also be destroy one, called HIDE_)DIALOGUE
        //NAH no need cuz theres only 1 bg so I used a random black fade up pic (i made it in firealpaca)
        // GameObject containerDialogue = new GameObject("Dynamic Image");
        // RectTransform containerDialogue_RT = containerDialogue.GetComponent<RectTransform>();
        // containerDialogue_RT = new Vector2(0,0); //to fix later


        /*
            SAY SCREEN:
                        "CharName:"
            "Text abab bab xcvsx gs gdgsababah
            sdbamsb asbdjah bajbda sba sadb d."

            CHOICE SCREEN:
            > I don't know
            > Ok, goodbye...
            > Cool!

            INPUT SCREEN:
            > |
        */


    }
    //Is how I called it:
    // SHOW_CHOICE(POPULATE_CHOICES_A());

    void SHOW_CHOICE(List<ChoiceModel> choices){
        foreach (ChoiceModel c in choices){
            GameObject pChoice = Instantiate(prefabChoice, pParentChoiceBTNS);
            TMP_Text txtChoice = pChoice.GetComponentInChildren<TMP_Text>();
            txtChoice.text = c.choiceText;  //set the text in the new button
            
            Button btnChoice = pChoice.GetComponentInChildren<Button>();
            string code = c.choiceCode;
            btnChoice.onClick.AddListener(() => OnChoiceSelected(code)); //set temp lanf
        }
    }

    void OnChoiceSelected(string choiceCode)
    {
        //TODO: instruct the thing to branch out to this specific branch
        Debug.Log("Test line 122, should return the actual code of this specific choice. -> "+choiceCode+" AND FULL IS: "+branchName+choiceCode);
        tempChoice = choiceCode;
        
        //delete the choice screen
        HIDE_CHOICE();
        
        // //tas enable uli yung walking (REMOVE SA THESIS GAME)
        // cameraController.enabled = true;
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    void HIDE_CHOICE()
    {
        foreach (Transform child in pParentChoiceBTNS)
            {
                Destroy(child.gameObject);
            }
    }


    void HIDE_DIALOGUE(){
        talking = false;
        textWhat.text = "FINISHED TALKING";
        dialogueBG.SetActive(false);
    }

    private void Choice(int index){
        //  else if(code == "choice")
        // {
        //     //CANNOT skip through
        //     talking=false;
        //     Debug.Log(savedGame.playerName);

        //     //instatiate depending on how many choices there are
        // }
    }

    private void Say(int index, List<SayModel> messages){      
        Debug.Log("Line 100 in Say() "+index+" ---------- CODE:"+ messages[index].code);
        string code = messages[index].code;

        if (code == "say")
        {
            textWho.text = messages[index].charName;
            textWhat.text = messages[index].msg;

        //todo edit the chat sprite to change exps to exp+.png
            Debug.Log("Who: "+textWho.text+" : \""+textWhat .text+"\"");

            // btnSkip.interactable = true;     //can skip through
            StartCoroutine(WaitForNextLine(index, messages, WAITSECONDS));
        }
        else if(code == "input")
        {
            // btnSkip.interactable = false;    //cannot skip through

            //ask for player input
            // panelInputName.SetActive(true);
            talking=false;
        }
        else if(code=="choice")
        {
            //ok so here is how the branching works:
            //choices A lead to A,B,C,D. If user picks A,
            // it leads to choice branch AA or dialogue branch of this name.
            switch(messages[index].msg){
                case("A"):
                    SHOW_CHOICE(POPULATE_CHOICES_A());
                    break;
            }
            
        }
    }

    //TODO AUTOMATE NEXT LINE AFTER WAITING CERTAIN SECONDS
    //error CS0305: Using the generic type 'IEnumerator<T>' requires 1 type arguments
    private IEnumerator WaitForNextLine(int currentIndex, List<SayModel> msgs, float delay){
        yield return new WaitForSeconds(delay);
        nextLine(currentIndex, msgs);
    }

    //basically +1 sa msgs index, to magaganap if naclick ung dialogue box
    public void nextLine(int msgIndex, List<SayModel> messages){
        if(talking)
        {
            Debug.Log("Line 138 in NEXT LINE!!");

            if(msgIndex<messages.Count-1){
                msgIndex+=1;
                Say(msgIndex, messages);
            }
            else{
                //end of lines
                talking = false;
                HIDE_DIALOGUE();
            }
        }
        // else{
            // btnSkip.interactable = false;
        // }

        
    }




    ///////////////////////////////////////////////////
    /// START DIALOGUES AND SETTING UP HERE
    /// ///////////////////////////////////////////////////
    
    
    //StartDialogue is what is being called from EventHandler.js
    //This is to access the private TALKS_X
    public void StartDialogue(int dialogueChapter)
    {
        switch(dialogueChapter){
            case 1:
            {
                messages = TALK_CHAPTER_1();
                break;

            }
            case 2:
            {
                messages = TALK_DAY1_START();
                break;
            }
        }
        msgIndex = 0;
        talking = true;
        SHOW_DIALOGUE();
        Debug.Log("Line 259: Show_DIALOGUE");
        Say(msgIndex, messages); //start saying! Pass the index pointer and the list of msges
    }


    private List<SayModel> TALK_DAY1_START(){
        List<SayModel> messages = new List<SayModel>();
        
        messages.Add(new SayModel{code="say", charName=whoPlayer, msg="Whew! Time to start the day..."});
        messages.Add(new SayModel{code="say", charName=whoCaller, msg="I should probably look around first to familiarize myself."});
        messages.Add(new SayModel{code="say", charName=whoPlayer, msg="Oh, there's a bulletin board. I should refer to that when making the drinks..."});
        return messages;
    }

    private List<SayModel> TALK_CHAPTER_1(){
        List<SayModel> messages = new List<SayModel>();
        
        messages.Add(new SayModel{code="say", charName=whoPlayer, msg="AAAAAA!"});
        messages.Add(new SayModel{code="say", charName=whoCaller, msg="BBBBBBBBBB"});
        messages.Add(new SayModel{code="say", charName=whoPlayer, msg="The delay is: "+WAITSECONDS+" seconds. There will be a choice next."});
        messages.Add(new SayModel{code="choice", msg="A"}); //msg in this case becomes the branch name. branch A has choices: A,B,C -> if you click A, branch becomes AA, and so on...
        return messages;
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
