using UnityEngine;

public class LoopLauncher : MonoBehaviour
{
    [Header("Fırlatma Ayarları")]
    public float launchForce = 25f;
    public bool fixRotation = true; // Eğer işaretliyse karakteri düzeltir (0 dereceye çeker/resetler)

    // Not: Fırlatma yönü bu objenin "Yeşil Oku" (Transform.up) yönüdür.
    // Sahneye koyduğunuzda objeyi fırlatmak istediğiniz yöne çevirin.

    private void OnTriggerEnter2D(Collider2D other)
    {
        CharacterMovement player = other.GetComponent<CharacterMovement>();
        if (player != null)
        {
            // Fırlatma yönünü hesapla
            Vector2 launchDir = transform.up;
            Vector2 finalVelocity = launchDir * launchForce;

            // Oyuncuya uygula
            player.ApplyExternalLaunch(finalVelocity, fixRotation);
        }
    }
}
