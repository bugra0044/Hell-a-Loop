using UnityEngine;

public class BreakableIce : MonoBehaviour
{
    public GameObject breakParticles; // Parçalanma efekti
    public AudioClip breakSound;      // Ses

    // Bu fonksiyonu artýk CharacterMovement scripti uzaktan çaðýracak
    public void Break()
    {
        // 1. Önce Collider'ý kapat ki fizik motoru o karede bile çarpmasýn
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Efektler
        if (breakParticles != null)
            Instantiate(breakParticles, transform.position, Quaternion.identity);

        // Yok et
        Destroy(gameObject);
    }
}