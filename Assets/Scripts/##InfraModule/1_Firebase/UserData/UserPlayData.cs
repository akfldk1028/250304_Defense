using System;
using System.Collections.Generic;
using UnityEngine;

public class UserPlayData : IUserData
{
    public bool IsLoaded { get; set; }
    public int MaxClearedChapter { get; set; }
    //Not saved to playerprefs
    public int SelectedChapter { get; set; } = 1;
    //Not saved to playerprefs
    public int SelectedStage { get; set; } = 1;

    public void SetDefaultData()
    {
        Debug.Log($"{GetType()}::SetDefaultData");

        MaxClearedChapter = 0;
        SelectedChapter = 1;
        SelectedStage = 1;
    }

    public void LoadData()
    {
        Debug.Log($"{GetType()}::LoadData");

        // FirebaseManager.Instance.LoadUserData<UserPlayData>(() =>
        // {
        //     IsLoaded = true;
        // });
    }

    public void SaveData()
    {
        Debug.Log($"{GetType()}::SaveData");

        // FirebaseManager.Instance.SaveUserData<UserPlayData>(ConvertDataToFirestoreDict());
    }

    private Dictionary<string, object> ConvertDataToFirestoreDict()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>()
        {
            { "MaxClearedChapter", MaxClearedChapter }
        };

        return dict;
    }

    public void SetData(Dictionary<string, object> firestoreDict)
    {
        ConvertFirestoreDictToData(firestoreDict);
    }

    private void ConvertFirestoreDictToData(Dictionary<string, object> dict)
    {
        MaxClearedChapter = Convert.ToInt32(dict["MaxClearedChapter"]);
    }
}
