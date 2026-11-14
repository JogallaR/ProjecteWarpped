using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathMenu : MonoBehaviour
{
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // Nom exacte de l'escena de menu

    private bool buttonsReady;

    public void Init(GameObject death)
    {
        deathPanel = death;
        buttonsReady = false;
    }

    public void FinishGame()
    {
        if (deathPanel == null) return;

        SetupButtons();
        deathPanel.SetActive(true);

        Time.timeScale = 0f; // Congela el temps i la fisica

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // El boto "Try Again" de l'escena no tenia cap accio assignada (o anava al menu).
    // Aqui li posem "tornar a jugar el nivell" i afegim un boto "Main Menu" al costat.
    private void SetupButtons()
    {
        if (buttonsReady || deathPanel == null) return;
        buttonsReady = true;

        Transform retryTr = deathPanel.transform.Find("RespawnButton");
        if (retryTr == null)
        {
            Button any = deathPanel.GetComponentInChildren<Button>(true);
            if (any != null) retryTr = any.transform;
        }
        if (retryTr == null) return;

        Button retry = retryTr.GetComponent<Button>();
        if (retry != null)
        {
            retry.onClick = new Button.ButtonClickedEvent();
            retry.onClick.AddListener(RetryLevel);
        }
        SetButtonText(retryTr, "TRY AGAIN");

        Transform menuTr = deathPanel.transform.Find("MainMenuButton");
        if (menuTr == null)
        {
            GameObject clone = Instantiate(retryTr.gameObject, retryTr.parent);
            clone.name = "MainMenuButton";
            menuTr = clone.transform;
        }

        RectTransform retryRt = retryTr as RectTransform;
        RectTransform menuRt = menuTr as RectTransform;
        if (retryRt != null && menuRt != null)
        {
            Vector2 basePos = retryRt.anchoredPosition;
            retryRt.anchoredPosition = basePos + new Vector2(-240f, 0f);
            menuRt.anchoredPosition = basePos + new Vector2(240f, 0f);
        }

        Button menu = menuTr.GetComponent<Button>();
        if (menu != null)
        {
            menu.onClick = new Button.ButtonClickedEvent();
            menu.onClick.AddListener(ReturnToMainMenu);
        }
        SetButtonText(menuTr, "MAIN MENU");
    }

    private static void SetButtonText(Transform button, string text)
    {
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = text;
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Imprescindible: tornar el temps a 1 abans de carregar el menu
        GameStats.fase = 1;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
