using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using System.Collections;

public class DebugToFile : MonoBehaviour
{
    [Header("Log Settings")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool includeStackTraceForWarnings = false;
    [SerializeField] private bool logSceneChanges = true;
    [SerializeField] private int maxLogFileSize = 5000000; // 5MB in bytes
    [SerializeField] private int maxLogFiles = 10; // Maximum number of log files to keep

    [System.NonSerialized] private string baseLogFileName;
    [System.NonSerialized] private string currentLogFilePath;
    [System.NonSerialized] private FileStream logFileStream;
    [System.NonSerialized] private StreamWriter logWriter;
    [System.NonSerialized] private bool isInitialized = false;

    private static DebugToFile instance;
    // private static bool applicationIsQuitting = false;

    void Awake()
    {
        // Only persist and log in development builds
        if (!Debug.isDebugBuild)
        {
            Destroy(gameObject);
            return;
        }

        // Robust singleton pattern that survives scene changes
        if (instance != null)
        {
            if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Move to DontDestroyOnLoad scene explicitly
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
        }

        if (enableLogging && !isInitialized)
        {
            InitializeLogging();
        }

        // Subscribe to scene management events
        if (logSceneChanges && !isInitialized)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }
    }

    void Start()
    {
        // Additional safety check - ensure logging is working after Start
        if (!isInitialized && enableLogging && Debug.isDebugBuild)
        {
            Debug.LogWarning("[DebugToFile] Logging not initialized in Awake, attempting in Start");
            InitializeLogging();
        }

        // Test log to confirm it's working
        Debug.Log($"[DebugToFile] Logger active in scene: {SceneManager.GetActiveScene().name}");
    }

    void InitializeLogging()
    {
        if (isInitialized) return;

        try
        {
            // Create base filename with timestamp
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            baseLogFileName = $"unity_log_{timestamp}";

            // Clean up old log files
            CleanupOldLogFiles();

            // Create new log file
            CreateNewLogFile();

            // Subscribe to ALL Unity log messages (this captures everything)
            Application.logMessageReceived += OnLogMessageReceived;
            Application.logMessageReceivedThreaded += OnLogMessageReceivedThreaded;

            // Also capture unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            isInitialized = true;

            // Log session information
            LogSessionInfo();

        }
        catch (Exception e)
        {
            Debug.LogError($"[DebugToFile] Failed to initialize logging: {e}");
            enableLogging = false;
        }
    }

