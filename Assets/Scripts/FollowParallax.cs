using UnityEngine;

public class FollowParallax : MonoBehaviour
{
    public Transform player;      // Oyuncu referansı
    public float smoothSpeed = 5f; // Takip yumuşaklığı
    public float moveThreshold = 2f; // Oyuncu merkezden ne kadar uzaklaşınca arkaplan hareket etsin?
    public Vector3 lastPlayerPosition;
    [Header("Layer Settings")]
    public Transform[] layers;      // Katmanların listesi
    public float[] parallaxFactors; // Her katman için hız çarpanı (Uzak olan küçük, yakın olan büyük)

    private float[] layerWidths; // Her katmanın genişliğini saklar

    void Start()
    {
        lastPlayerPosition = player.position;
        
        // Otomatik Genişlik Hesaplama
        layerWidths = new float[layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            SpriteRenderer sr = layers[i].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                layerWidths[i] = sr.bounds.size.x;
            }
            else
            {
                // SpriteRenderer yoksa varsayılan bir değer ata (veya hata vermemesi için 0 yapma)
                Debug.LogWarning($"Parallax Katmanı {layers[i].name} üzerinde SpriteRenderer bulunamadı! Döngü çalışmayabilir.");
                layerWidths[i] = 40f; // Tahmini bir genişlik
            }
        }
    }

    void Update()
    {
        // 1. ANA GRUP TAKİBİ: Oyuncu sınıra yaklaşınca tüm grubu kaydır
        float distance = Vector3.Distance(transform.position, player.position);
        
        if (distance > moveThreshold)
        {
            Vector3 targetPosition = new Vector3(player.position.x, player.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }

        // 2. İÇ KATMAN PARALLAX ve DÖNGÜ (INFINITE SCROLL)
        Vector3 deltaMovement = player.position - lastPlayerPosition;
        
        for (int i = 0; i < layers.Length; i++)
        {
            // A. Parallax Hareketi
            Vector3 layerOffset = new Vector3(deltaMovement.x * parallaxFactors[i], deltaMovement.y * parallaxFactors[i], 0);
            layers[i].localPosition += layerOffset;

            // B. Sonsuz Döngü (Wrapping) Kontrolü
            // Eğer katman merkezden (parent'tan) kendi genişliğinin yarısından fazla uzaklaştıysa başa sar
            float width = layerWidths[i];
            if (width > 0)
            {
                // Sağa çok gittiyse sola at
                if (layers[i].localPosition.x > width) 
                {
                    layers[i].localPosition -= new Vector3(width, 0, 0);
                }
                // Sola çok gittiyse sağa at
                else if (layers[i].localPosition.x < -width) 
                {
                    layers[i].localPosition += new Vector3(width, 0, 0);
                }
            }
        }

        lastPlayerPosition = player.position;
    }
}