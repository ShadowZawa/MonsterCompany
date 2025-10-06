using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EnemyState
{
    Idle,
    Move,
    Attack
}

public class EnemyAI : MonoBehaviour
{
    private EnemyState currentState = EnemyState.Idle;
    private int currentHealth;
    public int getCurrentHealth => currentHealth;
    private Animator _animator;
    public Team team=Team.Blue;
    public int maxHealth = 50;
    public float moveSpeed = 1.5f;
    public float attackRadius = 0.5f;
    public int damage = 10;
    private GameObject target;
    private float attackInterval = 1f;
    private float attackTimer = 0f;
    public bool isBoating = true;

    void Start()
    {
        currentHealth = maxHealth;
        FindTarget();
        _animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        if (GameManager.instance.isOver) return;
        gameObject.tag = (team == Team.Blue) ? "Blue" : "Red";
        switch (currentState)
        {
            case EnemyState.Idle:
                _animator.Play("Idle");
                if (!isBoating)
                {

                    FindTarget();
                    if (target != null)
                    {
                        currentState = EnemyState.Move;
                    }
                }

                break;
            case EnemyState.Move:
                if (target == null)
                {
                    currentState = EnemyState.Idle;
                    break;
                }
                float dist = Vector3.Distance(transform.position, target.transform.position);
                if (dist > attackRadius)
                {
                    _animator.Play("Run");
                    MoveToTarget();
                }
                else
                {
                    currentState = EnemyState.Attack;
                }
                break;
            case EnemyState.Attack:
                if (target == null)
                {
                    currentState = EnemyState.Idle;
                    break;
                }
                float distA = Vector3.Distance(transform.position, target.transform.position);
                if (distA > attackRadius)
                {
                    currentState = EnemyState.Move;
                    break;
                }
                attackTimer += Time.deltaTime;
                if (attackTimer >= attackInterval)
                {
                    _animator.Play("Attack");
                    AttackTarget();
                    attackTimer = 0f;
                }
                break;
        }
    }

    void FindTarget()
    {
        GameObject[] candidates = GameObject.FindGameObjectsWithTag(team == Team.Blue ? "Red" : "Blue");
        target = null;
        if (candidates.Length > 0)
        {
            GameObject nearest = candidates[0];
            float minDist = Vector3.Distance(transform.position, nearest.transform.position);
            for (int i = 1; i < candidates.Length; i++)
            {
                float dist = Vector3.Distance(transform.position, candidates[i].transform.position);
                if (dist < minDist) 
                {
                    minDist = dist;
                    nearest = candidates[i];
                }
            }
            if (minDist > 20) return;
            // 避免自己成為目標
            if (nearest != this.gameObject)
            {
                target = nearest;
            }
        }
    }

    void MoveToTarget()
    {
        if (target != null && target != this.gameObject)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    void AttackTarget()
    {
        // 假設塔有 takeDamage(int) 方法
        if (target != null && target != this.gameObject)
        {
            target.SendMessage("takeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void takeDamage(int dmg)
    {
        //print("enemy take dmg:"+dmg);
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {

            Destroy(gameObject);
        }
    }
}
