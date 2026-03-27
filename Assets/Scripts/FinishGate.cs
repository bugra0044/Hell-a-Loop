using UnityEngine;
using UnityEngine.SceneManagement; // Scene reload icin gerekirse

public class FinishGate : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject winPanel; // Kazandin Paneli

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Oyuncu bitise geldi mi?
        if (other.CompareTag("Player") || other.GetComponent<CharacterMovement>() != null)
        {
            FinishLevel();
        }
    }

    void FinishLevel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f; // Oyunu durdur
        }
    }

    // Bu fonksiyonu UI Button'in OnClick() olayina baglayin
    public void QuitGame()
    {
        Debug.Log("Oyundan Cikiliyor...");
        Application.Quit();
    }

    // Opsiyonel: Yeniden baslatma butonu lazim olursa
   
}
