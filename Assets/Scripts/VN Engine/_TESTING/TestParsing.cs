using System.Collections;
using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

namespace TESTING
{
    public class TestParsing : MonoBehaviour
    {
        void Start()
        {
            SendFiletoParse();
        }

        void SendFiletoParse()
        {
            List<string> lines = FileManager.ReadTextAsset("testFile", false);
            foreach (string line in lines)
            {
                DIALOGUE_LINE parsedLine = DialogueParser.Parse(line);
            }
        }

    }
}

