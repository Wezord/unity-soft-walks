using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IKController : MonoBehaviour
{

	public bool IKActive = false;

	private Animator animator;

	void Start()
    {
		animator = GetComponent<Animator>();
	}

	void OnAnimatorIK()
	{
		if (animator && animator.isActiveAndEnabled)
		{
			if (IKActive)
			{
				SetIKFoot(AvatarIKGoal.RightFoot, HumanBodyBones.RightFoot);
				SetIKFoot(AvatarIKGoal.LeftFoot, HumanBodyBones.LeftFoot);
			} else
			{
				animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
				animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
			}
		}
	}

	private void SetIKFoot(AvatarIKGoal goal, HumanBodyBones bone)
	{
		Transform foot = animator.GetBoneTransform(bone);

		var targetPosition = foot.position;

		if (targetPosition.y > 0)
		{
			targetPosition.y *= 3f;
		}

		//animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 1);
		//animator.SetIKHintPosition(AvatarIKHint.RightKnee, Vector3.forward * 1000f);

		animator.SetIKPositionWeight(goal, 1f);
		animator.SetIKPosition(goal, targetPosition);
	}
}
