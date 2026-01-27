using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class PowerUpPickup : MonoBehaviour
{
    [Header("Bu obje hangi gücü veriyor?")]
    public PowerUpData powerUpData; // Buraya CekicKuyruk veya JordanAir dosyasýný sürükleyeceksin

    [Header("Görsel Ayarlar")]
    public float floatSpeed = 2f; // Yukarý aþaðý süzülme hýzý
    public float floatHeight = 0.2f; // Süzülme mesafesi

    private Vector3 startPos;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPos = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        GetComponent<BoxCollider2D>().isTrigger = true; // Trigger olduðundan emin olalým

        // Objeye resim atamayý unutursan diye, ScriptableObject'teki ikonu otomatik kullanýr
        if (powerUpData != null && powerUpData.icon != null)
        {
            spriteRenderer.sprite = powerUpData.icon;
        }
    }

    void Update()
    {
        // Basit bir süzülme animasyonu (Idle Floating)
        float newY = startPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece "Player" tagine sahip obje alabilir
        if (other.CompareTag("Player"))
        {
            // Oyuncunun hareket scriptine ulaþ
            CharacterMovement playerMovement = other.GetComponent<CharacterMovement>();

            if (playerMovement != null)
            {
                // ESKÝYÝ SÝL, YENÝYÝ ATA:
                // Sadece deðiþkeni deðiþtiriyoruz, eski güç otomatik olarak devreden çýkar.
                playerMovement.activePowerUp = powerUpData;

                Debug.Log(powerUpData.powerUpName + " gücü alýndý!");

                // Efekt veya ses eklenecekse buraya yazýlýr (PlaySound, Instantiate Particle vs.)

                // Objeyi sahneden yok et
                Destroy(gameObject);
            }
        }
    }
}