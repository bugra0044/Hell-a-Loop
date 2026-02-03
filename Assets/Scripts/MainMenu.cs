using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject creditsMenu;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject GameTimerTxt;
    public TimeManager timeManager;
    
    void Start()
    {
        Time.timeScale = 0f; // Pause game on start
    }
    
    public void PlayGame()
    {
        Time.timeScale = 1f; // Resume game
        if (timeManager != null)
        {
            timeManager.StartGame();
            creditsMenu.SetActive(false);
            mainMenu.SetActive(false);
            GameTimerTxt.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false); // Fallback: disable self if no panel assigned
        }
    }
    public void ToggleCredits(bool isMenuOpen)
    {
        if (isMenuOpen)
        {
            creditsMenu.SetActive(true);
            mainMenu.SetActive(false);
        }
        else
        {
            creditsMenu.SetActive(false);
            mainMenu.SetActive(true);
        }
        
    }
}
