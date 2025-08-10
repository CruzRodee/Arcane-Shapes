using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoPlayerScript : MonoBehaviour
{
    //Settings
    private string sceneName; //For determining LO or HO
    
    //Video Stuff
    private VideoPlayer videoPlayer;
    private const string loBGPath1 = "Videos/Shaders/Lo-Bg-Shader_Comp.mp4";
    private const string hoBGPath1 = "Videos/Shaders/Ho-Bg-Shader_Comp.mp4";

    private readonly string[] squareSpells = {
        "Square/Treasure", "Square/StoneCube", "Square/Shield"
    };
    private readonly string[] rectangleSpells = {
        "Rectangle/Minimap", "Rectangle/Door", "Rectangle/Move"
    };
    private readonly string[] triangleSpells = {
        "Triangle/Shelter", "Triangle/Sandwich", "Triangle/Ice"
    };
    private readonly string[] circleSpells = {
        "Circle/Portal", "Circle/Light", "Circle/Missile"
    };
    private readonly string[] semiCircleSpells = {
        "SemiCircle/Rain", "SemiCircle/Mushroom", "SemiCircle/Slash"
    };
    private readonly string[] compoundSpells = {
        "Compound/House", "Compound/ChargedExplosion", "Compound/CubicBarrier",
        "Compound/HolyHalo", "Compound/ThorHammer", "Compound/TimeStop"
    };

    private string streamingAssetsPath = "";
    private Camera classroomCamera;
    private Camera mainCamera;

    //Audio/SFX Stuff
    private AudioSource soundSource;
    private float volumeFactor = 1.0f; //Multiplier of volume for mute / volume slider functions

    void Awake()
    {
        Debug.Log("VideoPlayer Online");

        //Get path to StreamingAssets
        streamingAssetsPath = Application.streamingAssetsPath;

        //Get scene name
        sceneName = SceneManager.GetActiveScene().name;

        //Create and attach AudioSource for BGM
        soundSource = GetComponent<AudioSource>();
        if (soundSource == null)
        {
            soundSource = gameObject.AddComponent<AudioSource>();
        }

        //Settings
        if (GlobalVariables.isMute)
        {
            volumeFactor = 0f;
            soundSource.volume = 1 * volumeFactor;
        }

        //VideoPlayer
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        //Get cameras
        mainCamera = Camera.main;
        classroomCamera = GameObject.Find("ClassroomCamera").GetComponent<Camera>();
        videoPlayer.targetCamera = classroomCamera;

        videoPlayer.loopPointReached += EndReached; 
    }

    void EndReached(VideoPlayer vp)
    {
        PlayBGAnim();
    }

    public float GetVideoLength()
    {
        return (float)videoPlayer.length + 2;
    }

    string GetSpellPath(GameBehaviour.SHAPES shape)
    {
        string path = "Videos/Spells/";
        int level = GlobalVariables.level - 1 >= 0 ? GlobalVariables.level - 1 : 0;

        switch (shape)
        {
            case GameBehaviour.SHAPES.SQUARE:
                path = Path.Combine(path, squareSpells[level]);
                break;
            case GameBehaviour.SHAPES.RECTANGLE:
                path = Path.Combine(path, rectangleSpells[level]);
                break;
            case GameBehaviour.SHAPES.TRIANGLE:
                path = Path.Combine(path, triangleSpells[level]);
                break;
            case GameBehaviour.SHAPES.CIRCLE:
                path = Path.Combine(path, circleSpells[level]);
                break;
            case GameBehaviour.SHAPES.SEMI_CIRCLE:
                path = Path.Combine(path, semiCircleSpells[level]);
                break;
            case GameBehaviour.SHAPES.NONE: //This is HO
                path = Path.Combine(path, compoundSpells[level]);
                break;
        }

        return path;
    }

    public void PlaySpellIntro(GameBehaviour.SHAPES shape)
    {
        string path = GetSpellPath(shape);
        path = Path.Combine(streamingAssetsPath, path);
        path = Path.Combine(path, "intro.mp4");
        StartCoroutine(PlayVideo(path));
    }

    // 0 = Under, 1 = Over, 2 = Good
    public void PlaySpellAnim(GameBehaviour.SHAPES shape, int state)
    {
        string path = GetSpellPath(shape);

        path = Path.Combine(streamingAssetsPath, path);
        switch (state)
        {
            case 0:
                path = Path.Combine(path, "under.mp4");
                break;
            case 1:
                path = Path.Combine(path, "over.mp4");
                break;
            case 2:
                path = Path.Combine(path, "good.mp4");
                break;
        }

        StartCoroutine(PlayVideo(path));
    }

    public void Stop()
    {
        videoPlayer.Stop();
    }

    public void PlayBGAnim()
    {
        string path;

        if (sceneName == "GameLevelScene_v1")
        {
            switch (GlobalVariables.level)
            {
                default:
                    path = Path.Combine(streamingAssetsPath, loBGPath1);
                    break;
            }
        }
        else
        {
            switch (GlobalVariables.level)
            {
                default:
                    path = Path.Combine(streamingAssetsPath, hoBGPath1);
                    break;
            }
        }

        StartCoroutine(PlayVideo(path));
    }

    public IEnumerator PlayVideo(string path)
    {
        videoPlayer.Stop();

        using (UnityWebRequest www = UnityWebRequest.Get(path))
        {
            yield return www.SendWebRequest();

            if(www.result == UnityWebRequest.Result.Success)
            {
                videoPlayer.url = path;
                videoPlayer.Prepare();

                if (!videoPlayer.isPlaying)
                    videoPlayer.Play();
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        //Dynamically update which camera is being used
        if (!videoPlayer.targetCamera.gameObject.activeInHierarchy)
        {
            if (videoPlayer.targetCamera.gameObject.name == mainCamera.gameObject.name)
                videoPlayer.targetCamera = classroomCamera;
            else
                videoPlayer.targetCamera = mainCamera;
        }
    }
}
