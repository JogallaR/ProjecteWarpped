using UnityEngine;

public class OptionsPanelController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainOptionsPanel;
    [SerializeField] private GameObject soundPanel;
    [SerializeField] private GameObject controlsPanel;

    private void OnEnable()
    {
        ResetPanels();
    }

    public void ResetPanels()
    {
        if (mainOptionsPanel != null) mainOptionsPanel.SetActive(true);
        if (soundPanel != null) soundPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // Comprova si qualsevol submenú (So o Controls) està actualment obert
    public bool IsInSubmenu()
    {
        bool isSoundActive = soundPanel != null && soundPanel.activeSelf;
        bool isControlsActive = controlsPanel != null && controlsPanel.activeSelf;

        return isSoundActive || isControlsActive;
    }
    
    public void OpenSoundPanel()
    {
        if (mainOptionsPanel != null) mainOptionsPanel.SetActive(false);
        if (soundPanel != null) soundPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void OpenControlsPanel()
    {
        if (mainOptionsPanel != null) mainOptionsPanel.SetActive(false);
        if (soundPanel != null) soundPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void BackToOptions()
    {
        ResetPanels();
    }
}