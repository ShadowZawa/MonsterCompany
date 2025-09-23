using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum Team
{
    Blue,
    Red
}
public enum ResourceType
{
    Gold,
    Meat,
    Wood
}
public class TeamModel
{
    public int gold = 200;
    public int wood = 200;
    public int meat = 200;
    public int score = 0;


    public TowerModel[] towers;
    public MobModel[] mobs;
}
[Serializable]
public class StageModel
{
    public string stageName = "Unknown";
    public string stageScene = "Unknown";

}
public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject blueLoc;
    public GameObject redLoc;
    public TeamModel blueTeam = new TeamModel();
    public TeamModel redTeam = new TeamModel();

    private float countDownTime = 0f;
    private bool isCounting = false;
    public bool isOver { get; private set; } = false;

    public int getScore(Team team)
    {
        TeamModel model = (team == Team.Blue) ? blueTeam : redTeam;
        return model.score;
    }
    public int getResource(Team team, ResourceType type)
    {
        TeamModel model = (team == Team.Blue) ? blueTeam : redTeam;
        switch (type)
        {
            case ResourceType.Gold:
                return model.gold;
            case ResourceType.Meat:
                return model.meat;
            case ResourceType.Wood:
                return model.wood;
        }
        return 0;
    }
    public void costResource(Team team, ResourceType type, int amount)
    {
        TeamModel model = (team == Team.Blue) ? blueTeam : redTeam;
        switch (type)
        {
            case ResourceType.Gold:
                model.gold -= amount;
                EventBus.Instance.Publish(new ResourceUpdateEvent(team, ResourceType.Gold));
                break;
            case ResourceType.Meat:
                model.meat -= amount;
                EventBus.Instance.Publish(new ResourceUpdateEvent(team, ResourceType.Meat));
                break;
            case ResourceType.Wood:
                model.wood -= amount;
                EventBus.Instance.Publish(new ResourceUpdateEvent(team, ResourceType.Wood));
                break;
        }
    }

    public void addResource(Team team, ResourceType type, int amount)
    {
        TeamModel model = (team == Team.Blue) ? blueTeam : redTeam;
        switch (type)
        {
            case ResourceType.Gold:
                model.gold += amount;
                break;
            case ResourceType.Meat:
                model.meat += amount;
                break;
            case ResourceType.Wood:
                model.wood += amount;
                break;
        }
    }

    public void startGame(float seconds = 1800f)
    {
        StartCoroutine(StartGameCountdown(seconds));
    }

    private IEnumerator StartGameCountdown(float seconds)
    {
        isCounting = false;
        for (int i = 3; i >= 1; i--)
        {
            MessageBox.instance.ShowTitle(i.ToString(), Color.yellow);
            yield return new WaitForSeconds(1f);
        }
        MessageBox.instance.ShowTitle("開始！", Color.green);
        yield return new WaitForSeconds(1f);
        countDownTime = seconds;
        isCounting = true;
    }

    public float getCountTime()
    {
        return countDownTime;
    }

    void Update()
    {
        if (isCounting && countDownTime > 0f)
        {
            countDownTime -= Time.deltaTime;
            if (countDownTime <= 0f)
            {
                countDownTime = 0f;
                isCounting = false;

                // 可在此觸發遊戲結束事件
                GameOver();
            }
        }
    }
    public void GameOver()
    {
        // 顯示遊戲結束畫面
        // 根據分數決定勝利隊伍
        isOver = true;
        string result;
        if (blueTeam.score > redTeam.score)
        {
            result = "藍隊獲勝！";
        }
        else if (redTeam.score > blueTeam.score)
        {
            result = "紅隊獲勝！";
        }
        else
        {
            result = "平手！";
        }
        MessageBox.instance.ShowMessage("遊戲結束！" + result, Color.cyan);
        // delay 3 seconds then show detail
        StartCoroutine(DelayAndSwitchScene(3f));
    }

    private IEnumerator DelayAndSwitchScene(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Switch to the main menu scene
        SceneManager.LoadScene("MenuScene");
    }

    void Awake()
    {
        if (MasterData.instance == null)
        {
            SceneManager.LoadScene("MenuScene");
            return; 
        }
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {

        startGame();
        print("Start Game:" + MasterData.instance.currentStage.stageName);
        
    }

}
