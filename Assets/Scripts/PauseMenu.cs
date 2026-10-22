
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private PauseBlur pauseBlur;

    private bool isPaused;

    private void Start()
    {
        pausePanel.SetActive(false);
        pauseBlur.BlurEnabled = false;

        Time.timeScale = 1f;
        isPaused = false;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (Keyboard.current != null &&
    Keyboard.current.escapeKey.wasPressedThisFrame)
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
        pauseBlur.BlurEnabled = true;

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        pauseBlur.BlurEnabled = false;

        Time.timeScale = 1f;
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
