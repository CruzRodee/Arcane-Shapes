using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;   //text
//save game file

public class MM_UIScript : MonoBehaviour
{
    private Text overlayText;
    public GameObject overlayPanel;
    public Button btnContinue;
    public GameObject panelNotify;
    public GameObject graphy;

    public GameObject panelInputPass;
    public InputField passwordInputField;

    private string savePath;
    private bool gameExists = false;
    // Start is called before the first frame update

    private GameData savedGame;
    private int mode;

    private SaveLoadController saverLoader = new SaveLoadController();

    private Animator screenFade;
    private const float TRANSITIONDELAY = 1.2f;
    private bool canQuit = true;

    //For the credits screen dont delete
    public GameObject panelCredits;

    public void ToggleCredits()
    {
        if (panelCredits != null)
        {
            panelCredits.SetActive(false);
        }
        else
        {
            panelCredits.SetActive(true);
        }
    }

    private void Awake()
    {
        screenFade = GameObject.Find("ScreenFade").GetComponent<Animator>();
        overlayText = GameObject.Find("TextOverlay").GetComponent<Text>(); //Cache this instead of too many Find() calls

        if (!Debug.isDebugBuild) //Disable all debug stuff
        {
            //Disable and Remove Graphy when not debug
            if (graphy != null)
            {
                graphy.SetActive(false);
                Destroy(graphy);
            }

            Debug.unityLogger.logEnabled = false;
        }
    }
    void Start()
    {
        screenFade.SetTrigger("sceneIn"); //Fade-in animation

        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");

        savedGame = saverLoader.loadGame(savePath);
        if (savedGame != null)
        {
            gameExists = true;
            btnContinue.interactable = true;
            GlobalVariables.isMute = savedGame.prefMute;
        }
        else
        {
            btnContinue.interactable = false;
            GlobalVariables.isMute = false;
        }
        overlayPanel.SetActive(false);
        panelNotify.SetActive(false);
        panelCredits.SetActive(false);

        //password hardcoded
        panelInputPass.SetActive(false);

        // mode = -1;

    }

    public void openPassField()
    {    //made it separate so I can assign the buttons to this
        panelInputPass.SetActive(true);
    }

    public void passwordWrong()
    {
        panelInputPass.SetActive(false);
        panelNotify.SetActive(true);
        overlayText.text = "Ang Password ay mali, maaring hintayin ang Guro upang malaman ang Password...";
        // panelNotify.SetActive(false); //had to do this cuz the second usage needs to stay on screen so they dont click anything else
    }

    public void notifyDeleteGame()
    {
        panelInputPass.SetActive(false);
        panelNotify.SetActive(true);
        overlayText.text = "RESET GAME COMPLETE. Binura na ang Saved Game.";
        //make continue not interactalble
        btnContinue.interactable = false;
        saverLoader.resetGame(Path.Combine(Application.persistentDataPath, "saveData.json"));
    }

    public void checkPassword()
    {
        // panelNotify.SetActive(false);

        // PASS UNLOCK ALL: ALL
        // PASS LOWER: SIMPLE
        // PASS HIGHER: COMPOUND

        panelInputPass.SetActive(false);
        if (passwordInputField.text == "ALL")
        {
            // saverLoader.saveGame(Path.Combine(Application.persistentDataPath, "saveData.json"), 0); //ON SECOND THOUGHTSCRAP THIS LAWL
            mode = 0;
            // saverLoader.updateMode(Path.Combine(Application.persistentDataPath, "saveData.json"), mode);
            LoadFirstScene();
        }
        else if (passwordInputField.text == "SIMPLE")
        {
            mode = 1;
            // saverLoader.updateMode(Path.Combine(Application.persistentDataPath, "saveData.json"), mode);
            // saverLoader.saveGame(Path.Combine(Application.persistentDataPath, "saveData.json"), 1);
            LoadFirstScene();
        }
        else if (passwordInputField.text == "COMPOUND")
        {
            mode = 2;
            // saverLoader.updateMode(Path.Combine(Application.persistentDataPath, "saveData.json"), mode);
            // saverLoader.saveGame(Path.Combine(Application.persistentDataPath, "saveData.json"), 2);
            LoadFirstScene();
        }
        else if (passwordInputField.text == "RESET")
        {
            notifyDeleteGame();
        }

        else
        {
            passwordWrong();

            //notify screen say PASSWORD INVALID. PLS ASK COORDINATOR FOR PASSWORD
        }

    }

