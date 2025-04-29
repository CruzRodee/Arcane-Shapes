using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class StoneCubeScript : BaseLOScript
{
    private const float time = 2;

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }
    public override void SuccessfulCast()
    {
        base.SuccessfulCast();

        // Enable Mesh Renderer
        this.transform.Find("Stone_Cube").gameObject.GetComponent<Renderer>().enabled = true;

        try
        {
            // TODO: Add VFX Graph magic effects here later
            temp.Add(Instantiate(vfxSet[0], this.transform.position, this.transform.rotation));
        }
        finally
        {
            Debug.Log("How bout I run anyway?");
            // VFX after cube expansion
            Invoke(nameof(AfterFlash), time);
        }
    }

    private void AfterFlash()
    {
        temp.Add(Instantiate(vfxSet[0], this.transform.position, this.transform.rotation));
    }
}
