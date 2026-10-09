using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using UnityEngine;

public class GridBehaviour : MonoBehaviour
{
    public int rows = 15;
    public int columns = 15;
    public float scale = 1f;
    public GameObject gridPrefab;
    public Vector3 leftBottomLocation = Vector3.zero;
    public GridStat[,] gridArray;
    
    static readonly Vector2Int[] Directions = {Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left};

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
        gridArray = new GridStat[columns, rows];
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject obj = Instantiate(gridPrefab, GridToWorld(x,y), Quaternion.identity, transform);
                obj.name = $"Tile_{x}_{y}";
                if (!obj.TryGetComponent(out GridStat stat))
                    stat = obj.AddComponent<GridStat>();
                stat.x = x;
                stat.y = y;
                gridArray[x, y] = stat;
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

}