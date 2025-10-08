using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlackCardModel
{
    public int id;
    public string name;
    public int type;
    public int goldCost;
    public int woodCost;
    public int meatCost;
    public string description;
    public int duration;
    public string effectFunction;
    public int rarity;
    public int riskLevel;
}

[Serializable]
public class BlackCardData
{
    public string version;
    public List<BlackCardModel> cards;

    public static BlackCardData LoadFromResources(string resourcePath = "BlackCards")
    {
        TextAsset ta = Resources.Load<TextAsset>(resourcePath);
        if (ta == null)
        {
            Debug.LogError($"BlackCardData: 無法在 Resources 中找到 {resourcePath}.json");
            return null;
        }
        try
        {
            BlackCardData data = JsonUtility.FromJson<BlackCardData>(ta.text);
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"BlackCardData: 解析 JSON 失敗: {ex.Message}");
            return null;
        }
    }

    public BlackCardModel GetById(int id)
    {
        if (cards == null) return null;
        return cards.Find(c => c.id == id);
    }
}
