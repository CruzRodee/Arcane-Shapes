
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TempDialogueRunner : MonoBehaviour
{

    [SerializeField] private TextAsset fileToRead = null;
    void Start()
    {
        StartCoroutine(Running());
    }

    IEnumerator Running()
    {
        List<string> lines = FileManager.ReadTextAsset(fileToRead);

        VNDialogueSystem.instance.Say(lines);

        // Wait until the conversation actually finishes
        while (VNDialogueSystem.instance.isRunningConversation)
        {
            yield return null;
        }

        Debug.Log("Finished reading dialogue file.");
        //PlayerDataManager.instance.EndSession();

        // Test PlayerDataManager directly
        // Debug.Log("=== Testing PlayerDataManager Directly ===");

        // if (PlayerDataManager.instance != null)
        // {
        //     Debug.Log("PlayerDataManager instance found!");

        //     // Print data before saving
        //     Debug.Log("Data BEFORE calling EndSession:");
        //     PlayerDataManager.instance.PrintCurrentSession();

        //     // Save the data
        //     Debug.Log("Calling EndSession to save data...");
        //     PlayerDataManager.instance.EndSession();

        //     Debug.Log("EndSession completed!");
        // }
        // else
        // {
        //     Debug.LogError("PlayerDataManager instance is NULL!");
        // }

        SceneManager.LoadScene("LevelSelect");
    }
}
