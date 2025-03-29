using Spine;
using System.Collections;
using System.Collections.Generic;
using Unity.Assets.Scripts.Objects;
using UnityEngine;

public class NormalAttack : SkillBase
{
	public override bool Init()
	{
		if (base.Init() == false)
			return false;

		return true;
	}

	public override void SetInfo(Creature owner, int skillTemplateID, ClientCreature clientCreature)
	{
		base.SetInfo(owner, skillTemplateID, clientCreature);
	}

	public override void DoSkill()
	{
		base.DoSkill();

		Owner.CreatureState = ECreatureState.Skill;
		ClientCreature.PlayAnimation(0, SkillData.AnimName, false);

		Owner.LookAtTarget(Owner.Target);
	}

	void PickupTargetAndProcessHit()
	{
	}

	protected override void OnAttackEvent()
	{
		if (Owner.Target.IsValid() == false)
			return;

		if (SkillData.ProjectileId == 0)
		{
			// Melee 난투
			Owner.Target.OnDamaged(Owner, this);
		}
		else
		{
			// Ranged
		}
	}
}
