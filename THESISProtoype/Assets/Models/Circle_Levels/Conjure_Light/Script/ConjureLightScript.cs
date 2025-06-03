using UnityEngine;
using UnityEngine.VFX;

public class ConjureLightScript : BaseLOScript
{
    private const float SCALING_VAR = 0.6f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);

    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation).transform.localScale = SCALING;

        // Enable VFX
        this.GetComponent<VisualEffect>().enabled = true;

        // Enable Lights
        this.transform.Find("Lights").gameObject.SetActive(true);
    }
}
