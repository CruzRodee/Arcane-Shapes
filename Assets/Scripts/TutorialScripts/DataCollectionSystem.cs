using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

[System.Serializable]
public class PlayerData
{
    public string playerName = "";
    public int playerAge = 0;
    public int playerGrade = 0;
    public string playerSex = "";
    public bool areaKnown = false;
    public string areaUnderstanding = "";
    public int squareAreaAnswer = 0;
    public int rectangleAreaAnswer = 0;
    public List<string> wrongShapes = new List<string>();
    public Dictionary<string, int> attentionCheckLoops = new Dictionary<string, int>();
    public Dictionary<string, int> questionRetries = new Dictionary<string, int>();
    public DateTime sessionStart;
    public DateTime sessionEnd;
    public float totalTimeSpent = 0f; // in seconds
}

public class DataCollectionSystem : MonoBehaviour
{
    [Header("Data Collection Settings")]
    [SerializeField] private bool enableDataCollection = true;
    [SerializeField] private string csvFileName = "PlayerDataCollection.csv";

    // Current session data
    private PlayerData currentPlayerData;
    private string currentQuestionKey = "";
    private Dictionary<string, string> correctShapeAnswers;

    // Events for DialogueSystem to subscribe to
    public static System.Action<string, string> OnChoiceSelected;
    public static System.Action<string, string, string> OnInputReceived; // inputType, value, questionKey
    public static System.Action<string> OnQuestionStarted;
    public static System.Action OnSessionCompleted;

