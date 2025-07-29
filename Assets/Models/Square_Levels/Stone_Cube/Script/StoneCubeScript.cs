using System.Collections;
using UnityEngine;

public class StoneCubeScript : BaseLOScript
{
    private const float time = 2;

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);
    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
    }
    public override void SuccessfulCast()
    {
        base.SuccessfulCast();

        // Enable Mesh Renderer
        this.transform.Find("Stone_Cube").gameObject.GetComponent<Renderer>().enabled = true;

        StartCoroutine(CastAnimAndSFX());
    }

    private IEnumerator CastAnimAndSFX()
    {
        //Play sfx
        PlayRandomSFX(1, p, v);

        yield return new WaitForSeconds(0.05f); //Wait for sync

        //Play VFX
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation);

        yield return new WaitForSeconds(time); //Wait for anim to finish

        //Play sfx
        PlayRandomSFX(1, p, v);

        yield return new WaitForSeconds(0.05f); //Wait for sync

        //Play VFX
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation);
    }
}
