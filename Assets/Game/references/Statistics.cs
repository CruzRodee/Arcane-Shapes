using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Persistent statistics tracker with game context recording
/// Access via: GameStatistics.Instance
/// </summary>
public class GameStatistics : MonoBehaviour
{
    // Singleton instance
    private static GameStatistics _instance;
    public static GameStatistics Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameStatistics>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("GameStatistics");
                    _instance = go.AddComponent<GameStatistics>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    // ==========================================
    // DATA STRUCTURES
    // ==========================================

    /// <summary>
    /// Context information captured when an event occurs
    /// </summary>
    [System.Serializable]
    public class GameContext
    {
        public int difficulty;           // 1-4
        public int levelIndex;           // 0-4 (which world)
        public int problemIndex;         // 0-4 (which problem within that level)
        public string shapeName;         // "Square", "Circle", etc.
        public int currentLives;         // Lives at the time of event
        public int currentScore;         // Score at the time of event
        public string timestamp;         // When the event occurred

        public GameContext(int diff, int level, int problem, string shape, int lives, int score)
        {
            difficulty = diff;
            levelIndex = level;
            problemIndex = problem;
            shapeName = shape;
            currentLives = lives;
            currentScore = score;
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public override string ToString()
        {
            return $"[Diff:{difficulty} Level:{levelIndex} Problem:{problemIndex} Shape:{shapeName} Lives:{currentLives} Score:{currentScore}] at {timestamp}";
        }
    }

    /// <summary>
    /// A recorded event with full context
    /// </summary>
    [System.Serializable]
    public class GameEvent
    {
        public string eventType;         // "GameOver", "LifeLost", "CorrectAnswer", etc.
        public GameContext context;
        public string additionalInfo;    // Optional extra details

        public GameEvent(string type, GameContext ctx, string info = "")
        {
            eventType = type;
            context = ctx;
            additionalInfo = info;
        }
    }

    [System.Serializable]
    public class StatisticsData
    {
        // Session Statistics
        public int currentSessionGameOvers = 0;
        public int currentSessionLivesLost = 0;
        public int currentSessionProblemsCompleted = 0;
        public int currentSessionCorrectAnswers = 0;
        public int currentSessionWrongAnswers = 0;
        public float currentSessionPlayTime = 0f;

        // All-Time Statistics
        public int totalGameOvers = 0;
        public int totalLivesLost = 0;
        public int totalProblemsCompleted = 0;
        public int totalCorrectAnswers = 0;
        public int totalWrongAnswers = 0;
        public float totalPlayTime = 0f;

        // Per-Level Statistics
        public List<LevelStatsData> levelStatistics = new List<LevelStatsData>();

        // NEW: Event history with full context
        public List<GameEvent> eventHistory = new List<GameEvent>();

        // NEW: Difficulty-based statistics
        public Dictionary<int, DifficultyStats> difficultyStatistics = new Dictionary<int, DifficultyStats>();

        // Timestamps
        public string firstPlayedDate;
        public string lastPlayedDate;
    }

    [System.Serializable]
    public class LevelStatsData
    {
        public string levelName;
        public int timesPlayed = 0;
        public int timesCompleted = 0;
        public int gameOvers = 0;
        public int livesLost = 0;
        public int correctAnswers = 0;
        public int wrongAnswers = 0;
        public float bestTime = float.MaxValue;
        public float totalTime = 0f;

        public float GetAverageTime()
        {
            return timesCompleted > 0 ? totalTime / timesCompleted : 0f;
        }

        public float GetAccuracy()
        {
            int total = correctAnswers + wrongAnswers;
            return total > 0 ? (float)correctAnswers / total * 100f : 0f;
        }
    }

    /// <summary>
    /// NEW: Statistics broken down by difficulty level
    /// </summary>
    [System.Serializable]
    public class DifficultyStats
    {
        public int difficulty;
        public int gameOvers = 0;
        public int livesLost = 0;
        public int correctAnswers = 0;
        public int wrongAnswers = 0;
        public int problemsCompleted = 0;

