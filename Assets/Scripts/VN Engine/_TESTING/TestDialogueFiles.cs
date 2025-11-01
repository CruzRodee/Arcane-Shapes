
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TestDialogueFiles : MonoBehaviour
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

        SceneManager.LoadScene("MainMenu");

    }
}
