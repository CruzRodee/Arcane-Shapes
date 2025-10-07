using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;
using UnityEditor;
using System.Runtime.InteropServices;

public class TestDialogueFiles : MonoBehaviour
{

    [SerializeField] private TextAsset fileToRead = null;
    void Start()
    {
        StartConversation();
    }

    void StartConversation()
    {
        List<string> lines = FileManager.ReadTextAsset(fileToRead);

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            DIALOGUE_LINE dl = DialogueParser.Parse(line);

            for (int i = 0; i < dl.commandData.commands.Count; i++)
            {
                DL_COMMAND_DATA.Command command = dl.commandData.commands[i];
                Debug.Log($"Command [{i}] '{command.name}' has arguments [{string.Join(", ", command.arguments)}]");
            }
        }

        //VNDialogueSystem.instance.Say(lines);
    }
}
