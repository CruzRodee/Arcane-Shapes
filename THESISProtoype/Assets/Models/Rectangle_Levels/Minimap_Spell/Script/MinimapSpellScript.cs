using UnityEngine;

public class MinimapSpellScript : BaseLOScript
{
    private const float time = 1.0f;
    private Vector3 OFFSET = new Vector3(0, 0, -7f);
    private const float CAST_DURATION = 0.30f;
    private Vector3 ENDSCALE = new Vector3(0.75f, 0.5f, 0.5f);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);
    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        // Spawn Minimap spell and texture with expand effect
        this.GetComponent<Renderer>().enabled = true;
        this.transform.Find("MinimapTexture").gameObject.GetComponent<Renderer>().enabled = true;
        StartCoroutine(LocalScaleOverTime(this.gameObject, CAST_DURATION, ENDSCALE));

        //Play SFX
        if (sfxSet.Length > 0)
            PlaySFX(sfxSet[0], 0.8f, 0.6f);
    }
}