    // Singleton pattern for easy access
    public static DataCollectionSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeSystem();
    }

    private void InitializeSystem()
    {
        currentPlayerData = new PlayerData();
        currentPlayerData.sessionStart = DateTime.Now;

        // Initialize correct answers dictionary
        correctShapeAnswers = new Dictionary<string, string>
        {
            {"square", "Square"},
            {"rectangle", "Rectangle"},
            {"circle", "Circle"},
            {"semicircle", "Semicircle"},
            {"triangle", "Triangle"}
        };

        // Subscribe to events
        OnChoiceSelected += HandleChoiceSelected;
        OnInputReceived += HandleInputReceived;
        OnQuestionStarted += HandleQuestionStarted;
        OnSessionCompleted += HandleSessionCompleted;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        OnChoiceSelected -= HandleChoiceSelected;
        OnInputReceived -= HandleInputReceived;
        OnQuestionStarted -= HandleQuestionStarted;
        OnSessionCompleted -= HandleSessionCompleted;
    }

    #region Public Methods for DialogueSystem to Call

    /// <summary>
    /// Call this when starting a new question to set context
    /// </summary>
    public static void StartQuestion(string questionKey)
    {
        OnQuestionStarted?.Invoke(questionKey);
    }

    /// <summary>
    /// Call this when player makes a choice
    /// </summary>
    public static void RecordChoice(string choiceCode, string questionKey = "")
    {
        OnChoiceSelected?.Invoke(choiceCode, questionKey);
    }

    /// <summary>
    /// Call this when player provides input
    /// </summary>
    public static void RecordInput(string inputType, string value, string questionKey = "")
    {
        OnInputReceived?.Invoke(inputType, value, questionKey);
    }

    /// <summary>
    /// Call this when the questionnaire is complete
    /// </summary>
    public static void CompleteSession()
    {
        OnSessionCompleted?.Invoke();
    }

    /// <summary>
    /// Get current player name for dialogue display
    /// </summary>
    public static string GetPlayerName()
    {
        return Instance?.currentPlayerData?.playerName ?? "Player";
    }

    /// <summary>
    /// Check if a shape was previously answered incorrectly (for graying out choices)
    /// </summary>
    public static bool WasShapeAnsweredIncorrectly(string shapeName)
    {
        return Instance?.currentPlayerData?.wrongShapes?.Contains(shapeName) ?? false;
    }

    /// <summary>
    /// Get available choices for a shape question (excluding grayed out ones)
    /// </summary>
    public static List<string> GetAvailableShapeChoices(string shapeName)
    {
        List<string> allChoices = new List<string> { "Square", "Rectangle", "Circle", "Triangle" };

        if (shapeName == "semicircle")
        {
            allChoices = new List<string> { "Semicircle", "Circle", "Square", "Rectangle" };
        }

        // If this shape was answered incorrectly before, only show the correct answer
        if (Instance != null && Instance.currentPlayerData.wrongShapes.Contains(shapeName))
        {
            string correctAnswer = Instance.correctShapeAnswers.ContainsKey(shapeName)
                ? Instance.correctShapeAnswers[shapeName]
                : allChoices[0];
            return new List<string> { correctAnswer };
        }

        return allChoices;
    }

    #endregion

    #region Event Handlers

    private void HandleQuestionStarted(string questionKey)
    {
        currentQuestionKey = questionKey;
        Debug.Log($"[DataCollection] Question started: {questionKey}");
    }

    private void HandleChoiceSelected(string choiceCode, string questionKey)
    {
        if (!enableDataCollection) return;

        string key = string.IsNullOrEmpty(questionKey) ? currentQuestionKey : questionKey;
        Debug.Log($"[DataCollection] Choice selected: {choiceCode} for question: {key}");

        // Handle attention checks
        if (key.Contains("attention_check"))
        {
            if (choiceCode == "Hindi")
            {
                if (!currentPlayerData.attentionCheckLoops.ContainsKey(key))
                    currentPlayerData.attentionCheckLoops[key] = 0;
                currentPlayerData.attentionCheckLoops[key]++;
                Debug.Log($"[DataCollection] Attention check loop #{currentPlayerData.attentionCheckLoops[key]} for {key}");
            }
        }

        // Handle shape identification
        if (key.StartsWith("shape_"))
        {
            string shapeName = key.Replace("shape_", "");
            bool isCorrect = IsCorrectShapeAnswer(shapeName, choiceCode);

            if (!isCorrect)
            {
                if (!currentPlayerData.wrongShapes.Contains(shapeName))
                {
                    currentPlayerData.wrongShapes.Add(shapeName);
                    Debug.Log($"[DataCollection] Wrong shape answer recorded: {shapeName}");
                }

                if (!currentPlayerData.questionRetries.ContainsKey(key))
                    currentPlayerData.questionRetries[key] = 0;
                currentPlayerData.questionRetries[key]++;
                Debug.Log($"[DataCollection] Shape retry #{currentPlayerData.questionRetries[key]} for {shapeName}");
            }
        }

        // Handle specific data collection choices
        switch (key)
        {
            case "player_sex":
                currentPlayerData.playerSex = choiceCode;
                Debug.Log($"[DataCollection] Player sex recorded: {choiceCode}");
                break;
            case "area_known":
                currentPlayerData.areaKnown = (choiceCode == "Opo");
                Debug.Log($"[DataCollection] Area knowledge recorded: {currentPlayerData.areaKnown}");
                break;
            case "excited_check":
                // Just for tracking, no specific data storage needed
                break;
        }
    }

    private void HandleInputReceived(string inputType, string value, string questionKey)
    {
        if (!enableDataCollection) return;

        string key = string.IsNullOrEmpty(questionKey) ? currentQuestionKey : questionKey;
        Debug.Log($"[DataCollection] Input received: {inputType} = {value} for question: {key}");

        switch (inputType.ToLower())
        {
            case "name":
                currentPlayerData.playerName = value;
                break;

            case "age":
                if (int.TryParse(value, out int age))
                {
                    currentPlayerData.playerAge = age;
                }
                else
                {
                    Debug.LogWarning($"[DataCollection] Invalid age input: {value}");
                }
                break;

            case "grade":
                if (int.TryParse(value, out int grade))
                {
                    currentPlayerData.playerGrade = grade;
                }
                else
                {
                    Debug.LogWarning($"[DataCollection] Invalid grade input: {value}");
                }
                break;

            case "area_understanding":
                currentPlayerData.areaUnderstanding = value;
                break;

            case "square_area":
                if (int.TryParse(value, out int squareArea))
                {
                    currentPlayerData.squareAreaAnswer = squareArea;
                    if (squareArea != 4) // Correct answer is 4
                    {
                        if (!currentPlayerData.questionRetries.ContainsKey("square_area"))
                            currentPlayerData.questionRetries["square_area"] = 0;
                        currentPlayerData.questionRetries["square_area"]++;
                        Debug.Log($"[DataCollection] Square area wrong answer. Retry #{currentPlayerData.questionRetries["square_area"]}");
                    }
                }
                break;

            case "rectangle_area":
                if (int.TryParse(value, out int rectArea))
                {
                    currentPlayerData.rectangleAreaAnswer = rectArea;
                    if (rectArea != 6) // Correct answer is 6
                    {
                        if (!currentPlayerData.questionRetries.ContainsKey("rectangle_area"))
                            currentPlayerData.questionRetries["rectangle_area"] = 0;
                        currentPlayerData.questionRetries["rectangle_area"]++;
                        Debug.Log($"[DataCollection] Rectangle area wrong answer. Retry #{currentPlayerData.questionRetries["rectangle_area"]}");
                    }
                }
                break;
        }
    }

    private void HandleSessionCompleted()
    {
        if (!enableDataCollection) return;

        currentPlayerData.sessionEnd = DateTime.Now;
        currentPlayerData.totalTimeSpent = (float)(currentPlayerData.sessionEnd - currentPlayerData.sessionStart).TotalSeconds;

        Debug.Log($"[DataCollection] Session completed for {currentPlayerData.playerName}. Duration: {currentPlayerData.totalTimeSpent:F2} seconds");

        SavePlayerData();
    }

    #endregion

    #region Helper Methods

    private bool IsCorrectShapeAnswer(string shapeName, string answer)
    {
        return correctShapeAnswers.ContainsKey(shapeName) && correctShapeAnswers[shapeName] == answer;
    }

    /// <summary>
    /// Check if a numeric answer is correct and handle retries
    /// </summary>
    public static bool ValidateNumericAnswer(string questionType, int answer, int correctAnswer)
    {
        if (Instance == null || !Instance.enableDataCollection) return true;

        bool isCorrect = (answer == correctAnswer);

        if (!isCorrect)
        {
            if (!Instance.currentPlayerData.questionRetries.ContainsKey(questionType))
                Instance.currentPlayerData.questionRetries[questionType] = 0;
            Instance.currentPlayerData.questionRetries[questionType]++;
            Debug.Log($"[DataCollection] {questionType} wrong answer. Retry #{Instance.currentPlayerData.questionRetries[questionType]}");
        }

        return isCorrect;
    }

    #endregion

    #region Data Saving

    private void SavePlayerData()
    {
        try
        {
            // Save JSON file
            SaveAsJSON();

            // Save to CSV
            SaveAsCSV();

            Debug.Log($"[DataCollection] Data saved successfully for player: {currentPlayerData.playerName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DataCollection] Failed to save data: {e.Message}");
        }
    }

    private void SaveAsJSON()
    {
        string json = JsonUtility.ToJson(currentPlayerData, true);
        string fileName = $"PlayerData_{SanitizeFileName(currentPlayerData.playerName)}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(filePath, json);
        Debug.Log($"[DataCollection] JSON saved to: {filePath}");
    }

    private void SaveAsCSV()
    {
        string csvPath = Path.Combine(Application.persistentDataPath, csvFileName);
        bool fileExists = File.Exists(csvPath);

        using (StreamWriter writer = new StreamWriter(csvPath, true))
        {
            // Write header if file doesn't exist
            if (!fileExists)
            {
                writer.WriteLine("PlayerName,Age,Grade,Sex,AreaKnown,AreaUnderstanding,SquareAreaAnswer,RectangleAreaAnswer,WrongShapes,AttentionCheckLoops,QuestionRetries,SessionStart,SessionEnd,TotalTimeSpent");
            }

            // Prepare data
            string wrongShapesStr = string.Join(";", currentPlayerData.wrongShapes);
            string attentionLoopsStr = SerializeDictionary(currentPlayerData.attentionCheckLoops);
            string retriesStr = SerializeDictionary(currentPlayerData.questionRetries);

            // Write player data
            writer.WriteLine($"\"{currentPlayerData.playerName}\",{currentPlayerData.playerAge},{currentPlayerData.playerGrade},\"{currentPlayerData.playerSex}\",{currentPlayerData.areaKnown},\"{EscapeCSV(currentPlayerData.areaUnderstanding)}\",{currentPlayerData.squareAreaAnswer},{currentPlayerData.rectangleAreaAnswer},\"{wrongShapesStr}\",\"{attentionLoopsStr}\",\"{retriesStr}\",{currentPlayerData.sessionStart:yyyy-MM-dd HH:mm:ss},{currentPlayerData.sessionEnd:yyyy-MM-dd HH:mm:ss},{currentPlayerData.totalTimeSpent:F2}");
        }

        Debug.Log($"[DataCollection] CSV data appended to: {csvPath}");
    }

    private string SerializeDictionary(Dictionary<string, int> dict)
    {
        List<string> pairs = new List<string>();
        foreach (var kvp in dict)
        {
            pairs.Add($"{kvp.Key}:{kvp.Value}");
        }
        return string.Join(";", pairs);
    }

    private string SanitizeFileName(string fileName)
    {
        string invalid = new string(Path.GetInvalidFileNameChars());
        foreach (char c in invalid)
        {
            fileName = fileName.Replace(c.ToString(), "");
        }
        return fileName;
    }

    private string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        return value.Replace("\"", "\"\""); // Escape quotes for CSV
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Print Current Data")]
    private void PrintCurrentData()
    {
        if (currentPlayerData == null)
        {
            Debug.Log("[DataCollection] No data collected yet.");
            return;
        }

        Debug.Log($"[DataCollection] Current Player Data:\n" +
                  $"Name: {currentPlayerData.playerName}\n" +
                  $"Age: {currentPlayerData.playerAge}\n" +
                  $"Grade: {currentPlayerData.playerGrade}\n" +
                  $"Sex: {currentPlayerData.playerSex}\n" +
                  $"Area Known: {currentPlayerData.areaKnown}\n" +
                  $"Wrong Shapes: {string.Join(", ", currentPlayerData.wrongShapes)}\n" +
                  $"Attention Loops: {currentPlayerData.attentionCheckLoops.Count}\n" +
                  $"Question Retries: {currentPlayerData.questionRetries.Count}");
    }

    [ContextMenu("Test Save Data")]
    private void TestSaveData()
    {
        // Fill with test data
        currentPlayerData.playerName = "TestPlayer";
        currentPlayerData.playerAge = 12;
        currentPlayerData.playerGrade = 6;
        currentPlayerData.playerSex = "Lalaki";
        currentPlayerData.areaKnown = true;
        currentPlayerData.areaUnderstanding = "Test understanding";
        currentPlayerData.wrongShapes.Add("square");
        currentPlayerData.attentionCheckLoops["attention_check_1"] = 2;
        currentPlayerData.questionRetries["shape_square"] = 1;

        HandleSessionCompleted();
    }

    #endregion
}