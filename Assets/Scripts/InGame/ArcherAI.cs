using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ArcherState
{
    Idle,
    Patrol,
    Chasing,
    Attack
}
public class ArcherAI : MonoBehaviour
{
    public Team team;
    private Animator _animator;
    private ArcherTowerController _towerController;
    private ArcherState currentState;
    private Vector3 targetPosition;
    private float moveSpeed = 2f;
    private int health;
    private float patrolRadius = 3f;
    private float patrolInterval = 2f;
    private float patrolTimer = 0f;
    private float attackInterval = 1.2f;
    private float attackTimer = 0f;
    private float attackRange = 5f;
    private GameObject currentTarget;
    private bool isInitialized = false;
    public GameObject arrowPrefab;

    void Start()
    {
        currentState = ArcherState.Patrol;
        _animator = GetComponent<Animator>();
    }

    public void init(ArcherTowerController controller)
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
        if (!isInitialized) return;

        switch (currentState)
        {
            case ArcherState.Idle:
                _animator.Play("Idle", 0);
                break;
            case ArcherState.Patrol:
                _animator.Play("Run ", 0);
                Patrol();
                patrolTimer += Time.deltaTime;
                if (patrolTimer >= patrolInterval)
                {
                    SetNewPatrolTarget();
                    patrolTimer = 0f;
                }
                SearchForEnemy();
                break;
            case ArcherState.Chasing:
                _animator.Play("Run ", 0);
                ChaseEnemy();
                break;
            case ArcherState.Attack:
                _animator.Play("Shoot", 0);
                AttackEnemy();
                break;
        }
    }

    void Patrol()
    {
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0;
        if (direction.magnitude < 0.2f)
        {
            _animator.Play("Idle", 0);
        }
        else
        {
            _animator.Play("Run ", 0);

            transform.position += direction.normalized * moveSpeed * Time.deltaTime;
        }
    }

    void SetNewPatrolTarget()
    {
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
                if (dist < patrolRadius && dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy;
                }
            {
                
            }
        }
        if (nearest != null)
        {
            currentTarget = nearest;
            currentState = ArcherState.Chasing;
            attackTimer = 0f;
        }
    }

    void ChaseEnemy()
    {
        if (currentTarget == null)
        {
            currentState = ArcherState.Patrol;
            return;
        }
        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (_towerController != null)
        {
            if (dist > _towerController.towerRadius)
            {
                currentTarget = null;
                currentState = ArcherState.Patrol;
                return;
            }

            if (dist <= attackRange) // 遠程攻擊距離
            {
                currentState = ArcherState.Attack;
                return;
            }
        }
        else
        {
            if (dist > patrolRadius)
            {
                currentTarget = null;
                currentState = ArcherState.Patrol;
                return;
            }

            if (dist <= 2f) // 遠程攻擊距離
            {
                currentState = ArcherState.Attack;
                return;
            }
        }
        _animator.Play("Run ", 0);
        Vector3 direction = (currentTarget.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    void AttackEnemy()
    {
        if (currentTarget == null)
        {
            currentState = ArcherState.Patrol;
            return;
        }
        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (dist > _towerController.towerRadius)
        {
            currentTarget = null;
            currentState = ArcherState.Patrol;
            return;
        }
        attackTimer += Time.deltaTime;
        if (attackTimer >= attackInterval)
        {
            ShootArrow(currentTarget);
            attackTimer = 0f;
        }
        // 可加上面向敵人
    }

    void ShootArrow(GameObject target)
    {
        if (arrowPrefab == null || target == null) return;
        Vector3 start = transform.position + Vector3.up * 1f;
        Vector3 end = target.transform.position + Vector3.up * 1f;
        GameObject arrow = Instantiate(arrowPrefab, start, Quaternion.identity);
        StartCoroutine(ArrowParabola(arrow, start, end, 0.5f, target));
    }

    IEnumerator ArrowParabola(GameObject arrow, Vector3 start, Vector3 end, float duration, GameObject target)
    {
        float t = 0;
        float height = 2f; // 拋物線最高點高度
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            // 拋物線插值
            Vector3 mid = Vector3.Lerp(start, end, t);
            float parabola = 4 * height * t * (1 - t); // 標準化拋物線
            mid.y += parabola;
            arrow.transform.position = mid;
            // 旋轉箭頭
            Vector3 dir = (end - start).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            yield return null;
        }
        // 命中
        if (target != null)
        {
            target.SendMessage("takeDamage", _towerController.soilderDamage, SendMessageOptions.DontRequireReceiver);
        }
        Destroy(arrow);
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