using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Saldiri Ayarlari")]
    public Transform attackPoint;      // Hitbox merkezi
    public Vector2 attackSize = new Vector2(2f, 1f); // Kutu hitbox boyutu
    public LayerMask enemyLayers;     // Dusman katmani
    public LayerMask buttonLayers;    // Buton katmani
    public int Damage = 1;

    [Header("Zamanlama")]
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;
    public float pogoForce = 15f;     // Pogo ziplama gucu
    
    [Header("Dinamik Hitbox")]
    public float speedReachMultiplier = 0.05f; // Hiz arttikca menzil ne kadar artsin?

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Saldiri bekleme suresi kontrolu
        if (Time.time >= nextAttackTime)
        {
            // Grapple Sol Tik (0), Saldiri Sag Tik (1)
            if (Input.GetMouseButtonDown(1)) 
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        int currentDamage = Damage;
        float currentCooldown = attackCooldown;
        CharacterMovement moveScript = GetComponent<CharacterMovement>();
        
        // PowerUp Logic
        if (moveScript != null && moveScript.activePowerUp != null && moveScript.activePowerUp.hasHammerTail)
        {
            currentCooldown = moveScript.activePowerUp.attackCooldown;
            currentDamage = moveScript.activePowerUp.Damage;
            float dashDirection = transform.localScale.x;
            GetComponent<Rigidbody2D>().AddForce(new Vector2(dashDirection * moveScript.activePowerUp.dashForce, 0), ForceMode2D.Impulse);
        }

        if (anim != null) anim.SetTrigger("Attack");

        nextAttackTime = Time.time + currentCooldown;

        // --- DINAMIK MENZIL HESABI ---
        Vector2 currentAttackSize = attackSize;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float speed = rb.linearVelocity.magnitude;
            // Hizimiza bagli olarak genisligi (x) artiriyoruz
            currentAttackSize.x += speed * speedReachMultiplier;
        }

        // 1. BUTONLARI ALGILA
        if (attackPoint != null)
        {
            Collider2D[] hitButtons = Physics2D.OverlapBoxAll(attackPoint.position, currentAttackSize, 0f, buttonLayers);
            foreach (var btnCol in hitButtons)
            {
                DoorButton btn = btnCol.GetComponent<DoorButton>();
                if (btn != null)
                {
                    btn.PressButton();
                }
            }

            // 2. DUSMANLARI ALGILA
            Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, currentAttackSize, 0f, enemyLayers);
            bool hitAnyEnemy = false;

            // 3. Her bir dusmana hasar ver
            foreach (Collider2D enemy in hitEnemies)
            {
                Enemy enemyScript = enemy.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    enemyScript.TakeDamage(currentDamage);
                    hitAnyEnemy = true;
                }
            }

            // 4. POGO MOVEMENT
            if (hitAnyEnemy)
            {
                // rb already grabbed above
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
                    rb.AddForce(Vector2.up * pogoForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    // Hitbox'i editor'de ciz
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        
        Vector2 currentGizmoSize = attackSize;
        
        // Eger oyun calisiyorsa hiz etkisini goster
        if (Application.isPlaying) 
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                 float speed = rb.linearVelocity.magnitude;
                 currentGizmoSize.x += speed * speedReachMultiplier;
            }
        }
        
        Gizmos.DrawWireCube(attackPoint.position, currentGizmoSize);
    }
}