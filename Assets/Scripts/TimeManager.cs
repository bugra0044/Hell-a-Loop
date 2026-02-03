using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    [Header("Zaman Ayarlar�")]
    public float totalTime = 180f;
    private float currentTime;

    [Header("I��nlanma Ayarlar�")]
    public Transform playerTransform;
    public Vector3 spawnPoint;

    [Header("UI Elemanlar�")]
    public TextMeshProUGUI timerText;

    private bool isGameActive = false;

    void Start()
    {
        spawnPoint = playerTransform.position;
        UpdateTimerUI();
    }

    void Update()
    {
        if (isGameActive && currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            UpdateTimerUI();
        }
        else if (isGameActive && currentTime <= 0)
        {
            ResetLoop();
        }
    }

    public void StartGame()
    {
        currentTime = totalTime;
        isGameActive = true;
        UpdateTimerUI();
    }

    public void TakeDamage(float timePenalty)
    {
        currentTime -= timePenalty;
        Debug.Log("Hasar al�nd�! S�reden giden: " + timePenalty);
    }

    void ResetLoop()
    {
        playerTransform.position = spawnPoint;

        currentTime = totalTime;

        // �ste�e ba�l�: H�z� s�f�rla ki karakter u�arak ba�lamas�n
        Rigidbody2D rb = playerTransform.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        Debug.Log("S�re bitti! D�ng� ba�a d�nd�.");
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