        public float GetAccuracy()
        {
            int total = correctAnswers + wrongAnswers;
            return total > 0 ? (float)correctAnswers / total * 100f : 0f;
        }
    }

    // The actual data
    [SerializeField] private StatisticsData data = new StatisticsData();

    // Track session start time
    private float sessionStartTime;

    // NEW: Max events to keep in history (to prevent huge save files)
    private const int MAX_EVENT_HISTORY = 500;

    // ==========================================
    // UNITY LIFECYCLE
    // ==========================================

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadFromPlayerPrefs();

        if (data.levelStatistics == null || data.levelStatistics.Count == 0)
        {
            InitializeLevelStats();
        }

        if (data.eventHistory == null)
        {
            data.eventHistory = new List<GameEvent>();
        }

        if (data.difficultyStatistics == null)
        {
            data.difficultyStatistics = new Dictionary<int, DifficultyStats>();
        }

        if (string.IsNullOrEmpty(data.firstPlayedDate))
        {
            data.firstPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        data.lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        sessionStartTime = Time.time;

        Debug.Log("GameStatistics: Initialized with context tracking");
    }

    void OnApplicationQuit()
    {
        SaveToPlayerPrefs();
        Debug.Log("GameStatistics: Saved on quit");
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveToPlayerPrefs();
        }
    }

    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void InitializeLevelStats()
    {
        data.levelStatistics = new List<LevelStatsData>
        {
            new LevelStatsData { levelName = "Square Mastery" },
            new LevelStatsData { levelName = "Rectangle Workshop" },
            new LevelStatsData { levelName = "Circle Academy" },
            new LevelStatsData { levelName = "Triangle Training" },
            new LevelStatsData { levelName = "Semi-Circle School" }
        };
    }

    private DifficultyStats GetOrCreateDifficultyStats(int difficulty)
    {
        if (!data.difficultyStatistics.ContainsKey(difficulty))
        {
            data.difficultyStatistics[difficulty] = new DifficultyStats { difficulty = difficulty };
        }
        return data.difficultyStatistics[difficulty];
    }

    // ==========================================
    // PUBLIC API - RECORDING WITH CONTEXT
    // ==========================================

