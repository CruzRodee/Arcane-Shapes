using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnderCastScript : PlayerBaseAnimScript
{
    public Texture focusFace;
    public GameObject sparkVFX;
    private GameObject temp;
    private Vector3 OFFSET = new(0, (float)2, 0);

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        temp = Instantiate(sparkVFX, player.transform.position + (player.transform.forward * 3) + OFFSET, player.transform.rotation);
        faceMeshRenderer.material.SetTexture("_BaseMap", focusFace);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject.Destroy(temp);
    }
}
