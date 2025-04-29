using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGrowthScript : BaseLOScript
{
    private Vector3 OFFSET = new Vector3(0f, 2f, 0f);
    private const float SCALING_VAR = 2f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.0f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        try
        {
            // VFX Graph flash
            temp.Add(Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            // Enable tree object
            this.transform.Find("Tree").gameObject.SetActive(true);
        }
    }
}
