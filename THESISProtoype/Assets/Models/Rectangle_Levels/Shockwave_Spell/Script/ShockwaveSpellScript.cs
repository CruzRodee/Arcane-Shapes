using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveSpellScript : BaseLOScript
{
    public GameObject[] floorRows;
    
    private const float time = 2.0f;
    private const float SCALING_VAR = 1.5f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private const float CAST_DURATION = 0.05f;
    private const float SHOCK_DELAY = 0.02f;
    private Vector3 MOVEOFFSET = new Vector3(0f, 0.75f, 0f);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.5f, 7.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        try
        {
            // Play burst vfx
            temp.Add(Instantiate(vfxSet[0], this.transform.position, this.transform.rotation));
            temp[0].transform.localScale = SCALING;
        }
        finally
        {
            Debug.Log("How bout I run anyway?");

            // Call ShockMotion on each row with a slight delay
            int multiplier = 1;
            foreach (GameObject row in floorRows)
            {
                StartCoroutine(ShockMotion(row, multiplier));
                multiplier++;
            }
        }
    }

    private IEnumerator ShockMotion(GameObject obj, int multiplier)
    {
        //Wait delay
        yield return new WaitForSeconds(SHOCK_DELAY * multiplier);
        
        StartCoroutine(MoveOverTime(obj, CAST_DURATION, obj.transform.position + MOVEOFFSET));

        //Wait Duration
        yield return new WaitForSeconds(CAST_DURATION);

        StartCoroutine(MoveOverTime(obj, CAST_DURATION, obj.transform.position - MOVEOFFSET));
    }
}
