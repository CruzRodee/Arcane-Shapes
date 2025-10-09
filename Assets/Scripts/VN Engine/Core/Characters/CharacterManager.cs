using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTERS
{
    public class CharacterManager : MonoBehaviour
    {
        public static CharacterManager instance { get; private set; }
        private Dictionary<string, Character> characters = new Dictionary<string, Character>();

        private CharacterConfigSO config => VNDialogueSystem.instance.config.characterConfigurationAsset;

        private void Awake()
        {
            instance = this;
        }

        public Character CreateCharacter(string characterName)
        {
            if (characters.ContainsKey(characterName.ToLower()))
            {
                Debug.LogWarning($"Character with name {characterName} already exists. Did not create the character.");
                return null;
            }

            CHARACTER_INFO info = GetCharacterInfo(characterName);

            Character character = CreateCharacterFromInfo(info);

            if (character == null)
            {
                Debug.LogError($"Failed to create character with name {characterName}.");
                return null;
            }

            characters.Add(characterName.ToLower(), character);
            Debug.Log($"Character {characterName} created and added to CharacterManager.");

            return character;
        }


        private CHARACTER_INFO GetCharacterInfo(string characterName)
        {
            CHARACTER_INFO result = new CHARACTER_INFO();
            result.name = characterName;

            result.config = config.GetConfig(characterName);

            return result;
        }

        private Character CreateCharacterFromInfo(CHARACTER_INFO info)
        {
            if (info.config == null) return null;

            switch (info.config.characterType)
            {
                case Character.CharacterType.Text:
                    return new Character_Text(info.name);
                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name);
                case Character.CharacterType.Live2D:
                    return new Character_Live2D(info.name);
                case Character.CharacterType.Model3D:
                    return new Character_Model3D(info.name);
                default:
                    return null;
            }
        }

        private class CHARACTER_INFO
        {
            public string name = "";
            public CharacterConfigData config = null;
        }

    }
}