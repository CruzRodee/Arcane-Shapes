using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreenScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI percentageText;
    private VideoPlayerScript videoPlayer;

    private float progress = 0f;

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

        percentageText.text = progress + "%";
    }

    private void StartUpCaching()
    {
        videoPlayer.CacheAllVideos(
            onComplete: () => {
                Debug.Log("All videos cached! Game ready.");
                percentageText.text = "100%";
                GlobalVariables.isStartUp = false; //Next loading screen not startup anymore
                SceneManager.LoadScene("MainMenu");
            },
            onError: (error) => {
                Debug.LogError($"Caching failed: {error}");
                GlobalVariables.isStartUp = false; //Next loading screen not startup anymore
                SceneManager.LoadScene("MainMenu");
            }
        );
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
