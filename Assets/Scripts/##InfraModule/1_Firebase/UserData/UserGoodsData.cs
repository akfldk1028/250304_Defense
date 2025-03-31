using System.Collections.Generic;
using UnityEngine;

public class UserGoodsData : IUserData
{
    public bool IsLoaded { get; set; }
    //보석
    public long Gem { get; set; }
    //골드
    public long Gold { get; set; }

    public void SetDefaultData()
    {
        Debug.Log($"{GetType()}::SetDefaultData");

        Gem = 0;
        Gold = 0;
    }

    public void LoadData()
    {
        Debug.Log($"{GetType()}::LoadData");

        // FirebaseManager.Instance.LoadUserData<UserGoodsData>(() =>
        // {
        //     IsLoaded = true;
        // });
    }

    public void SaveData()
    {
        Debug.Log($"{GetType()}::SaveData");

        // FirebaseManager.Instance.SaveUserData<UserGoodsData>(ConvertDataToFirestoreDict());
    }

    private Dictionary<string, object> ConvertDataToFirestoreDict()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            { "Gem", Gem },
            { "Gold", Gold }
        };

        return dict;
    }

    public void SetData(Dictionary<string, object> firestoreDict)
    {
        ConvertFirestoreDictToData(firestoreDict);
    }

    private void ConvertFirestoreDictToData(Dictionary<string, object> dict)
    {
        Gem = (long)dict["Gem"];
        Gold = (long)dict["Gold"];
    }
}
