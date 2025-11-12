using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runs dialogue in the VN scene based on the file passed from VNSceneManager.
/// Automatically returns to the previous scene when dialogue finishes.
/// </summary>
public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private TextAsset fallbackDialogueFile = null; // For testing in editor

    void Start()
    {
        StartCoroutine(Running());
    }

    IEnumerator Running()
    {
        TextAsset fileToRead = null;

        // Get dialogue file from VNSceneManager
        if (VNSceneManager.instance != null)
        {
            fileToRead = VNSceneManager.instance.GetDialogueFile();
        }

        // Fallback for testing in editor
        if (fileToRead == null)
        {
            Debug.LogWarning("[DialogueRunner] No dialogue file from VNSceneManager, using fallback");
            fileToRead = fallbackDialogueFile;
        }

        if (fileToRead == null)
        {
            Debug.LogError("[DialogueRunner] No dialogue file available to run!");
            yield break;
        }

        Debug.Log($"[DialogueRunner] Starting dialogue from: {fileToRead.name}");

        List<string> lines = FileManager.ReadTextAsset(fileToRead);

        VNDialogueSystem.instance.Say(lines);

        Debug.Log("[DialogueRunner] Waiting for dialogue to finish");

        // Wait until the conversation actually finishes
        while (VNDialogueSystem.instance.isRunningConversation)
        {
            yield return null;
        }

        Debug.Log("[DialogueRunner] Finished reading dialogue file.");

        // Return to previous scene
        if (VNSceneManager.instance != null)
        {
            VNSceneManager.instance.ReturnToPreviousScene();
        }
    }
}