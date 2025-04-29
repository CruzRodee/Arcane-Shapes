using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasureChestScript : BaseLOScript
{
    private const float time = 2;
    private Vector3 OFFSET = new(0, (float)1.5, 0);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.0f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }
    public override void SuccessfulCast()
    {
        base.SuccessfulCast();

        try
        {
            // TODO: Add VFX Graph magic effects here later
            temp.Add(Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation));
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

        } 
    }
}