    void CreateNewLogFile()
    {
        string sceneInfo = SceneManager.GetActiveScene().name;
        currentLogFilePath = Path.Combine(Application.persistentDataPath, $"{baseLogFileName}_{sceneInfo}.txt");

        // Ensure we don't overwrite existing files
        int counter = 1;
        string originalPath = currentLogFilePath;
        while (File.Exists(currentLogFilePath))
        {
            currentLogFilePath = Path.ChangeExtension(originalPath, null) + $"_{counter:000}.txt";
            counter++;
        }

        // Create new file stream
        logFileStream = new FileStream(currentLogFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        logWriter = new StreamWriter(logFileStream);
        logWriter.AutoFlush = true;
    }

    void CleanupOldLogFiles()
    {
        try
        {
            string[] logFiles = Directory.GetFiles(Application.persistentDataPath, "unity_log_*.txt");
            if (logFiles.Length > maxLogFiles)
            {
                // Sort by creation time and delete oldest files
                Array.Sort(logFiles, (x, y) => File.GetCreationTime(x).CompareTo(File.GetCreationTime(y)));

                for (int i = 0; i < logFiles.Length - maxLogFiles; i++)
                {
                    try
                    {
                        File.Delete(logFiles[i]);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[DebugToFile] Could not delete old log file {logFiles[i]}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[DebugToFile] Error during log file cleanup: {e.Message}");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!enableLogging || !isInitialized) return;

        try
        {
            WriteLogEntry($"SCENE LOADED: {scene.name} (Mode: {mode})", LogType.Log);

            // Create a new log file for the new scene if desired
            // This ensures each scene gets its own log section
            WriteLogEntry($"=== SCENE: {scene.name} ===", LogType.Log);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DebugToFile] Error logging scene load: {e}");
        }
    }

    void OnSceneUnloaded(Scene scene)
    {
        if (!enableLogging || !isInitialized) return;

        try
        {
            WriteLogEntry($"SCENE UNLOADED: {scene.name}", LogType.Log);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DebugToFile] Error logging scene unload: {e}");
        }
    }

    // Main log callback - handles logs from main thread
    void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (!enableLogging || logWriter == null) return;

        ProcessLogMessage(logString, stackTrace, type, "[MAIN]");
    }

    // Threaded log callback - handles logs from background threads
    void OnLogMessageReceivedThreaded(string logString, string stackTrace, LogType type)
    {
        if (!enableLogging || logWriter == null) return;

        // Queue threaded logs to be processed on main thread for thread safety
        StartCoroutine(ProcessThreadedLog(logString, stackTrace, type));
    }

    IEnumerator ProcessThreadedLog(string logString, string stackTrace, LogType type)
    {
        yield return null; // Wait one frame to ensure thread safety
        ProcessLogMessage(logString, stackTrace, type, "[THREAD]");
    }

    void ProcessLogMessage(string logString, string stackTrace, LogType type)
    {
        ProcessLogMessage(logString, stackTrace, type, "");
    }

    void ProcessLogMessage(string logString, string stackTrace, LogType type, string threadInfo)
    {
        try
        {
            // Check file size and rotate if necessary
            if (logFileStream != null && logFileStream.Length > maxLogFileSize)
            {
                RotateLogFile();
            }

            WriteLogEntry($"{threadInfo}{logString}", type);

            // Include stack trace based on log type and settings
            bool includeStack = (type == LogType.Error || type == LogType.Exception) ||
                               (type == LogType.Assert) ||
                               (type == LogType.Warning && includeStackTraceForWarnings);

            if (includeStack && !string.IsNullOrEmpty(stackTrace))
            {
                // Clean up stack trace for better readability
                string[] stackLines = stackTrace.Split('\n');
                foreach (string line in stackLines)
                {
                    if (!string.IsNullOrEmpty(line.Trim()))
                    {
                        WriteLogEntry($"  {line.Trim()}", type);
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Use Console to avoid recursive logging
            Console.WriteLine($"[DebugToFile] Error processing log message: {e}");
        }
    }

    void RotateLogFile()
    {
        try
        {
            WriteLogEntry("=== LOG FILE SIZE LIMIT REACHED - ROTATING ===", LogType.Log);

            // Close current file
            logWriter?.Close();
            logFileStream?.Close();

            // Create new file with incremented number
            string directory = Path.GetDirectoryName(currentLogFilePath);
            string filenameWithoutExt = Path.GetFileNameWithoutExtension(currentLogFilePath);
            string extension = Path.GetExtension(currentLogFilePath);

            int counter = 1;
            string newPath;
            do
            {
                newPath = Path.Combine(directory, $"{filenameWithoutExt}_part{counter:000}{extension}");
                counter++;
            } while (File.Exists(newPath));

            currentLogFilePath = newPath;
            logFileStream = new FileStream(currentLogFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            logWriter = new StreamWriter(logFileStream);
            logWriter.AutoFlush = true;

            WriteLogEntry("=== LOG FILE ROTATED - CONTINUING ===", LogType.Log);
        }
        catch (Exception e)
        {
            Console.WriteLine($"[DebugToFile] Error rotating log file: {e}");
        }
    }

    void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (!enableLogging || logWriter == null) return;

        try
        {
            WriteLogEntry($"UNHANDLED EXCEPTION: {e.ExceptionObject}", LogType.Exception);
            logWriter?.Flush();
        }
        catch
        {
            // Can't do much here
        }
    }

    void WriteLogEntry(string message, LogType type)
    {
        if (logWriter == null) return;

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string logLevel = GetLogTypeString(type);
        string currentScene = SceneManager.GetActiveScene().name;

        string formattedLog = $"[{timestamp}] [{logLevel}] [{currentScene}] {message}";

        logWriter.WriteLine(formattedLog);
    }

    string GetLogTypeString(LogType type)
    {
        switch (type)
        {
            case LogType.Error: return "ERROR";
            case LogType.Assert: return "ASSERT";
            case LogType.Warning: return "WARN";
            case LogType.Log: return "INFO";
            case LogType.Exception: return "EXCEPTION";
            default: return type.ToString().ToUpper();
        }
    }

    void LogSessionInfo()
    {
        WriteLogEntry("=== LOGGING SESSION STARTED ===", LogType.Log);
        WriteLogEntry($"Device: {SystemInfo.deviceModel}", LogType.Log);
        WriteLogEntry($"OS: {SystemInfo.operatingSystem}", LogType.Log);
        WriteLogEntry($"Unity Version: {Application.unityVersion}", LogType.Log);
        WriteLogEntry($"App Version: {Application.version}", LogType.Log);
        WriteLogEntry($"Graphics Device: {SystemInfo.graphicsDeviceName}", LogType.Log);
        WriteLogEntry($"Memory Size: {SystemInfo.systemMemorySize}MB", LogType.Log);
        WriteLogEntry($"Persistent Data Path: {Application.persistentDataPath}", LogType.Log);
        WriteLogEntry($"Log File: {currentLogFilePath}", LogType.Log);
        WriteLogEntry($"Initial Scene: {SceneManager.GetActiveScene().name}", LogType.Log);
        WriteLogEntry("======================================", LogType.Log);
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (enableLogging && logWriter != null)
        {
            WriteLogEntry($"APPLICATION PAUSE: {pauseStatus}", LogType.Log);
            if (pauseStatus) logWriter?.Flush(); // Ensure logs are written when pausing
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (enableLogging && logWriter != null)
        {
            WriteLogEntry($"APPLICATION FOCUS: {hasFocus}", LogType.Log);
        }
    }

    void OnDestroy()
    {
        CleanupLogging();
    }

    void OnApplicationQuit()
    {
        CleanupLogging();
    }

    void CleanupLogging()
    {
        if (!isInitialized) return;

        try
        {
            if (logWriter != null)
            {
                WriteLogEntry("=== LOGGING SESSION ENDED ===", LogType.Log);

                // Unsubscribe from all events
                Application.logMessageReceived -= OnLogMessageReceived;
                Application.logMessageReceivedThreaded -= OnLogMessageReceivedThreaded;

                if (logSceneChanges)
                {
                    SceneManager.sceneLoaded -= OnSceneLoaded;
                    SceneManager.sceneUnloaded -= OnSceneUnloaded;
                }

                AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

                logWriter.Close();
                logWriter = null;
            }

            if (logFileStream != null)
            {
                logFileStream.Close();
                logFileStream = null;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[DebugToFile] Error during cleanup: {e}");
        }
        finally
        {
            isInitialized = false;
            if (instance == this) instance = null;
        }
    }

    // Public method to manually log messages
    public static void LogMessage(string message, LogType type = LogType.Log)
    {
        switch (type)
        {
            case LogType.Log:
                Debug.Log(message);
                break;
            case LogType.Warning:
                Debug.LogWarning(message);
                break;
            case LogType.Error:
                Debug.LogError(message);
                break;
        }
    }

    // Public method to get current log file path
    public static string GetCurrentLogFilePath()
    {
        return instance?.currentLogFilePath ?? "";
    }

    // Method to force create a new log file (useful for scene transitions)
    [ContextMenu("Create New Log File")]
    public void CreateNewLogFileManually()
    {
        if (!enableLogging || !isInitialized) return;

        try
        {
            WriteLogEntry("=== MANUALLY CREATING NEW LOG FILE ===", LogType.Log);

            // Close current file
            logWriter?.Close();
            logFileStream?.Close();

            // Create new file
            CreateNewLogFile();

            WriteLogEntry("=== NEW LOG FILE CREATED ===", LogType.Log);
            WriteLogEntry($"Current Scene: {SceneManager.GetActiveScene().name}", LogType.Log);
        }
        catch (Exception e)
        {
            Debug.LogError($"[DebugToFile] Failed to create new log file: {e}");
        }
    }

    // Method to clear old log files manually
    [ContextMenu("Clear Old Log Files")]
    public void ClearOldLogFiles()
    {
        CleanupOldLogFiles();
        Debug.Log("[DebugToFile] Old log files cleaned up");
    }

    // Method to force flush any pending writes
    public static void FlushLogs()
    {
        instance?.logWriter?.Flush();
    }

    // Get logging status
    public static bool IsLogging()
    {
        return instance != null && instance.isInitialized && instance.enableLogging;
    }
}