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
    public Dictionary<string, string> choices = new Dictionary<string, string>();
    public Dictionary<string, string> inputs = new Dictionary<string, string>();
    public DateTime sessionStart;
    public DateTime sessionEnd;
    public float totalTimeSpent = 0f;
}

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance { get; private set; }

    private PlayerSessionData currentSession;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            StartNewSession();
            InitializeVariableStore();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewSession()
    {
        currentSession = new PlayerSessionData();
        currentSession.sessionStart = DateTime.Now;
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

    public string GetPlayerName()
    {
        return string.IsNullOrEmpty(currentSession.playerName) ? "Player" : currentSession.playerName;
    }

    public void RecordChoice(string questionKey, string choice)
    {
        if (!string.IsNullOrEmpty(questionKey))
        {
            currentSession.choices[questionKey] = choice;
            Debug.Log($"[PlayerData] Recorded choice: {questionKey} = {choice}");
        }
    }

    public void RecordInput(string inputKey, string value)
    {
        if (!string.IsNullOrEmpty(inputKey))
        {
            currentSession.inputs[inputKey] = value;
            Debug.Log($"[PlayerData] Recorded input: {inputKey} = {value}");
        }
    }

    public void EndSession()
    {
        currentSession.sessionEnd = DateTime.Now;
        currentSession.totalTimeSpent = (float)(currentSession.sessionEnd - currentSession.sessionStart).TotalSeconds;
        SaveSessionData();
    }

    private void SaveSessionData()
    {
        try
        {
            // Save as JSON
            string json = JsonUtility.ToJson(currentSession, true);
            string fileName = $"PlayerData_{SanitizeFileName(currentSession.playerName)}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(filePath, json);

            Debug.Log($"[PlayerData] Session saved to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerData] Failed to save session: {e.Message}");
        }
    }

    private string SanitizeFileName(string fileName)
    {
        string invalid = new string(Path.GetInvalidFileNameChars());
        foreach (char c in invalid)
        {
            fileName = fileName.Replace(c.ToString(), "");
        }
        return string.IsNullOrEmpty(fileName) ? "Unknown" : fileName;
    }

    [ContextMenu("Print Current Session")]
    public void PrintCurrentSession()
    {
        if (currentSession == null)
        {
            Debug.Log("[PlayerData] No active session.");
            return;
        }

        Debug.Log($"[PlayerData] Current Session:\n" +
                  $"Name: {currentSession.playerName}\n" +
                  $"Age: {currentSession.playerAge}\n" +
                  $"Grade: {currentSession.playerGrade}\n" +
                  $"Sex: {currentSession.playerSex}\n" +
                  $"Area Known: {currentSession.areaKnown}\n" +
                  $"Choices: {currentSession.choices.Count}\n" +
                  $"Inputs: {currentSession.inputs.Count}");
    }
}