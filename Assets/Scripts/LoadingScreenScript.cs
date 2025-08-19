using System;
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
    }

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayerScript>();

        videoPlayer.CacheAllVideos(
            onComplete: () => {
                Debug.Log("All videos cached! Game ready.");
                percentageText.text = "100%";
                SceneManager.LoadScene("MainMenu");
            },
            onError: (error) => {
                Debug.LogError($"Caching failed: {error}");
                SceneManager.LoadScene("MainMenu");
            }
        );
    }

    void Update()
    {
        if(videoPlayer == null) //Wait for component to load
            return;
        
        float cacheProg = (float)Math.Round(videoPlayer.CacheProgress * 100f, 2);
        if(progress != cacheProg)
        {
            progress = cacheProg;
            percentageText.text = progress + "%";
        }
    }
}
