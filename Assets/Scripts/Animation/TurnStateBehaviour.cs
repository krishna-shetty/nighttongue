using UnityEngine;

public class TurnStateBehaviour : StateMachineBehaviour
{
    [SerializeField] string turningParam = "IsTurning";

    public override void OnStateExit(
        Animator animator,
        AnimatorStateInfo stateInfo,
        int layerIndex)
    {
        animator.SetBool(turningParam, false);
        
    }
}
