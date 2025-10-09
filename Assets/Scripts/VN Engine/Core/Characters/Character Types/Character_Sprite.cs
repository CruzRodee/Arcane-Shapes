using UnityEngine;

namespace CHARACTERS
{
    public class Character_Sprite : Character
    {
        public Character_Sprite(string name) : base(name)
        {
            Debug.Log($"Character_Sprite created with name: {name}");
        }
    }
}