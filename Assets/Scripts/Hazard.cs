using UnityEngine;
using UnityEngine.SceneManagement;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckAndRestart(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CheckAndRestart(collision.gameObject);
    }

    void CheckAndRestart(GameObject obj)
    {
        // Oyuncu mu?
        if (obj.CompareTag("Player") || obj.GetComponent<CharacterMovement>() != null)
        {
            RestartLevel();
        }
    }

    void RestartLevel()
    {
        // Mevcut sahneyi bastan yukle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
