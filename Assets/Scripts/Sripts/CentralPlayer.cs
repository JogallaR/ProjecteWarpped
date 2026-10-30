using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CentralPlayer : MonoBehaviour
{
    public int vida = 20;
    public int maxVida = 20;
    public bool inSettings = false;

    [SerializeField] PlayerMover mover;
    [SerializeField] TMP_Text textHealth;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] DeathMenu deathMenu;
    [SerializeField] GameObject deathPlane;

    void Awake()
    {
        if (!mover) mover = GetComponentInChildren<PlayerMover>();
    }

    public void Init(TMP_Text healthText, GameObject settings, DeathMenu menu, GameObject death)
    {
    textHealth = healthText;
    settingsPanel = settings;
    deathMenu = menu;
    deathPlane = death;
    }

    void Start()
    {
        mover.enabled = true;
        try {
            deathMenu.Init(deathPlane);
        }
        catch
        {
            print("Error on death plane");
        }
        ;
        SetSettings(false);
        UpdateUI();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || mover == null) return;

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            SetSettings(!inSettings);
            return;
        }

        if (inSettings) return;

        if (keyboard.fKey.wasPressedThisFrame)
        {
            TakeDamage(5);
            return;
        }

        if (keyboard.hKey.wasPressedThisFrame)
        {
            Heal(1);
            return;
        }

        if (keyboard.enterKey.wasPressedThisFrame && !mover.IsMoving)
            mover.EndTurn();
    }

    void SetSettings(bool open)
    {
        inSettings = open;
        if (mover) mover.enabled = !open;
        if (settingsPanel) settingsPanel.SetActive(open);
    }

    public void TakeDamage(int amount)
    {
        vida = Mathf.Max(vida - amount, 0);
        UpdateUI();
        if (vida == 0) Die();
    }

    public void Heal(int amount)
    {
        vida = Mathf.Min(vida + amount, maxVida);
        UpdateUI();
        if (mover) mover.enabled = true;
    }

    void Die()
    {
        if (mover) mover.enabled = false;
        print("Player died");
        deathMenu.FinishGame();
    }

    void UpdateUI()
    {
        if (textHealth) textHealth.text = $"{vida}";
    }
}