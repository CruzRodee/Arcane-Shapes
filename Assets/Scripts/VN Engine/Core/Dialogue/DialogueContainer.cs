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
        /// <summary>
        /// The main dialogue box GameObject. It should be the root of the dialogue UI elements.
        /// </summary>
        public GameObject dialogueBox;

        /// <summary>
        /// The TextMeshProUGUI component for displaying the speaker's name.
        /// </summary>
        public NameContainer nameContainer;

        /// <summary>
        /// The TextMeshProUGUI component for displaying the dialogue text.
        /// </summary>
        public TextMeshProUGUI dialogueText;
    }
}