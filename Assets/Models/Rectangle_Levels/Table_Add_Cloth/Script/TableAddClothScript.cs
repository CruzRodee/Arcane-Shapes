using UnityEngine;

public class TableAddClothScript : BaseLOScript
{
    private const float time = (float)0.5;
    private Vector3 OFFSET = new Vector3(0, (float)2.5, 0);
    private const float SCALING_VAR = (float)1.25;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 1.0f, 0.0f);
    private new void Awake()
    {
        base.Awake();
        this.transform.localPosition += SPAWNOFFSET;
    }
    public override void SuccessfulCast()
    {
        //Play SFX burst
        PlayRandomSFX(1, p, v);

        // Play burst vfx
        Invoke(nameof(BurstVFX), 0.05f); //Delay to match sound

        //Get cloth component and turn on renderer
        GameObject tempObject = this.transform.Find("Cloth").gameObject;
        tempObject.GetComponent<Renderer>().enabled = true;
    }

    private void BurstVFX()
    {
        Instantiate(vfxSet[0], this.transform.position + OFFSET, this.transform.rotation).transform.localScale = SCALING;
    }
}
