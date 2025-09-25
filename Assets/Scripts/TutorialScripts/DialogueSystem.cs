using UnityEngine;
using TMPro; //Text TMP Pro
using System.Collections; //for IEnumerator
using System.Collections.Generic;//List
using UnityEngine.UI;   //Text



//NOTE_DIA: Don't forget to drop the prefab called "DialogueSystem" inside the game

public class DialogueSystem : MonoBehaviour
{
    #region Fields and Properties
    // //remove sa thesis game
    // [SerializeField] private MonoBehaviour cameraController;
    private Queue<SayModel> diaQueue = new Queue<SayModel>();
    private Coroutine diaCoroutine; //this is now the single coroutine thread

    [SerializeField] private bool isRunning = false;
    private float WAITSECONDS;
    private List<SayModel> messages;
    private List<ChoiceModel> choices;
    private Tut_UIEventsScript uiEvents;



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

    [SerializeField] private GameObject panelInputName;
    [SerializeField] private InputField inputField;

    [SerializeField] private TMP_Text textWho; //gameobject for the name of the character speaking
    [SerializeField] private TMP_Text textWhat; //game obvjec for what the char is saying

    [Header("Data Collection Integration")]
    [SerializeField] private bool enableDataCollection = true;
    private string currentQuestionKey = "";

    #endregion

    #region Models
    public class SayModel
    {
        public string code;
        public string charName;
        public string exp;
        public string msg;
        public List<ChoiceModel> choices;

        // Data collection fields
        public string inputType = ""; // "name", "age", "grade", "sex", "area_understanding", "square_area", "rectangle_area"
        public string questionKey = ""; // for tracking retries and context
    };

    [System.Serializable]
    public class ChoiceModel
    {
        public string choiceText;   //button text
        public string choiceCode;   //what the choice/branch is called for easier reference
    };


    public List<ChoiceModel> POPULATE_CHOICES_A()
    {
        List<ChoiceModel> choices = new List<ChoiceModel>();
        branchName = branchName + "A";

        choices.Add(new ChoiceModel { choiceText = "Jack Vessalius did nothing wrong.", choiceCode = "A" });
        choices.Add(new ChoiceModel { choiceText = "--- should not have been born.", choiceCode = "B" });
        choices.Add(new ChoiceModel { choiceText = "--- is an anomaly.", choiceCode = "C" });
        choices.Add(new ChoiceModel { choiceText = "This cruel beautiful world...", choiceCode = "D" });

        return choices;
    }

    #endregion

