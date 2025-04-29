using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerGoodTrace4Script : PlayerBaseAnimScript
{
    public Texture gleeFace;
    public GameObject vfx4;
    private GameObject temp;
    private Vector3 OFFSET = new(0, (float)1.75, 0);

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        faceMeshRenderer.material.SetTexture("_BaseMap", gleeFace);
        temp = Instantiate(vfx4, player.transform.position + OFFSET, player.transform.rotation);
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
