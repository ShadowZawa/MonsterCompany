using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a script that controls soilder's AI with 3 states 
/*
    Idle: The soilder is not doing anything.
    Patrol: The soilder is moving between waypoints.(towerController.towerRadius)
    Attack: The soilder is attacking the enemy in the radius(towerController.towerRadius) with damage (towerController.soilderDamage)
    enemy would have tag "Enemy"
*/
public enum SoliderState
{
    Idle,
    Patrol,
    Chasing,
    Attack
}
public class SoilderAI : MonoBehaviour
{
    public Team team;
    private Animator _animator;
    private WarriorTowerController _towerController;
    private SoliderState currentState;
    private Vector3 targetPosition;
    private float moveSpeed = 2f;
    private int health;
    private float patrolRadius = 3f;
    private float patrolInterval = 2f;
    private float patrolTimer = 0f;
    private float attackInterval = 1f;
    private float attackTimer = 0f;
    private GameObject currentTarget;
    private bool isInitialized = false;

    void Start()
    {
        currentState = SoliderState.Patrol; 
        _animator = GetComponent<Animator>();
        
    }
    public void init(WarriorTowerController controller)
    {
        _towerController = controller;
        health = _towerController.soilderHealth;
        SetNewPatrolTarget();
        patrolTimer = 0f;
        isInitialized = true;
        team = controller.team;
        gameObject.tag = (team == Team.Blue) ? "Blue" : "Red";

    }

    void Update()
    {
        if (GameManager.instance.isOver) return;
        if (!isInitialized) return;

        switch (currentState)
        {
            case SoliderState.Idle:
                _animator.Play("Idle");
                // 可擴充: 站立動畫
                break;
            case SoliderState.Patrol:
                Patrol();
                patrolTimer += Time.deltaTime;
                if (patrolTimer >= patrolInterval)
                {
                    SetNewPatrolTarget();
                    patrolTimer = 0f;
                }
                SearchForEnemy();
                break;
            case SoliderState.Chasing:
                ChaseEnemy();
                break;
            case SoliderState.Attack:
                _animator.Play("Attack 1");
                AttackEnemy();
                break;
        }
    }

    void Patrol()
    {
        Vector3 direction = (targetPosition - transform.position);
        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        { 
            // 由 patrolInterval 控制換點
            _animator.Play("Idle");
        }
        else
        { 
            _animator.Play("Run"); 
            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
        }
    }

    void SetNewPatrolTarget()
    {
        if (_towerController == null) return;
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * patrolRadius;
        targetPosition = _towerController.transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
    }

    void SearchForEnemy()
    {
        string enemyTag = (team == Team.Blue) ? "Red" : "Blue";
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        GameObject nearest = null;
        float minDist = float.MaxValue;
        foreach (var enemy in enemies)
        {
            if (enemy == this.gameObject) continue;

            // 嘗試從目標物件取得其 team，若取得失敗則保守跳過
            Team? candidateTeam = GetTeamFromObject(enemy);
            if (candidateTeam.HasValue && candidateTeam.Value == this.team)
            {
                // 同隊，跳過
                continue;
            }

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (_towerController != null)
            {
                if (dist < _towerController.towerRadius && dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy;
                }
            }
            else
            {
                if (dist < minDist && dist < patrolRadius)
                {
                    minDist = dist;
                    nearest = enemy;
                }
            }
        }
        if (nearest != null)
        {
            currentTarget = nearest;
            currentState = SoliderState.Chasing;
            attackTimer = 0f;
        }
    }

    // 輔助函式：嘗試從常見組件讀取 team 值
    private Team? GetTeamFromObject(GameObject obj)
    {
        // 檢查 EnemyAI
        var enemyAI = obj.GetComponent<EnemyAI>();
        if (enemyAI != null) return enemyAI.team;

        // 檢查 SoilderAI
        var soilderAI = obj.GetComponent<SoilderAI>();
        if (soilderAI != null) return soilderAI.team;

        // 檢查 FarmerAI
        var farmerAI = obj.GetComponent<FarmerAI>();
        if (farmerAI != null) return farmerAI.team;

        // 檢查 HouseController
        var house = obj.GetComponent<HouseController>();
        if (house != null) return house.team;

        // 檢查 WarriorTowerController / ArcherTowerController
        var warrior = obj.GetComponent<WarriorTowerController>();
        if (warrior != null) return warrior.team;
        var archer = obj.GetComponent<ArcherTowerController>();
        if (archer != null) return archer.team;

        // 無法確定
        return null;
    }
    void ChaseEnemy()
    {
        if (currentTarget == null)
        {
            currentState = SoliderState.Patrol;
            return;
        }
        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (_towerController != null)
        {
            if (dist > _towerController.towerRadius)
            {
                currentTarget = null;
                currentState = SoliderState.Patrol;
                return;
            }
        }
        if (dist <= 0.5f) // 進入攻擊距離
            {
                currentState = SoliderState.Attack;
                return;
            }
        _animator.Play("Run");
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }
    

    void AttackEnemy()
    {
        if (currentTarget == null)
        {
            currentState = SoliderState.Patrol;
            return;
        }
        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (dist > _towerController.towerRadius)
        {
            currentTarget = null;
            currentState = SoliderState.Patrol;
            return;
        }
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            // 假設敵人有 TakeDamage(int) 方法
            currentTarget.SendMessage("takeDamage", _towerController.soilderDamage, SendMessageOptions.DontRequireReceiver);
            attackTimer = 0f;
        }
        // 可加上面向敵人
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