    #region Unity Lifecycle

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        WAITSECONDS = 3f;
        whoPlayer = "Estudyante"; //default name
        whoCaller = "Professor Oz";
        //describe the textbox to be instatiated here
        uiEvents = FindObjectOfType<Tut_UIEventsScript>();
    }

    void Update()
    {

    }

    #endregion

    #region Dialogue UI

    void SHOW_DIALOGUE()
    {
        dialogueBG.SetActive(true);
        dialogueContainer.SetActive(true);
    }
    //Is how I called it:
    // SHOW_CHOICE(POPULATE_CHOICES_A());

    void SHOW_CHOICE(List<ChoiceModel> choices)
    {
        foreach (ChoiceModel c in choices)
        {
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
        Debug.Log("Test line 122, should return the actual code of this specific choice. -> " + choiceCode + " AND FULL IS: " + branchName + choiceCode);
        tempChoice = choiceCode;

        // Data Collection part
        if (enableDataCollection)
            DataCollectionSystem.RecordChoice(choiceCode, currentQuestionKey);

        //delete the choice screen
        HIDE_CHOICE();
        talking = false; //this will continue the dialogue
    }

    void HIDE_CHOICE()
    {
        foreach (Transform child in pParentChoiceBTNS)
        {
            Destroy(child.gameObject);
        }
    }


    void HIDE_DIALOGUE()
    {
        talking = false;
        textWhat.text = "FINISHED TALKING";
        dialogueBG.SetActive(false);

        // ADD THIS FOR DATA COLLECTION:
        if (enableDataCollection)
            DataCollectionSystem.CompleteSession();
    }

    #endregion

    #region Dialogue Logic

    private void Choice(int index)
    {
        //  else if(code == "choice")
        // {
        //     //CANNOT skip through
        //     talking=false;
        //     Debug.Log(savedGame.playerName);

        //     //instatiate depending on how many choices there are
        // }
    }


    private void Say(int index, SayModel message)
    {     //from list  
        Debug.Log("Line 167 in Say() " + index + " ---------- CODE:" + message.code);
        string code = message.code;

        if (!string.IsNullOrEmpty(message.questionKey))
        {
            currentQuestionKey = message.questionKey;
            if (enableDataCollection)
                DataCollectionSystem.StartQuestion(message.questionKey);
        }

        if (code == "say")
        {
            // Using Data Collection System for Player Name
            string displayName = message.charName;
            string displayMsg = message.msg;

            if (enableDataCollection)
            {
                displayName = displayName.Replace("[Player Name]", DataCollectionSystem.GetPlayerName());
                displayMsg = displayMsg.Replace("[Player Name]", DataCollectionSystem.GetPlayerName());
            }

            textWho.text = displayName;
            textWhat.text = displayMsg;
            //todo edit the chat sprite to change exps to exp+.png
        }
        else if (code == "input")
        {
            // setting input context for data collection
            currentQuestionKey = message.questionKey;

            // btnSkip.interactable = false;    //cannot skip through
            panelInputName.SetActive(true);
            talking = true;
        }
        //ok so here is how the branching works:
        //choices A lead to A,B,C,D. If user picks A,
        // it leads to choice branch AA or dialogue branch of this name.
        else if (code == "choice")
        {
            // MODIFY CHOICES BASED ON DATA COLLECTION (for shape questions):
            List<ChoiceModel> choices = new List<ChoiceModel>();

            if (message.questionKey.StartsWith("shape_") && enableDataCollection)
            {
                string shapeName = message.questionKey.Replace("shape_", "");
                List<string> availableChoices = DataCollectionSystem.GetAvailableShapeChoices(shapeName);

                foreach (string choice in availableChoices)
                {
                    choices.Add(new ChoiceModel { choiceText = choice, choiceCode = choice });
                }
            }
            else
            {
                switch (message.msg)
                {
                    case ("A"):
                        choices = POPULATE_CHOICES_A();
                        break;
                    case ("ready_check"):
                        choices.Add(new ChoiceModel { choiceText = "Opo", choiceCode = "Opo" });
                        choices.Add(new ChoiceModel { choiceText = "Hindi", choiceCode = "Hindi" });
                        break;
                    case ("player_sex"):
                        choices.Add(new ChoiceModel { choiceText = "Lalaki", choiceCode = "Lalaki" });
                        choices.Add(new ChoiceModel { choiceText = "Babae", choiceCode = "Babae" });
                        break;
                    case ("area_known"):
                        choices.Add(new ChoiceModel { choiceText = "Opo", choiceCode = "Opo" });
                        choices.Add(new ChoiceModel { choiceText = "Hindi", choiceCode = "Hindi" });
                        break;
                    default:
                        Debug.Log("No choices available for this branch: " + message.msg);
                        break;
                }
            }

            SHOW_CHOICE(choices);
            talking = true;
        }
    }

    public void OnInputSubmitted()
    {
        // get input from the input field
        string inputValue = inputField.text.Trim();

        if (string.IsNullOrEmpty(inputValue))
            return;

        if (enableDataCollection)
        {
            string inputType = GetInputTypeFromQuestionKey(currentQuestionKey);

            // Validate numeric answers if needed
            if (inputType == "age" || inputType == "grade")
            {
                if (!int.TryParse(inputValue, out _))
                {
                    if (uiEvents != null)
                        uiEvents.ShowInputError("Numero lang ang dapat mong ilagay. (Ex. 5, 10, 15)");
                    return;
                }
            }
            if (inputType == "square_area" || inputType == "rectangle_area")
            {
                if (int.TryParse(inputValue, out int numericAnswer))
                {
                    int correctAnswer = (inputType == "square_area") ? 4 : 6;
                    bool isCorrect = DataCollectionSystem.ValidateNumericAnswer(inputType, numericAnswer, correctAnswer);

                    if (!isCorrect)
                    {
                        textWhat.text = "Mali ang iyong sagot. Pakinggan mo ulit ako ah!";
                        return;
                    }
                }
                else
                {
                    if (uiEvents != null)
                        uiEvents.ShowInputError("Numero lang ang dapat mong ilagay.");
                    return;
                }
            }

            DataCollectionSystem.RecordInput(inputType, inputValue, currentQuestionKey);

            if (currentQuestionKey == "player_name")
            {
                whoPlayer = DataCollectionSystem.GetPlayerName();
                UpdateWhoPlayerInMessages(whoPlayer);
                if (uiEvents != null)
                    uiEvents.UpdatePlayerNameInSave(whoPlayer);
            }
        }

        panelInputName.SetActive(false);
        inputField.text = ""; //clear the input field for next use
        talking = false; // This should trigger dialogue continuation

        if (diaCoroutine == null)
        {
            diaCoroutine = StartCoroutine(RunDialogue());
        }
    }
    private void UpdateWhoPlayerInMessages(string newName)
    {
        if (messages == null) return;
        foreach (var msg in messages)
        {
            if (msg.charName == whoPlayer || msg.charName == "Estudyante")
            {
                msg.charName = newName;
            }
        }
    }

    private string GetInputTypeFromQuestionKey(string questionKey)
    {
        switch (questionKey)
        {
            case "player_name": return "name";
            case "player_age": return "age";
            case "player_grade": return "grade";
            case "area_understanding": return "area_understanding";
            case "square_area": return "square_area";
            case "rectangle_area": return "rectangle_area";
            default: return "text";
        }
    }

    #endregion


    #region Dialogue Starters and Coroutine

    ///////////////////////////////////////////////////
    /// START DIALOGUES AND SETTING UP HERE
    /// ///////////////////////////////////////////////////


    //StartDialogue is what is being called from EventHandler.js
    //This is to access the private TALKS_X
    public void StartDialogue(int dialogueChapter)
    {
        //Numerical para di nakakalito yung calling
        switch (dialogueChapter)
        {
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
            case 3:
                {
                    messages = TALK_PREGAME_QUESTIONNAIRE_TEST();
                    break;
                }
            case 4:
                {
                    messages = TALK_INTRO_ARCANA_ACADEMY();
                    break;
                }
        }

        if (messages != null)
        {
            foreach (var msg in messages)
            {
                diaQueue.Enqueue(msg); //addto current queue of msgs
            }
        }
        if (!isRunning)
        {
            SHOW_DIALOGUE();
            diaCoroutine = StartCoroutine(RunDialogue());
        }
    }

    private IEnumerator RunDialogue()
    {
        isRunning = true;
        int index = 0;
        while (diaQueue.Count > 0)
        {
            SayModel currMsgs = diaQueue.Dequeue();
            Say(index, currMsgs);

            if (currMsgs.code == "say")
            {
                yield return new WaitForSeconds(WAITSECONDS);
            }
            else // "input" or "choice"
            {
                // Wait until player responds (talking becomes false)
                while (talking)
                {
                    yield return null;
                }
                // After player input/choice, reset talking for next message
                talking = true;
            }
            index++;
        }

        isRunning = false;
        HIDE_DIALOGUE();
    }
    #endregion

    #region Dialogue Scenarios  

    /////////////////////////////////////////
    /// DIALOGUES, INPUT HERE
    /////////////////////////////////////////

    private List<SayModel> TALK_DAY1_START()
    {
        List<SayModel> messages = new List<SayModel>();

        messages.Add(new SayModel { code = "say", charName = whoPlayer, msg = "Whew! Time to start the day..." });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "I should probably look around first to familiarize myself." });
        messages.Add(new SayModel { code = "say", charName = whoPlayer, msg = "Oh, there's a bulletin board. I should refer to that when making the drinks..." });
        return messages;
    }

    private List<SayModel> TALK_CHAPTER_1()
    {
        List<SayModel> messages = new List<SayModel>();

        messages.Add(new SayModel { code = "say", charName = whoPlayer, msg = "AAAAAA!" });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "BBBBBBBBBB" });
        messages.Add(new SayModel { code = "say", charName = whoPlayer, msg = "The delay is: " + WAITSECONDS + " seconds. There will be a choice next." });
        messages.Add(new SayModel { code = "choice", msg = "A" }); //msg in this case becomes the branch name. branch A has choices: A,B,C -> if you click A, branch becomes AA, and so on...
        return messages;
    }

    private List<SayModel> TALK_PREGAME_QUESTIONNAIRE_TEST()
    {
        List<SayModel> messages = new List<SayModel>();

        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Hello! Before we start, I have a few questions for you." });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "First, what is your name?" });
        messages.Add(new SayModel { code = "input", msg = "", questionKey = "player_name" });

        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Nice to meet you, [Player Name]!" });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "How old are you?" });
        messages.Add(new SayModel { code = "input", msg = "", questionKey = "player_age" });

        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "What grade are you in?" });
        messages.Add(new SayModel { code = "input", msg = "", questionKey = "player_grade" });

        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "What is your favorite shape?" });
        messages.Add(new SayModel { code = "choice", msg = "A" }); //choices: Square, Rectangle, Circle, Semicircle, Triangle

        return messages;
    }

    private List<SayModel> TALK_INTRO_ARCANA_ACADEMY()
    {
        List<SayModel> messages = new List<SayModel>();

        // Player intro
        messages.Add(new SayModel { code = "say", charName = whoPlayer, msg = "Grabe! Nandito na talaga ako sa Arcana Academy!" });
        messages.Add(new SayModel { code = "say", charName = whoPlayer, msg = "Sabi nila dito mo matututunan kung paano lumikha ng mga mahiwagang bagay gamit lang ang anyo at mana." });
        messages.Add(new SayModel { code = "say", charName = whoPlayer, msg = "Hindi pa rin ako makapaniwala… estudyante na ako dito!" });
        messages.Add(new SayModel { code = "say", charName = whoPlayer, msg = "Hmm... Saan kaya ako magsisimula?" });

        // Professor Oz enters
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Oy! Lumayo ka muna diyan!" });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ay, pasensya na! Halos tamaan ka ng lumilipad kong scroll. Minsan parang may sarili silang utak, hehe." });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Bago ka lang dito, 'no? Kita ko pa sa'yo yung kislap sa mata mo!" });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ako nga pala si Propesor Oz! Oo, wizard ako, pero wag kang mag-alala, hindi ako nangangagat!" });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ikaw? Anong pangalan mo?" });

        // Player name input (with questionKey)
        messages.Add(new SayModel { code = "input", charName = whoPlayer, msg = "Ah... Ako si...", questionKey = "player_name" });

        // After input, the player's name is now known
        messages.Add(new SayModel { code = "say", charName = whoPlayer, msg = "Ako si [Player Name]! Astig makilala ka, Propesor Oz!" });

        // Professor Oz welcomes player
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Aba, magaling, [Player Name]! Maligayang pagdating sa Arcana Academy!" });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Bago tayo magsimula, tanungin lang muna kita ng ilang bagay para sa rekord ng Academy." });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ready ka na ba? Pakinggan mo ako ng maigi ah!" });
        messages.Add(new SayModel { code = "choice", msg = "ready_check", questionKey = "ready_check" });

        // After ready_check, the Say() logic should branch based on the player's choice:
        // If "Opo"
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ayan! Ganyan ang gusto ko, handa at masigasig!", questionKey = "ready_yes" });
        // If "Hindi"
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ay, hindi ba? Well, wala kang choice! Haha!", questionKey = "ready_no" });

        // Repeat block (will be re-enqueued if "Hindi" is chosen)
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ready ka na ba?", questionKey = "ready_repeat" });
        messages.Add(new SayModel { code = "choice", msg = "ready_check", questionKey = "ready_check_repeat" });

        // Continue after "Opo" is chosen
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ilang taon ka na ba?" });
        messages.Add(new SayModel { code = "input", charName = whoPlayer, msg = "", questionKey = "player_age" });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ang bata mo pa pala! Sige, anong grade ka naman?" });
        messages.Add(new SayModel { code = "input", charName = whoPlayer, msg = "", questionKey = "player_grade" });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ah! Okay, hmm. . ." });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Babae ka ba o lalake, iho o iha? Medyo malabo yung mata ko pasensya na." });
        messages.Add(new SayModel { code = "choice", msg = "player_sex", questionKey = "player_sex" });

        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Okay! Sige, kilala na kita!" });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ngayon, magsimula tayo sa isang napakahalagang konsepto: area." });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Pero bago tayo magsimula, may mga tanong lang muna ako sayo..." });
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Excited ka na bang matuto?" });
        messages.Add(new SayModel { code = "choice", msg = "area_known", questionKey = "area_known" });

        // After area_known choice
        // If "Opo"
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ayan! Ganyan ang gusto ko—handa at masigasig!", questionKey = "area_yes" });
        // If "Hindi"
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ay, hindi ba? Well, wala kang choice! Haha!", questionKey = "area_no" });

        // Repeat block (will be re-enqueued if "Hindi" is chosen)
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Ready ka na ba?", questionKey = "area_repeat" });
        messages.Add(new SayModel { code = "choice", msg = "area_known", questionKey = "area_known_repeat" });

        // Continue after "Opo" is chosen for area_known
        messages.Add(new SayModel { code = "say", charName = whoCaller, msg = "Bago natin talakayin ang AREA ng mga hugis, kailangan alam mo muna ang mga tawag dito!" });

        return messages;
    }

    // Update is called once per frame

    #endregion
}
