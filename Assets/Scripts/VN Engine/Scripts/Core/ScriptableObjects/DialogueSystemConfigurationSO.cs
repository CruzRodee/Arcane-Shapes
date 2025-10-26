
using UnityEngine;
using CHARACTERS;
using TMPro;

namespace DIALOGUE
{
    [CreateAssetMenu(fileName = "Dialogue System Configuration Asset", menuName = "Dialogue System/Dialogue System Configuration Asset")]
    public class DialogueSystemConfigurationSO : ScriptableObject
    {
        public CharacterConfigSO characterConfigurationAsset;

        public Color defaultTextColor = Color.white;
        public TMP_FontAsset defaultFont;

        public float dialogueFontScale = 1f;
        public float defaultDialogueFontSize = 41.9f;
        public float defaultNameFontSize = 36f;

    }
}