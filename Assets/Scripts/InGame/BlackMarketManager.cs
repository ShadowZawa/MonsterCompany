using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public enum BlackCardType
{
    SelfBuff,
    EnemyDebuff,
    Special
}
public class BlackMarketManager : MonoBehaviour
{
    public GameObject blackMarketPanel; // 黑市面板
    private bool isOpen = false; // 黑市是否開啟
    public List<BlackChoiceUIModel> choices = new List<BlackChoiceUIModel>(); // 黑市選項列表
    public void Open()
    {
        isOpen = true;
        
        Debug.Log("[BlackMarket] 開始執行 Open()");
        
        // 檢查資料是否存在
        if (CardLoader.instance == null || CardLoader.instance.blackCardData == null)
        {
            Debug.LogError("[BlackMarket] CardLoader 或 blackCardData 未初始化");
            return;
        }

        var allCards = CardLoader.instance.blackCardData.cards;
        if (allCards == null || allCards.Count == 0)
        {
            Debug.LogWarning("[BlackMarket] 沒有可用的黑市卡片");
            return;
        }
        
        Debug.Log($"[BlackMarket] 找到 {allCards.Count} 張卡片");

        if (choices == null || choices.Count < 3)
        {
            Debug.LogWarning($"[BlackMarket] choices 列表未正確設定（當前數量: {(choices == null ? "null" : choices.Count.ToString())}）");
            return;
        }
        
        Debug.Log($"[BlackMarket] choices 數量: {choices.Count}");

        // 隨機抽取3張不同的卡片
        var selectedCards = allCards.OrderBy(x => UnityEngine.Random.value).Take(3).ToList();
        
        Debug.Log($"[BlackMarket] 已抽取 {selectedCards.Count} 張卡片");

        // 顯示面板
        if (blackMarketPanel != null)
        {
            blackMarketPanel.SetActive(true);
            Debug.Log("[BlackMarket] 黑市面板已啟用");
        }
        else
        {
            Debug.LogWarning("[BlackMarket] blackMarketPanel 為 null");
        }

        // 設置每個選項的資訊
        for (int i = 0; i < 3 && i < selectedCards.Count; i++)
        {
            var card = selectedCards[i];
            var choice = choices[i];

            Debug.Log($"[BlackMarket] 處理 Choice[{i}]: choice={(choice == null ? "null" : "存在")}, card.name={card.name}");

            if (choice == null)
            {
                Debug.LogWarning($"[BlackMarket] Choice[{i}] 為 null，跳過");
                continue;
            }
            
            if (choice.gameObject == null)
            {
                Debug.LogWarning($"[BlackMarket] Choice[{i}].gameObject 為 null");
                continue;
            }

            Debug.Log($"[BlackMarket] Choice[{i}] GameObject 名稱: {choice.gameObject.name}, 啟用狀態: {choice.gameObject.activeSelf}");

            // 設置卡片資訊
            if (choice.titleText != null)
            {
                choice.titleText.text = card.name;
                Debug.Log($"[BlackMarket] Choice[{i}] 設置標題: {card.name}");
            }
            else
            {
                Debug.LogWarning($"[BlackMarket] Choice[{i}].titleText 為 null");
            }

            if (choice.descriptionText != null)
            {
                choice.descriptionText.text = card.description;
            }
            else
            {
                Debug.LogWarning($"[BlackMarket] Choice[{i}].descriptionText 為 null");
            }

            if (choice.goldCost != null)
                choice.goldCost.text = card.goldCost.ToString();

            if (choice.woodCost != null)
                choice.woodCost.text = card.woodCost.ToString();

            if (choice.meatCost != null)
                choice.meatCost.text = card.meatCost.ToString();

            // 啟用選項物件
            choice.gameObject.SetActive(true);
            Debug.Log($"[BlackMarket] Choice[{i}] 已設為 Active");
        }

        // 隱藏多餘的選項
        for (int i = 3; i < choices.Count; i++)
        {
            if (choices[i] != null && choices[i].gameObject != null)
                choices[i].gameObject.SetActive(false);
        }
        
        Debug.Log("[BlackMarket] Open() 執行完成");
    }
    public void Toggle()
    {
        if (!isOpen)
        {
            blackMarketPanel.SetActive(false);
            return;
        }
        blackMarketPanel.SetActive(!blackMarketPanel.activeSelf);
    }



}