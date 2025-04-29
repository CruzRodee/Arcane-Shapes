using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalScript : BaseLOScript
{
    private const float SCALING_VAR = 1.25f;
    private const float SCALING_FINAL = 10f;
    private const float CAST_DURATION = 0.25f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private Vector3 FINALSCALE = new Vector3(SCALING_FINAL, SCALING_FINAL, SCALING_FINAL);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
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

            // Enable Mesh Renderer
            this.GetComponent<Renderer>().enabled = true;

            //Scale to 10
            StartCoroutine(LocalScaleOverTime(this.gameObject, CAST_DURATION, FINALSCALE));
        }
    }
}
