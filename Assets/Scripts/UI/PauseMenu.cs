using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public bool gameIsPaused = false;

    [SerializeField] 
    private GameObject pauseMenuUI;
    
    private void Start()
    {
        gameIsPaused = false;
        pauseMenuUI.SetActive(false);
    }
    
    public void TogglePause()
    {
        Debug.Log(gameIsPaused);
        gameIsPaused = !gameIsPaused;
        Time.timeScale = gameIsPaused ? 0 : 1;
        pauseMenuUI.SetActive(gameIsPaused);
    }
}
