using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenStoneDoorScript : BaseLOScript
{
    private const float time = 2.0f;
    private Vector3 OFFSET = new Vector3(0f, 0f, 0f);
    private const float SCALING_VAR = 1.5f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private const float CAST_DURATION = 2.0f;
    private Vector3 MOVEOFFSET = new Vector3(9f, 0, 0);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 3.3f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        try
        {
            // Play burst vfx
            temp.Add(Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            // Get the two door meshes
            GameObject door1 = this.transform.Find("Door1").gameObject;
            GameObject door2 = this.transform.Find("Door2").gameObject;

            // MoveOvertime for both
            StartCoroutine(MoveOverTime(door1, CAST_DURATION, this.transform.position + MOVEOFFSET));
            StartCoroutine(MoveOverTime(door2, CAST_DURATION, this.transform.position - MOVEOFFSET));
        }
    }
}
