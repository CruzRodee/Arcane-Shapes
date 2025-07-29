using System.Collections;
using UnityEngine;

public class EmptyBookshelfScript : BaseLOScript
{
    private const float time = (float)0.5;
    private Vector3 OFFSET = new Vector3(0, (float)2.5, 0);
    private const float SCALING_VAR = (float)1.5;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.28f, 0.0f);
    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        // TODO: Get all book and potion components using tag "EmptyBookshelfPart" and
        // Enable all mesh renderers using a loop through all components
        GameObject[] tempObjects = GameObject.FindGameObjectsWithTag("EmptyBookshelfPart");
        foreach (GameObject obj in tempObjects)
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }
        }

        StartCoroutine(CastAnimAndSFX());
    }

    private IEnumerator CastAnimAndSFX()
    {
        //Play sfx
        PlayRandomSFX(1, p, v);

        yield return new WaitForSeconds(0.05f); //Wait for sync

        // Play VFX
        if (vfxSet != null && vfxSet.Length > 0)
        {
            Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation).transform.localScale = SCALING;
        }
    }
}
