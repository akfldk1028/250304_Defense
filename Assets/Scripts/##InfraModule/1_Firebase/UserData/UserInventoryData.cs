using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[Serializable]
public class UserItemData
{
    public long SerialNumber; //unique value
    public int ItemId;

    public UserItemData(long serialNumber, int itemId)
    {
        SerialNumber = serialNumber;
        ItemId = itemId;
    }
}

//wrapper class to parse data to JSON using JSONUtility
[Serializable]
public class UserInventoryItemDataListWrapper
{
    public List<UserItemData> InventoryItemDataList;
}

public class UserItemStats
{
    public int AttackPower;
    public int Defense;

    public UserItemStats(int attackPower, int defense)
    {
        AttackPower = attackPower;
        Defense = defense;
    }
}

public class UserInventoryData : IUserData
{
    public bool IsLoaded { get; set; }
    public UserItemData EquippedWeaponData { get; set; }
    public UserItemData EquippedShieldData { get; set; }
    public UserItemData EquippedChestArmorData { get; set; }
    public UserItemData EquippedBootsData { get; set; }
    public UserItemData EquippedGlovesData { get; set; }
    public UserItemData EquippedAccessoryData { get; set; }

    public List<UserItemData> InventoryItemDataList { get; set; } = new List<UserItemData>();

    public Dictionary<long, UserItemStats> EquippedItemDic { get; set; } = new Dictionary<long, UserItemStats>();

