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

        // L'Esc ara el gestiona nomes el PauseMenu (abans aqui es desactivava el PlayerMover
        // i en sortir amb el boto RESUME el personatge es quedava sense poder-se moure)
        inSettings = PauseMenu.IsPaused;

        if (inSettings) return;

        //Debug
        if (keyboard.fKey.wasPressedThisFrame)
        {
            TakeDamage(5);
            //Not for testing death plane
            return;
        }

        //Debug
        if (keyboard.hKey.wasPressedThisFrame)
        {
            Heal(1);
            return;
        }

        if (GameSettings.WasPressed(GameAction.SkipTurn) && !mover.IsMoving && mover.enabled)
            mover.EndTurn();
    }

    void SetSettings(bool open)
    {
        // Ja no activa/desactiva cap panell: la pausa i les opcions les gestiona el PauseMenu
        inSettings = open;
    }

    public void TakeDamage(int amount)
    {
        GameStats.malRebut += amount;
        if (amount > 0 && vida > 0) GameAudio.Play(Sfx.PlayerHurt);
        vida = Mathf.Max(vida - amount, 0);
        UpdateUI();
        if (vida == 0) Die();
    }

    public void Heal(int amount)
    {
        GameStats.cures++;
        if (amount > 0) GameAudio.Play(Sfx.Heal, 0.8f, 0f);
        vida = Mathf.Min(vida + amount, maxVida);
        UpdateUI();
        if (mover) mover.enabled = true;
    }

    void Die()
    {
        if (mover) mover.enabled = false;
        print("Player died");
        GameAudio.Play(Sfx.PlayerDeath, 1f, 0f);
        deathMenu.FinishGame();
    }

    public void SetClassMaxHealth(int classHealth)
    {
        maxVida = classHealth;
        vida = classHealth;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (textHealth) textHealth.text = $"{vida}";
    }
}