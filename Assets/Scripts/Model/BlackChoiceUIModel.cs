
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlackChoiceUIModel : MonoBehaviour
{
    [Header("UI元件")]
    public TextMeshProUGUI titleText; 
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI goldCost;
    public TextMeshProUGUI woodCost;
    public TextMeshProUGUI meatCost;
    public Button selectButton; // 選擇按鈕
    
    [Header("內部資訊")]
    public int cardIndex; // 這張卡牌在選項中的索引
    
    private BlackMarketManager blackMarketManager;
    
    void Start()
    {
        // 自動尋找BlackMarketManager
        if (blackMarketManager == null)
        {
            blackMarketManager = FindObjectOfType<BlackMarketManager>();
        }
        
        // 設置按鈕點擊事件
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(() => OnSelectCard());
        }
        else
        {
            // 如果沒有設置專門的按鈕，將整個GameObject當作按鈕
            Button button = GetComponent<Button>();
            if (button == null)
                button = gameObject.AddComponent<Button>();
            
            button.onClick.AddListener(() => OnSelectCard());
        }
    }
    
    /// <summary>
    /// 當玩家點擊選擇這張卡牌時調用
    /// </summary>
    public void OnSelectCard()
    {
        if (blackMarketManager != null)
        {
            blackMarketManager.SelectCard(cardIndex);
        }
        else
        {
            Debug.LogError("[BlackChoiceUI] BlackMarketManager not found!");
        }
    }
    
    /// <summary>
    /// 設置卡牌索引
    /// </summary>
    public void SetCardIndex(int index)
    {
        cardIndex = index;
    }
}
