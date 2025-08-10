using UnityEngine;

public class AnimScript : MonoBehaviour
{
    // Inputs
    public GameObject[] square_Levels, rectangle_levels, triangle_levels,
        circle_levels, semicircle_levels, compound_levels;
    public GameBehaviour.SHAPES[] compound_main_shapes;

    //Defaults
    public float spellDuration = 3f;

    private void Awake()
    {
        //TODO: Set spellduration here to equal half the mp4 length
    }

    public void AcquireSpell()
    {
        
    }

    public void CastSpell()
    {
    }
}
