using TMPro;
using UnityEngine;

namespace CHARACTERS
{
    [System.Serializable]
    public class CharacterConfigData
    {
        public string name;
        public string alias;
        public Character.CharacterType characterType;
        public Color nameColor;
        public Color dialogueColor;
        public TMP_FontAsset nameFont;
        public TMP_FontAsset dialogueFont;

        public float nameFontScale = 1f;
        public float dialogueFontScale = 1f;

        public CharacterConfigData Copy()
        {
            CharacterConfigData result = new CharacterConfigData();

            result.name = name;
            result.alias = alias;
            result.characterType = characterType;
            result.nameFont = nameFont;
            result.dialogueFont = dialogueFont;

            result.nameColor = nameColor;
            result.dialogueColor = dialogueColor;

            result.dialogueFontScale = dialogueFontScale;
            result.nameFontScale = nameFontScale;

            return result;
        }

        private static Color defaultNameColor => VNDialogueSystem.instance.config.defaultNameColor;
        private static Color defaultTextColor => VNDialogueSystem.instance.config.defaultTextColor;
        private static TMP_FontAsset defaultFont => VNDialogueSystem.instance.config.defaultFont;

        public static CharacterConfigData Default
        {
            get
            {
                CharacterConfigData result = new CharacterConfigData();

                result.name = "";
                result.alias = "";
                result.characterType = Character.CharacterType.Text;

                result.nameFont = defaultFont;
                result.dialogueFont = defaultFont;
                result.nameColor = defaultNameColor;
                result.dialogueColor = defaultTextColor;

                result.dialogueFontScale = 1f;
                result.nameFontScale = 1f;

                return result;
            }
        }
    }
}