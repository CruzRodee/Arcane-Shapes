using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainbowScript : BaseLOScript
{
    private const float SCALING_VAR = 1.5f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private const float CAST_DURATION = 0.5f;
    private Vector3 ENDSCALE = new Vector3(0.9f, 0.9f, 0.9f);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.0f, 2.0f);

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        // Enable Rainbow Model
        this.transform.Find("Rainbow_Model").gameObject.SetActive(true);

        // Scale larger
        StartCoroutine(LocalScaleOverTime(this.transform.Find("Rainbow_Model").gameObject, CAST_DURATION, ENDSCALE));

        Invoke(nameof(RevealPot), CAST_DURATION + 0.25f);
    }

    private void RevealPot()
    {
        try
        {
            // VFX Graph flash
            temp.Add(Instantiate(vfxSet[0], this.transform.position, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            //Enable Pot model
            this.transform.Find("PotOfGold_Model").gameObject.SetActive(true);
        }
    }
}
