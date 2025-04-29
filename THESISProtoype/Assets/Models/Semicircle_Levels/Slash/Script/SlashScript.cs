using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SlashScript : BaseLOScript
{
    public GameObject cube1, cube2;
    
    private const float SCALING_VAR = 1.2f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private Vector3 CUTDIF = new Vector3(0, 0, 0.2f);
    private const float FLASHDELAY = 0.2f;

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        // Enable VFX
        this.GetComponent<VisualEffect>().enabled = true;

        // VFX flash and move
        Invoke(nameof(Flash), FLASHDELAY);
    }

    private void Flash()
    {
        try
        {
            // VFX Graph flash
            temp.Add(Instantiate(vfxSet[0], this.transform.Find("Spark").position, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            // Move Cubes sideways a bit for slash gap
            cube1.transform.localPosition -= CUTDIF;
            cube2.transform.localPosition += CUTDIF;
        }
    }
}
