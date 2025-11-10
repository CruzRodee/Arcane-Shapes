using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerSessionData
{

    public string playerName = "";
    public string playerAge = "";
    public string playerGrade = "";
    public string playerSex = "";
    public bool areaKnown = false;

    // Checkpoint tracking
    public List<CheckpointData> checkpoints = new List<CheckpointData>();

    public DateTime sessionStart;
    public DateTime sessionEnd;
    public float totalTimeSpent = 0f;
}

[Serializable]
public class CheckpointData
{
    public string checkpointName;
    public DateTime timestamp;
    public float timeFromSessionStart;
    public float timeFromLastCheckpoint;
    public Dictionary<string, object> dataSnapshot = new Dictionary<string, object>();
}

public class PlayerDataManager : MonoBehaviour
{
    private string currentSessionFilePath = null;
    public static PlayerDataManager instance { get; private set; }

    private PlayerSessionData currentSession;

    // Track if we're in an active session
    public bool isSessionActive => currentSession != null;

    // Configuration
    [SerializeField] private bool autoSaveOnCheckpoint = true;
    [SerializeField] private string saveDirectory = "PlayerData";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// Ensures PlayerDataManager exists in the scene. Call this from any script.
    /// </summary>
    public static void EnsureInstance()
    {
        if (instance != null) return;

        GameObject pdm = new GameObject("PlayerDataManager");
        pdm.AddComponent<PlayerDataManager>();
        DontDestroyOnLoad(pdm);
    }

    /// <summary>
    /// Starts a new gameplay session. Safe to call multiple times.
    /// </summary>
    public void StartNewSession()
    {
        if (currentSession != null)
        {
            Debug.LogWarning("[PlayerData] Session already active. Ending previous session first.");
            EndSession(autoSave: false);
        }

        currentSession = new PlayerSessionData();
        currentSession.sessionStart = DateTime.Now;

        // Generate persistent file path for this session
        string directoryPath = Path.Combine(Application.persistentDataPath, saveDirectory);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        currentSessionFilePath = Path.Combine(directoryPath,
            $"PlayerData_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        InitializeVariableStore();

        Debug.Log($"[PlayerData] New session started at {currentSession.sessionStart}");
        Debug.Log($"[PlayerData] Session file: {currentSessionFilePath}");
    }

    private void InitializeVariableStore()
    {
        // Create PlayerData database in VariableStore
        VariableStore.CreateDatabase("PlayerData");

        // Link variables to currentSession properties
        VariableStore.CreateVariable("PlayerData.playerName",
            currentSession.playerName,
            () => currentSession.playerName,
            value => currentSession.playerName = value);

        VariableStore.CreateVariable("PlayerData.playerAge",
            currentSession.playerAge,
            () => currentSession.playerAge,
            value => currentSession.playerAge = value);

        VariableStore.CreateVariable("PlayerData.playerGrade",
            currentSession.playerGrade,
            () => currentSession.playerGrade,
            value => currentSession.playerGrade = value);

        VariableStore.CreateVariable("PlayerData.playerSex",
            currentSession.playerSex,
            () => currentSession.playerSex,
            value => currentSession.playerSex = value);

        VariableStore.CreateVariable("PlayerData.areaKnown",
            currentSession.areaKnown,
            () => currentSession.areaKnown,
            value => currentSession.areaKnown = value);

        Debug.Log("[PlayerDataManager] VariableStore initialized with linked PlayerData variables");
    }

    /// <summary>
    /// Saves a checkpoint with current data. This is your new milestone system.
    /// </summary>
    /// <param name="checkpointName">Identifier for this checkpoint (e.g., "Tutorial_Complete", "Level_1_Start")</param>
    /// <param name="additionalData">Optional extra data to save with this checkpoint</param>
    /// <summary>
    /// Saves a checkpoint with current data. This is your new milestone system.
    /// </summary>
    /// <param name="checkpointName">Identifier for this checkpoint (e.g., "Tutorial_Complete", "Level_1_Start")</param>
    /// <param name="additionalData">Optional extra data to save with this checkpoint</param>
    public void SaveCheckpoint(string checkpointName, Dictionary<string, object> additionalData = null)
    {
        if (currentSession == null)
        {
            Debug.LogError($"[PlayerData] Cannot save checkpoint '{checkpointName}' - no active session!");
            return;
        }

        float currentTime = (float)(DateTime.Now - currentSession.sessionStart).TotalSeconds;

        // Calculate time from last checkpoint
        float timeFromLast = 0f;
        if (currentSession.checkpoints.Count > 0)
        {
            CheckpointData lastCheckpoint = currentSession.checkpoints[currentSession.checkpoints.Count - 1];
            timeFromLast = currentTime - lastCheckpoint.timeFromSessionStart;
        }
        else
        {
            // First checkpoint - time from session start
            timeFromLast = currentTime;
        }

        CheckpointData checkpoint = new CheckpointData
        {
            checkpointName = checkpointName,
            timestamp = DateTime.Now,
            timeFromSessionStart = currentTime,
            timeFromLastCheckpoint = timeFromLast,
            dataSnapshot = new Dictionary<string, object>()
        };

        // Capture current VariableStore state
        checkpoint.dataSnapshot["playerName"] = currentSession.playerName;
        checkpoint.dataSnapshot["playerAge"] = currentSession.playerAge;
        checkpoint.dataSnapshot["playerGrade"] = currentSession.playerGrade;
        checkpoint.dataSnapshot["playerSex"] = currentSession.playerSex;
        checkpoint.dataSnapshot["areaKnown"] = currentSession.areaKnown;

        // Add any additional data
        if (additionalData != null)
        {
            foreach (var kvp in additionalData)
            {
                checkpoint.dataSnapshot[kvp.Key] = kvp.Value;
            }
        }

        currentSession.checkpoints.Add(checkpoint);

        Debug.Log($"[PlayerData] Checkpoint '{checkpointName}' saved at {checkpoint.timestamp}\n" +
                  $"  Time from session start: {checkpoint.timeFromSessionStart:F2}s\n" +
                  $"  Time from last checkpoint: {checkpoint.timeFromLastCheckpoint:F2}s");

        // Auto-save if enabled - updates the SAME file
        if (autoSaveOnCheckpoint)
        {
            SaveSessionData();
        }
    }

    public void EndSession(bool autoSave = true)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("[PlayerData] No active session to end.");
            return;
        }

