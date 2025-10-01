using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;

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

        // foreach (string line in lines)
        // {
        //     if (string.IsNullOrEmpty(line))
        //         continue;

        //     Debug.Log($"Segmenting line '{line}'");
        //     DIALOGUE_LINE dialogueLine = DialogueParser.Parse(line);

        //     int i = 0;
        //     foreach (DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment in dialogueLine.dialogue.segments)
        //     {
        //         Debug.Log($"Segment [{i++}] = '{segment.dialogue}'  [signal={segment.startSignal.ToString()}{(segment.signalDelay > 0 ? $" {segment.signalDelay}" : "")}]");
        //     }
        // }

        VNDialogueSystem.instance.Say(lines);
    }
}
