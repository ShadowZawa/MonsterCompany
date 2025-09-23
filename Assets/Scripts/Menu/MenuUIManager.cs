using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuUIManager : MonoBehaviour
{
    public Button startGameBtn;
    public Button settingBtn;
    public Button backpackBtn;
    public Button storeBtn;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI diamondText;

    public GameObject GamePanel;
    public GameObject BagpackPanel;


    void Awake()
    {
        EventBus.Instance.Subscribe<CurrencyUpdateEvent>(updateCurrency);
    }

    void Start()
    {
        startGameBtn.onClick.AddListener(onClickStartGame);
        GamePanel.SetActive(false);
        // 初始更新一次貨幣顯示
        if (DataBaseManager.instance != null && DataBaseManager.instance.auth != null)
        {
            updateCurrency(new CurrencyUpdateEvent());
        }
    }
    public void onClickBackpack()
    {
        if (BagpackPanel.activeSelf)
        {
            BagpackPanel.SetActive(false);
        }
        else
        {
            BagpackPanel.SetActive(true);
            init();
        }
    }
    public void onClickStartGame()
    {
        GamePanel.SetActive(true);
    }
    public void init()
    {
        if (!CardLoader.instance.hasInit) CardLoader.instance.Init();

        // 清除現有卡片
        foreach (Transform child in BagpackPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // 設定卡片佈局參數
        float cardWidth = 100f;
        float cardHeight = 128f;
        float margin = 20f;
        int cardsPerRow = 3;
        
        for (int i = 0; i < CardLoader.instance.mobData.towers.Length; i++)
        {
            mobLevelData tower = CardLoader.instance.mobData.towers[i];
            
            // 計算位置（每行3張卡片）
            int row = i / cardsPerRow;
            int col = i % cardsPerRow;
            float posX = 80 + col * (cardWidth + margin); // 從x=80開始排列
            float posY = 300 - row * (cardHeight + margin); // 從y=300開始向下排列
            
            // 生成卡片
            GameObject cardObj = Instantiate(CardLoader.instance.cardPrefab2, BagpackPanel.transform);
            RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
            
            // 設定位置和大小
            rectTransform.anchoredPosition = new Vector2(posX, posY);
            //rectTransform.sizeDelta = new Vector2(cardWidth, cardHeight);

            // 設定卡片資訊
            CardUIModel cardUI = cardObj.GetComponent<CardUIModel>();
            if (cardUI != null)
            {
                // 設定卡片資訊
                cardUI.cardName.text = tower.displayName;
                cardUI.cardWoodCost.text = tower.woodCost.ToString();
                cardUI.cardMeatCost.text = tower.meatCost.ToString();
                cardUI.cardImage.sprite = tower.previewBlueImage;

                // 加入點擊事件，顯示詳細資訊
                Button selectBtn = cardObj.GetComponent<Button>();
                if (selectBtn != null)
                {
                    selectBtn.onClick.AddListener(() => {
                        // 生成詳細資訊視窗
                        GameObject detailObj = Instantiate(CardLoader.instance.detailUIPrefab, BagpackPanel.transform);
                        DetailUIModel detailUI = detailObj.GetComponent<DetailUIModel>();
                        
                        if (detailUI != null)
                        {
                            // 設置所有詳細資訊
                            detailUI.detailImage.sprite = tower.previewBlueImage;
                            detailUI.Name.text = tower.displayName;
                            detailUI.Level.text = "Level " + tower.levels[0].level.ToString();
                            detailUI.Description.text = tower.description;
                            detailUI.WoodCost.text = tower.woodCost.ToString();
                            detailUI.MeatCost.text = tower.meatCost.ToString();
                            detailUI.Health.text = tower.levels[0].health.ToString();
                            detailUI.Damage.text = tower.levels[0].damage.ToString();
                            detailUI.Range.text = tower.levels[0].range.ToString();
                            detailUI.cardCount.text = "1";

                            // 設置關閉按鈕
                            detailUI.closeBtn.onClick.AddListener(() => {
                                Destroy(detailObj);
                            });

                            // 將詳細資訊視窗置中
                            RectTransform detailRect = detailObj.GetComponent<RectTransform>();
                            detailRect.anchoredPosition = Vector2.zero;
                        }
                    });
                }

                string towerName = tower.name;
                
            }
        }

        
        
    }

    public void updateCurrency(CurrencyUpdateEvent e)
    {
        print("currency was updated");
        if (DataBaseManager.instance != null && DataBaseManager.instance.auth != null)
        {
            if (coinText != null) coinText.text = DataBaseManager.instance.auth.coin.ToString();
            if (diamondText != null) diamondText.text = DataBaseManager.instance.auth.diamond.ToString();
        }
    }
    public void StartGame(string name)
    {
        StageModel stage = new StageModel();
        stage.stageName = name;
        stage.stageScene = "GameScene";
        MasterData.instance.currentStage = stage;
        SceneManager.LoadScene(stage.stageScene);
    }

}
