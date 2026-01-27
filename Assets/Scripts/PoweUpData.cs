using UnityEngine;

[CreateAssetMenu(fileName = "New PowerUp", menuName = "Game/PowerUp")]
public class PowerUpData : ScriptableObject
{
    public string powerUpName;
    public Sprite icon;

    [Header("Hareket Ayarlarý")]
    public float speedMultiplier = 1f;      // Hýz çarpaný
    public float jumpMultiplier = 1f;       // Zýplama çarpaný
    public float machRateMultiplier = 1f;   // Mach artýþ hýzý çarpaný
    public float dashForce = 10f;

    [Header("Saldýrý Ayarlarý")]
    public float attackCooldown = 0.5f; // Ýki saldýrý arasý bekleme süresi (Saniye)
    public int Damage = 1;

    [Header("Özel Yetenekler")]
    public bool canDoubleJump = false;      // Jordan Air için
    public bool hasHammerTail = false;      // Çekiç Kuyruk için
    public bool isGhostMode = false;        // Fýrtýnanýn Gözü için
    public bool canReachPhase5 = false;
}