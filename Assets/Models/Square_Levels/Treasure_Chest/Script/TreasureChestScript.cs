using System.Collections;
using UnityEngine;

public class TreasureChestScript : BaseLOScript
{
    private const float time = 2;
    private Vector3 OFFSET = new(0, (float)1.5, 0);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.0f, 0.0f);
    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
    }
    public override void SuccessfulCast()
    {
        base.SuccessfulCast();

        StartCoroutine(CastAnimAndSFX());
    }

    private IEnumerator CastAnimAndSFX()
    {
        //Play burst sfx
        PlayRandomSFX(1, p, v);

        yield return new WaitForSeconds(0.05f); //Wait for sync

        //Play chest open VFX and SFX
        PlaySFX(sfxSet[2], 1.5f);
        Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation);
    }
}
