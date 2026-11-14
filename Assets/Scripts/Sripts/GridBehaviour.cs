using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class GridBehaviour : MonoBehaviour
{
    //15
    public int rows = 0;
    //15
    public int columns = 0;
    public float scale = 1f;
    public GameObject gridPrefab;
    public Vector3 leftBottomLocation = Vector3.zero;
    public GridStat[,] gridArray;
    static readonly Vector2Int[] Directions = {Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};

    [SerializeField] private ParticleSystem healEffect;

    [SerializeField] private ParticleSystem damageEffect;

    void Awake()
    {
        if(!gridPrefab)
        {
            Debug.LogError("Falta el prefab del grid", this);
            return;
        }
        GenerateGrid();
    }

    void GenerateGrid()
    {
        AlignGridToCircularBase();
        gridArray = new GridStat[columns, rows];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (!IsInsideCircularBoard(x, y))
                {
                    gridArray[x, y] = null;
                    continue;
                }

                GameObject obj = Instantiate(gridPrefab, GridToWorld(x,y), Quaternion.identity, transform);
                obj.name = $"Tile_{x}_{y}";
                if (!obj.TryGetComponent(out GridStat stat))
                    stat = obj.AddComponent<GridStat>();
                stat.x = x;
                stat.y = y;
                gridArray[x, y] = stat;
            }
        }
        bossSpawnArea();
    }

    void AlignGridToCircularBase()
    {
        GameObject baseObj = GameObject.Find("Base");
        if (baseObj == null) return;

        Renderer[] renderers = baseObj.GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        float halfCols = (columns - 1) * 0.5f * scale;
        float halfRows = (rows - 1) * 0.5f * scale;
        leftBottomLocation.x = bounds.center.x - halfCols;
        leftBottomLocation.z = bounds.center.z - halfRows;
    }

    public bool IsInsideCircularBoard(int x, int y)
    {
        if (x < 0 || x >= columns || y < 0 || y >= rows) return false;
        float cx = (columns - 1) * 0.5f;
        float cy = (rows - 1) * 0.5f;
        float dx = x - cx;
        float dy = y - cy;
        float maxRadiusSq = (columns == 19 && rows == 19)
            ? 90.1f
            : ((Mathf.Min(columns, rows) * 0.5f) * (Mathf.Min(columns, rows) * 0.5f) + 0.5f);
        return (dx * dx + dy * dy) <= maxRadiusSq;
    }

    public Vector2Int GetValidSpawnTile(Vector2Int requested)
    {
        if (InBounds(requested.x, requested.y) && gridArray[requested.x, requested.y] != null)
        {
            return requested;
        }

        Vector2Int best = new Vector2Int(columns / 2, Mathf.Clamp(rows - 5, 0, rows - 1));
        float bestDistSq = float.MaxValue;
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (gridArray[x, y] == null) continue;
                if (x >= 8 && x <= 10 && y >= 8 && y <= 10) continue;
                float dSq = (x - requested.x) * (x - requested.x) + (y - requested.y) * (y - requested.y);
                if (dSq < bestDistSq)
                {
                    bestDistSq = dSq;
                    best = new Vector2Int(x, y);
                }
            }
        }
        return best;
    }

    public void DamageEffect(int x, int y)
    {
        if (!damageEffect) return;
        ParticleSystem DMGeffect = Instantiate(damageEffect, new Vector3(leftBottomLocation.x + scale * x, leftBottomLocation.y + 0.3f, leftBottomLocation.z + scale * y), Quaternion.identity);
        DMGeffect.Play();
        Destroy(DMGeffect.gameObject, DMGeffect.main.duration);
    }

    public void SpawnHitEffectAtWorldPos(Vector3 worldPos, float scaleMultiplier = 1f)
    {
        if (!damageEffect) return;
        ParticleSystem DMGeffect = Instantiate(damageEffect, worldPos, Quaternion.identity);
        DMGeffect.transform.localScale *= scaleMultiplier;
        DMGeffect.Play();
        Destroy(DMGeffect.gameObject, Mathf.Max(DMGeffect.main.duration, 1.5f));
    }
    public void UseHeal(int x, int y)
    {
        if (!InBounds(x, y) || gridArray[x, y] == null) return;
        if (healEffect)
        {
            ParticleSystem effect = Instantiate(healEffect,new Vector3(leftBottomLocation.x + scale * x, leftBottomLocation.y + 0.3f, leftBottomLocation.z + scale * y),Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
        gridArray[x,y].UseHeal();
    }

    public void bossSpawnArea()
    {
        
        for (int x = 8; x != 11; x++)
        {
            for (int y = 8; y != 11; y++)
            {    
                if (InBounds(x, y) && gridArray[x, y] != null)
                {
                    gridArray[x,y].bossArea();
                }
            }
        }
    }

    public Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(leftBottomLocation.x + scale * x, leftBottomLocation.y, leftBottomLocation.z + scale * y);
    }

    public bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < columns && y < rows;
    }

    public bool IsWalkable(int x, int y)
    {
        return InBounds(x, y) && gridArray[x, y] != null && gridArray[x, y].walkable;
    }

    public bool doesDamage(int x, int y)
    {
        return InBounds(x, y) && gridArray[x, y] != null && gridArray[x, y].dangerous;
    }

    public void chekcDisableDamage()
    {
        for (int columCheck = 0; columCheck != columns; columCheck++)
        {
            for (int rowCheck = 0; rowCheck != rows; rowCheck++)
            {    
                if (gridArray[columCheck, rowCheck] != null)
                {
                    gridArray[columCheck,rowCheck].DeactivateCheck();
                }
            }
        }
    }

    public List<GridStat> FindPath(Vector2Int start, Vector2Int end)
    {
        List<GridStat> path = new List<GridStat>();
        if (!IsWalkable(start.x, start.y) || !IsWalkable(end.x, end.y))
            return path;

        //Check if they have been visited
        foreach (GridStat tile in gridArray)
            if (tile) tile.visited = -1;
        gridArray[start.x, start.y].visited = 0;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end) break;
            int step = gridArray[current.x, current.y].visited;
            foreach (Vector2Int dir in Directions)
            {
                Vector2Int next = current + dir;
                if (IsWalkable(next.x, next.y) && gridArray[next.x, next.y].visited == -1)
                {
                    gridArray[next.x, next.y].visited = step + 1;
                    queue.Enqueue(next);
                }
            }
        }

        if (gridArray[end.x, end.y].visited == -1)
            return path;

        Vector2Int p = end;
        path.Add(gridArray[p.x, p.y]);
        while (p != start)
        {
            int step = gridArray[p.x, p.y].visited;
            foreach (Vector2Int dir in Directions)
            {
                Vector2Int prev = p + dir;
                if (IsWalkable(prev.x, prev.y) && gridArray[prev.x, prev.y].visited == step - 1)
                {
                    p = prev;
                    path.Add(gridArray[p.x, p.y]);
                    break;
                }
            }
        }
        path.Reverse();
        return path;
    }

    //Safewalk

    public List<GridStat> Safewalk(Vector2Int start, Vector2Int end)
    {
        List<GridStat> path = new List<GridStat>();
        if (!IsWalkable(start.x, start.y) || !IsWalkable(end.x, end.y))
            return path;

        //Check if they have been visited
        foreach (GridStat tile in gridArray)
            if (tile) tile.visited = -1;
        gridArray[start.x, start.y].visited = 0;

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == end) break;
            int step = gridArray[current.x, current.y].visited;
            foreach (Vector2Int dir in Directions)
            {
                Vector2Int next = current + dir;
                if (IsWalkable(next.x, next.y) && gridArray[next.x, next.y].visited == -1 && !gridArray[next.x, next.y].dangerous)
                {
                    gridArray[next.x, next.y].visited = step + 1;
                    queue.Enqueue(next);
                }
            }
        }

        if (gridArray[end.x, end.y].visited == -1)
            return path;

        Vector2Int p = end;
        path.Add(gridArray[p.x, p.y]);
        while (p != start)
        {
            int step = gridArray[p.x, p.y].visited;
            foreach (Vector2Int dir in Directions)
            {
                Vector2Int prev = p + dir;
                if (IsWalkable(prev.x, prev.y) && gridArray[prev.x, prev.y].visited == step - 1)
                {
                    p = prev;
                    path.Add(gridArray[p.x, p.y]);
                    break;
                }
            }
        }
        path.Reverse();
        return path;
    }



}