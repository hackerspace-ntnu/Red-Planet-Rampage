using System.Linq;
using UnityEngine;

public class ResetTriggerOnEnterState : StateMachineBehaviour
{
    [SerializeField]
    private bool ResetOnEnter = true;

    [SerializeField]
    private bool ResetOnExit = true;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (ResetOnEnter)
            ResetTriggers(animator);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (ResetOnExit)
            ResetTriggers(animator);
    }

    // Resets animation trigger so that we don't end up replaying animations needlessly >_<
    private void ResetTriggers(Animator animator)
    {
        foreach (var p in animator.parameters.Where(p => p.type is AnimatorControllerParameterType.Trigger))
        {
            animator.ResetTrigger(p.name);
        }
    }
}
