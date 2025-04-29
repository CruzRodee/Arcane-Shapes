using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class BellScript : BaseLOScript
{
    private const float SCALING_VAR = 1.25f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);

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
            // Enable Mesh
            this.GetComponent<Renderer>().enabled = true;

            // Enable VFX
            this.transform.Find("MusicBurst").gameObject.SetActive(true);
        }
    }
}
