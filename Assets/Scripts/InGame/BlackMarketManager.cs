using UnityEngine.UI;
using UnityEngine;



public enum BlackCardType
{
    SelfBuff,
    EnemyDebuff,
    Special
}
public class BlackMarketManager
{
    public GameObject blackMarketPanel; // 黑市面板
    private bool isOpen = false; // 黑市是否開啟
    public void Open()
    {
        isOpen = true;
        blackMarketPanel.SetActive(true);
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