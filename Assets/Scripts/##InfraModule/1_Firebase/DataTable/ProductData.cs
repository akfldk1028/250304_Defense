using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductData
{
    public string ProductId;
    public ProductType ProductType;
    public string ProductName;
    public PurchaseType PurchaseType;
    public int PurchaseCost;
    public int RewardGem;
    public int RewardGold;
    public int RewardItemId;
}

public enum ProductType
{
    Pack,
    Chest,
    Gem,
    Gold
}
public enum PurchaseType
{
    IAP,
    Ad,
    Gem
}
