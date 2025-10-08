using UnityEngine;
using TMPro;

namespace DIALOGUE
{
    [System.Serializable]
    /// <summary>
    /// The box that holds the name text on screen. Part of the DialogueContainer.
    /// </summary>
    public class NameContainer
    {
        [SerializeField] private GameObject nameBox;
        [SerializeField] private TextMeshProUGUI nameText;
        public void Show(string nameToShow = "")
        {
            nameBox.SetActive(true);

            if (nameToShow != string.Empty)
                nameText.text = nameToShow;
        }

        public void Hide()
        {
            nameBox.SetActive(false);
        }
    }
}