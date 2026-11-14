using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    // Altres scripts (PlayerMover, CentralPlayer...) ho consulten per no llegir tecles mentre hi ha pausa
    public static bool IsPaused { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Controllers")]
    [SerializeField] private OptionsPanelController optionsPanelController;

    [Header("Blur")]
    [SerializeField] private PauseBlur pauseBlur;

    [Header("Escena")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Ordre de dibuix del canvas del menu (per sobre del HUD)")]
    [SerializeField] private int menuCanvasSortingOrder = 100;

    private bool isPaused;
    private bool isOptionsOpen;
    private bool hudHidden;
    private readonly List<Canvas> hiddenHudCanvases = new List<Canvas>();

    private void Awake()
    {
        // El canvas del menu de pausa sempre per sobre de qualsevol altre (HUD inclos)
        Canvas menuCanvas = pausePanel != null ? pausePanel.GetComponentInParent<Canvas>(true) : null;
        if (menuCanvas != null)
        {
            menuCanvas = menuCanvas.rootCanvas;
            menuCanvas.sortingOrder = Mathf.Max(menuCanvas.sortingOrder, menuCanvasSortingOrder);
        }
    }

    private void Start()
    {
        ResumeGame();
    }

    private void OnDestroy()
    {
        SetHudHidden(false);
        IsPaused = false;
    }

    private bool BelongsToMenu(Canvas c)
    {
        if (pausePanel != null && pausePanel.transform.IsChildOf(c.transform)) return true;
        if (optionsPanel != null && optionsPanel.transform.IsChildOf(c.transform)) return true;
        if (transform.IsChildOf(c.transform)) return true;
        return false;
    }

    // Amaga tots els canvas del HUD (botons d'atac, mana, vida, controls...) mentre el menu de pausa es obert
    private void SetHudHidden(bool hidden)
    {
        hudHidden = hidden;

        if (hidden)
        {
            Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in canvases)
            {
                if (c == null || !c.enabled || !c.isRootCanvas) continue;
                if (c.renderMode == RenderMode.WorldSpace) continue;
                if (BelongsToMenu(c)) continue;

                c.enabled = false;
                if (!hiddenHudCanvases.Contains(c)) hiddenHudCanvases.Add(c);
            }
        }
        else
        {
            foreach (Canvas c in hiddenHudCanvases)
            {
                if (c != null) c.enabled = true;
            }
            hiddenHudCanvases.Clear();
        }
    }

    private void Update()
    {
        // Si algun script torna a activar el HUD mentre som en pausa, el tornem a amagar
        if (hudHidden)
        {
            foreach (Canvas c in hiddenHudCanvases)
            {
                if (c != null && c.enabled) c.enabled = false;
            }
        }

        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        // La pantalla de reassignar tecles gestiona ella mateixa l'Esc
        if (GameSettings.EscapeConsumedByUi)
            return;

        if (isOptionsOpen)
        {
            // 1. Si estem dins de So o Controls -> Tornar al menu principal d'Opcions
            if (optionsPanelController != null && optionsPanelController.IsInSubmenu())
            {
                optionsPanelController.BackToOptions();
            }
            // 2. Si ja estem al menu principal d'Opcions -> Tornar al menu de Pausa
            else
            {
                BackToPauseMenu();
            }
        }
        else if (!isPaused)
        {
            // 3. Si el joc no esta pausat -> Pausar el joc
            PauseGame();
        }
        else
        {
            // 4. Si estem al menu de Pausa -> Reprendre el joc
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        IsPaused = true;
        isOptionsOpen = false;

        pausePanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (optionsPanelController != null) optionsPanelController.CloseAll();
        SetHudHidden(true);

        if (pauseBlur != null)
            pauseBlur.BlurEnabled = true;

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        IsPaused = false;
        isOptionsOpen = false;

        pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (optionsPanelController != null) optionsPanelController.CloseAll();
        SetHudHidden(false);

        if (pauseBlur != null)
            pauseBlur.BlurEnabled = false;

        Time.timeScale = 1f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void OpenOptions()
    {
        isPaused = true;
        IsPaused = true;
        isOptionsOpen = true;

        pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (optionsPanelController != null) optionsPanelController.ResetPanels();
        SetHudHidden(true);

        if (pauseBlur != null)
            pauseBlur.BlurEnabled = true;

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void BackToPauseMenu()
    {
        isPaused = true;
        IsPaused = true;
        isOptionsOpen = false;

        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (optionsPanelController != null) optionsPanelController.CloseAll();
        pausePanel.SetActive(true);
        SetHudHidden(true);

        if (pauseBlur != null)
            pauseBlur.BlurEnabled = true;

        Time.timeScale = 0f;
    }

    public void ReturnToMainMenu()
    {
        SetHudHidden(false);
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        Application.Quit();
    }
}
