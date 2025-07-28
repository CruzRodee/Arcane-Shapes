using System.Collections;
using UnityEngine;

public class MoveSpellScript : BaseLOScript
{
    private const float time = 0.75f;
    private Vector3 OFFSET = new Vector3(-5f, 0, 0);
    private const float SCALING_VAR = 0.5f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(2.5f, 4.0f, 0.0f);
    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        //Lower volume of burst
        v[0] = 0.25f;
        v[1] = v[0];

        StartCoroutine(CastAnimAndSFX());
    }

    private IEnumerator CastAnimAndSFX()
    {
        //Play sfx for burst
        PlayRandomSFX(1, p, v);

        yield return new WaitForSeconds(0.05f); //Wait for sync

        // Magical Burst effect before moving
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation).transform.localScale = SCALING;

        //Moving block
        StartCoroutine(MoveOverTime(this.gameObject, time, this.transform.position + OFFSET));

        yield return new WaitForSeconds(time * 1.2f); //Wait for move finish

        //Play sfx for burst
        PlayRandomSFX(1, p, v);

        yield return new WaitForSeconds(0.05f); //Wait for sync

        //Magical burst effect after moving
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation).transform.localScale = SCALING;
    }
}
