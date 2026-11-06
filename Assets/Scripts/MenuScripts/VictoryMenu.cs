using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryMenu : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void Win()
    {
        int t = Mathf.FloorToInt(Time.timeSinceLevelLoad);
        if (statsText)
            statsText.text = $"Temps: {t / 60:00}:{t % 60:00}\n" +
                             $"Torns: {GameStats.torns}\n" +
                             $"Passos: {GameStats.passos}\n" +
                             $"Mal rebut: {GameStats.malRebut}\n" +
                             $"Cures: {GameStats.cures}";

        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}