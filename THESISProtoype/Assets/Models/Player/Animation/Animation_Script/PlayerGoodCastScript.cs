using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGoodCastScript : PlayerBaseAnimScript
{
    public Texture gleeFace, focusFace;
    public GameObject squareVFX, rectangleVFX, triangleVFX, circleVFX, semiCircleVFX;
    private GameObject temp, shapeObj;
    private Vector3 OFFSET = new(0, (float)2, 0);

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);
        faceMeshRenderer.material.SetTexture("_BaseMap", focusFace);
        switch (animator.GetInteger("shapeIndex"))
        {
            case 0:
                {
                    shapeObj = squareVFX;
                    break;
                }
            case 1:
                {
                    shapeObj = rectangleVFX;
                    break;
                }
            case 2:
                {
                    shapeObj = triangleVFX;
                    break;
                }
            case 3:
                {
                    shapeObj = circleVFX;
                    break;
                }
            case 4:
                {
                    shapeObj = semiCircleVFX;
                    break;
                }
            default:
                {
                    Debug.Log("Ruh roh. How did we get here? (Spell shape not set correctly)");
                    break;
                }
        }
        
        temp = Instantiate(shapeObj, player.transform.position + (player.transform.forward*3) + OFFSET, player.transform.rotation);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        faceMeshRenderer.material.SetTexture("_BaseMap", gleeFace);
        GameObject.Destroy(temp);
    }
}
