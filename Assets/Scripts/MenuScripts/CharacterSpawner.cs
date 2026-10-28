using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Prefabs dels Personatges")]
    [Tooltip("Posa aquí els Prefabs dels personatges en EL MATEIX ORDRE que al menú de selecció.")]
    [SerializeField] private GameObject[] characterPrefabs;

    [Header("Punt d'Aparició")]
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        // Llegim el personatge triat (si no n'hi ha cap, agafarà el 0 per defecte)
        int selectedIndex = PlayerPrefs.GetInt("SelectedCharacterIndex", 0);

        if (characterPrefabs == null || characterPrefabs.Length == 0)
        {
            Debug.LogError("Error: No has assignat els prefabs dels personatges al CharacterSpawner!");
            return;
        }

        // Ens assegurem que l'índex no estigui fora de rang
        if (selectedIndex < 0 || selectedIndex >= characterPrefabs.Length)
        {
            selectedIndex = 0;
        }

        // Si no s'ha especificat un punt d'aparició, utilitzem la posició d'aquest mateix GameObject
        Vector3 spawnPosition = (spawnPoint != null) ? spawnPoint.position : transform.position;
        Quaternion spawnRotation = (spawnPoint != null) ? spawnPoint.rotation : transform.rotation;

        // Creem (instanciem) el personatge seleccionat a la pantalla de joc
        Instantiate(characterPrefabs[selectedIndex], spawnPosition, spawnRotation);
    }
}