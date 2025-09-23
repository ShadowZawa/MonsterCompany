using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;



[Serializable]
public class PlayerData
{
    [BsonId]
    public string userId;
    public string username;
    public int coin;
    public int diamond;
    public List<CardData> cards;

    public List<String> MobDecks; //Max Size 5 只存名稱
    public List<String> TowerDecks; //Max Size 5 只存名稱

}
[Serializable]
public enum CardType
{
    Mob,
    Tower
}
[Serializable]
public class CardData
{
    public string cardId; //(cardName)
    public CardType type;
    public int level;
    public int count;
}