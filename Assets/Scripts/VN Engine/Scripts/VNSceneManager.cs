using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages transitions to and from the Visual Novel scene with dialogue file configuration.
/// Singleton pattern ensures data persists across scene loads.
/// Auto-instantiates when first accessed.
/// </summary>
public class VNSceneManager : MonoBehaviour
{
    private static VNSceneManager _instance;
    public static VNSceneManager instance
    {
        get
        {
            if (_instance == null)
            {
                // Create a new GameObject with this component
                GameObject go = new GameObject("VNSceneManager");
                _instance = go.AddComponent<VNSceneManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[VNSceneManager] Auto-instantiated singleton");
            }
            return _instance;
        }
    }

    private TextAsset dialogueFileToPlay;
    private string returnSceneName;
    private string nextSceneName;
    private const string VN_SCENE_NAME = "VisualNovelScene";

    private void Awake()
    {
        // If instance already exists and it's not this, destroy this
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set this as the instance and persist across scenes
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Loads the VN scene with the specified dialogue file.
    /// </summary>
    /// <param name="dialogueFile">The dialogue file to play in the VN scene</param>
    /// <param name="returnScene">The scene to return to after dialogue finishes (optional)</param>
    /// <param name="nextScene">If specified, loads this scene instead of returning to previous scene (optional)</param>
    public void LoadVNScene(TextAsset dialogueFile, string returnScene = null, string nextScene = null)
    {
        if (dialogueFile == null)
        {
            Debug.LogError("[VNSceneManager] Cannot load VN scene - dialogue file is null!");
            return;
        }

        dialogueFileToPlay = dialogueFile;
        returnSceneName = string.IsNullOrEmpty(returnScene) ? SceneManager.GetActiveScene().name : returnScene;
        nextSceneName = nextScene;

        Debug.Log($"[VNSceneManager] Loading VN scene with dialogue: {dialogueFile.name}, will return to: {returnSceneName}");
        if (!string.IsNullOrEmpty(nextScene))
        {
            Debug.Log($"[VNSceneManager] After dialogue, will load: {nextScene}");
        }

        SceneManager.LoadScene(VN_SCENE_NAME);
    }

    /// <summary>
    /// Loads the VN scene using a dialogue file path from Resources folder.
    /// </summary>
    /// <param name="dialogueFilePath">Path to dialogue file in Resources folder (without .txt extension)</param>
    /// <param name="returnScene">The scene to return to after dialogue finishes (optional)</param>
    /// <param name="nextScene">If specified, loads this scene instead of returning to previous scene (optional)</param>
    public void LoadVNSceneByPath(string dialogueFilePath, string returnScene = null, string nextScene = null)
    {
        string fullPath = FilePaths.GetPathToResource(FilePaths.resources_dialogueFiles, dialogueFilePath);
        TextAsset dialogueFile = Resources.Load<TextAsset>(fullPath);

        if (dialogueFile == null)
        {
            Debug.LogError($"[VNSceneManager] Could not find dialogue file at path: {fullPath}");
            return;
        }

        LoadVNScene(dialogueFile, returnScene, nextScene);
    }

    /// <summary>
    /// Gets the dialogue file to play in the VN scene. Called by DialogueRunner.
    /// </summary>
    public TextAsset GetDialogueFile()
    {
        return dialogueFileToPlay;
    }

    /// <summary>
    /// Returns to the scene that called the VN scene, or loads the next scene if specified.
    /// </summary>
    public void ReturnToPreviousScene()
    {
        // Check if a next scene was specified
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[VNSceneManager] Loading next scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
            ClearData();
            return;
        }

        if (string.IsNullOrEmpty(returnSceneName))
        {
            Debug.LogWarning("[VNSceneManager] No return scene set, loading LevelSelect by default");
            SceneManager.LoadScene("LevelSelect");
            return;
        }

        Debug.Log($"[VNSceneManager] Returning to scene: {returnSceneName}");
        SceneManager.LoadScene(returnSceneName);
        ClearData();
    }

    /// <summary>
    /// Clears stored data after scene transition.
    /// </summary>
    public void ClearData()
    {
        dialogueFileToPlay = null;
        returnSceneName = null;
        nextSceneName = null;
    }
}