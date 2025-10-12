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

        // 清除現有卡片(但保留 Exit 按鈕)
        foreach (Transform child in BagpackPanel.transform)
        {
            // 跳過名為 Exit 的物件
            if (child.gameObject.name == "Exit")
            {
                // 添加 Layout Element 並忽略它,避免被 Grid Layout 控制
                LayoutElement layoutElement = child.GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = child.gameObject.AddComponent<LayoutElement>();
                }
                layoutElement.ignoreLayout = true;
                continue;
            }
            Destroy(child.gameObject);
        }

        // 添加 Grid Layout Group 來自動排列卡片
        GridLayoutGroup gridLayout = BagpackPanel.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
        {
            gridLayout = BagpackPanel.AddComponent<GridLayoutGroup>();
        }
        
        // 設定 Grid 參數
        gridLayout.cellSize = new Vector2(100f, 128f);  // 卡片大小
        gridLayout.spacing = new Vector2(20f, 20f);     // 間距
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 3;  // 每行3張卡片
        gridLayout.childAlignment = TextAnchor.UpperLeft;
        gridLayout.padding = new RectOffset(80, 20, 20, 20);  // 左上右下邊距
        
        for (int i = 0; i < CardLoader.instance.mobData.towers.Length; i++)
        {
            mobLevelData tower = CardLoader.instance.mobData.towers[i];
            
            // 生成卡片 (位置由 Grid Layout 自動管理)
            GameObject cardObj = Instantiate(CardLoader.instance.cardPrefab2, BagpackPanel.transform);

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
        //print("currency was updated");
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
