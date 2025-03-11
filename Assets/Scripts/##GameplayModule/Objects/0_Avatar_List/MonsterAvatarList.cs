using System;
using UnityEngine;

namespace Unity.Assets.Scripts.Objects
{
    /// <summary>
    /// This ScriptableObject will be the container for all possible Avatars inside BossRoom.
    /// <see cref="Avatar"/>
    /// </summary>
    [CreateAssetMenu]
    public sealed class MonsterAvatarList : ScriptableObject
    {
        [SerializeField]
        MonsterAvatarSO[] m_MonsterAvatars;

        public bool TryGetAvatar(Guid guid, out MonsterAvatarSO avatarValue)
        {
            avatarValue = Array.Find(m_MonsterAvatars, avatar => avatar.Guid == guid);

            return avatarValue != null;
        }

        public MonsterAvatarSO GetRandomAvatar()
        {
            if (m_MonsterAvatars == null || m_MonsterAvatars.Length == 0)
            {
                return null;
            }

            return m_MonsterAvatars[UnityEngine.Random.Range(0, m_MonsterAvatars.Length)];
        }
    }
}
