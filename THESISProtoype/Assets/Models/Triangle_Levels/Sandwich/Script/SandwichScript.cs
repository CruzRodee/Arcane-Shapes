using UnityEngine;

public class SandwichScript : BaseLOScript
{
    private const float SCALING_VAR = 0.5f;
    private Vector3 OFFSET = new Vector3(0f, 1.0f, 0f);
    private Vector3 SCALING = new Vector3(SCALING_VAR, SCALING_VAR, SCALING_VAR);

    private Vector3 SPAWNOFFSET = new Vector3(0.0f, 1.0f, 0.0f);
    private void Awake()
    {
        this.transform.localPosition += SPAWNOFFSET;
    }

    public override void SuccessfulCast()
    {
        //Get sandwich object
        GameObject sandwich = this.transform.Find("SandwichAndPlate/Sandwich").gameObject;

        Instantiate(vfxSet[0], sandwich.transform.position + OFFSET, sandwich.transform.rotation).transform.localScale = SCALING;

        // Enable Mesh Renderer for sandwich
        sandwich.GetComponent<Renderer>().enabled = true;
    }
}
