using UnityEngine;

public class GridStat : MonoBehaviour
{
    public int visited = -1;
    public int x = 0;
    public int y = 0;
    public bool walkable = true;
    public bool dangerous = false;
    public bool heal = false;
    public bool isSpawn = false;

    Renderer rend;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
    }

    void Start()
    {
        if (isSpawn) return;

        int spec = Random.Range(1, 100);

        if (spec <= 25)
        {
            dangerous = true;
            rend.material.color = Color.red;
        }
        if (spec == 80)
        {
            heal = true;
            rend.material.color = Color.green;
        }
        if (spec > 90)
        {
            walkable = false;
            rend.material.color = Color.black;
        }
    }

    public void MakeSpawnSafe()
    {
        isSpawn = true;
        walkable = true;
        dangerous = false;
        heal = false;
        if (rend) rend.material.color = Color.white;
    }

    public void UseHeal()
    {
        heal = false;
        rend.material.color = Color.white;
        print("Spawn x:" + x + "| y:" + y);
    }
}