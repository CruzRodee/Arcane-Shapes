using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGoodTrace3Script : PlayerBaseAnimScript
{
    public Texture focusFace;
    public GameObject vfx3;
    private GameObject temp;
    private Vector3 OFFSET = new(0, (float)2, 0);
    private Quaternion ROTATE = Quaternion.Euler(90, 0, 0);

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        faceMeshRenderer.material.SetTexture("_BaseMap", focusFace);
        temp = Instantiate(vfx3, player.transform.position + OFFSET, player.transform.rotation * ROTATE);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        faceMeshRenderer.material.SetTexture("_BaseMap", defaultFace);
        GameObject.Destroy(temp);
    }
}
