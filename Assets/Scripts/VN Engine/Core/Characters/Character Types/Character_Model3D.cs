using UnityEngine;

namespace CHARACTERS
{
    public class Character_Model3D : Character
    {
        public Character_Model3D(string name) : base(name)
        {
            Debug.Log($"Character_Model3D created with name: {name}");
        }
    }
}