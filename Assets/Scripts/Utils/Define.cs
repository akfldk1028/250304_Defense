using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class LayerNames
{
    public static readonly int Monster = LayerMask.NameToLayer("Monster");
	public static readonly int Hero = LayerMask.NameToLayer("Hero");

    // 다른 레이어도 필요하면 추가
}


public static class Define
{

	public enum ESkillSlot
	{
		Default,
		Env,
		A,
		B
	}
	public enum EOrganizer
	{
		HOST,
		CLIENT
	}
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
		Monster,
		Hero
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
	public const int HERO_WIZARD_ID = 201000;
	public const int HERO_KNIGHT_ID = 201001;
	public const int HERO_LION_ID = 201003;

	public const int MONSTER_SLIME_ID = 202001;
	public const int MONSTER_SPIDER_COMMON_ID = 202002;
	public const int MONSTER_WOOD_COMMON_ID = 202004;
	public const int MONSTER_GOBLIN_ARCHER_ID = 202005;
	public const int MONSTER_BEAR_ID = 202006;

	public const int ENV_TREE1_ID = 300001;
	public const int ENV_TREE2_ID = 301000;

	public const char MAP_TOOL_WALL = '0';
	public const char MAP_TOOL_NONE = '1';
	public const char MAP_TOOL_SEMI_WALL = '2';

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

	public enum EHeroMoveState
	{
		None,
		TargetMonster,
		CollectEnv,
		ReturnToCamp,
		ForceMove,
		ForcePath
	}


public static class SortingLayers
{
	public const int SPELL_INDICATOR = 200;
	public const int CREATURE = 300;
	public const int ENV = 300;
	public const int NPC = 310;
	public const int PROJECTILE = 310;
	public const int SKILL_EFFECT = 310;
	public const int DAMAGE_FONT = 410;
}

public static class AnimName
{
	public const string ATTACK_A = "attack";
	public const string ATTACK_B = "attack";
	public const string SKILL_A = "skill";
	public const string SKILL_B = "skill";
	public const string IDLE = "idle";
	public const string MOVE = "move";
	public const string DAMAGED = "hit";
	public const string DEAD = "dead";
	public const string EVENT_ATTACK_A = "event_attack";
	public const string EVENT_ATTACK_B = "event_attack";
	public const string EVENT_SKILL_A = "event_attack";
	public const string EVENT_SKILL_B = "event_attack";
}