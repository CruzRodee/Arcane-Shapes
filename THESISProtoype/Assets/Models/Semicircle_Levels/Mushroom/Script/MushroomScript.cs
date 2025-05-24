using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class MushroomScript : BaseLOScript
{
    private const float SCALING_VAR = 1.12f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 OFFSET = new Vector3(0, 2.0f, 0);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation).transform.localScale = SCALING;

        // Enable all children
        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(true);
        }
    }
}
