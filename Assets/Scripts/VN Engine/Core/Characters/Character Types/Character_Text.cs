using UnityEngine;

namespace CHARACTERS
{
    public class Character_Text : Character
    {
        public Character_Text(string name) : base(name)
        {
            Debug.Log($"Character_Text created with name: {name}");
        }
    }
}