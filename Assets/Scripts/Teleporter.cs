using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Hedef")]
    public Transform destination; // Isinlanilacak yer

    [Header("Ayarlar")]
    public bool keepMomentum = true; // Hizini korusun mu? (False ise sifirlar)

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece oyuncuyu isinla
        if (other.CompareTag("Player") || other.GetComponent<CharacterMovement>() != null)
        {
            if (destination != null)
            {
                Teleport(other.transform);
            }
        }
    }

    void Teleport(Transform playerTransform)
    {
        // 1. Pozisyonu degistir
        playerTransform.position = destination.position;

        // 2. Momentum ayari
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null && !keepMomentum)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Opsiyonel: Isinlanma efekti/sesi eklenebilir
    }
}
