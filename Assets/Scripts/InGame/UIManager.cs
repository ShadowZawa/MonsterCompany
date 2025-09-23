
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Resource Display")]
    public Team team;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI meatText;

    public TextMeshProUGUI blueScoreText;
    public TextMeshProUGUI redScoreText;

    [Header("Game Panels")]
    public GameObject Towerpanel;
    public GameObject Mobpanel;

    [Header("Game Systems")]
    public TowerBuilder towerBuilder;
    public MobManager mobManager;

    private void Start()
    {
        EventBus.Instance.Subscribe<ResourceUpdateEvent>(UpdateResourceDisplay);
        EventBus.Instance.Subscribe<ScoreUpdateEvent>(UpdateScoreDisplay);
        EventBus.Instance.Subscribe<CurrencyUpdateEvent>(e => UpdateUI());
        UpdateUI();
        
        // 等待其他管理器初始化完成後創建按鈕
        Invoke("InitializeUI", 0.1f);
    }
    public void TriggerUI(int index)
    {
        Towerpanel.SetActive(index == 0);
        Mobpanel.SetActive(index == 1);
    }
    public void UpdateUI()
    {
        goldText.text = GameManager.instance.getResource(team, ResourceType.Gold).ToString();
        woodText.text = GameManager.instance.getResource(team, ResourceType.Wood).ToString();
        meatText.text = GameManager.instance.getResource(team, ResourceType.Meat).ToString();

        blueScoreText.text = GameManager.instance.getScore(Team.Blue).ToString();
        redScoreText.text = GameManager.instance.getScore(Team.Red).ToString();
    }
    public void UpdateScoreDisplay(ScoreUpdateEvent scoreEvent)
    {
        switch (scoreEvent.team)
        {
            case Team.Blue:
                blueScoreText.text = scoreEvent.Amount.ToString();
                break;
            case Team.Red:
                redScoreText.text = scoreEvent.Amount.ToString();
                break;
        }
    }
    public void UpdateResourceDisplay(ResourceUpdateEvent resourceEvent)
    {
        switch (resourceEvent.type)
        {
            case ResourceType.Gold:
                goldText.text = GameManager.instance.getResource(team, ResourceType.Gold).ToString();
                break;
            case ResourceType.Wood:
                woodText.text = GameManager.instance.getResource(team, ResourceType.Wood).ToString();
                break;
            case ResourceType.Meat:
                meatText.text = GameManager.instance.getResource(team, ResourceType.Meat).ToString();
                break;
        }
    }

    void InitializeUI()
    {
        if (!CardLoader.instance.hasInit)
        {
            CardLoader.instance.Init();
        }

        if (towerBuilder != null && Towerpanel != null)
        {
            CreateTowerButtons();
        }

        if (mobManager != null && Mobpanel != null)
        {
            CreateMobButtons();
        }
    }

    void CreateTowerButtons()
    {
        // 清除現有按鈕
        foreach (Transform child in Towerpanel.transform)
        {
            if (child.gameObject != Towerpanel)
                Destroy(child.gameObject);
        }

        var towerDataList = CardLoader.instance.mobData.towers;
        // 為每個塔創建按鈕
        for (int i = 0; i < towerDataList.Length; i++)
        {
            var towerData = towerDataList[i];
            GameObject cardObj = Instantiate(CardLoader.instance.cardPrefab, Towerpanel.transform);
            cardObj.transform.position = new Vector2(200,150) + Vector2.right * i*300;

            // 設置卡片信息
            CardUIModel cardUI = cardObj.GetComponent<CardUIModel>();
            if (cardUI != null)
            {
                cardUI.cardImage.GetComponent<RectTransform>().sizeDelta = new Vector2(50,50);
                cardUI.cardImage.sprite = team == Team.Blue ? towerData.previewBlueImage : towerData.previewRedImage;
                cardUI.SetCardInfo(towerData.displayName, towerData.woodCost, towerData.meatCost);
            }

            // 設置按鈕事件
            int index = i;
            Button button = cardObj.GetComponent<Button>();
            if (button == null)
            {
                button = cardObj.AddComponent<Button>();
            }
            button.onClick.AddListener(() => towerBuilder.StartBuild(index));
        }
    }

    void CreateMobButtons()
    {
        // 清除現有按鈕
        foreach (Transform child in Mobpanel.transform)
        {
            if (child.gameObject != Mobpanel)
                Destroy(child.gameObject);
        }

        var mobDataList = CardLoader.instance.mobData.mobs;
        // 為每個怪物創建按鈕
        for (int i = 0; i < mobDataList.Length; i++)
        {
            var mobData = mobDataList[i];
            GameObject cardObj = Instantiate(CardLoader.instance.cardPrefab, Mobpanel.transform);

            // 設置卡片圖片
            cardObj.transform.position = new Vector2(200, 150) + Vector2.right * i * 300;
            // 設置卡片信息
            CardUIModel cardUI = cardObj.GetComponent<CardUIModel>();
            if (cardUI != null)
            {
                cardUI.cardImage.sprite = mobData.previewImage;
                cardUI.SetCardInfo(mobData.displayName, mobData.woodCost, mobData.meatCost);
            }

            // 設置按鈕事件
            int index = i;
            Button button = cardObj.GetComponent<Button>();
            if (button == null)
            {
                button = cardObj.AddComponent<Button>();
            }
            button.onClick.AddListener(() => mobManager.EnqueueMob(index));
        }
        
        // 創建一個新的按鈕物件
        GameObject startBoatBtn = new GameObject("StartBoatButton");
        startBoatBtn.transform.SetParent(Mobpanel.transform, false);
        
        // 添加必要的UI組件
        Button startBoatButton = startBoatBtn.AddComponent<Button>();
        Image buttonImage = startBoatBtn.AddComponent<Image>();
        //buttonImage.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 100, 50), new Vector2(0.5f, 0.5f));
        // 創建並設置按鈕文字
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(startBoatBtn.transform, false);
        TextMeshProUGUI buttonText = textObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "啟航";
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontSize = 20; 
        buttonText.color = Color.black;


        // 設置按鈕位置和大小
        RectTransform buttonRect = startBoatBtn.GetComponent<RectTransform>();
        buttonRect.transform.position = new Vector2(200, 150) + Vector2.right * mobDataList.Length * 300;
        buttonRect.sizeDelta = new Vector2(100, 50);
        
        // 設置文字位置
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // 添加點擊事件
        startBoatButton.onClick.AddListener(() => mobManager.StartBoat());
    }
}