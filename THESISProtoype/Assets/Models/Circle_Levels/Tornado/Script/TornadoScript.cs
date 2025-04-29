using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class TornadoScript : BaseLOScript
{
    private const float SCALING_VAR = 0.6f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private const float CAST_DURATION = 7f;
    private Vector3 ENDSCALE = new Vector3(0.12f, 0.12f, 0.12f);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.2f, 0.0f);

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
        this.SPELLDURATION = 5.0f; // Set custom spell duration for longer/shorter spells
    }

    public override void SuccessfulCast()
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

            //Enable spin
            base.SuccessfulCast();

            // Enable Model
            this.transform.Find("Sketchfab_model").gameObject.SetActive(true);

            // Scale larger
            StartCoroutine(LocalScaleOverTime(this.gameObject, CAST_DURATION, ENDSCALE));
        }
    }
}
