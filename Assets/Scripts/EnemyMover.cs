using System.Collections;
using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    private const string CurrentStageKey = "CurrentStage";

    [Header("Enemy Data")]
    [SerializeField] private EnemyData enemyData;

    [Header("Stage Speed Scaling")]
    [SerializeField] private int maxSpeedStage = 40;
    [SerializeField] private float maxMoveSpeed = 4.5f;

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.12f;
    [SerializeField] private float knockbackMultiplier = 1f;
    [SerializeField] private float knockbackCooldown = 0f;

    [Header("Knockback Limit")]
    [SerializeField] private bool limitMaxRightPositionAfterKnockback = false;
    [SerializeField] private float maxRightPositionAfterKnockback = 8.8f;

    private float finalMoveSpeed;
    private float lastKnockbackTime = -999f;
    private Coroutine knockbackCoroutine;

    private void Start()
    {
        CalculateStageMoveSpeed();
    }

    private void Update()
    {
        transform.Translate(Vector3.left * finalMoveSpeed * Time.deltaTime);
    }

    public void ApplyKnockback(float distance)
    {
        if (distance <= 0f)
            return;

        if (Time.time < lastKnockbackTime + knockbackCooldown)
            return;

        float finalDistance = distance * knockbackMultiplier;

        if (finalDistance <= 0f)
            return;

        lastKnockbackTime = Time.time;

        if (knockbackCoroutine != null)
            StopCoroutine(knockbackCoroutine);

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(finalDistance));
    }

    private IEnumerator KnockbackRoutine(float distance)
    {
        float timer = 0f;
        Vector3 startPosition = transform.position;

        float targetX = startPosition.x + distance;

        if (limitMaxRightPositionAfterKnockback)
        {
            if (startPosition.x < maxRightPositionAfterKnockback)
                targetX = Mathf.Min(targetX, maxRightPositionAfterKnockback);
            else
                targetX = startPosition.x;
        }

        Vector3 targetPosition = new Vector3(targetX, startPosition.y, startPosition.z);

        while (timer < knockbackDuration)
        {
            timer += Time.deltaTime;
            float t = timer / knockbackDuration;

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        transform.position = targetPosition;
        knockbackCoroutine = null;
    }

    private void CalculateStageMoveSpeed()
    {
        if (enemyData == null)
        {
            finalMoveSpeed = 0f;
            return;
        }

        int currentStage = PlayerPrefs.GetInt(CurrentStageKey, 1);
        currentStage = Mathf.Max(currentStage, 1);

        float baseMoveSpeed = enemyData.moveSpeed;

        if (currentStage >= maxSpeedStage)
        {
            finalMoveSpeed = maxMoveSpeed;
            return;
        }

        float progress = (float)(currentStage - 1) / (maxSpeedStage - 1);
        finalMoveSpeed = Mathf.Lerp(baseMoveSpeed, maxMoveSpeed, progress);
    }
}