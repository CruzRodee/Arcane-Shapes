using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;

/// <summary>
/// VNDialogueSystem manages the core visual novel dialogue logic and acts as a singleton for global access.
/// It holds a reference to the DialogueContainer and provides a static instance for other scripts to use.
/// </summary>
public class VNDialogueSystem : MonoBehaviour
{
    /// <summary>
    /// The main container holding dialogue UI references and data.
    /// </summary>
    public DialogueContainer dialogueContainer;// = new DialogueContainer();
    private ConversationManager conversationManager;
    private TextArchitect architect;
    public bool isRunningConversation => conversationManager.isRunning;

    public static VNDialogueSystem instance { get; private set; }

    public delegate void DialogueSystemEvent();
    public event DialogueSystemEvent onUserPrompt_Next;

    /// <summary>
    /// Ensures only one instance of VNDialogueSystem exists (singleton pattern).
    /// Destroys duplicate instances if found.
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Initialize();
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    bool _initialized = false;
    private void Initialize()
    {
        if (_initialized)
            return;

        architect = new TextArchitect(dialogueContainer.dialogueText);
        conversationManager = new ConversationManager(architect);
        _initialized = true;
    }

    public void OnUserPrompt_Next()
    {
        onUserPrompt_Next?.Invoke();
    }

    public void ShowSpeakerName(string speakerName = "")
    {
        if (speakerName.ToLower() != "narrator")
            dialogueContainer.nameContainer.Show(speakerName);
        else
            HideSpeakerName();
    }

    public void HideSpeakerName() => dialogueContainer.nameContainer.Hide();

    public void Say(string speaker, string dialogue)
    {
        List<string> conversation = new List<string>() { $"{speaker} \"{dialogue}\"" };
        Say(conversation);
    }

    public void Say(List<string> conversation)
    {
        conversationManager.StartConversation(conversation);
    }
}
