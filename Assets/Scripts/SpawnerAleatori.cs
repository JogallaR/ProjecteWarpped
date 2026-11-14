using UnityEngine;

public class SpawnerAleatori : MonoBehaviour
{
    public GameObject[] prefabsEnemics;
    public Transform puntAparicio;
    
    // Afegeix aquesta variable per dir-li a l'Spawner quin és el jugador actiu
    public Transform jugadorActual;

    public Transform EnemicActual { get; private set; }

    void Start()
    {
        if (EnemicActual == null)
        {
            SpawnEnemic();
        }
    }

    public void SpawnEnemic()
    {
        if (EnemicActual != null) return;
        if (prefabsEnemics != null && prefabsEnemics.Length > 0 && puntAparicio != null)
        {
            int indexAleatori = Random.Range(0, prefabsEnemics.Length);
            
            // Creem l'enemic i el desem en una variable per poder parlar amb ell
            GameObject enemicCreat = Instantiate(prefabsEnemics[indexAleatori], puntAparicio.position, puntAparicio.rotation);
            EnemicActual = enemicCreat.transform;

            // Busquem o afegim l'script MirarObjectiu al nou enemic
            MirarObjectiu scriptRotacioEnemic = enemicCreat.GetComponent<MirarObjectiu>();
            if (scriptRotacioEnemic == null)
            {
                scriptRotacioEnemic = enemicCreat.AddComponent<MirarObjectiu>();
            }

            // Si encara no tenim la referència del jugador, la busquem al CharacterSpawner
            if (jugadorActual == null)
            {
                CharacterSpawner charSpawner = FindFirstObjectByType<CharacterSpawner>();
                if (charSpawner != null && charSpawner.SpawnedPlayer != null)
                {
                    jugadorActual = charSpawner.SpawnedPlayer.transform;
                }
            }

            if (jugadorActual != null)
            {
                scriptRotacioEnemic.AssignarObjectiu(jugadorActual);

                MirarObjectiu scriptRotacioJugador = jugadorActual.GetComponent<MirarObjectiu>();
                if (scriptRotacioJugador == null)
                {
                    scriptRotacioJugador = jugadorActual.gameObject.AddComponent<MirarObjectiu>();
                }
                scriptRotacioJugador.AssignarObjectiu(EnemicActual);
            }
        }
    }
}