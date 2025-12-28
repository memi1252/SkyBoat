using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class monster : MonoBehaviour
{
    [Header("Monster Stats")]
    public float range = 10f;
    public int damage = 20;
    public float hp = 100;
    public float attakeDelay = 2f;
    
    [Header("AI Settings")]
    public float attackRange = 2f;
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 5f;
    
    private NavMeshAgent agent;
    private Transform player;
    private bool isAttacking = false;
    private bool isDead = false;
    private float lastAttackTime;
    
    [Header("Animation")]
    public Animator animator;
    
    private enum MonsterState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }
    
    private MonsterState currentState = MonsterState.Idle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
        }
        
        // 애니메이터가 할당되지 않았다면 자동으로 찾기
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
    
    private void Start()
    {
        // 플레이어 찾기 (Player 태그를 가진 오브젝트)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        lastAttackTime = Time.time;
    }

    private void Update()
    {
        if (isDead || player == null) return;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case MonsterState.Idle:
                HandleIdleState(distanceToPlayer);
                break;
            case MonsterState.Chasing:
                HandleChasingState(distanceToPlayer);
                break;
            case MonsterState.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;
        }
        
        // 애니메이션 업데이트
        UpdateAnimation();
    }
    
    private void HandleIdleState(float distanceToPlayer)
    {
        // 플레이어가 감지 범위 내에 들어오면 추격 시작
        if (distanceToPlayer <= range)
        {
            currentState = MonsterState.Chasing;
        }
    }
    
    private void HandleChasingState(float distanceToPlayer)
    {
        // 플레이어가 범위를 벗어나면 Idle로 돌아감
        if (distanceToPlayer > range)
        {
            currentState = MonsterState.Idle;
            agent.ResetPath();
            return;
        }
        
        // 공격 범위 내에 들어오면 공격 상태로 전환
        if (distanceToPlayer <= attackRange)
        {
            currentState = MonsterState.Attacking;
            agent.ResetPath();
            return;
        }
        
        // 플레이어 추격
        agent.SetDestination(player.position);
        
        // 플레이어 방향으로 회전
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void HandleAttackingState(float distanceToPlayer)
    {
        // 플레이어가 공격 범위를 벗어나면 추격 상태로 전환
        if (distanceToPlayer > attackRange)
        {
            currentState = MonsterState.Chasing;
            return;
        }
        
        // 공격 딜레이 체크
        if (Time.time - lastAttackTime >= attakeDelay && !isAttacking)
        {
            StartCoroutine(AttackPlayer());
        }
        
        // 플레이어 방향으로 회전
        Vector3 direction = (player.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }
    
    private IEnumerator AttackPlayer()
    {
        isAttacking = true;
        
        // 공격 애니메이션 트리거
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // 공격 애니메이션 시간 대기 (0.5초 후 실제 데미지 적용)
        yield return new WaitForSeconds(1.7f);
        
        // 플레이어가 여전히 공격 범위 내에 있는지 확인
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // 플레이어에게 데미지 적용
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            else
            {
                Debug.Log($"Monster dealt {damage} damage to player!");
            }
        }
        
        lastAttackTime = Time.time;
        
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }
    
    private void UpdateAnimation()
    {
        if (animator == null) return;
        
        // 이동 속도에 따른 애니메이션
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
        
        // 상태에 따른 애니메이션
        animator.SetBool("IsChasing", currentState == MonsterState.Chasing);
        animator.SetBool("IsAttacking", isAttacking);
        animator.SetBool("IsDead", isDead);
    }
    
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        
        hp -= damageAmount;
        
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        if (hp <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isDead = true;
        currentState = MonsterState.Dead;
        
        if (agent != null)
        {
            agent.enabled = false;
        }
        
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
        }
        
        // 콜라이더 비활성화 (선택적)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        // 일정 시간 후 오브젝트 제거 (선택적)
        Destroy(gameObject, 3f);
    }
    
    // 기즈모로 감지 범위와 공격 범위 표시
    private void OnDrawGizmosSelected()
    {
        // 감지 범위 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
        
        // 공격 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
