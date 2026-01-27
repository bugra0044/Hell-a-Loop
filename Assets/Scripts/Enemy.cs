using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Düþman Ýstatistikleri")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Görsel Efekt (Opsiyonel)")]
    public Color damageColor = Color.red;
    private Color originalColor;
    private SpriteRenderer sprite;

    void Start()
    {
        currentHealth = maxHealth;
        sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) originalColor = sprite.color;
    }

    // PlayerAttack scriptinden çaðrýlacak fonksiyon
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log(gameObject.name + " hasar aldý! Kalan Can: " + currentHealth);

        // Hasar alma efekti (Görsel geri bildirim)
        if (sprite != null) Invoke("ResetColor", 0.1f);
        if (sprite != null) sprite.color = damageColor;

        // Can sýfýra düþerse
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void ResetColor()
    {
        sprite.color = originalColor;
    }

    void Die()
    {
        Debug.Log(gameObject.name + " öldü!");
        // Burada ölüm animasyonunu tetikleyebilirsin
        Destroy(gameObject);
    }
}