    /// <summary>
    /// Record a game over event WITH game context
    /// Usage: GameStatistics.Instance.RecordGameOver(difficulty, levelIndex, problemIndex, shapeName, lives, score);
    /// </summary>
    public void RecordGameOver(int difficulty, int levelIndex, int problemIndex, string shapeName, int currentLives, int currentScore)
    {
        // Create context
        GameContext context = new GameContext(difficulty, levelIndex, problemIndex, shapeName, currentLives, currentScore);

        // Record event with context
        GameEvent gameEvent = new GameEvent("GameOver", context, $"Game over at difficulty {difficulty}");
        AddEventToHistory(gameEvent);

        // Update counters
        data.currentSessionGameOvers++;
        data.totalGameOvers++;

        if (levelIndex >= 0 && levelIndex < data.levelStatistics.Count)
        {
            data.levelStatistics[levelIndex].gameOvers++;
        }

        // Update difficulty stats
        GetOrCreateDifficultyStats(difficulty).gameOvers++;

        Debug.Log($"[Stats] Game Over: {context}");
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Record lives lost WITH game context
    /// Usage: GameStatistics.Instance.RecordLivesLost(amount, difficulty, levelIndex, problemIndex, shapeName, lives, score);
    /// </summary>
    public void RecordLivesLost(int amount, int difficulty, int levelIndex, int problemIndex, string shapeName, int currentLives, int currentScore)
    {
        GameContext context = new GameContext(difficulty, levelIndex, problemIndex, shapeName, currentLives, currentScore);
        GameEvent gameEvent = new GameEvent("LifeLost", context, $"Lost {amount} life(lives)");
        AddEventToHistory(gameEvent);

        data.currentSessionLivesLost += amount;
        data.totalLivesLost += amount;

        if (levelIndex >= 0 && levelIndex < data.levelStatistics.Count)
        {
            data.levelStatistics[levelIndex].livesLost += amount;
        }

        GetOrCreateDifficultyStats(difficulty).livesLost += amount;

        Debug.Log($"[Stats] {amount} lives lost: {context}");
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Record a correct answer WITH game context
    /// </summary>
    public void RecordCorrectAnswer(int difficulty, int levelIndex, int problemIndex, string shapeName, int currentLives, int currentScore)
    {
        GameContext context = new GameContext(difficulty, levelIndex, problemIndex, shapeName, currentLives, currentScore);
        GameEvent gameEvent = new GameEvent("CorrectAnswer", context);
        AddEventToHistory(gameEvent);

        data.currentSessionCorrectAnswers++;
        data.totalCorrectAnswers++;

        if (levelIndex >= 0 && levelIndex < data.levelStatistics.Count)
        {
            data.levelStatistics[levelIndex].correctAnswers++;
        }

        GetOrCreateDifficultyStats(difficulty).correctAnswers++;

        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Record a wrong answer WITH game context
    /// </summary>
    public void RecordWrongAnswer(int difficulty, int levelIndex, int problemIndex, string shapeName, int currentLives, int currentScore)
    {
        GameContext context = new GameContext(difficulty, levelIndex, problemIndex, shapeName, currentLives, currentScore);
        GameEvent gameEvent = new GameEvent("WrongAnswer", context);
        AddEventToHistory(gameEvent);

        data.currentSessionWrongAnswers++;
        data.totalWrongAnswers++;

        if (levelIndex >= 0 && levelIndex < data.levelStatistics.Count)
        {
            data.levelStatistics[levelIndex].wrongAnswers++;
        }

        GetOrCreateDifficultyStats(difficulty).wrongAnswers++;

        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Record problem completion WITH game context
    /// </summary>
    public void RecordProblemCompleted(int difficulty, int levelIndex, int problemIndex, string shapeName, int currentLives, int currentScore)
    {
        GameContext context = new GameContext(difficulty, levelIndex, problemIndex, shapeName, currentLives, currentScore);
        GameEvent gameEvent = new GameEvent("ProblemCompleted", context);
        AddEventToHistory(gameEvent);

        data.currentSessionProblemsCompleted++;
        data.totalProblemsCompleted++;

        if (levelIndex >= 0 && levelIndex < data.levelStatistics.Count)
        {
            data.levelStatistics[levelIndex].timesPlayed++;
        }

        GetOrCreateDifficultyStats(difficulty).problemsCompleted++;

        Debug.Log($"[Stats] Problem completed: {context}");
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Record level completion with time
    /// </summary>
    /// <summary>
    /// Record level completion WITH game context
    /// Usage: GameStatistics.Instance.RecordLevelCompleted(difficulty, levelIndex, problemIndex, shapeName, lives, score, completionTime);
    /// </summary>
    public void RecordLevelCompleted(int difficulty, int levelIndex, int problemIndex, string shapeName, int currentLives, int currentScore, float completionTime)
    {
        // Create context
        GameContext context = new GameContext(difficulty, levelIndex, problemIndex, shapeName, currentLives, currentScore);

        // Record event with context
        GameEvent gameEvent = new GameEvent("LevelCompleted", context, $"Completed in {completionTime:F2}s");
        AddEventToHistory(gameEvent);

        // Update level statistics
        if (levelIndex >= 0 && levelIndex < data.levelStatistics.Count)
        {
            LevelStatsData stats = data.levelStatistics[levelIndex];
            stats.timesCompleted++;
            stats.totalTime += completionTime;

            if (completionTime < stats.bestTime)
            {
                stats.bestTime = completionTime;
                Debug.Log($"[Stats] NEW BEST TIME for {stats.levelName}: {completionTime:F2}s");
            }
        }

        Debug.Log($"[Stats] Level completed: {context} in {completionTime:F2}s");
        SaveToPlayerPrefs();
    }

    /// <summary>
    /// Simplified version for backward compatibility
    /// </summary>
    public void RecordLevelCompleted(int levelIndex, float completionTime)
    {
        RecordLevelCompleted(1, levelIndex, 0, "Unknown", 0, 0, completionTime);
    }

    /// <summary>
    /// Add event to history with size management
    /// </summary>
    private void AddEventToHistory(GameEvent gameEvent)
    {
        data.eventHistory.Add(gameEvent);

        // Keep only the most recent events to prevent huge save files
        if (data.eventHistory.Count > MAX_EVENT_HISTORY)
        {
            data.eventHistory.RemoveAt(0);
        }
    }

    // ==========================================
    // SIMPLIFIED API (for backward compatibility)
    // ==========================================

    /// <summary>
    /// Simplified version without context (uses defaults)
    /// </summary>
    public void RecordGameOver(int levelIndex = -1)
    {
        RecordGameOver(1, levelIndex, 0, "Unknown", 0, 0);
    }

    public void RecordLivesLost(int amount, int levelIndex = -1)
    {
        RecordLivesLost(amount, 1, levelIndex, 0, "Unknown", 0, 0);
    }

    public void RecordCorrectAnswer(int levelIndex = -1)
    {
        RecordCorrectAnswer(1, levelIndex, 0, "Unknown", 0, 0);
    }

    public void RecordWrongAnswer(int levelIndex = -1)
    {
        RecordWrongAnswer(1, levelIndex, 0, "Unknown", 0, 0);
    }

    public void RecordProblemCompleted(int levelIndex = -1)
    {
        RecordProblemCompleted(1, levelIndex, 0, "Unknown", 0, 0);
    }

    // ==========================================
    // PUBLIC API - DATA ACCESS
    // ==========================================

    public StatisticsData GetData()
    {
        return data;
    }

    public LevelStatsData GetLevelStats(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < data.levelStatistics.Count)
        {
            return data.levelStatistics[levelIndex];
        }
        return null;
    }

    public DifficultyStats GetDifficultyStats(int difficulty)
    {
        if (data.difficultyStatistics.ContainsKey(difficulty))
        {
            return data.difficultyStatistics[difficulty];
        }
        return null;
    }

    public List<GameEvent> GetEventHistory()
    {
        return data.eventHistory;
    }

    public List<GameEvent> GetEventsByType(string eventType)
    {
        List<GameEvent> filtered = new List<GameEvent>();
        foreach (var evt in data.eventHistory)
        {
            if (evt.eventType == eventType)
            {
                filtered.Add(evt);
            }
        }
        return filtered;
    }

    public int GetSessionGameOvers() => data.currentSessionGameOvers;
    public int GetTotalGameOvers() => data.totalGameOvers;
    public int GetSessionLivesLost() => data.currentSessionLivesLost;
    public int GetTotalLivesLost() => data.totalLivesLost;

    public void UpdatePlayTime(float deltaTime)
    {
        data.currentSessionPlayTime += deltaTime;
        data.totalPlayTime += deltaTime;
    }

    public void ResetSessionStats()
    {
        data.currentSessionGameOvers = 0;
        data.currentSessionLivesLost = 0;
        data.currentSessionProblemsCompleted = 0;
        data.currentSessionCorrectAnswers = 0;
        data.currentSessionWrongAnswers = 0;
        data.currentSessionPlayTime = 0f;
        sessionStartTime = Time.time;

        Debug.Log("[Stats] Session stats reset");
    }

    // ==========================================
    // PERSISTENCE
    // ==========================================

    private const string SAVE_KEY = "GameStatistics_Data";

    private void SaveToPlayerPrefs()
    {
        try
        {
            data.lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Stats] Failed to save: {e.Message}");
        }
    }

    private void LoadFromPlayerPrefs()
    {
        try
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                data = JsonUtility.FromJson<StatisticsData>(json);
                Debug.Log($"[Stats] Loaded from PlayerPrefs");
            }
            else
            {
                Debug.Log($"[Stats] No saved data found, starting fresh");
                data = new StatisticsData();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Stats] Failed to load: {e.Message}");
            data = new StatisticsData();
        }
    }

    public string ExportToJson()
    {
        data.lastPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        return JsonUtility.ToJson(data, true);
    }

    public string GetSummaryReport()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("========== GAME STATISTICS SUMMARY ==========");
        sb.AppendLine($"First Played: {data.firstPlayedDate}");
        sb.AppendLine($"Last Played: {data.lastPlayedDate}");
        sb.AppendLine($"Total Events Recorded: {data.eventHistory.Count}");
        sb.AppendLine();

        sb.AppendLine("SESSION STATS:");
        sb.AppendLine($"  Game Overs: {data.currentSessionGameOvers}");
        sb.AppendLine($"  Lives Lost: {data.currentSessionLivesLost}");
        sb.AppendLine($"  Problems Completed: {data.currentSessionProblemsCompleted}");
        sb.AppendLine($"  Correct: {data.currentSessionCorrectAnswers}, Wrong: {data.currentSessionWrongAnswers}");
        sb.AppendLine();

        sb.AppendLine("ALL-TIME STATS:");
        sb.AppendLine($"  Game Overs: {data.totalGameOvers}");
        sb.AppendLine($"  Lives Lost: {data.totalLivesLost}");
        sb.AppendLine($"  Problems Completed: {data.totalProblemsCompleted}");
        sb.AppendLine($"  Correct: {data.totalCorrectAnswers}, Wrong: {data.totalWrongAnswers}");

        float totalAccuracy = (data.totalCorrectAnswers + data.totalWrongAnswers) > 0
            ? (float)data.totalCorrectAnswers / (data.totalCorrectAnswers + data.totalWrongAnswers) * 100f
            : 0f;
        sb.AppendLine($"  Overall Accuracy: {totalAccuracy:F1}%");
        sb.AppendLine();

        sb.AppendLine("PER-DIFFICULTY STATS:");
        foreach (var kvp in data.difficultyStatistics)
        {
            DifficultyStats diffStats = kvp.Value;
            sb.AppendLine($"  Difficulty {diffStats.difficulty}:");
            sb.AppendLine($"    Game Overs: {diffStats.gameOvers}");
            sb.AppendLine($"    Lives Lost: {diffStats.livesLost}");
            sb.AppendLine($"    Problems Completed: {diffStats.problemsCompleted}");
            sb.AppendLine($"    Accuracy: {diffStats.GetAccuracy():F1}%");
        }
        sb.AppendLine();

        sb.AppendLine("PER-LEVEL STATS:");
        foreach (var levelStat in data.levelStatistics)
        {
            sb.AppendLine($"  {levelStat.levelName}:");
            sb.AppendLine($"    Played: {levelStat.timesPlayed}, Completed: {levelStat.timesCompleted}");
            sb.AppendLine($"    Game Overs: {levelStat.gameOvers}, Lives Lost: {levelStat.livesLost}");
            sb.AppendLine($"    Accuracy: {levelStat.GetAccuracy():F1}%");
            sb.AppendLine($"    Best Time: {(levelStat.bestTime < float.MaxValue ? $"{levelStat.bestTime:F2}s" : "N/A")}");
        }
        sb.AppendLine();

        sb.AppendLine("RECENT EVENTS (Last 10):");
        int startIndex = Mathf.Max(0, data.eventHistory.Count - 10);
        for (int i = startIndex; i < data.eventHistory.Count; i++)
        {
            GameEvent evt = data.eventHistory[i];
            sb.AppendLine($"  [{evt.eventType}] {evt.context}");
        }

        sb.AppendLine("============================================");

        return sb.ToString();
    }

    public void ClearAllData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        data = new StatisticsData();
        InitializeLevelStats();
        data.firstPlayedDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        data.lastPlayedDate = data.firstPlayedDate;
        Debug.Log("[Stats] All data cleared");
    }
}