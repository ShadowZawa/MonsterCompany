using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Farmer有兩種型態 
    meat - 會前往最近的(tag有Meat的物件 並開始執行 Farming採集物資 超過storage上限後會拿執行BackToDepot到達重生點(_controller.spawnPos)後會繼續重新採集物資)
    tree - 會前往最近的(tag有Tree的物件並開始執行 Farming採集物資 超過storage上限後會拿執行BackToDepot到達重生點(_controller.spawnPos)後會繼續重新採集物資 )
*/
public enum FarmerState
{
    Idle,
    MovingToTarget,
    Farming,
    BackToDepot,
}

public class FarmerAI : MonoBehaviour
{
    public Team team;
    private Animator _animator;
    private HouseController _controller;
    private FarmerState currentState;
    private float moveSpeed = 2f;
    private int health;
    public int getCurrentHealth => health;
    private int storage = 0;
    private int maxStorage = 10;
    private GameObject currentTarget;
    private bool isInitialized = false; 
    public FarmerTargetType targetType;

    public void init(HouseController controller)
    {
        _controller = controller;
        health = _controller.farmerHealth;
        isInitialized = true;
        team = controller.team;
        gameObject.tag = (team == Team.Blue) ? "Blue" : "Red";
        currentState = FarmerState.Idle;
        targetType = controller.farmerTarget;
    }

    void Start()
    {
        _animator = GetComponent<Animator>();
        currentState = FarmerState.Idle;
    }

    void Update()
    {
        if (!isInitialized) return;

        switch (currentState)
        {
            case FarmerState.Idle:
                _animator?.Play("Idle");
                targetType = _controller.farmerTarget;
                SearchForResource();
                break;
            case FarmerState.MovingToTarget:
                MoveToTarget();
                break;
            case FarmerState.Farming:
                _animator?.Play("Heal");
                FarmResource();
                break;
            case FarmerState.BackToDepot:
                MoveToDepot();
                break;
        }
    }

    void SearchForResource()
    {
        string tag = (targetType == FarmerTargetType.meat) ? "Meat" : "Tree";
        GameObject[] resources = GameObject.FindGameObjectsWithTag(tag);
        GameObject nearest = null;
        float minDist = float.MaxValue;
        foreach (var res in resources)
        {
            float dist = Vector3.Distance(transform.position, res.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = res;
            }
        }
        if (nearest != null)
        {
            currentTarget = nearest;
            currentState = FarmerState.MovingToTarget;
        }
    }

    void MoveToTarget()
    {
        if (currentTarget == null)
        {
            currentState = FarmerState.Idle;
            return;
        }
        Vector3 direction = (currentTarget.transform.position - transform.position);
        direction.y = 0;
        if (direction.magnitude < 0.3f)
        {
            currentState = FarmerState.Farming;
            return;
        }
        _animator?.Play("Run");
        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
    }

    private float farmTimer = 0f;
    void FarmResource()
    {
        if (currentTarget == null)
        {
            currentState = FarmerState.Idle;
            return;
        }
        farmTimer += Time.deltaTime;
        if (farmTimer >= _controller.farmerCollectSpeed)
        {
            currentTarget.SendMessage("collect", _controller.farmerDamage, SendMessageOptions.DontRequireReceiver);
            storage += _controller.farmerDamage;
            farmTimer = 0f;
        }
        if (storage >= maxStorage)
        {
            currentState = FarmerState.BackToDepot;
            farmTimer = 0f;
        }
    }

    void MoveToDepot()
    {
        Vector3 depotPos = _controller.spawnPos.position;
        Vector3 direction = (depotPos - transform.position);
        direction.y = 0;
        if (direction.magnitude < 0.3f)
        {
            if (targetType == FarmerTargetType.meat)
            {

                GameManager.instance.addResource(team, ResourceType.Meat, storage);
                EventBus.Instance.Publish(new ResourceUpdateEvent(team, ResourceType.Meat));
            }
            else
            {
                GameManager.instance.addResource(team, ResourceType.Wood, storage);
                EventBus.Instance.Publish(new ResourceUpdateEvent(team, ResourceType.Wood));
                
            }
            storage = 0;
            currentState = FarmerState.Idle;
            return;
        }
        _animator?.Play("Run");
        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
    }

    public void takeDamage(int dmg)
    {
        health -= dmg;
        if (health < 0)
        {
            Destroy(gameObject);
        }
    }
}
