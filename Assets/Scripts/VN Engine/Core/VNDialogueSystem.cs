using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// VNDialogueSystem manages the core visual novel dialogue logic and acts as a singleton for global access.
/// It holds a reference to the DialogueContainer and provides a static instance for other scripts to use.
/// </summary>
public class VNDialogueSystem : MonoBehaviour
{
    /// <summary>
    /// The main container holding dialogue UI references and data.
    /// </summary>
    public DialogueContainer dialogueContainer = new DialogueContainer();

    /// <summary>
    /// Singleton instance of the VNDialogueSystem for global access.
    /// </summary>
    public static VNDialogueSystem instance;

    /// <summary>
    /// Ensures only one instance of VNDialogueSystem exists (singleton pattern).
    /// Destroys duplicate instances if found.
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
