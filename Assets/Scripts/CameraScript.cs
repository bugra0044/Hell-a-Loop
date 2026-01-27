using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public Transform target; // Takip edilecek karakter
    public float smoothSpeed = 0.125f; // Takip yumuþaklýðý (0-1 arasý)
    public Vector3 offset = new Vector3(0, 2, -10); // Karakterin biraz üstünde ve gerisinde durmasý için

    [Header("Hýz Adaptasyonu")]
    public Rigidbody2D playerRb;
    public float lookAheadDistance = 2f; // Karakterin gittiði yöne doðru kameranýn kaymasý

    void LateUpdate() // Kamera takibi için her zaman LateUpdate kullanýlýr
    {
        if (target == null) return;

        // 1. Hedef pozisyonu belirle
        Vector3 desiredPosition = target.position + offset;

        // 2. "Look Ahead" - Karakter hýzlý gidiyorsa kamerayý o yöne kaydýr
        if (playerRb != null)
        {
            float horizontalOffset = playerRb.linearVelocity.x * 0.1f * lookAheadDistance;
            desiredPosition.x += horizontalOffset;
        }

        // 3. Yumuþak geçiþ (Lerp)
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // 4. Kamerayý hareket ettir
        transform.position = smoothedPosition;
    }
}