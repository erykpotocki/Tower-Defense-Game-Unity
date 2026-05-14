using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;

    [Header("Boss Settings")]
    [SerializeField] private bool isBoss = false;
    [SerializeField] private float bossAttackInterval = 1.5f;

    private CastleHealth currentCastle;
    private Coroutine bossAttackCoroutine;
    private EnemyMover enemyMover;

    private void Awake()
    {
        enemyMover = GetComponent<EnemyMover>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CastleHealth castle = other.GetComponent<CastleHealth>();

        if (castle == null || enemyData == null)
            return;

        if (isBoss)
        {
            StartBossAttack(castle);
        }
        else
        {
            castle.TakeDamage(enemyData.damage);
            Destroy(gameObject);
        }
    }

    private void StartBossAttack(CastleHealth castle)
    {
        currentCastle = castle;

        if (enemyMover != null)
            enemyMover.enabled = false;

        if (bossAttackCoroutine == null)
            bossAttackCoroutine = StartCoroutine(BossAttackRoutine());
    }

    private IEnumerator BossAttackRoutine()
    {
        while (currentCastle != null)
        {
            currentCastle.TakeDamage(enemyData.damage);

            yield return new WaitForSeconds(bossAttackInterval);
        }

        bossAttackCoroutine = null;
    }

    private void OnDestroy()
    {
        if (bossAttackCoroutine != null)
            StopCoroutine(bossAttackCoroutine);
    }
}