using System.Collections;
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
    private readonly int level = GlobalVariables.level - 1 >= 0 ? GlobalVariables.level - 1 : 0; //Level variable
    private string cachePath = string.Empty;

    private const string loBGPath1 = "Lo-Bg-Shader_Comp.mp4";
    private const string hoBGPath1 = "Ho-Bg-Shader_Comp.mp4";

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

    private string streamingAssetsPath = string.Empty;
    private Camera classroomCamera;
    private Camera mainCamera;

    //Audio/SFX Stuff
    private float volumeFactor = 1.0f; //Multiplier of volume for mute / volume slider functions

    void Awake()
    {
        Debug.Log("VideoPlayer Online");

        //Get path to StreamingAssets
        streamingAssetsPath = Application.streamingAssetsPath;

        //Get scene name
        sceneName = SceneManager.GetActiveScene().name;

        //Settings
        if (GlobalVariables.isMute)
        {
            volumeFactor = 0f;

            videoPlayer.SetDirectAudioVolume(0, 1 * volumeFactor);
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

    //Method for replaceing Path.Combine(), spits out an android compatible Path
    private string AndroidPathCombine(string first, string second)
    {
        return first + "/" + second;
    }

    string GetSpellPath(GameBehaviour.SHAPES shape)
    {
        string path = "Videos/Spells";

        switch (shape)
        {
            case GameBehaviour.SHAPES.SQUARE:
                path = AndroidPathCombine(path, squareSpells[level]);
                break;
            case GameBehaviour.SHAPES.RECTANGLE:
                path = AndroidPathCombine(path, rectangleSpells[level]);
                break;
            case GameBehaviour.SHAPES.TRIANGLE:
                path = AndroidPathCombine(path, triangleSpells[level]);
                break;
            case GameBehaviour.SHAPES.CIRCLE:
                path = AndroidPathCombine(path, circleSpells[level]);
                break;
            case GameBehaviour.SHAPES.SEMI_CIRCLE:
                path = AndroidPathCombine(path, semiCircleSpells[level]);
                break;
            case GameBehaviour.SHAPES.NONE: //This is HO
                path = AndroidPathCombine(path, compoundSpells[level]);
                break;
        }

        return path;
    }

    public void PlaySpellIntro(GameBehaviour.SHAPES shape)
    {
        string filename = "Intro.mp4";

        string path = GetSpellPath(shape);
        cachePath = AndroidPathCombine(Application.persistentDataPath, path);
        cachePath = AndroidPathCombine(cachePath, filename);

        path = AndroidPathCombine(streamingAssetsPath, path);
        path = AndroidPathCombine(path, filename);
        StartCoroutine(PlayVideo(path));
    }

    // 0 = Under, 1 = Over, 2 = Good
    public void PlaySpellAnim(GameBehaviour.SHAPES shape, int state)
    {
        string filename = string.Empty;
        string path = GetSpellPath(shape);
        cachePath = AndroidPathCombine(Application.persistentDataPath, path);

        path = AndroidPathCombine(streamingAssetsPath, path);
        switch (state)
        {
            case 0:
                filename = "Under.mp4";
                break;
            case 1:
                filename = "Over.mp4";
                break;
            case 2:
                filename = "Good.mp4";
                break;
        }

        //Finish up paths
        path = AndroidPathCombine(path, filename);
        cachePath = AndroidPathCombine(cachePath, filename);

        StartCoroutine(PlayVideo(path));
    }

    public void Stop()
    {
        videoPlayer.Stop();
    }

    public void PlayBGAnim()
    {
        string path = "Videos/Shaders";

        if (sceneName == "GameLevelScene_v1")
        {
            switch (GlobalVariables.level)
            {
                default:
                    path = AndroidPathCombine(path, loBGPath1);
                    break;
            }
        }
        else
        {
            switch (GlobalVariables.level)
            {
                default:
                    path = AndroidPathCombine(path, hoBGPath1);
                    break;
            }
        }

        string streamPath = AndroidPathCombine(streamingAssetsPath, path);
        cachePath = AndroidPathCombine(Application.persistentDataPath, path);

        StartCoroutine(PlayVideo(streamPath));
    }

    public IEnumerator PlayVideo(string path)
    {
        videoPlayer.Stop();

        if (!File.Exists(cachePath)) //Load and cache if file not yet loaded
        {
            using UnityWebRequest www = UnityWebRequest.Get(path);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(cachePath)); // ensure folder exists
                    File.WriteAllBytes(cachePath, www.downloadHandler.data);
                    Debug.Log($"[VideoLoader] Video copied to: {cachePath}");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[VideoLoader] Failed to write video file: {e}");
                    yield break;
                }
            }
            else
            {
                Debug.LogError($"[VideoLoader] Failed to load video from StreamingAssets: {www.error}");
                yield break;
            }
        }
        else 
        {
            Debug.Log($"[VideoLoader] Using cached video: {cachePath}");
        }

        //Load video
#if UNITY_ANDROID
        videoPlayer.url = "jar:file://" + cachePath;
#endif
#if UNITY_EDITOR //Editor No like jar jar
        videoPlayer.url = "file://" + cachePath;
#endif

        //Prepare video
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        if (!videoPlayer.isPlaying)
            videoPlayer.Play();
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
