using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveSpellScript : BaseLOScript
{
    private const float time = 0.75f;
    private Vector3 OFFSET = new Vector3(-5f, 0, 0);
    private const float SCALING_VAR = 0.5f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(2.5f, 4.0f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        try
        {
            // Magical Burst effect before moving
            temp.Add(Instantiate(vfxSet[0], this.transform.position, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            StartCoroutine(MoveOverTime(this.gameObject, time, this.transform.position + OFFSET));

            //Magical burst effect after moving
            Invoke(nameof(AfterFlash), time * 1.2f);
        }
    }

    private void AfterFlash()
    {
        try
        {
            temp.Add(Instantiate(vfxSet[0], this.transform.position, this.transform.rotation));
            temp[1].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

        }
    }
}