    public void SetDefaultData()
    {
        Debug.Log($"{GetType()}::SetDefaultData");

        //serial number => DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4") 
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 11001));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 11002));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 22001));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 22002));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 33001));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 33002));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 44001));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 44002));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 55001));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 55002));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 65001));
        InventoryItemDataList.Add(new UserItemData(long.Parse(DateTime.Now.ToString("yyyyMMddHHmmss") + UnityEngine.Random.Range(0, 9999).ToString("D4")), 65002));

        EquippedWeaponData = new UserItemData(InventoryItemDataList[0].SerialNumber, InventoryItemDataList[0].ItemId);    
        EquippedShieldData = new UserItemData(InventoryItemDataList[2].SerialNumber, InventoryItemDataList[2].ItemId);

        SetEquippedItemDic();
    }

    public void LoadData()
    {
        Debug.Log($"{GetType()}::LoadData");

        bool result = false;

        try
        {
            string weaponJson = PlayerPrefs.GetString("EquippedWeaponData");
            if(!string.IsNullOrEmpty(weaponJson))
            {
                EquippedWeaponData = JsonUtility.FromJson<UserItemData>(weaponJson);
                Debug.Log($"EquippedWeaponData: SN:{EquippedWeaponData.SerialNumber} ItemId:{EquippedWeaponData.ItemId}");
            }

            string shieldJson = PlayerPrefs.GetString("EquippedShieldData");
            if(!string.IsNullOrEmpty(shieldJson))
            {
                EquippedShieldData = JsonUtility.FromJson<UserItemData>(shieldJson);
                Debug.Log($"EquippedShieldData: SN:{EquippedShieldData.SerialNumber} ItemId:{EquippedShieldData.ItemId}");
            }

            string chestArmorJson = PlayerPrefs.GetString("EquippedChestArmorData");
            if (!string.IsNullOrEmpty(chestArmorJson))
            {
                EquippedChestArmorData = JsonUtility.FromJson<UserItemData>(chestArmorJson);
                Debug.Log($"EquippedChestArmorData: SN:{EquippedChestArmorData.SerialNumber} ItemId:{EquippedChestArmorData.ItemId}");
            }

            string bootsJson = PlayerPrefs.GetString("EquippedBootsData");
            if (!string.IsNullOrEmpty(bootsJson))
            {
                EquippedBootsData = JsonUtility.FromJson<UserItemData>(bootsJson);
                Debug.Log($"EquippedBootsArmorData: SN:{EquippedBootsData.SerialNumber} ItemId:{EquippedBootsData.ItemId}");
            }

            string glovesJson = PlayerPrefs.GetString("EquippedBootsData");
            if (!string.IsNullOrEmpty(glovesJson))
            {
                EquippedBootsData = JsonUtility.FromJson<UserItemData>(glovesJson);
                Debug.Log($"EquippedBootsData: SN:{EquippedBootsData.SerialNumber} ItemId:{EquippedBootsData.ItemId}");
            }

            string accessoryJson = PlayerPrefs.GetString("EquippedAccessoryData");
            if (!string.IsNullOrEmpty(accessoryJson))
            {
                EquippedAccessoryData = JsonUtility.FromJson<UserItemData>(accessoryJson);
                Debug.Log($"EquippedChestArmorData: SN:{EquippedAccessoryData.SerialNumber} ItemId:{EquippedAccessoryData.ItemId}");
            }

            string inventoryItemDataListJson = PlayerPrefs.GetString("InventoryItemDataList");
            if(!string.IsNullOrEmpty(inventoryItemDataListJson))
            {
                UserInventoryItemDataListWrapper itemDataListWrapper = JsonUtility.FromJson<UserInventoryItemDataListWrapper>(inventoryItemDataListJson);
                InventoryItemDataList = itemDataListWrapper.InventoryItemDataList;

                Debug.Log("InventoryItemDataList");
                foreach (var item in InventoryItemDataList)
                {
                    Debug.Log($"SerialNumber:{item.SerialNumber} ItemId:{item.ItemId}");
                }
            }

            SetEquippedItemDic();

            result = true;
        }
        catch (System.Exception e)
        {
            Debug.Log("Load failed (" + e.Message + ")");
        }
    }

    public void SaveData()
    {
        Debug.Log($"{GetType()}::SaveData");

        bool result = false;

        try
        {
            string weaponJson = JsonUtility.ToJson(EquippedWeaponData);
            PlayerPrefs.SetString("EquippedWeaponData", weaponJson);
            if(EquippedWeaponData != null)
            {
                Debug.Log($"EquippedWeaponData: SN:{EquippedWeaponData.SerialNumber} ItemId:{EquippedWeaponData.ItemId}");
            }

            string shieldJson = JsonUtility.ToJson(EquippedShieldData);
            PlayerPrefs.SetString("EquippedShieldData", shieldJson);
            if (EquippedShieldData != null)
            {
                Debug.Log($"EquippedShieldData: SN:{EquippedShieldData.SerialNumber} ItemId:{EquippedShieldData.ItemId}");
            }

            string chestArmorJson = JsonUtility.ToJson(EquippedChestArmorData);
            PlayerPrefs.SetString("EquippedChestArmorData", chestArmorJson);
            if (EquippedChestArmorData != null)
            {
                Debug.Log($"EquippedChestArmorData: SN:{EquippedChestArmorData.SerialNumber} ItemId:{EquippedChestArmorData.ItemId}");
            }

            string bootsJson = JsonUtility.ToJson(EquippedBootsData);
            PlayerPrefs.SetString("EquippedBootsData", bootsJson);
            if (EquippedBootsData != null)
            {
                Debug.Log($"EquippedBootsData: SN:{EquippedBootsData.SerialNumber} ItemId:{EquippedBootsData.ItemId}");
            }

            string glovesJson = JsonUtility.ToJson(EquippedGlovesData);
            PlayerPrefs.SetString("EquippedGlovesData", glovesJson);
            if (EquippedGlovesData != null)
            {
                Debug.Log($"EquippedGlovesData: SN:{EquippedGlovesData.SerialNumber} ItemId:{EquippedGlovesData.ItemId}");
            }

            string accessoryJson = JsonUtility.ToJson(EquippedAccessoryData);
            PlayerPrefs.SetString("EquippedAccessoryData", accessoryJson);
            if (EquippedAccessoryData != null)
            {
                Debug.Log($"EquippedAccessoryData: SN:{EquippedAccessoryData.SerialNumber} ItemId:{EquippedAccessoryData.ItemId}");
            }

            UserInventoryItemDataListWrapper inventoryItemDataListWrapper = new UserInventoryItemDataListWrapper();
            inventoryItemDataListWrapper.InventoryItemDataList = InventoryItemDataList;
            string inventoryItemDataListJson = JsonUtility.ToJson(inventoryItemDataListWrapper);
            PlayerPrefs.SetString("InventoryItemDataList", inventoryItemDataListJson);

            Debug.Log("InventoryItemDataList");
            foreach(var item in InventoryItemDataList)
            {
                Debug.Log($"SerialNumber:{item.SerialNumber} ItemId:{item.ItemId}");
            }
            PlayerPrefs.Save();

            result = true;
        }
        catch (System.Exception e)
        {
            Debug.Log("Load failed (" + e.Message + ")");
        }
    }

    public void SetEquippedItemDic()
    {
        // if(EquippedWeaponData != null)
        // {
        //     var itemData = DataTableManager.Instance.GetItemData(EquippedWeaponData.ItemId);
        //     if (itemData != null)
        //     {
        //         EquippedItemDic.Add(EquippedWeaponData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
        //     }
        // }

        // if (EquippedShieldData != null)
        // {
        //     var itemData = DataTableManager.Instance.GetItemData(EquippedShieldData.ItemId);
        //     if (itemData != null)
        //     {
        //         EquippedItemDic.Add(EquippedShieldData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
        //     }
        // }

        // if (EquippedChestArmorData != null)
        // {
        //     var itemData = DataTableManager.Instance.GetItemData(EquippedChestArmorData.ItemId);
        //     if (itemData != null)
        //     {
        //         EquippedItemDic.Add(EquippedChestArmorData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
        //     }
        // }

        // if (EquippedBootsData != null)
        // {
        //     var itemData = DataTableManager.Instance.GetItemData(EquippedBootsData.ItemId);
        //     if (itemData != null)
        //     {
        //         EquippedItemDic.Add(EquippedBootsData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
        //     }
        // }

        // if (EquippedGlovesData != null)
        // {
        //     var itemData = DataTableManager.Instance.GetItemData(EquippedGlovesData.ItemId);
        //     if (itemData != null)
        //     {
        //         EquippedItemDic.Add(EquippedGlovesData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
        //     }
        // }

        // if (EquippedAccessoryData != null)
        // {
        //     var itemData = DataTableManager.Instance.GetItemData(EquippedAccessoryData.ItemId);
        //     if (itemData != null)
        //     {
        //         EquippedItemDic.Add(EquippedAccessoryData.SerialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
        //     }
        // }
    }

    public bool IsEquipped(long serialNumber)
    {
        return EquippedItemDic.ContainsKey(serialNumber);
    }

    // public void EquipItem(long serialNumber, int itemId)
    // {
    //     var itemData = DataTableManager.Instance.GetItemData(itemId);
    //     if(itemData == null)
    //     {
    //         Debug.LogError($"Item data does not exist. ItemId:{itemId}");
    //         return;
    //     }

    //     var itemType = (ItemType)(itemId / 10000);
    //     switch (itemType)
    //     {
    //         case ItemType.Weapon:
    //             if(EquippedWeaponData != null)
    //             {
    //                 EquippedItemDic.Remove(EquippedWeaponData.SerialNumber);
    //                 EquippedWeaponData = null;
    //             }
    //             EquippedWeaponData = new UserItemData(serialNumber, itemId);
    //             break;
    //         case ItemType.Shield:
    //             if (EquippedShieldData != null)
    //             {
    //                 EquippedItemDic.Remove(EquippedShieldData.SerialNumber);
    //                 EquippedShieldData = null;
    //             }
    //             EquippedShieldData = new UserItemData(serialNumber, itemId);
    //             break;
    //         case ItemType.ChestArmor:
    //             if (EquippedChestArmorData != null)
    //             {
    //                 EquippedItemDic.Remove(EquippedChestArmorData.SerialNumber);
    //                 EquippedChestArmorData = null;
    //             }
    //             EquippedChestArmorData = new UserItemData(serialNumber, itemId);
    //             break;
    //         case ItemType.Gloves:
    //             if (EquippedGlovesData != null)
    //             {
    //                 EquippedItemDic.Remove(EquippedGlovesData.SerialNumber);
    //                 EquippedGlovesData = null;
    //             }
    //             EquippedGlovesData = new UserItemData(serialNumber, itemId);
    //             break;
    //         case ItemType.Boots:
    //             if (EquippedBootsData != null)
    //             {
    //                 EquippedItemDic.Remove(EquippedBootsData.SerialNumber);
    //                 EquippedBootsData = null;
    //             }
    //             EquippedBootsData = new UserItemData(serialNumber, itemId);
    //             break;
    //         case ItemType.Accessory:
    //             if (EquippedAccessoryData != null)
    //             {
    //                 EquippedItemDic.Remove(EquippedAccessoryData.SerialNumber);
    //                 EquippedAccessoryData = null;
    //             }
    //             EquippedAccessoryData = new UserItemData(serialNumber, itemId);
    //             break;
    //         default:
    //             break;
    //     }

    //     EquippedItemDic.Add(serialNumber, new UserItemStats(itemData.AttackPower, itemData.Defense));
    // }

    // public void UnequipItem(long serialNumber, int itemId)
    // {
    //     var itemType = (ItemType)(itemId / 10000);
    //     switch (itemType)
    //     {
    //         case ItemType.Weapon:
    //             EquippedWeaponData = null;
    //             break;
    //         case ItemType.Shield:
    //             EquippedShieldData = null;
    //             break;
    //         case ItemType.ChestArmor:
    //             EquippedChestArmorData = null;
    //             break;
    //         case ItemType.Gloves:
    //             EquippedGlovesData = null;
    //             break;
    //         case ItemType.Boots:
    //             EquippedBootsData = null;
    //             break;
    //         case ItemType.Accessory:
    //             EquippedAccessoryData = null;
    //             break;
    //         default:
    //             break;
    //     }

    //     EquippedItemDic.Remove(serialNumber);
    // }

    public UserItemStats GetUserTotalItemStats()
    {
        var totalAttackPower = 0;
        var totalDefense = 0;

        foreach (var item in EquippedItemDic)
        {
            totalAttackPower += item.Value.AttackPower;
            totalDefense += item.Value.Defense;
        }

        return new UserItemStats(totalAttackPower, totalDefense);
    }

    public void SetData(Dictionary<string, object> firestoreDict)
    {
        throw new NotImplementedException();
    }
}
