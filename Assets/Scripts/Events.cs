using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ResourceUpdateEvent
{
    public Team team;       // 資源所屬隊伍
    public ResourceType type;
    public ResourceUpdateEvent(Team team, ResourceType type)
    {
        this.team = team;
        this.type = type;
    }
}
public struct CurrencyUpdateEvent
{
    
}
public struct ScoreUpdateEvent
{
    public Team team;
    public int Amount;
    public ScoreUpdateEvent(Team team, int amount)
    {
        this.team = team;
        this.Amount = amount;
    }
}

