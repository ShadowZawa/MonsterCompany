
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

    [Header("Detail Info Panel")]
    public GameObject detailPanel;
    public DetailUIModel detailUI;

    [Header("Game Systems")]
    public TowerBuilder towerBuilder;
    public MobManager mobManager;
    
    private GameObject currentSelectedObject;
    private Camera mainCamera;

    private void Start()
    {
        EventBus.Instance.Subscribe<ResourceUpdateEvent>(UpdateResourceDisplay);
        EventBus.Instance.Subscribe<ScoreUpdateEvent>(UpdateScoreDisplay);
        EventBus.Instance.Subscribe<CurrencyUpdateEvent>(e => UpdateUI());
        UpdateUI();
        
        mainCamera = Camera.main;
        if (detailPanel != null)
            detailPanel.SetActive(false);
        
        // 等待其他管理器初始化完成後創建按鈕
        Invoke("InitializeUI", 0.1f);
    }
    
    void Update()
    {
        // 處理點擊檢測
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            Vector3 screenPosition;
            if (Input.touchCount > 0)
                screenPosition = Input.GetTouch(0).position;
            else
                screenPosition = Input.mousePosition;
                
            // 檢查是否點擊到UI
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
                
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
            worldPosition.z = 0;
            
            // 使用射線檢測點擊的物件
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
            
            if (hit.collider != null)
            {
                Debug.Log(hit.collider.gameObject.name);
                GameObject clickedObject = hit.collider.gameObject;
                ShowDetailInfo(clickedObject);
            }
            else
            {
                HideDetailInfo();
            }
        }
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
    
    void ShowDetailInfo(GameObject obj)
    {
        currentSelectedObject = obj;
        
        if (detailPanel == null || detailUI == null)
            return;
        
        // 關閉其他面板
        Towerpanel.SetActive(false);
        Mobpanel.SetActive(false);
        
        // 顯示詳細資訊面板
        detailPanel.SetActive(true);
        
        string description = GetObjectDescription(obj);
        detailUI.Description.text = description;
        
        // 設置按鈕功能
        SetupDetailButtons(obj);
    }
    
    void HideDetailInfo()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);
        currentSelectedObject = null;
    }
    
    string GetObjectDescription(GameObject obj)
    {
        string description = "";
        
        // 檢測EnemyAI組件（Mob）
        EnemyAI enemy = obj.GetComponent<EnemyAI>();
        if (enemy != null)
        {
            description += $"單位類型: 怪物\n";
            description += $"隊伍: {enemy.team}\n";
            description += $"血量: {enemy.getCurrentHealth}/{enemy.maxHealth}\n";
            description += $"攻擊力: {enemy.damage}\n";
            description += $"移動速度: {enemy.moveSpeed}\n";
            description += $"攻擊範圍: {enemy.attackRadius}";
            return description;
        }
        
        // 檢測HouseController組件（House Tower）
        HouseController house = obj.GetComponent<HouseController>();
        if (house != null)
        {
            description += $"建築類型: 房屋\n";
            description += $"隊伍: {house.team}\n";
            description += $"血量: {house.getHealth}/{house.maxHealth}\n";
            description += $"農民血量: {house.farmerHealth}\n";
            description += $"農民攻擊力: {house.farmerDamage}\n";
            description += $"採集速度: {house.farmerCollectSpeed}s\n";
            description += $"目標資源: {(house.farmerTarget == FarmerTargetType.meat ? "肉類" : "木材")}";
            return description;
        }
        
        // 檢測其他塔類型組件
        var archerTower = obj.GetComponent<ArcherTowerController>();
        if (archerTower != null)
        {
            description += $"建築類型: 弓箭塔\n";
            description += $"隊伍: {archerTower.team}\n";
            description += $"血量: {archerTower.getHealth}/{archerTower.maxHealth}\n";
            description += $"塔範圍: {archerTower.towerRadius}\n";
            description += $"士兵血量: {archerTower.soilderHealth}\n";
            description += $"士兵攻擊力: {archerTower.soilderDamage}\n";
            description += $"士兵攻擊範圍: {archerTower.soilderAttackRange}";
            return description;
        }
        
        var warriorTower = obj.GetComponent<WarriorTowerController>();
        if (warriorTower != null)
        {
            description += $"建築類型: 戰士塔\n";
            description += $"隊伍: {warriorTower.team}\n";
            description += $"血量: {warriorTower.getHealth}/{warriorTower.maxHealth}\n";
            description += $"塔範圍: {warriorTower.towerRadius}\n";
            description += $"士兵血量: {warriorTower.soilderHealth}\n";
            description += $"士兵攻擊力: {warriorTower.soilderDamage}";
            return description;
        }
        
        // 檢測FarmerAI組件（農民）
        FarmerAI farmer = obj.GetComponent<FarmerAI>();
        if (farmer != null)
        {
            description += $"單位類型: 農民\n";
            description += $"隊伍: {farmer.team}\n";
            description += $"血量: {farmer.getCurrentHealth}\n";
            description += $"目標資源: {(farmer.targetType == FarmerTargetType.meat ? "肉類" : "木材")}";
            return description;
        }
        CastleModel castle = obj.GetComponent<CastleModel>();
        if (castle != null)
        {
            description += $"單位類型: 主堡\n";
            description += $"隊伍: {castle.team}\n";
            description += $"血量: {castle.getHealth}/{castle.maxHealth}\n";
            return description;
        }
        
        return $"物件名稱: {obj.name}\n無詳細資訊";
    }
    
    void SetupDetailButtons(GameObject obj)
    {
        // 清除所有按鈕事件
        detailUI.useBtn.onClick.RemoveAllListeners();
        // 檢查是否為House Tower，如果是則顯示模式切換按鈕
        HouseController house = obj.GetComponent<HouseController>();
        if (house != null)
        {
            detailUI.useBtn.gameObject.SetActive(true);
            detailUI.useBtn.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = "切換模式";
            detailUI.useBtn.onClick.AddListener(() => ToggleFarmerMode(house));
        }
        else
        {
            //Debug.Log("Not a HouseController");
            detailUI.useBtn.gameObject.SetActive(false);
        }
    }
    
    void ToggleFarmerMode(HouseController house)
    {
        // 切換農民的目標資源類型
        house.farmerTarget = (house.farmerTarget == FarmerTargetType.meat) 
            ? FarmerTargetType.tree 
            : FarmerTargetType.meat;
            
        // 更新顯示資訊
        ShowDetailInfo(currentSelectedObject);
        
        // 顯示切換訊息
        string newTarget = (house.farmerTarget == FarmerTargetType.meat) ? "肉類" : "木材";
        MessageBox.instance.ShowMessage($"農民目標已切換為: {newTarget}", Color.green);
    }
}