using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryMenu : MonoBehaviour
{
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string nextPhaseSceneName = "SecondLevel";

    private Button mainMenuButton;
    private Button nextPhaseButton;

    private void Start()
    {
        EnsureChoiceButtons();
    }

    public void Win()
    {
        EnsureChoiceButtons();

        int t = Mathf.FloorToInt(Time.timeSinceLevelLoad);
        if (statsText)
        {
            statsText.text = $"Phase: {GameStats.fase}\n" +
                             $"Time: {t / 60:00}:{t % 60:00}\n" +
                             $"Turns: {GameStats.torns}\n" +
                             $"Steps: {GameStats.passos}\n" +
                             $"Damage taken: {GameStats.malRebut}\n" +
                             $"Heals: {GameStats.cures}";
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        GameAudio.Play(Sfx.Victory, 1f, 0f);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void EnsureChoiceButtons()
    {
        if (victoryPanel == null) return;

        Transform menuBtnTr = victoryPanel.transform.Find("RespawnButton") ?? victoryPanel.transform.Find("MainMenuButton");
        if (menuBtnTr == null)
        {
            Button existingBtn = victoryPanel.GetComponentInChildren<Button>(true);
            if (existingBtn != null) menuBtnTr = existingBtn.transform;
        }

        if (menuBtnTr == null) return;

        mainMenuButton = menuBtnTr.GetComponent<Button>();
        RectTransform menuRt = menuBtnTr.GetComponent<RectTransform>();
        if (menuRt != null)
        {
            menuRt.anchoredPosition = new Vector2(-240f, 150f);
        }

        TMP_Text menuTxt = menuBtnTr.GetComponentInChildren<TMP_Text>(true);
        if (menuTxt != null)
        {
            menuTxt.text = "MAIN MENU";
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick = new Button.ButtonClickedEvent();
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        Transform nextBtnTr = victoryPanel.transform.Find("NextPhaseButton");
        if (nextBtnTr == null)
        {
            GameObject clone = Instantiate(menuBtnTr.gameObject, victoryPanel.transform);
            clone.name = "NextPhaseButton";
            nextBtnTr = clone.transform;
        }

        RectTransform nextRt = nextBtnTr.GetComponent<RectTransform>();
        if (nextRt != null)
        {
            nextRt.anchoredPosition = new Vector2(240f, 150f);
        }

        TMP_Text nextTxt = nextBtnTr.GetComponentInChildren<TMP_Text>(true);
        if (nextTxt != null)
        {
            nextTxt.text = "NEXT PHASE";
        }

        nextPhaseButton = nextBtnTr.GetComponent<Button>();
        if (nextPhaseButton != null)
        {
            if (nextTxt != null) nextPhaseButton.targetGraphic = nextTxt;
            nextPhaseButton.onClick = new Button.ButtonClickedEvent();
            nextPhaseButton.onClick.AddListener(ContinueToNextPhase);
        }
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        GameStats.fase = 1;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ContinueToNextPhase()
    {
        Time.timeScale = 1f;
        GameStats.fase++;

        string currentScene = SceneManager.GetActiveScene().name;
        string targetScene = nextPhaseSceneName;

        if (string.IsNullOrEmpty(targetScene) || currentScene == targetScene)
        {
            targetScene = currentScene == "FirstLevel" ? "SecondLevel" : "FirstLevel";
        }

        if (Application.CanStreamedLevelBeLoaded(targetScene))
        {
            SceneManager.LoadScene(targetScene);
        }
        else
        {
            SceneManager.LoadScene(currentScene);
        }
    }
}