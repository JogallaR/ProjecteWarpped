using UnityEngine;
using TMPro;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Prefabs dels Personatges")]
    [Tooltip("Posa aquí els Prefabs dels personatges en EL MATEIX ORDRE que al menú de selecció.")]
    [SerializeField] private GameObject[] characterPrefabs;
    [SerializeField] private float characterScale = 0.65f;

    [Header("Casella d'Aparició")]
    [SerializeField] private Vector2Int spawnTile = Vector2Int.zero;
    //SpawnPositoin

    [Header("Referències de l'escena")]
    [SerializeField] private GridBehaviour grid;
    [SerializeField] private TMP_Text textHealth;
    [SerializeField] private TMP_Text moveText;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private DeathMenu deathMenu;
    [SerializeField] private GameObject deathPannel;

    public CentralPlayer SpawnedPlayer { get; private set; }
    public PlayerMover SpawnedMover { get; private set; }

    private void Start()
    {
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);

        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogError("Error: No has assignat els prefabs dels personatges al CharacterSpawner!");
            return;
        }

        if (grid == null || grid.gridArray == null)
        {
            Debug.LogError("Error: El Grid no està assignat o no s'ha generat!");
            return;
        }

        if (deathMenu == null)
        {
            Debug.LogError("Error: No has assignat el DeathMenu de l'escena al CharacterSpawner!");
        }

        if (selectedIndex < 0 || selectedIndex >= characterPrefabs.Length)
        {
            selectedIndex = 0;
        }

        if (!grid.InBounds(spawnTile.x, spawnTile.y))
        {
            Debug.LogWarning($"Casella {spawnTile} fora del grid, s'usa (0,0).");
            spawnTile = Vector2Int.zero;
        }

        grid.gridArray[spawnTile.x, spawnTile.y].MakeSpawnSafe();

        Vector3 spawnPosition = grid.GridToWorld(spawnTile.x, spawnTile.y);
        GameObject instance = Instantiate(characterPrefabs[selectedIndex], spawnPosition, transform.rotation);
        instance.transform.localScale *= characterScale;

        SpawnedPlayer = instance.GetComponent<CentralPlayer>();
        if (SpawnedPlayer != null)
        {
            SpawnedPlayer.Init(textHealth, settingsPanel, deathMenu, deathPannel);
        }
        else
        {
            Debug.LogError($"El prefab {instance.name} no té el component CentralPlayer!");
        }

        SpawnedMover = instance.GetComponentInChildren<PlayerMover>();
        if (SpawnedMover != null)
        {
            SpawnedMover.Init(grid, moveText, spawnTile);
        }
        else
        {
            Debug.LogError($"El prefab {instance.name} no té el component PlayerMover!");
        }
    }
}