using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Controllers")]
    [SerializeField] private OptionsPanelController optionsPanelController;

    [Header("Blur")]
    [SerializeField] private PauseBlur pauseBlur;

    [Header("Escena")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;
    private bool isOptionsOpen;

    private void Start()
    {
        ResumeGame();
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isOptionsOpen)
            {
                // 1. Si estem dins de So o Controls -> Tornar al menú principal d'Opcions
                if (optionsPanelController != null && optionsPanelController.IsInSubmenu())
                {
                    optionsPanelController.BackToOptions();
                }
                // 2. Si ja estem al menú principal d'Opcions -> Tornar al menú de Pausa
                else
                {
                    BackToPauseMenu();
                }
            }
            else if (!isPaused)
            {
                // 3. Si el joc no està pausat -> Pausar el joc
                PauseGame();
            }
            else
            {
                // 4. Si estem al menú de Pausa -> Reprendre el joc
                ResumeGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        isOptionsOpen = false;

        pausePanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        if (pauseBlur != null)
            pauseBlur.BlurEnabled = true;

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        isOptionsOpen = false;

        pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);

        if (pauseBlur != null)
            pauseBlur.BlurEnabled = false;

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OpenOptions()
    {
        isPaused = true;
        isOptionsOpen = true;

        pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);

        if (pauseBlur != null)
            pauseBlur.BlurEnabled = true;

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void BackToPauseMenu()
    {
        isPaused = true;
        isOptionsOpen = false;

        if (optionsPanel != null) optionsPanel.SetActive(false);
        pausePanel.SetActive(true);

        if (pauseBlur != null)
            pauseBlur.BlurEnabled = true;

        Time.timeScale = 0f;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}