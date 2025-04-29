using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOverCastScript : PlayerBaseAnimScript
{
    public Texture focusFace;
    public GameObject explodeVFX;
    private GameObject temp;
    private Vector3 OFFSET = new(0, (float)2, 0);

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        faceMeshRenderer.material.SetTexture("_BaseMap", focusFace);
        temp = Instantiate(explodeVFX, player.transform.position + (player.transform.forward * 3) + OFFSET, player.transform.rotation);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.Destroy(temp);
    }
}
