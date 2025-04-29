using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TableAddClothScript : BaseLOScript
{
    private const float time = (float) 0.5;
    private Vector3 OFFSET = new Vector3(0, (float)2.5, 0);
    private const float SCALING_VAR = (float)1.25;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 1.0f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }
    public override void SuccessfulCast()
    {
        try
        {
            // TODO: Add VFX Graph magic effects here later
            temp.Add(Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            // TODO: Get cloth component and turn on renderer
            GameObject tempObject = this.transform.Find("Cloth").gameObject;
            tempObject.GetComponent<Renderer>().enabled = true;
        }
    }
}