    public void DoContinue()
    {
        canQuit = false;
        Debug.Log("CONTINUE");
        panelNotify.SetActive(true); //nts: always set active true because if inactive ndi makikita ung children comps

        overlayText.text = "Saved game Loaded!";
        //Jump to the game immediately (load all saved data)
        LoadHallScene(); //Data loaded at start, continue button disabled by default so fast fingers cant press accidentalt
    }

    public void DoNewGame()
    {
        canQuit = false;
        if (gameExists)
        {
            overlayPanel.SetActive(true);
            overlayText.text = "Magsimula ng bagong laro? Ang lumang Saved Game ay hindi na maaaring ituloy gawa nito.";
        }
        else //no previous game yet
            openPassField();
        // LoadFirstScene();
        // Moved the rest to to LoadFirstScene since that is where new games always go anyways
    }

    public void DoCredits()
    {

        if (panelCredits.activeInHierarchy)
        {
            panelCredits.SetActive(false);
            canQuit = true;
            Debug.Log("Close Credits");
        }
        else
        {
            panelCredits.SetActive(true);
            canQuit = false;
            Debug.Log("Open Credits");
        }
    }

    public void LoadFirstScene()
    {
        Debug.Log("new game");
        panelNotify.SetActive(true);

        overlayText.text = "Handa nang magsimula ng bagong game!";

        saverLoader.saveGame(Path.Combine(Application.persistentDataPath, "saveData.json"), "You", false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, "NONE", mode, false, 0, 0);
        Debug.Log("New Game with mode: " + mode);
        Invoke(nameof(DelayedSceneOut), TRANSITIONDELAY - 0.5f);
        Invoke(nameof(DelayedTut1), TRANSITIONDELAY);

    }
    public void LoadHallScene()
    {
        Invoke(nameof(DelayedSceneOut), TRANSITIONDELAY - 0.5f);
        Invoke(nameof(DelayedHall), TRANSITIONDELAY);
    }

    private void DelayedSceneOut()
    {
        screenFade.SetTrigger("sceneOut");
    }

    private void DelayedTut1()
    {
        SceneManager.LoadScene("VNFormative 1");
    }
    private void DelayedHall()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void DoQuit() // For quit button
    {
        if (!canQuit)
            return;

        Debug.Log("Quitting Game");
        try
        {
            //Check what platform first to make the button work everywhere
            if (Application.platform == RuntimePlatform.Android)
            {
                QuitApplicationUtility.MoveAndroidApplicationToBack();
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                Application.Quit();
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public void ClosePanel()
    {
        canQuit = true;
        overlayPanel.SetActive(false);
        // if(overlayPanel != null) //it's on screen
        // {
        //     isActive  = !isActive;
        //     overlayPanel.SetActive(isActive);
        // }
    }
    public void CloseNotifyPanel()
    {
        canQuit = true;
        panelNotify.SetActive(false);
        // if(overlayPanel != null) //it's on screen
        // {
        //     isActive  = !isActive;
        //     overlayPanel.SetActive(isActive);
        // }
    }

    // Copy pasted from here: https://docs.unity3d.com/2022.3/Documentation/Manual/android-quit.html
    public class QuitApplicationUtility
    {
        public static void MoveAndroidApplicationToBack()
        {
            AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call<bool>("moveTaskToBack", true);
        }
    }
}
