using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdleScript : PlayerBaseAnimScript
{
    public Texture blinkFace;
    private float BLINKTIME = 2.0f;
    private float BLINKDURATION = 0.1f;
    private float elapsed = 0f;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Blinking animation
        if (elapsed >= BLINKTIME) { 
            faceMeshRenderer.material.SetTexture("_BaseMap", blinkFace);
        }
        if (elapsed >= BLINKTIME + BLINKDURATION) { 
            faceMeshRenderer.material.SetTexture("_BaseMap", defaultFace);
            elapsed = 0f; //Reset counter
            BLINKTIME = UnityEngine.Random.Range(2.0f, 5.0f); //Randomized blink time
            BLINKDURATION = UnityEngine.Random.Range(0.1f, 0.4f); //Randomized blink duration
        }
        elapsed += Time.deltaTime;
        //-----------------
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
