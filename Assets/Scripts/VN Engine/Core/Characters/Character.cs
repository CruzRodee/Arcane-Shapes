using UnityEngine;
using DIALOGUE;
using System.Collections;
using System.Collections.Generic;

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
            dialogueSystem.ApplySpeakerDataToDialogueContainer(name);
            return dialogueSystem.Say(dialogue);
        }

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