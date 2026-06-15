using UnityEngine;
using System.Collections.Generic;

public class Katana : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform katanaPivot;

    [Header("Combat")]
    [SerializeField] private float attackRange = 3.5f;
    // Базовый урон теперь берется из PlayerDarya, но можно оставить как запасной вариант
    [SerializeField] private int fallbackDamage = 10;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visuals & Physics")]
    [SerializeField] private Animator anim;
    [SerializeField] private Collider2D hitCollider;

    [Header("Audio")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField, Range(0f, 1f)] private float attackSoundVolume = 1f;
    [SerializeField] private AudioSource audioSource;

    [Header("Debug")]
    [SerializeField] private bool logDamageDebug = true;

    private float timeSinceLastAttack;
    private bool isAttacking;
    private HashSet<MonoBehaviour> damagedEnemies = new HashSet<MonoBehaviour>();

    // Кэш ссылки на игрока, чтобы не искать его каждый удар
    private PlayerDarya cachedPlayer;

    private void Start()
    {
        if (audioSource == null && attackSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (hitCollider != null)
        {
            hitCollider.enabled = false;
            if (!hitCollider.isTrigger)
            {
                Debug.LogWarning("[Katana] HitCollider should be a Trigger! Fixing...");
                hitCollider.isTrigger = true;
            }
        }
        else
        {
            Debug.LogError("[Katana] HitCollider NOT assigned! Add a Collider2D with IsTrigger.");
        }

        timeSinceLastAttack = attackCooldown;

        PlayerDarya ownerPlayer = GetComponentInParent<PlayerDarya>();
        if (ownerPlayer != null)
            playerTransform = ownerPlayer.transform;
        else if (playerTransform == null && transform.parent != null)
            playerTransform = transform.parent;

        if (katanaPivot == null)
            katanaPivot = transform;

        cachedPlayer = ownerPlayer != null ? ownerPlayer : FindAnyObjectByType<PlayerDarya>();

        LogDamageDebug($"Initialized. ownerPlayer={(ownerPlayer != null ? ownerPlayer.name : "none")}, cachedPlayer={(cachedPlayer != null ? cachedPlayer.name : "none")}, hitCollider={(hitCollider != null ? hitCollider.name : "none")}, enemyLayerMask={enemyLayer.value}");
    }

    private void Update()
    {
        timeSinceLastAttack += Time.deltaTime;

        if (!isAttacking)
        {
            RotateTowardsNearestEnemy();

            if (timeSinceLastAttack >= attackCooldown && HasEnemyInRange())
            {
                Attack();
            }
        }
    }

    private bool HasEnemyInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        LogDamageDebug($"Range scan at {transform.position}: hits={hits.Length}, attackRange={attackRange}");
        foreach (var hit in hits)
        {
            if (TryGetDamageableEnemy(hit, out MonoBehaviour enemy))
            {
                LogDamageDebug($"Enemy in range: collider={hit.name}, enemy={enemy.name}, enemyType={enemy.GetType().Name}");
                return true;
            }

            LogDamageDebug($"Collider in enemy layer but no supported damage script: collider={hit.name}, root={hit.transform.root.name}");
        }
        return false;
    }

    private void RotateTowardsNearestEnemy()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange * 2, enemyLayer);
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (!TryGetDamageableEnemy(hit, out MonoBehaviour enemy))
                continue;

            float dist = Vector2.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy.transform;
            }
        }

        if (closest != null && katanaPivot != null)
        {
            Vector2 dir = (closest.position - katanaPivot.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            katanaPivot.localRotation = Quaternion.Euler(0, 0, angle);
        }
        else if (katanaPivot != null)
        {
            katanaPivot.localRotation = Quaternion.identity;
        }
    }

    private void Attack()
    {
        isAttacking = true;
        timeSinceLastAttack = 0f;
        damagedEnemies.Clear();

        LogDamageDebug($"Attack started. position={transform.position}, damage={(cachedPlayer != null ? Mathf.RoundToInt(cachedPlayer.CurrentDamage) : fallbackDamage)}, hitCollider={(hitCollider != null ? hitCollider.name : "none")}");
        PlayAttackSound();

        if (anim != null)
        {
            anim.SetTrigger("Attack");
            LogDamageDebug("Animator Attack trigger sent.");
        }
        else
        {
            LogDamageDebug("No Animator assigned.");
        }

        if (hitCollider != null)
        {
            hitCollider.enabled = true;
            LogDamageDebug($"Hitbox enabled. collider={hitCollider.name}, isTrigger={hitCollider.isTrigger}, bounds={hitCollider.bounds}");
            Invoke(nameof(DisableHitbox), 0.2f);
        }
        else
        {
            LogDamageDebug("Cannot enable hitbox because hitCollider is not assigned.");
        }

        Invoke(nameof(ResetAttackState), 0.5f);

        // Убрали ошибочный код отсюда
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        LogDamageDebug($"Trigger enter: other={other.name}, layer={LayerMask.LayerToName(other.gameObject.layer)}, isAttacking={isAttacking}");

        if (!isAttacking)
        {
            LogDamageDebug($"Ignored trigger because katana is not attacking: other={other.name}");
            return;
        }

        if (((1 << other.gameObject.layer) & enemyLayer) == 0)
        {
            LogDamageDebug($"Ignored trigger because layer is outside enemyLayer mask: other={other.name}, layer={LayerMask.LayerToName(other.gameObject.layer)}, mask={enemyLayer.value}");
            return;
        }

        if (!TryGetDamageableEnemy(other, out MonoBehaviour enemyScript))
        {
            LogDamageDebug($"Ignored trigger because no supported enemy damage script was found: other={other.name}, root={other.transform.root.name}");
            return;
        }

        if (damagedEnemies.Contains(enemyScript))
        {
            LogDamageDebug($"Ignored duplicate hit in same attack: enemy={enemyScript.name}, enemyType={enemyScript.GetType().Name}");
            return;
        }

        if (enemyScript != null)
        {
            int finalDamage = fallbackDamage; // Значение по умолчанию

            if (cachedPlayer != null)
            {
                // Берем текущий урон игрока с учетом баффов
                finalDamage = Mathf.RoundToInt(cachedPlayer.CurrentDamage);
            }

            enemyScript.SendMessage("TakeDamage", finalDamage, SendMessageOptions.DontRequireReceiver);
            damagedEnemies.Add(enemyScript);

            Debug.Log($"[Katana] HIT collider={other.name}, enemy={enemyScript.name}, enemyType={enemyScript.GetType().Name}, damage={finalDamage}");
        }
    }

    private bool TryGetDamageableEnemy(Collider2D hit, out MonoBehaviour enemy)
    {
        enemy = hit.GetComponentInParent<EnemyDarya>();
        if (enemy != null)
            return true;

        enemy = hit.GetComponentInParent<Enemy>();
        if (enemy != null)
            return true;

        enemy = hit.GetComponentInParent<EnemySkeletonOnionEnemy>();
        return enemy != null;
    }

    private void DisableHitbox()
    {
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
            LogDamageDebug($"Hitbox disabled. damagedEnemies={damagedEnemies.Count}");
        }
    }

    private void ResetAttackState()
    {
        LogDamageDebug($"Attack reset. damagedEnemies={damagedEnemies.Count}");
        isAttacking = false;
        damagedEnemies.Clear();
    }

    private void LogDamageDebug(string message)
    {
        if (!logDamageDebug)
            return;

        Debug.Log($"[KatanaDamageDebug] {message}", this);
    }

    private void PlayAttackSound()
    {
        if (attackSound == null)
            return;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.PlayOneShot(attackSound, attackSoundVolume);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange * 2);
    }
}
