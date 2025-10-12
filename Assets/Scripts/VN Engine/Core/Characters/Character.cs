using UnityEngine;
using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace CHARACTERS
{
    public class Character
    {
        public string name = "";
        public string displayName = "";
        public RectTransform root = null;
        public CharacterConfigData config;

        public VNDialogueSystem dialogueSystem => VNDialogueSystem.instance;

        public Character(string name, CharacterConfigData config)
        {
            this.name = name;
            displayName = name;
            this.config = config;
        }

        // External functionality if you don't want to use the dialogue text files to make characters speak
        public Coroutine Say(string dialogue) => Say(new List<string> { dialogue });

        public Coroutine Say(List<string> dialogue)
        {
            dialogueSystem.ShowSpeakerName(displayName);
            UpdateTextCustomizationsOnScreen();
            return dialogueSystem.Say(dialogue);
        }

        // Functions for changing characters' config at runtime
        public void SetNameColor(Color color) => config.nameColor = color;
        public void SetNameFont(TMP_FontAsset font) => config.nameFont = font;
        public void SetDialogueColor(Color color) => config.dialogueColor = color;
        public void SetDialogueFont(TMP_FontAsset font) => config.dialogueFont = font;
        public void ResetConfigurationData() => config = CharacterManager.instance.GetCharacterConfig(name);

        // Force update the dialogue container with the current config values
        public void UpdateTextCustomizationsOnScreen() => dialogueSystem.ApplySpeakerDataToDialogueContainer(config);

        public enum CharacterType
        {
            Text,
            Sprite,
            SpriteSheet,
            Live2D,
            Model3D
        }

    }
}