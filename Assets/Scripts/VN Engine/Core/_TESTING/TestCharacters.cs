using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTERS;

namespace TESTING
{
    public class TestCharacters : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            Character Oz = CharacterManager.instance.CreateCharacter("Oz");
            Character Student = CharacterManager.instance.CreateCharacter("S");
            Character Player = CharacterManager.instance.CreateCharacter("Player");
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}