using System;
using UnityEngine;

/// <summary>
/// Pulls the character's hands onto targets (for example a locker door handle) during chosen parts of
/// the current animation, so the hands stay on the door even if the door animation is only roughly timed.
/// Put this on the same GameObject as the Animator, and turn on "IK Pass" on that Animator layer.
/// </summary>
[RequireComponent(typeof(Animator))]
public class HandGrabIK : MonoBehaviour
{
    [Serializable]
    private class Grab
    {
        [Tooltip("Only grab while this Animator state is playing (its name as shown in the Animator window).\n" +
                 "Leave empty to grab in every state.")]
        public string state;

        [Tooltip("Which hand grabs.")]
        public AvatarIKGoal hand = AvatarIKGoal.RightHand;

        [Tooltip("Where the hand goes. Make it a child of the door so it moves with the door.")]
        public Transform target;

        [Tooltip("How strongly the hand sticks to the target, from 0 (follows the animation) to 1 (fully on the target).\n" +
                 "X axis is the animation's progress: 0 = first frame, 1 = last frame (frame / last frame number).")]
        public AnimationCurve weight = AnimationCurve.Constant(0f, 1f, 0f);

        [Tooltip("Also turn the hand to match the target's rotation. Leave off to keep the animated hand angle.")]
        public bool matchRotation;
    }

    private static readonly AvatarIKGoal[] Goals =
    {
        AvatarIKGoal.LeftHand, AvatarIKGoal.RightHand, AvatarIKGoal.LeftFoot, AvatarIKGoal.RightFoot
    };

    [Tooltip("One entry per grab. The same hand can have several entries (e.g. open the door, then pull it closed).")]
    [SerializeField] private Grab[] grabs;

    [Tooltip("Animator layer whose current animation drives the weight curves. That layer needs IK Pass on.")]
    [SerializeField] private int layer;

    private Animator animator;

    void Awake() => animator = GetComponent<Animator>();

    void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex != layer || grabs == null)
            return;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

        // Clamp so the last value of each curve holds once the clip has finished.
        float progress = Mathf.Clamp01(stateInfo.normalizedTime);

        foreach (AvatarIKGoal goal in Goals)
        {
            // If one hand has several grabs, the one with the highest weight right now wins.
            Grab active = null;
            float activeWeight = 0f;

            foreach (Grab grab in grabs)
            {
                if (grab.hand != goal || grab.target == null)
                    continue;

                if (!string.IsNullOrEmpty(grab.state) && !stateInfo.IsName(grab.state))
                    continue;

                float weight = Mathf.Clamp01(grab.weight.Evaluate(progress));
                if (weight > activeWeight)
                {
                    active = grab;
                    activeWeight = weight;
                }
            }

            animator.SetIKPositionWeight(goal, activeWeight);
            animator.SetIKRotationWeight(goal, active != null && active.matchRotation ? activeWeight : 0f);

            if (active == null)
                continue;

            animator.SetIKPosition(goal, active.target.position);
            animator.SetIKRotation(goal, active.target.rotation);
        }
    }
}
