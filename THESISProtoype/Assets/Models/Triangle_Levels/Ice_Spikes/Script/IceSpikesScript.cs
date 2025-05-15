using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSpikesScript : BaseLOScript
{
    private const float SCALING_VAR = 1.5f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 1.75f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation).transform.localScale = SCALING;

        // Enable Mesh Renderer for ice
        this.GetComponent<Renderer>().enabled = true;
    }
}
