using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject targetDoor; // Açılacak kapı (Direkt Obje)
    public Sprite pressedSprite;  // Basılınca görünecek görsel (Opsiyonel)
    public bool destroyOnPress = true; // Basılınca buton yok olsun mu?

    private bool isPressed = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // if (isPressed) return; // Debug icin bu satiri gecici kapatiyorum ki surekli gorelim
        
        Debug.Log($"Button Trigger: {other.name} - Tag: {other.tag}");

        // Oyuncu (CharacterMovement) veya Saldiri (PlayerAttack) objesi değerse
        // GetComponentInParent kullaniyoruz ki child objeler (orn: Kilic, Ayak) carpinca da algilasin
        if (other.GetComponentInParent<CharacterMovement>() != null)
        {
            Debug.Log("Player Detected inside Button!");
            PressButton();
        }
    }
    public void PressButton() // Made public for PlayerAttack to call
    {
        Debug.Log("PressButton called!"); // Debug Check
        isPressed = true;
        
        // Görsel değiştir
        if (pressedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = pressedSprite;
        }

        // Kapıyı aç/kapat (Toggle)
        if (targetDoor != null)
        {
            // Eğer kapalıysa (inactive) -> aç
            // Eğer açıksa (active) -> kapat
            targetDoor.SetActive(!targetDoor.activeSelf);
        }

        // Butonun kendisini yok et
        if (destroyOnPress)
        {
            Destroy(gameObject, 0.1f); // Hafif gecikmeli yok olsun
        }
    }
}
