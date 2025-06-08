using UnityEngine;

public class TreeGrowthScript : BaseLOScript
{
    private Vector3 OFFSET = new Vector3(0f, 2f, 0f);
    private const float SCALING_VAR = 2f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 0.0f, 0.0f);
    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        //Play SFX burst at max volume
        v[0] = 1f;
        v[1] = v[0];
        PlayRandomSFX(1, p, v);

        // Play burst vfx
        Invoke(nameof(BurstVFX), 0.05f); //Delay to match sound
    }

    private void BurstVFX()
    {
        Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation).transform.localScale = SCALING;

        // Enable tree object
        this.transform.Find("Tree").gameObject.SetActive(true);
    }
}
