using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimScript : MonoBehaviour
{
    // Inputs
    public GameObject player;
    public GameObject transitionVFX;
    public GameObject[] square_Levels, rectangle_levels, triangle_levels,
        circle_levels, semicircle_levels;

    // Outputs
    public PlayerModelScript playerScript;
    public BaseLOScript currentScript;

    //Workarounds
    public GameObject currentSpell;

    private void Awake()
    {
        playerScript = player.GetComponent<PlayerModelScript>();
    }

    public void AcquireSpell()
    {
        currentSpell = GameObject.FindGameObjectWithTag("Spell");
        currentScript = currentSpell.GetComponent<BaseLOScript>();
    }

    public void CastSpell()
    {
        currentScript.SuccessfulCast();
    }
}
