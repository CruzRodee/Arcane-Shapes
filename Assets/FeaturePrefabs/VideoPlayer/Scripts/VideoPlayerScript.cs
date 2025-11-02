using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Manages video playback for spell animations and background shaders.
/// Handles caching, progress tracking, and platform-specific video loading.
/// </summary>
public class VideoPlayerScript : MonoBehaviour
{
    #region Constants

    private readonly string[] bgShaders = {
        "BG_LO_School.mp4", "BG_LO_Dungeon.mp4", "BG_LO_Forest.mp4", "BG_LO_Corrupt.mp4", "BG_LO_Field.mp4",
        "BG_HO_Field.mp4", "BG_HO_School.mp4", "BG_HO_Dark.mp4"

    };

    private const string LO_SCENE_NAME = "GameLevelScene_v1";
    private const string SPELLS_FOLDERS = "Videos/Spells";
    private const string SHADERS_FOLDERS = "Videos/Shaders";

    #endregion

    #region Spell Data Arrays

    private readonly string[] squareSpells = {
        "Square/Treasure", "Square/StoneCube", "Square/Shield"
    };

    private readonly string[] rectangleSpells = {
        "Rectangle/Door", "Rectangle/Minimap", "Rectangle/Move"
    };

    private readonly string[] triangleSpells = {
        "Triangle/Shelter", "Triangle/Sandwich", "Triangle/Campfire"
    };

    private readonly string[] circleSpells = {
        "Circle/Light", "Circle/Missile", "Circle/Portal"
    };

    private readonly string[] semiCircleSpells = {
        "SemiCircle/Rain", "SemiCircle/Mushroom", "SemiCircle/Slash"
    };

    private readonly string[] compoundSpells = {
        "Compound/House", "Compound/ChargedExplosion", "Compound/CubicBarrier",
        "Compound/HolyHalo", "Compound/ThorHammer", "Compound/TimeStop"
    };

    private readonly string[] videoStates = { "Intro.mp4", "Under.mp4", "Over.mp4", "Good.mp4" };

    #endregion

    #region Public Properties

    public VideoPlayer videoPlayer;

    // Progress tracking properties
    public int TotalVideosToCache { get; private set; }
    public int VideosCached { get; private set; }
    public float CacheProgress => TotalVideosToCache > 0 ? (float)VideosCached / TotalVideosToCache : 0f;

    #endregion

    #region Private Fields

    // Scene and level data
    private string sceneName;
    private readonly int level = Mathf.Max(0, GlobalVariables.level - 1);

    // Video state
    private bool doLoop = true;
    private string cachePath = string.Empty;

    // Paths
    private string streamingAssetsPath = string.Empty;

    // Camera references
    private Camera classroomCamera;
    private Camera mainCamera;

    // Audio settings
    private float volumeFactor = 1.0f;

    // Caching callbacks
    private Action<float> OnCachingProgress;
    private Action OnCachingComplete;
    private Action<string> OnCachingError;

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        InitializeVideoPlayer();
        SetupCameras();
        ApplyAudioSettings();

