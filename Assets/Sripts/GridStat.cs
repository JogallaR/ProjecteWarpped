using Unity.VisualScripting;
using UnityEngine;

public class GridStat : MonoBehaviour
{
    public int visited = -1;
    public int x = 0; //Arriben de 0 a 15
    public int y = 0; //Arriben de 0 a 15 
    public bool walkable = true;
    public bool dangerous = false;
    public bool heal = false;
    Renderer rend;

    void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
    }

    void Start()
    {
        int spec = Random.Range(1, 100);

        if (spec <= 25){
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

    void Update(){}

    public void UseHeal()
    {
        heal = false;
        rend.material.color = Color.white;
        print("Spawn x:" + x + "| y:" + y); 
    }

}