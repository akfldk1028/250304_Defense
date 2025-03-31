using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Define;

public class DataTableManager : InitBase
{
    private const string DATA_PATH = "DataTable";


    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        // LoadChapterDataTable();
        // LoadItemDataTable();
        // LoadAchievementDataTable();
        // LoadProductDataTable();
        return true;
    }

    #region CHAPTER_DATA
    private const string CHAPTER_DATA_TABLE = "ChapterDataTable";
    // private List<ChapterData> ChapterDataTable = new List<ChapterData>();

    // private void LoadChapterDataTable()
    // {
    //     var parsedDataTable = CSVReader.Read($"{DATA_PATH}/{CHAPTER_DATA_TABLE}");

    //     foreach (var data in parsedDataTable)
    //     {
    //         var chapterData = new ChapterData
    //         {
    //             ChapterNo = Convert.ToInt32(data["chapter_no"]),
    //             ChapterName = data["chapter_name"].ToString(),
    //             TotalStages = Convert.ToInt32(data["total_stages"]),
    //             ChapterRewardGem = Convert.ToInt32(data["chapter_reward_gem"]),
    //             ChapterRewardGold = Convert.ToInt32(data["chapter_reward_gold"]),
    //         };

    //         ChapterDataTable.Add(chapterData);
    //     }
    // }

    // public ChapterData GetChapterData(int chapterNo)
    // {
    //     return ChapterDataTable.Where(item => item.ChapterNo == chapterNo).FirstOrDefault();
    // }
    // #endregion

    // #region ITEM_DATA
    // private const string ITEM_DATA_TABLE = "ItemDataTable";
    // private List<ItemData> ItemDataTable = new List<ItemData>();

    // private void LoadItemDataTable()
    // {
    //     var parsedDataTable = CSVReader.Read($"{DATA_PATH}/{ITEM_DATA_TABLE}");

    //     foreach (var data in parsedDataTable)
    //     {
    //         var itemData = new ItemData
    //         {
    //             ItemId = Convert.ToInt32(data["item_id"]),
    //             ItemName = data["item_name"].ToString(),
    //             AttackPower = Convert.ToInt32(data["attack_power"]),
    //             Defense = Convert.ToInt32(data["defense"]),
    //         };

    //         ItemDataTable.Add(itemData);
    //     }
    // }

    // public ItemData GetItemData(int itemId)
    // {
    //     return ItemDataTable.Where(item => item.ItemId == itemId).FirstOrDefault();
    // }
    // #endregion

    // #region ACHIEVEMENT_DATA
    // private const string ACHIEVEMENT_DATA_TABLE = "AchievementDataTable";
    // private List<AchievementData> AchievementDataTable = new List<AchievementData>();
    
    // public List<AchievementData> GetAchievementDataList()
    // {
    //     return AchievementDataTable;
    // }

    // private void LoadAchievementDataTable()
    // {
    //     var parsedDataTable = CSVReader.Read($"{DATA_PATH}/{ACHIEVEMENT_DATA_TABLE}");

    //     foreach (var data in parsedDataTable)
    //     {
    //         var achievementData = new AchievementData
    //         {
    //             AchievementType = (AchievementType)Enum.Parse(typeof(AchievementType), data["achievement_type"].ToString()),
    //             AchievementName = data["achievement_name"].ToString(),
    //             AchievementGoal = Convert.ToInt32(data["achievement_goal"]),
    //             AchievementRewardType = (RewardType)Enum.Parse(typeof(RewardType), data["achievement_reward_type"].ToString()),
    //             AchievementRewardAmount = Convert.ToInt32(data["achievement_reward_amount"])
    //         };

    //         AchievementDataTable.Add(achievementData);
    //     }
    // }

    // public AchievementData GetAchievementData(AchievementType achievementType)
    // {
    //     return AchievementDataTable.Where(item => item.AchievementType == achievementType).FirstOrDefault();
    // }
    // #endregion

    // #region PRODUCT_DATA
    // private const string PRODUCT_DATA_TABLE = "ProductDataTable";
    // private List<ProductData> ProductDataTable = new List<ProductData>();

    // public void LoadProductDataTable()
    // {
    //     var parsedDataTable = CSVReader.Read($"{DATA_PATH}/{PRODUCT_DATA_TABLE}");

    //     foreach (var data in parsedDataTable)
    //     {
    //         var productData = new ProductData
    //         {
    //             ProductId = data["product_id"].ToString(),
    //             ProductType = (ProductType)Enum.Parse(typeof(ProductType), data["product_type"].ToString()),
    //             ProductName = data["product_name"].ToString(),
    //             PurchaseType = (PurchaseType)Enum.Parse(typeof(PurchaseType), data["purchase_type"].ToString()),
    //             PurchaseCost = Convert.ToInt32(data["purchase_cost"]),
    //             RewardGem = Convert.ToInt32(data["reward_gem"]),
    //             RewardGold = Convert.ToInt32(data["reward_gold"]),
    //             RewardItemId = Convert.ToInt32(data["reward_item_id"])
    //         };
    //         ProductDataTable.Add(productData);
    //     }
    // }

    // public ProductData GetProductData(string productId)
    // {
    //     return ProductDataTable.Where(item => item.ProductId == productId).FirstOrDefault();
    // }

    // public List<ProductData> GetProductDatas(ProductType productType)
    // {
    //     return ProductDataTable.Where(item => item.ProductType == productType).ToList();
    // }
    #endregion
}