using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   //text
using System.IO;
using System;
using UnityEngine.SceneManagement;
//save game file

public class MM_UIScript : MonoBehaviour
{
    private Text overlayText; 
    public GameObject overlayPanel; 
    public Button btnContinue;
    public GameObject panelNotify; 

    private string savePath;
    private bool gameExists=false;
    // Start is called before the first frame update

    private GameData savedGame;

    private SaveLoadController saverLoader = new SaveLoadController();

    private Animator screenFade;
    private const float TRANSITIONDELAY = 1.2f;

    private void Awake()
    {
        screenFade = GameObject.Find("ScreenFade").GetComponent<Animator>();
    }
    void Start()
    {
        screenFade.SetTrigger("sceneIn"); //Fade-in animation

        savePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        // // loadGame();
        // saveGame("Emerut", false,0,0,0,0,0,0,0,0,0,0,0);

        savedGame = saverLoader.loadGame(savePath);
        if (savedGame != null)
        {
            gameExists = true;
            btnContinue.interactable = true;
        }
        else{
            btnContinue.interactable = false;
        }

        overlayPanel.SetActive(false);
        panelNotify.SetActive(false);

    }

    public void DoContinue(){
        Debug.Log("CONTINUE");
        panelNotify.SetActive(true); //nts: always set active true because if inactive ndi makikita ung children comps

        Text overlayText = GameObject.Find("TextOverlay").GetComponent<Text>();

        overlayText.text = "Saved game Loaded!";
        //Jump to the game immediately (load all saved data)
        LoadHallScene(); //Data loaded at start, continue button disabled by default so fast fingers cant press accidentalt
    }

    public void DoNewGame()
    {        
        if (gameExists){

            overlayPanel.SetActive(true);
            Text overlayText = GameObject.Find("PanelText").GetComponent<Text>();
            overlayText.text = "Previous game file already exists. Overwrite data?";
        }
        else
            LoadFirstScene();
        // Moved the rest to to LoadFirstScene since that is where new games always go anyways
    }

    public void DoCredits(){
        Debug.Log("Open Credits");   
    }

    public void LoadFirstScene(){
        Debug.Log("new game");
        panelNotify.SetActive(true);
        Text overlayText = GameObject.Find("TextOverlay").GetComponent<Text>();

        overlayText.text = "Creating a new game!";

        saverLoader.saveGame(Path.Combine(Application.persistentDataPath, "saveData.json"), "You", false, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        // saveGame("You",false,0,0,0,0,0,0,0,0,0,0,0);

        Invoke(nameof(DelayedSceneOut), TRANSITIONDELAY - 0.5f);
        Invoke(nameof(DelayedTut1), TRANSITIONDELAY);
        
    }
    public void LoadHallScene(){
        Invoke(nameof(DelayedSceneOut), TRANSITIONDELAY - 0.5f);
        Invoke(nameof(DelayedHall), TRANSITIONDELAY);
    }

    private void DelayedSceneOut()
    {
        screenFade.SetTrigger("sceneOut");
    }

    private void DelayedTut1()
    {
        SceneManager.LoadScene("Tutorial1");
    }
    private void DelayedHall()
    {
        SceneManager.LoadScene("LevelSelect");
    }

    public void DoQuit() // For quit button
    {
        Debug.Log("Quitting Game");
        try
        {
            QuitApplicationUtility.MoveAndroidApplicationToBack();
        }
        catch (Exception ex) 
        {
            Debug.Log("Probably in UnityEditor which causes exception. Otherwise see next log:");
            Debug.LogException(ex);
        }
    }

    public void ClosePanel()
    {
        overlayPanel.SetActive(false);
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