        currentSession.sessionEnd = DateTime.Now;
        currentSession.totalTimeSpent = (float)(currentSession.sessionEnd - currentSession.sessionStart).TotalSeconds;

        if (autoSave)
        {
            SaveSessionData();
        }

        Debug.Log($"[PlayerData] Session ended. Total time: {currentSession.totalTimeSpent:F2}s");
        Debug.Log($"[PlayerData] Final data saved to: {currentSessionFilePath}");

        currentSession = null;
        currentSessionFilePath = null;
    }

    private void SaveSessionData()
    {
        if (currentSession == null)
        {
            Debug.LogError("[PlayerData] Cannot save - no active session!");
            return;
        }

        if (string.IsNullOrEmpty(currentSessionFilePath))
        {
            Debug.LogError("[PlayerData] No file path set for current session!");
            return;
        }

        try
        {
            // Save entire session to the SAME file (overwrites previous state)
            string json = JsonUtility.ToJson(currentSession, true);
            File.WriteAllText(currentSessionFilePath, json);

            Debug.Log($"[PlayerData] Session updated: {currentSession.checkpoints.Count} checkpoints");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerData] Failed to save session: {e.Message}");
        }
    }

    public string GetPlayerName()
    {
        return currentSession != null && !string.IsNullOrEmpty(currentSession.playerName)
            ? currentSession.playerName
            : "Player";
    }

    /// <summary>
    /// Gets time spent in current session (in seconds)
    /// </summary>
    public float GetCurrentSessionTime()
    {
        if (currentSession == null) return 0f;
        return (float)(DateTime.Now - currentSession.sessionStart).TotalSeconds;
    }

    /// <summary>
    /// Gets time since last checkpoint (in seconds)
    /// </summary>
    public float GetTimeSinceLastCheckpoint()
    {
        if (currentSession == null || currentSession.checkpoints.Count == 0)
            return GetCurrentSessionTime();

        CheckpointData lastCheckpoint = currentSession.checkpoints[currentSession.checkpoints.Count - 1];
        return GetCurrentSessionTime() - lastCheckpoint.timeFromSessionStart;
    }

    /// <summary>
    /// Gets the number of checkpoints saved in current session
    /// </summary>
    public int GetCheckpointCount()
    {
        return currentSession?.checkpoints.Count ?? 0;
    }

    [ContextMenu("Print Current Session")]
    public void PrintCurrentSession()
    {
        if (currentSession == null)
        {
            Debug.Log("[PlayerData] No active session.");
            return;
        }

        string checkpointInfo = currentSession.checkpoints.Count > 0
            ? $"\n  Checkpoints: {currentSession.checkpoints.Count}"
            : "";

        Debug.Log($"[PlayerData] Current Session:\n" +
                  $"  Name: {currentSession.playerName}\n" +
                  $"  Age: {currentSession.playerAge}\n" +
                  $"  Grade: {currentSession.playerGrade}\n" +
                  $"  Sex: {currentSession.playerSex}\n" +
                  $"  Area Known: {currentSession.areaKnown}\n" +
                  $"  Session Time: {GetCurrentSessionTime():F2}s" +
                  checkpointInfo);

        foreach (var checkpoint in currentSession.checkpoints)
        {
            Debug.Log($"    - {checkpoint.checkpointName} @ {checkpoint.timeFromSessionStart:F2}s " +
                     $"(+{checkpoint.timeFromLastCheckpoint:F2}s from previous)");
        }
    }

    private void OnApplicationQuit()
    {
        // Auto-save on quit
        if (currentSession != null)
        {
            Debug.Log("[PlayerData] Application quitting - auto-saving session...");
            EndSession(autoSave: true);
        }
    }
}