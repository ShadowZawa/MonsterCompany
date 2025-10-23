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
    private EntityModel _model;
    private FarmerState currentState;
    private int storage = 0;
    private int maxStorage = 10;
    private GameObject currentTarget;
    private bool isInitialized = false; 
    public FarmerTargetType targetType;
    public EntityModel getModel => _model;

    public void init(HouseController controller)
    {
        _controller = controller;
        _model = GetComponent<EntityModel>();
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
        if (GameManager.instance.isOver) return;
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
        if (storage == 0)
        {
            targetType = _controller.farmerTarget;
        }
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
        if (nearest != null && Vector3.Distance(transform.position, nearest.transform.position) <= 20f)
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
        //direction.y = 0;
        if (Vector3.Distance(transform.position, currentTarget.transform.position) < 0.5f)
        {
            currentState = FarmerState.Farming;
            return;
        }
        _animator?.Play("Run");
        transform.position += direction.normalized * _model.moveSpeed * Time.deltaTime;
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
        if (farmTimer >= _model.attackInterval)
        {
            currentTarget.SendMessage("collect", _model.damage, SendMessageOptions.DontRequireReceiver);
            storage += _model.damage;
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
        Vector3 depotPos;
        if (_controller == null)
        {
            depotPos = (team == Team.Blue) ? GameManager.instance.blueCastle.transform.position : GameManager.instance.redCastle.transform.position;
        }
        else
        {

            depotPos = _controller.spawnPos.position;
        }
        Vector3 direction = (depotPos - transform.position);
        if (Vector3.Distance(depotPos, transform.position) < 0.3f)
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
            if (_controller == null)
            {
                Destroy(gameObject);
            }
            currentState = FarmerState.Idle;
            return;
        }
        _animator?.Play("Run");
        transform.position += direction.normalized * _model.moveSpeed * Time.deltaTime;
    }


}
