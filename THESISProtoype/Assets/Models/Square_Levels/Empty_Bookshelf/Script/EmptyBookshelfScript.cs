using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EmptyBookshelfScript : BaseLOScript
{
    private const float time = (float) 0.5;
    private Vector3 OFFSET = new Vector3(0, (float)2.5, 0);
    private const float SCALING_VAR = (float)1.5;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.28f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        // TODO: Get all book and potion components using tag "EmptyBookshelfPart" and
        // Enable all mesh renderers using a loop through all components
        GameObject[] tempObjects = GameObject.FindGameObjectsWithTag("EmptyBookshelfPart");
        foreach (GameObject obj in tempObjects)
        {
            obj.GetComponent<Renderer>().enabled = true;
        }

        // TODO: Add VFX Graph magic effects here later
        try
        {
            temp.Add(Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        } finally
        {
            Debug.Log("How bout I run anyway?");
        }
    }
}
