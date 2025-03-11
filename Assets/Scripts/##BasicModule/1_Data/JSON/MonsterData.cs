using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Assets.Scripts.Module.ApplicationLifecycle.Installers;



namespace Unity.Assets.Scripts.Data
{    
	#region MonsterData
	[Serializable]
	public class MonsterData : CreatureData
	{
		public int DropItemId;
	}

	[Serializable]
	public class MonsterDataLoader : ILoader<int, MonsterData>
	{
		public List<MonsterData> monsters = new List<MonsterData>();
		public Dictionary<int, MonsterData> MakeDict()
		{
			Dictionary<int, MonsterData> dict = new Dictionary<int, MonsterData>();
			
            //TODO ID 자동생성을 만들어야하나나
            foreach (MonsterData monster in monsters)
				dict.Add(monster.DataId, monster);
			return dict;
		}
	}
	#endregion

}