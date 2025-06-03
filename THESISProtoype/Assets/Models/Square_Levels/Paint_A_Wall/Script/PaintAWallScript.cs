using UnityEngine;

public class PaintAWallScript : BaseLOScript
{
    private const float time = (float)0.5;
    //private Vector3 OFFSET = new Vector3(0, (float)2.5, 0);
    private const float SCALING_VAR = 2.25f;
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);
    private Color paintColor;

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 3.5f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        // TODO: Add VFX Graph magic effects here later
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation).transform.localScale = SCALING;

        // TODO: Change color of object material to random color
        paintColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        this.GetComponent<Renderer>().material.SetColor("_BaseColor", paintColor);
    }
}
