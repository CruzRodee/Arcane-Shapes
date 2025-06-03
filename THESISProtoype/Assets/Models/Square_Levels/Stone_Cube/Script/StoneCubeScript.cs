using UnityEngine;

public class StoneCubeScript : BaseLOScript
{
    private const float time = 2;

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 4.0f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }
    public override void SuccessfulCast()
    {
        base.SuccessfulCast();

        // Enable Mesh Renderer
        this.transform.Find("Stone_Cube").gameObject.GetComponent<Renderer>().enabled = true;

        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation);
        Invoke(nameof(AfterFlash), time);
    }

    private void AfterFlash()
    {
        Instantiate(vfxSet[0], this.transform.position, this.transform.rotation);
    }
}
