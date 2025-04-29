using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VolcanoScript : BaseLOScript
{
    private const float time = 1.0f;
    private const float ERUPTDURATION = 8.0f;
    private Vector3 OFFSET = new Vector3(0f, 0.5f, 0f);
    private const float SCALING_VAR = 2.0f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 1.0f, 0.0f);
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
            temp.Add(Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            // Enable Volcano object and its children
            this.GetComponent<Renderer>().enabled = true;
            this.transform.Find("Lava Glow").gameObject.SetActive(true);

            //Invoke Eruption animation
            Invoke(nameof(Eruption), time);
        }
    }

    private void Eruption()
    {
        try
        {
            temp.Add(Instantiate(vfxSet[1], this.transform.position + OFFSET * 2, Quaternion.identity));
            Invoke(nameof(StopErupt), ERUPTDURATION);
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

        }
    }

    private void StopErupt()
    {
        try
        {
            temp[1].GetComponent<VisualEffect>().SetBool("isErupting", false);
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

        }
    }
}
