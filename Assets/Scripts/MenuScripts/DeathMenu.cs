using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // Afegit per carregar el Main Menu

public class DeathMenu : MonoBehaviour
{
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Nom exacte de l'escena de menú


    private void Start()
    {
    }

    private void Update()
    {
    }

    public void Init(GameObject death)
    {
        deathPanel = death;
    }

    public void FinishGame()
    {
        deathPanel.SetActive(true);
        
        Time.timeScale = 0f; // Congela el temps i la física
        
        // Mostrar i alliberar el cursor per navegar pel menú de pausa
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Imprescindible: tornar el temps a 1 abans de carregar el menú
        SceneManager.LoadScene(mainMenuSceneName);
        print("Death resp");
    }
}