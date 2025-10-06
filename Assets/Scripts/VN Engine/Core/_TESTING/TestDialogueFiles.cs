using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;
using UnityEditor;

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

        // for (int i = 0; i < lines.Count; i++)
        // {
        //     string line = lines[i];

        //     if (string.IsNullOrWhiteSpace(line))
        //         continue;

        //     DIALOGUE_LINE dl = DialogueParser.Parse(line);

        //     Debug.Log($"{dl.speaker.name} as [{(dl.speaker.castName != string.Empty ? dl.speaker.castName : dl.speaker.name)}]at {dl.speaker.castPosition}");

        //     List<(int l, string ex)> expr = dl.speaker.CastExpressions;
        //     Debug.Log($"CastExpressions count: {expr.Count}");
        //     for (int c = 0; c < expr.Count; c++)
        //     {
        //         Debug.Log($"[Layer[{expr[c].l}] = '{expr[c].ex}']");
        //     }
        // }

        VNDialogueSystem.instance.Say(lines);
    }
}
