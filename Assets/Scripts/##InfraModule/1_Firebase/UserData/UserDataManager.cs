using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UserDataManager : InitBase
{
    public bool ExistsSavedData { get; private set; }
    public List<IUserData> UserDataList { get; private set; } = new List<IUserData>();

    public override bool Init()
    {
      if (base.Init() == false)
        return false;

        UserDataList.Add(new UserSettingsData());
        UserDataList.Add(new UserGoodsData());
        UserDataList.Add(new UserInventoryData());
        UserDataList.Add(new UserPlayData());
        UserDataList.Add(new UserAchievementData());
        
        return true;
    }

    public void SetDefaultUserData()
    {
        for (int i = 0; i < UserDataList.Count; i++)
        {
            UserDataList[i].SetDefaultData();
        }
    }

    public void LoadUserData()
    {
        for (int i = 0; i < UserDataList.Count; i++)
        {
            UserDataList[i].LoadData();
        }
    }

    public void SaveUserData()
    {
        for (int i = 0; i < UserDataList.Count; i++)
        {
            UserDataList[i].SaveData();
        }
    }

    public T GetUserData<T>() where T : class, IUserData
    {
        return UserDataList.OfType<T>().FirstOrDefault();
    }
}
