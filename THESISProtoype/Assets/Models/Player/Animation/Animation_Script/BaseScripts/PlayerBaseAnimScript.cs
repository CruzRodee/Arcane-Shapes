using UnityEngine;

public class PlayerBaseAnimScript : StateMachineBehaviour
{
    public Texture defaultFace;
    protected Renderer faceMeshRenderer;
    protected GameObject player;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var faceMeshObj = GameObject.FindWithTag("PlayerFaceMesh");
        if (faceMeshObj == null)
        {
            Debug.LogError("Failed to find GameObject with tag 'PlayerFaceMesh'");
        }
        else
        {
            faceMeshRenderer = faceMeshObj.GetComponent<Renderer>();
            if (faceMeshRenderer == null)
            {
                Debug.LogError("Renderer component missing on 'PlayerFaceMesh' GameObject");
            }
        }
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Failed to find GameObject with tag 'Player'");
        }
        else
        {
            player = playerObj;
        }
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //
    //}

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
