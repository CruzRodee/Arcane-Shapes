using System.Collections;
using System.Collections.Generic;
using DIALOGUE;
using UnityEngine;

public class TestConversationQueue : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Running());
    }

    IEnumerator Running()
    {
        List<string> lines = new List<string>()
        {
            "This is line 1 from the original conversation.",
            "This is line 2 from the original conversation.",
            "This is line 3 from the original conversation."
        };

        yield return VNDialogueSystem.instance.Say(lines);

        VNDialogueSystem.instance.Hide();
    }

    void Update()
    {
        List<string> lines = new List<string>();
        Conversation conversation;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            lines = new List<string>()
            {
                "This is the start of an enqueued conversation.",
                "We can keep it going!"
            };

            conversation = new Conversation(lines);
            VNDialogueSystem.instance.conversationManager.Enqueue(conversation);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            lines = new List<string>()
            {
                "This is an important conversation!",
                "We did it!"
            };

            conversation = new Conversation(lines);
            VNDialogueSystem.instance.conversationManager.EnqueuePriority(conversation);
        }
    }
}
