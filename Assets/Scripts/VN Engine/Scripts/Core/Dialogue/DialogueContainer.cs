using System.Collections;
using CHARACTERS;
using GLTFast.Schema;
using TMPro;
using UnityEngine;


namespace DIALOGUE
{
    /// <summary>
    /// DialogueContainer holds references to the main dialogue UI elements for the visual novel system.
    /// </summary>
    [System.Serializable]
    public class DialogueContainer
    {
        private const float DEFAULT_FADE_SPEED = 3f;
        public GameObject dialogueBox;
        public NameContainer nameContainer;
        public TextMeshProUGUI dialogueText;

        private CanvasGroupController cgController;

        public void SetDialogueColor(Color color) => dialogueText.color = color;
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
        public void SetDialogueFontSize(float size) => dialogueText.fontSize = size;

        private bool initialized = false;

        public void Initialize()
        {
            if (initialized)
                return;

            cgController = new CanvasGroupController(VNDialogueSystem.instance, dialogueBox.GetComponent<CanvasGroup>());
            initialized = true;
        }

        public bool isVisible => cgController.isVisible;
        public Coroutine Show(float speed = 1f, bool immediate = false) => cgController.Show(speed, immediate);
        public Coroutine Hide(float speed = 1f, bool immediate = false) => cgController.Hide(speed, immediate);

    }
}