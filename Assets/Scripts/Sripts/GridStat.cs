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
    public bool warned = false;
    //public int decayTime = Random.Range(1,7);

    Renderer rend;
    Color baseColor = Color.white;
    Color warningColor;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        if (rend) baseColor = rend.material.color;
    }

    void Start()
    {
        if (isSpawn) return;

        int spec = Random.Range(1, 100);

        if (spec == 80) heal = true;
        if (spec > 81) walkable = false;

        RefreshColor();
    }

    public void MakeSpawnSafe()
    {
        isSpawn = true;
        walkable = true;
        dangerous = false;
        heal = false;
        RefreshColor();
    }

    public void bossArea()
    {
        isSpawn = false;
        walkable = false;
        dangerous = false;
        heal = false;
        RefreshColor();
    }

    public void UseHeal()
    {
        heal = false;
        RefreshColor();
        print("Spawn x:" + x + "| y:" + y);
    }

    public void SetWarning(Color color)
    {
        warned = true;
        warningColor = color;
        RefreshColor();
    }

    public void setDangerous()
    {
        dangerous = true;
        RefreshColor();
    }

    public void ClearWarning()
    {
        warned = false;
        RefreshColor();
    }

    void RefreshColor()
    {
        if (!rend) return;
        if (warned) rend.material.color = warningColor;
        else if (!walkable) rend.material.color = Color.black;
        else if (heal) rend.material.color = Color.green;
        else if (dangerous) rend.material.color = Color.red;
        else rend.material.color = baseColor;
    }

    public void DeactivateCheck()
    {
        if (dangerous)
        {
            int check = Random.Range(1,6);
            if (check == 1)
            {
                dangerous = false;
                RefreshColor();
            }
        }
    }
}