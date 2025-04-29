using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandwichScript : BaseLOScript
{
    private const float SCALING_VAR = 0.5f;
    private Vector3 OFFSET = new Vector3(0f, 1.0f, 0f);
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 1.0f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        //Get sandwich object
        GameObject sandwich = this.transform.Find("SandwichAndPlate/Sandwich").gameObject;

        try
        {
            // VFX Graph flash on sandwich
            temp.Add(Instantiate(vfxSet[0], sandwich.transform.position + OFFSET, sandwich.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            // Enable Mesh Renderer for sandwich
            sandwich.GetComponent<Renderer>().enabled = true;
        }
    }
}
