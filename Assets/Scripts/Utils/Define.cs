using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Define
{
    public enum EUIEvent
	{
		Click,
		PointerDown,
		PointerUp,
		Drag,
	}
		public enum EObjectType
	{
		None,
		HeroCamp,
		Creature,
		Projectile,
		Env,
		Effect,
		Monster
	}
		public enum ECreatureState
	{
		None,
		Idle,
		Move,
		Skill,
		OnDamaged,
		Dead
	}

	public enum EStatModType
	{
		Add,
		PercentAdd,
		PercentMult,
	}
    public enum EJoystickState
    {
        PointerDown,
        PointerUp,
        Drag
    }


}