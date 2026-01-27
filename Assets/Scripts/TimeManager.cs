using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    [Header("Zaman Ayarlarý")]
    public float totalTime = 180f;
    private float currentTime;

    [Header("Iþýnlanma Ayarlarý")]
    public Transform playerTransform;
    public Vector3 spawnPoint;

    [Header("UI Elemanlarý")]
    public TextMeshProUGUI timerText;

    void Start()
    {
        currentTime = totalTime;
        spawnPoint = playerTransform.position;
    }

    void Update()
    {
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            ResetLoop();
        }
    }

    public void TakeDamage(float timePenalty)
    {
        currentTime -= timePenalty;
        Debug.Log("Hasar alýndý! Süreden giden: " + timePenalty);
    }

    void ResetLoop()
    {
        playerTransform.position = spawnPoint;

        currentTime = totalTime;

        // Ýsteðe baðlý: Hýzý sýfýrla ki karakter uçarak baþlamasýn
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Debug.Log("Süre bitti! Döngü baþa döndü.");
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
