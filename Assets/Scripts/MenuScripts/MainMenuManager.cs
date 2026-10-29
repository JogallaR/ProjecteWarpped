
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panells de la UI")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject characterSelectPanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Objectes 3D")]
    [SerializeField] private GameObject characterPreview;

    [SerializeField] private CharacterSelector characterSelector;

    [Header("Configuració")]
    [SerializeField] private string gameSceneName = "GameScene";

    private void Start()
    {
        ShowMainMenu();
        Time.timeScale = 1f;
    }

    // --- NAVEGACIÓ ENTRE PANELLS ---

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }

        if (characterPreview != null)
        {
            characterPreview.SetActive(false);
        }
    }

    public void OpenNewGameSubmenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(true);
        }

        if (characterPreview != null)
        {
            characterPreview.SetActive(true);
        }

        // Inicialitzem la selecció només en obrir New Game.
        if (characterSelector != null)
        {
            characterSelector.InitializeCharacterSelection();
        }
        else
        {
            Debug.LogError(
                "No has assignat CharacterSelector al MainMenuManager!",
                this
            );
        }
    }


    public void OpenOptionsSubmenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        if (characterSelectPanel != null)
        {
            characterSelectPanel.SetActive(false);
        }

        if (characterPreview != null)
        {
            characterPreview.SetActive(false);
        }

        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("El panell d'opcions no està assignat!", this);
        }
    }

    // --- ACCIONS DE JOC ---

    public void StartGame()
    {
        Debug.Log("Començant partida...");

        SceneManager.LoadScene(gameSceneName);
    }

    public void ResumeGame()
    {
        Debug.Log("Carregant última partida desada...");
    }

    public void QuitGame()
    {
        Debug.Log("Tancant el joc...");

        Application.Quit();
    }
}
