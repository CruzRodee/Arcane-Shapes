using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class RainScript : BaseLOScript
{
    private const float SCALING_VAR = 1.0f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.0f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 5.0f; // Set custom spell duration for longer/shorter spells
    }

    public override void SuccessfulCast()
    {
        Transform rain = this.transform.Find("RainAndClouds");

        try
        {
            // VFX Graph flash
            temp.Add(Instantiate(vfxSet[0], rain.position, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            // Enable VFX
            rain.gameObject.GetComponent<VisualEffect>().enabled = true;
        }
    }
}
