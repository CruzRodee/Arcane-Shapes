using UnityEngine;

public class IceSpikesScript : BaseLOScript
{
    private const float SCALING_VAR = 1.5f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 1.75f, 0.0f);
    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        //Play SFX burst at lower volume
        v[0] = 0.35f;
        v[1] = v[0];
        PlayRandomSFX(1, p, v);

        //Play Ice SFX
        PlaySFX(sfxSet[2], 1f, 0.5f);

        // Play burst vfx
        Invoke(nameof(BurstVFX), 0.05f); //Delay to match sound

        // Enable Mesh Renderer for ice
        this.GetComponent<Renderer>().enabled = true;
    }

    private void BurstVFX()
    {
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation).transform.localScale = SCALING;
    }
}
