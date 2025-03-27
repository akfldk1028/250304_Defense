
using UnityEngine;


    public struct SessionPlayerData : ISessionPlayerData
    {
        public string PlayerName;
        public int PlayerNumber;
        public Vector3 PlayerPosition;
        public Quaternion PlayerRotation;
        /// Instead of using a NetworkGuid (two ulongs) we could just use an int or even a byte-sized index into an array of possible avatars defined in our game data source
        public NetworkGuid AvatarNetworkGuid;
        public int CurrentHitPoints;
        public bool HasCharacterSpawned;

        public SessionPlayerData(ulong clientID, string name, NetworkGuid avatarNetworkGuid, int currentHitPoints = 0, bool isConnected = false, bool hasCharacterSpawned = false)
        {
            Debug.Log("<color=cyan>[SessionPlayerData] 생성자 시작</color>");
            ClientID = clientID;
            Debug.Log("<color=cyan>[SessionPlayerData] ClientID 설정 완료</color>");
            PlayerName = name;
            Debug.Log("<color=cyan>[SessionPlayerData] PlayerName 설정 완료</color>");
            PlayerNumber = -1;
            Debug.Log("<color=cyan>[SessionPlayerData] PlayerNumber 설정 완료</color>");
            PlayerPosition = Vector3.zero;
            Debug.Log("<color=cyan>[SessionPlayerData] PlayerPosition 설정 완료</color>");
            PlayerRotation = Quaternion.identity;
            Debug.Log("<color=cyan>[SessionPlayerData] PlayerRotation 설정 완료</color>");
            AvatarNetworkGuid = avatarNetworkGuid;
            Debug.Log("<color=cyan>[SessionPlayerData] AvatarNetworkGuid 설정 완료</color>");
            CurrentHitPoints = currentHitPoints;
            Debug.Log("<color=cyan>[SessionPlayerData] CurrentHitPoints 설정 완료</color>");
            IsConnected = isConnected;
            Debug.Log("<color=cyan>[SessionPlayerData] IsConnected 설정 완료</color>");
            HasCharacterSpawned = hasCharacterSpawned;
            Debug.Log("<color=cyan>[SessionPlayerData] 생성자 완료</color>");
            Debug.Log("<color=cyan>[SessionPlayerData] 생성자 완료</color>");
            Debug.Log("<color=cyan>[SessionPlayerData] 생성자 완료</color>");
            Debug.Log("<color=cyan>[SessionPlayerData] 생성자 완료</color>");
            Debug.Log("<color=cyan>[SessionPlayerData] 생성자 완료</color>");
            Debug.Log("<color=cyan>[SessionPlayerData] 생성자 완료</color>");
            Debug.Log("<color=cyan>[SessionPlayerData] 생성자 완료</color>");
            Debug.Log("<color=cyan>[SessionPlayerData] 생성자 완료</color>");
            Debug.Log("<color=cyan>[SessionPlayerData] 생성자 완료</color>");
        }

        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }

        public void Reinitialize()
        {
            HasCharacterSpawned = false;
        }
    }
