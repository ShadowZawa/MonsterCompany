using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MongoDB.Driver;
using MongoDB.Bson;

public class DataBaseManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static DataBaseManager instance;
    public PlayerData auth;
    private IMongoDatabase database;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(instance);
            instance = this;
        }
    }
    void Start()
    {
        StartCoroutine(InitializeDatabase());
        EventBus.Instance.Subscribe<GameOverEvent>(e => dealGameOver(e.winningTeam));
    }

    private IEnumerator InitializeDatabase()
    {
        // 等待一幀確保其他組件都已初始化
        yield return null;

        //MongoClient client = new MongoClient("mongodb://db_user:Abc123456@125.228.7.197:27017/");
        //var nas = client.ListDatabaseNames();
        //database = client.GetDatabase("PlayerData");
        //Register("DefaultUser");
        //Login("DefaultUser");
        //EventBus.Instance.Publish(new CurrencyUpdateEvent());
        MessageBox.instance.ShowMessage("登入成功！", Color.green);
        auth = new PlayerData
        {
            userId = ObjectId.GenerateNewId().ToString(),
            username = "Guest",
            coin = 1000,     // 初始金幣
            diamond = 100,    // 初始鑽石
            cards = new List<CardData>() // 初始空卡片列表
        };
    }

    public bool Register(string username)
    {
        var collection = database.GetCollection<PlayerData>("Users");

        // 檢查用戶名是否已存在
        var existingUser = collection.Find(x => x.username == username).FirstOrDefault();
        if (existingUser != null)
        {
            MessageBox.instance.ShowMessage("用戶名已存在", Color.red);
            return false;
        }

        // 創建新用戶
        auth = new PlayerData
        {
            userId = ObjectId.GenerateNewId().ToString(),
            username = username,
            coin = 1000,     // 初始金幣
            diamond = 100,    // 初始鑽石
            cards = new List<CardData>() // 初始空卡片列表
        };

        // 寫入數據庫
        collection.InsertOne(auth);
        MessageBox.instance.ShowMessage("註冊成功！", Color.green);
        return true;
    }

    public bool Login(string username)
    {
        var collection = database.GetCollection<PlayerData>("Users");

        // 查找用戶
        auth = collection.Find(x => x.username == username).FirstOrDefault();

        if (auth == null)
        {
            MessageBox.instance.ShowMessage("用戶不存在", Color.red);
            return false;
        }

        /*
            讀取auth.MobDecks
            如不存在則建立預設(Warrior_Tower, Archer_Tower)
            讀取auth.TowerDecks
        */
        EventBus.Instance.Publish(new CurrencyUpdateEvent());
        MessageBox.instance.ShowMessage("登入成功！", Color.green);
        return true;
    }

    public int getInfo()
    {
        return auth.coin;
    }

    private void dealGameOver(Team? winner)
    {
        var collection = database.GetCollection<PlayerData>("Users");
        var filter = Builders<PlayerData>.Filter.Eq(u => u.username, auth.username);
        var update = Builders<PlayerData>.Update.Inc(u => u.coin, 10);
        collection.UpdateOne(filter, update);
    }
}
