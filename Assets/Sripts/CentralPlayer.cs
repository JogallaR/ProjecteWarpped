using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class CentralPlayer : MonoBehaviour
{
    public int vida = 20;
    public int maxVida = 20;
    public bool inSettings = false;

//Reference to Player Mover
    [SerializeField] PlayerMover mover;
    //Reference to healthTextMeshPro
    [SerializeField] TMP_Text textHealth;
    //Usless reference
    [SerializeField] GameObject settingsPanel;

    void Awake()
    {
        //Check if the playermover is working
        if (!mover) mover = GetComponentInChildren<PlayerMover>();
    }

    void Start()
    {
        SetSettings(false);

        UpdateUI();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null) return;

        //Obrir setings
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
        
        //Comprovacio de moviment
        if (keyboard.enterKey.wasPressedThisFrame && !mover.IsMoving)
            mover.EndTurn();
    }

    //Funcio per comprovar si s'esta a la configuracio
    void SetSettings(bool open)
    {
        inSettings = open;
        mover.enabled = !open;
        if (settingsPanel) settingsPanel.SetActive(open);
    }

    //Funcio mal
    public void TakeDamage(int amount)
    {
        vida = Mathf.Max(vida - amount, 0);
        if (vida < 0){vida = 0;}

        UpdateUI();
        if (vida == 0) Die();
    }

//Funcio curar
    public void Heal(int amount)
    {
        vida = Mathf.Min(vida + amount, maxVida);
        UpdateUI();

        //Testing
        mover.enabled = true;
    }

//Funcio morir
    void Die()
    {
        mover.enabled = false;
        print("Player died");
    }

//Actualitzar vida
    void UpdateUI()
    {
        if (textHealth) textHealth.text = $"{vida}";
    }
}