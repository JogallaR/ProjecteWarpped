using TMPro;
using UnityEngine;

public class OptionsPanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainOptionsPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject controlsPanel;

    private bool initialized;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        // Nomes tanquem els submenus: el panell principal l'obre el PauseMenu / MainMenuManager
        if (soundPanel != null) soundPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // Es pot cridar encara que aquest GameObject estigui desactivat (els botons ho fan)
    private void Initialize()
    {
        if (initialized) return;
        initialized = true;

        // A l'escena del joc el camp "Main Options Panel" estava buit: el busquem pel nom
        if (mainOptionsPanel == null)
        {
            Transform parent = soundPanel != null ? soundPanel.transform.parent
                : controlsPanel != null ? controlsPanel.transform.parent : null;
            Transform found = parent != null ? parent.Find("OptionsPanel") : null;
            if (found != null) mainOptionsPanel = found.gameObject;
        }

        // El GameObject "ControlsPanel" tenia un text "OPTIONS" propi que quedava sota el titol "CONTROLS"
        HideOwnRootText(controlsPanel);
        HideOwnRootText(soundPanel);

        // Afegeix els controls configurables (velocitat de camera, invertir eixos, tecles)
        if (controlsPanel != null && controlsPanel.GetComponent<ControlsSettingsUI>() == null)
            controlsPanel.AddComponent<ControlsSettingsUI>();
    }

    private static void HideOwnRootText(GameObject panel)
    {
        if (panel == null) return;
        TMP_Text rootText = panel.GetComponent<TMP_Text>();
        if (rootText != null) rootText.enabled = false;
    }

    public void ResetPanels()
    {
        Initialize();
        if (mainOptionsPanel != null) mainOptionsPanel.SetActive(true);
        if (soundPanel != null) soundPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // Tanca opcions, so i controls. El fa servir el menu de pausa
    public void CloseAll()
    {
        Initialize();
        if (mainOptionsPanel != null) mainOptionsPanel.SetActive(false);
        if (soundPanel != null) soundPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // Comprova si qualsevol submenu (So o Controls) esta actualment obert
    public bool IsInSubmenu()
    {
        bool isSoundActive = soundPanel != null && soundPanel.activeSelf;
        bool isControlsActive = controlsPanel != null && controlsPanel.activeSelf;

        return isSoundActive || isControlsActive;
    }

    public void OpenSoundPanel()
    {
        Initialize();
        if (mainOptionsPanel != null) mainOptionsPanel.SetActive(false);
        if (soundPanel != null) soundPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void OpenControlsPanel()
    {
        Initialize();
        if (mainOptionsPanel != null) mainOptionsPanel.SetActive(false);
        if (soundPanel != null) soundPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void BackToOptions()
    {
        ResetPanels();
    }
}
