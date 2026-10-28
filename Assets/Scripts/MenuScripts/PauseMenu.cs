using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Afegit per carregar el Main Menu

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private PauseBlur pauseBlur;
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Nom exacte de l'escena de menú

    private bool isPaused;

    private void Start()
    {
        // Ens assegurem que el joc comença despausat
        ResumeGame();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        
        if (pauseBlur != null) pauseBlur.BlurEnabled = true;

        Time.timeScale = 0f; // Congela el temps i la física
        
        // Mostrar i alliberar el cursor per navegar pel menú de pausa
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);

        if (pauseBlur != null) pauseBlur.BlurEnabled = false;

        Time.timeScale = 1f; // Reprèn la física i el temps
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Imprescindible: tornar el temps a 1 abans de carregar el menú
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}