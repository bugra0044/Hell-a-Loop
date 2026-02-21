using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    private float length, startPos;
    public GameObject cam;
    [Tooltip("0: Kamera ile aynı hız (Sonsuz uzak), 1: Sabit (En yakın)")]
    public float parallaxEffect;

    void Start()
    {
        startPos = transform.position.x;
        // Görselin genişliğini alıyoruz (Arkaplanın kendini tekrar etmesi için)
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        // Kameranın ne kadar hareket ettiğini hesapla
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);
        // Katmanı hareket ettir
        transform.position = new Vector3(startPos + dist, transform.position.y, transform.position.z);

        // Sonsuz döngü (Arkaplan bittikçe başa sarar)
        if (temp > startPos + length) startPos += length;
        else if (temp < startPos - length) startPos -= length;
    }
}