using System.Collections.Generic;
using UnityEngine;
using DIALOGUE;
using CHARACTERS;

/// <summary>
/// VNDialogueSystem manages the core visual novel dialogue logic and acts as a singleton for global access.
/// It holds a reference to the DialogueContainer and provides a static instance for other scripts to use.
/// </summary>
public class VNDialogueSystem : MonoBehaviour
{
    [SerializeField] private DialogueSystemConfigurationSO _config;
    public DialogueSystemConfigurationSO config => _config;
    /// <summary>
    /// The main container holding dialogue UI references and data.
    /// </summary>
    public DialogueContainer dialogueContainer;// = new DialogueContainer();
    private ConversationManager conversationManager;
    private TextArchitect architect;
    private AutoReader autoReader;
    [SerializeField] private CanvasGroup mainCanvas;

    public static VNDialogueSystem instance { get; private set; }
    public delegate void DialogueSystemEvent();
    public event DialogueSystemEvent onUserPrompt_Next;

    public bool isRunningConversation => conversationManager.isRunning;

    public DialogueContinuePrompt prompt;
    private CanvasGroupController cgController;

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

        cgController = new CanvasGroupController(this, mainCanvas);
        dialogueContainer.Initialize();

        if (TryGetComponent(out autoReader))
        {
            Debug.Log("[VNDialogueSystem] AutoReader component found! Initializing...");
            autoReader.Initialize(conversationManager);
        }
        else
        {
            Debug.LogError("[VNDialogueSystem] AutoReader component NOT found on this GameObject!");
        }
    }

    public void OnUserPrompt_Next()
    {
        onUserPrompt_Next?.Invoke();

        if (autoReader != null && autoReader.isOn)
        {
            autoReader.Disable();
        }
    }

    public void OnSystemPrompt_Next()
    {
        onUserPrompt_Next?.Invoke();
    }


    // Looks up the character by name and applies their config data to the dialogue container.
    // This is for if we don't have a config for them yet (e.g. loading from dialogue file).
    public void ApplySpeakerDataToDialogueContainer(string speakerName)
    {
        Character character = CharacterManager.instance.GetCharacter(speakerName);
        CharacterConfigData config = character != null ? character.config : CharacterManager.instance.GetCharacterConfig(speakerName);

        ApplySpeakerDataToDialogueContainer(config);
    }

    // Applies the given CharacterConfigData to the dialogue container. Skips the whole search process.
    public void ApplySpeakerDataToDialogueContainer(CharacterConfigData config)
    {
        // Set Dialogue Details
        dialogueContainer.SetDialogueColor(config.dialogueColor);
        dialogueContainer.SetDialogueFont(config.dialogueFont);
        float fontSize = this.config.defaultDialogueFontSize * this.config.dialogueFontScale * config.dialogueFontScale;
        dialogueContainer.SetDialogueFontSize(fontSize);

        // Set Name Details
        dialogueContainer.nameContainer.SetNameColor(config.nameColor);
        dialogueContainer.nameContainer.SetNameFont(config.nameFont);
        fontSize = this.config.defaultNameFontSize * config.nameFontScale;
        dialogueContainer.nameContainer.SetNameFontSize(fontSize);
    }
    public void ShowSpeakerName(string speakerName = "")
    {
        if (speakerName.ToLower() != "narrator")
            dialogueContainer.nameContainer.Show(speakerName);
        else
            HideSpeakerName();
    }

    public void HideSpeakerName() => dialogueContainer.nameContainer.Hide();

    public Coroutine Say(string speaker, string dialogue)
    {
        List<string> conversation = new List<string>() { $"{speaker} \"{dialogue}\"" };
        return Say(conversation);
    }

    public Coroutine Say(List<string> conversation)
    {
        return conversationManager.StartConversation(conversation);
    }

    public bool isVisible => cgController.isVisible;
    public Coroutine Show(float speed = 1f, bool immediate = false) => cgController.Show(speed, immediate);
    public Coroutine Hide(float speed = 1f, bool immediate = false) => cgController.Hide(speed, immediate);
}