        Debug.Log("VideoPlayer Online");
    }

    void Update()
    {
        UpdateCameraTarget();
    }

    #endregion

    #region Initialization

    private void InitializeVideoPlayer()
    {
        streamingAssetsPath = Application.streamingAssetsPath;
        sceneName = SceneManager.GetActiveScene().name;

        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        videoPlayer.loopPointReached += OnVideoEndReached;
    }

    private void SetupCameras()
    {
        mainCamera = Camera.main;

        GameObject classroomCameraObj = GameObject.Find("ClassroomCamera");
        if (classroomCameraObj != null)
        {
            classroomCamera = classroomCameraObj.GetComponent<Camera>();
            videoPlayer.targetCamera = classroomCamera;
        }
        else
        {
            Debug.LogWarning("[VideoPlayer] ClassroomCamera not found, using Main Camera");
            videoPlayer.targetCamera = mainCamera;
        }
    }

    private void ApplyAudioSettings()
    {
        if (GlobalVariables.isMute)
        {
            volumeFactor = 0f;
            videoPlayer.SetDirectAudioVolume(0, 1 * volumeFactor);
        }
    }

    #endregion

    #region Public API - Video Controls

    /// <summary>
    /// Plays the introduction animation for a specific spell shape
    /// </summary>
    public void PlaySpellIntro(GameBehaviour.SHAPES shape)
    {
        PlaySpellVideo(shape, videoStates[0]);
    }

    //Variable for Letting player know which error vid to play
    private string endState = "Under";

    /// <summary>
    /// Plays spell animation based on performance state
    /// </summary>
    /// <param name="shape">The spell shape</param>
    /// <param name="state">0 = Under, 1 = Over, 2 = Good</param>
    public void PlaySpellAnim(GameBehaviour.SHAPES shape, int state)
    {
        doLoop = false;

        string filename = state switch
        {
            0 => videoStates[1],
            1 => videoStates[2],
            2 => videoStates[3],
            _ => videoStates[1]
        };

        switch (state)
        {
            case 0:
                endState = "Under";
                break;
            case 1:
                endState = "Over";
                break;
            case 2:
                endState = "Good";
                break;
        }

        PlaySpellVideo(shape, filename);
    }

    /// <summary>
    /// Plays the background shader animation
    /// </summary>
    public void PlayBGAnim()
    {
        doLoop = true;
        string path = SHADERS_FOLDERS;

        if (IsLowOrderScene())
        {
            switch (GlobalVariables.loSelectedShape)
            {
                case GameBehaviour.SHAPES.SQUARE:
                    path = AndroidPathCombine(path, bgShaders[0]);
                    break;
                case GameBehaviour.SHAPES.RECTANGLE:
                    path = AndroidPathCombine(path, bgShaders[1]);
                    break;
                case GameBehaviour.SHAPES.TRIANGLE:
                    path = AndroidPathCombine(path, bgShaders[2]);
                    break;
                case GameBehaviour.SHAPES.CIRCLE:
                    path = AndroidPathCombine(path, bgShaders[3]);
                    break;
                case GameBehaviour.SHAPES.SEMI_CIRCLE:
                    path = AndroidPathCombine(path, bgShaders[4]);
                    break;
                default:
                    path = AndroidPathCombine(path, bgShaders[0]);
                    break;
            }
        }
        else
        {
            switch (GlobalVariables.level)
            {
                case 0:
                case 1:
                case 2:
                    path = AndroidPathCombine(path, bgShaders[5]);
                    break;
                case 3:
                case 4:
                    path = AndroidPathCombine(path, bgShaders[6]);
                    break;
                case 5:
                case 6:
                    path = AndroidPathCombine(path, bgShaders[7]);
                    break;
                default:
                    path = AndroidPathCombine(path, bgShaders[5]);
                    break;
            }
        }

        string streamPath = AndroidPathCombine(streamingAssetsPath, path);
        cachePath = AndroidPathCombine(Application.persistentDataPath, path);

        StartCoroutine(PlayVideo(streamPath));
    }

    /// <summary>
    /// Stops video playback
    /// </summary>
    public void Stop()
    {
        videoPlayer.Stop();
    }

    /// <summary>
    /// Gets the adjusted video length
    /// </summary>
    public float GetVideoLength()
    {
        float length = (float)videoPlayer.length * 0.4f;
        Debug.Log($"VideoLength: {length}");
        return length;
    }

    #endregion

    #region Public API - Caching

    /// <summary>
    /// Caches ALL videos for all levels and scenes at game start.
    /// Call this during initial loading screen to pre-cache everything.
    /// </summary>
    /// <param name="onProgress">Called with progress value 0.0 to 1.0</param>
    /// <param name="onComplete">Called when all videos are cached successfully</param>
    /// <param name="onError">Called if any video fails to cache, with error message</param>
    public void CacheAllVideos(Action<float> onProgress = null,
                              Action onComplete = null,
                              Action<string> onError = null)
    {
        OnCachingProgress = onProgress;
        OnCachingComplete = onComplete;
        OnCachingError = onError;

        StartCoroutine(CacheAllVideosCoroutine());
    }

    #endregion

    #region Private Methods - Video Playback

    private void PlaySpellVideo(GameBehaviour.SHAPES shape, string filename)
    {
        string path = GetSpellPath(shape);
        cachePath = AndroidPathCombine(Application.persistentDataPath, path);
        cachePath = AndroidPathCombine(cachePath, filename);

        string streamPath = AndroidPathCombine(streamingAssetsPath, path);
        streamPath = AndroidPathCombine(streamPath, filename);

        StartCoroutine(PlayVideo(streamPath));
    }

    private IEnumerator PlayVideo(string streamPath)
    {
        videoPlayer.Stop();

        // Load and cache if file not yet cached
        if (!File.Exists(cachePath))
        {
            using UnityWebRequest www = UnityWebRequest.Get(streamPath);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string directory = Path.GetDirectoryName(cachePath);
                    Directory.CreateDirectory(directory);
                    File.WriteAllBytes(cachePath, www.downloadHandler.data);
                    Debug.Log($"[VideoLoader] Video copied to: {cachePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[VideoLoader] Failed to write video file: {e}");
                    yield break;
                }
            }
            else
            {
                if (endState == "Good")
                {
                    Debug.LogError($"[VideoLoader] Failed to load video from StreamingAssets: {www.error}");
                    yield break;
                }
                else 
                {
                    Debug.Log("[VideoLoader] No Unique fail animation detected, playing defaults");
                    if(endState == "Under")
                    {
                        cachePath = "Videos/Spells/Generic/Under1.mp4";
                    }
                    else if(endState == "Over")
                    {
                        cachePath = "Videos/Spells/Generic/Over1.mp4";
                    }

                    cachePath = AndroidPathCombine(Application.persistentDataPath, cachePath);
                }
            }
        }
        else
        {
            Debug.Log($"[VideoLoader] Using cached video: {cachePath}");
        }

        // Set platform-specific URL
        SetVideoPlayerUrl();

        // Prepare and play video
        videoPlayer.Prepare();
        yield return new WaitUntil(() => videoPlayer.isPrepared);

        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }
    }

    private void SetVideoPlayerUrl()
    {
#if UNITY_ANDROID
        videoPlayer.url = ToAndroidFilePath(cachePath);
#endif
#if UNITY_EDITOR //Editor No like jar jar 
        videoPlayer.url = "file://" + cachePath;
#else
        videoPlayer.url = "file://" + cachePath;
#endif
    }

    #endregion

    #region Private Methods - Caching

    private IEnumerator CacheAllVideosCoroutine()
    {
        var videosToCache = new List<string>();

        // Add ALL background shader videos
        AddAllBackgroundVideosToCacheList(videosToCache);

        // Add ALL spell videos for all levels
        AddAllSpellVideosToCacheList(videosToCache);

        TotalVideosToCache = videosToCache.Count;
        VideosCached = 0;

        Debug.Log($"[VideoCache] Starting to cache {TotalVideosToCache} videos (ALL levels and scenes)");

        foreach (string videoPath in videosToCache)
        {
            yield return StartCoroutine(CacheVideoFile(videoPath));

            VideosCached++;
            float progress = CacheProgress;

            OnCachingProgress?.Invoke(progress);
            Debug.Log($"[VideoCache] Progress: {VideosCached}/{TotalVideosToCache} ({progress:P1})");
        }

        Debug.Log("[VideoCache] All videos cached successfully!");
        OnCachingComplete?.Invoke();
    }

    private IEnumerator CacheVideoFile(string relativePath)
    {
        string streamPath = AndroidPathCombine(streamingAssetsPath, relativePath);
        string localCachePath = AndroidPathCombine(Application.persistentDataPath, relativePath);

        // Skip if already cached
        if (File.Exists(localCachePath))
        {
            Debug.Log($"[VideoCache] Already cached: {relativePath}");
            yield break;
        }

        using UnityWebRequest www = UnityWebRequest.Get(streamPath);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string directory = Path.GetDirectoryName(localCachePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllBytes(localCachePath, www.downloadHandler.data);
                Debug.Log($"[VideoCache] Cached: {relativePath} ({www.downloadHandler.data.Length / 1024}KB)");
            }
            catch (Exception e)
            {
                string errorMsg = $"Failed to cache video {relativePath}: {e.Message}";
                Debug.LogError($"[VideoCache] {errorMsg}");
                OnCachingError?.Invoke(errorMsg);
            }
        }
        else
        {
            string errorMsg = $"Failed to load video {relativePath}, assuming default video desired.";
            Debug.LogWarning($"[VideoCache] {errorMsg}");
            // Do not invoke Cache error as failing to load is assumed to mean no custom video
        }
    }

    private void AddAllBackgroundVideosToCacheList(List<string> videosToCache)
    {
        string bgPath = SHADERS_FOLDERS;

        // Add both LO and HO background videos
        foreach (string bgShader in bgShaders) {
            videosToCache.Add(AndroidPathCombine(bgPath, bgShader));
        }
    }

    private void AddAllSpellVideosToCacheList(List<string> videosToCache)
    {
        // Cache ALL spell types and ALL levels
        var allSpellArrays = new[] {
            squareSpells, rectangleSpells, triangleSpells,
            circleSpells, semiCircleSpells, compoundSpells
        };

        foreach (var spellArray in allSpellArrays)
        {
            foreach (var spellPath in spellArray)
            {
                string baseSpellPath = AndroidPathCombine(SPELLS_FOLDERS, spellPath);

                foreach (string videoState in videoStates)
                {
                    string fullPath = AndroidPathCombine(baseSpellPath, videoState);
                    videosToCache.Add(fullPath);
                }
            }
        }

        //Manually Add the Generic videos
        videosToCache.Add("Videos/Spells/Generic/Under1.mp4");
        videosToCache.Add("Videos/Spells/Generic/Over1.mp4");
    }

    #endregion

    #region Private Methods - Utilities

    private string GetSpellPath(GameBehaviour.SHAPES shape)
    {
        string path = SPELLS_FOLDERS;

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
            case GameBehaviour.SHAPES.NONE: // HO compound spells
                path = AndroidPathCombine(path, compoundSpells[level]);
                break;
        }

        return path;
    }

    /// <summary>
    /// Android-compatible path combining method
    /// </summary>
    private string AndroidPathCombine(string first, string second)
    {
        return first + "/" + second;
    }

    private string ToAndroidFilePath(string path)
    {
        return "jar:file://" + path;
    }

    private bool IsLowOrderScene()
    {
        return sceneName == LO_SCENE_NAME;
    }

    private void UpdateCameraTarget()
    {
        // Dynamically update which camera is being used
        if (videoPlayer.targetCamera != null && !videoPlayer.targetCamera.gameObject.activeInHierarchy)
        {
            if (videoPlayer.targetCamera.gameObject.name == mainCamera.gameObject.name)
            {
                videoPlayer.targetCamera = classroomCamera;
            }
            else
            {
                videoPlayer.targetCamera = mainCamera;
            }
        }
    }

    #endregion

    #region Event Handlers

    private void OnVideoEndReached(VideoPlayer vp)
    {
        if (doLoop)
        {
            PlayBGAnim();
        }
    }

    #endregion
}