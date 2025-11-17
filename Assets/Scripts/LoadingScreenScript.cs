using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI percentageText;
    [SerializeField] private ShaderVariantCollection preloadedShaders;
    private VideoPlayerScript videoPlayer;

    private float progress = 0f;
    private bool isWarmUp = false;
    private int loadedShaders = 0;

    void Awake()
    {
        if (!Debug.isDebugBuild) //Disable all debug stuff
        {
            Debug.unityLogger.logEnabled = false;
        }

        videoPlayer = GetComponent<VideoPlayerScript>();
    }

    void Start()
    {
        if (!GlobalVariables.isStartUp)
            StartCoroutine(SceneLoading());
        else
            StartUpCaching();
    }

    void Update()
    {
        if(videoPlayer == null) //Wait for component to load
            return;
        
        if (GlobalVariables.isStartUp)
            progress = (float)Math.Round(videoPlayer.CacheProgress * 100f, 2);

        if(!isWarmUp)
            percentageText.text = progress + "%";
    }

    private void StartUpCaching()
    {
        videoPlayer.CacheAllVideos(
            onComplete: () => {
                Debug.Log("All videos cached! Preloading shaders.");
                StartCoroutine(WarmUpAndStart());
            },
            onError: (error) => {
                GlobalVariables.isStartUp = false; //Next loading screen not startup anymore
                SceneManager.LoadScene("MainMenu");
            }
        );
    }

    private IEnumerator WarmUpAndStart()
    {
        isWarmUp = true;

        preloadedShaders.WarmUpProgressively(1);

        loadedShaders++; //Load one new

        percentageText.text = $"Loading Shaders: {loadedShaders}/{preloadedShaders.shaderCount}";

        if (loadedShaders < preloadedShaders.shaderCount)
            yield return null; //Loop

        //Else load main menu

        Debug.Log("All shaders preloaded! Game Ready.");
        percentageText.text = "DONE!";
        GlobalVariables.isStartUp = false; //Next loading screen not startup anymore
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator SceneLoading()
    {
        AsyncOperation loadLevel = SceneManager.LoadSceneAsync(GlobalVariables.nextLevel);

        while (!loadLevel.isDone)
        {
            progress = (float)Math.Round((loadLevel.progress + 0.1f) * 100f, 2); //Need extra 10% since it only goes to 90%
            yield return null;
        }
    }
}
