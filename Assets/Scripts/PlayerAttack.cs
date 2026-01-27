using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Saldýrý Ayarlarý")]
    public Transform attackPoint;      // Hitbox'ýn merkezi (Karakterin önünde boþ bir obje)
    public Vector2 attackSize = new Vector2(2f, 1f); // Kutu hitbox'ýn boyutu
    public LayerMask enemyLayers;     // Hangi katmandaki objeler hasar alacak?
    public int Damage=1;

    [Header("Zamanlama")]
    public float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Sað týk kontrolü ve Cooldown (Bekleme süresi)
        if (Time.time >= nextAttackTime)
        {
            if (Input.GetMouseButtonDown(1)) // 1 = Sað týk
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        int currentDamage = Damage;
        float currentCooldown = attackCooldown;
        CharacterMovement moveScript=GetComponent<CharacterMovement>();
        if (moveScript != null && moveScript.activePowerUp != null && moveScript.activePowerUp.hasHammerTail)
        {
            currentCooldown = moveScript.activePowerUp.attackCooldown;
            currentDamage = moveScript.activePowerUp.Damage;
            float dashDirection = transform.localScale.x;
            GetComponent<Rigidbody2D>().AddForce(new Vector2(dashDirection * moveScript.activePowerUp.dashForce, 0), ForceMode2D.Impulse);
        }

        if (anim != null) anim.SetTrigger("Attack");

        nextAttackTime = Time.time + currentCooldown;


        // 2. Hitbox içindeki düþmanlarý algýla
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackSize, 0f, enemyLayers);

        // 3. Her bir düþmana hasar ver
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Hasar vverildi");
            Enemy enemyScript = enemy.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                enemyScript.TakeDamage(currentDamage); // 1 birim hasar ver
            }
        }
    }

    // Editörde hitbox'ý görebilmemiz için (Gizmos)
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(attackPoint.position, attackSize);
    }
}