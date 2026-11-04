using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyLogic : MonoBehaviour
{
    enum AttackType { Square, Cross, Row, Column, Diagonal, Ring }

    [SerializeField] TMP_Text bossHealth;
    [SerializeField] GridBehaviour grid;
    [SerializeField] Color warningColor = new Color(1f, 0.55f, 0f);

    public PlayerMover playerMover;
    public int vida = 20;
    public int damage = 3;
    public int minSize = 1;
    public int maxSize = 2;

    readonly List<GridStat> pendingTiles = new List<GridStat>();
    AttackType pendingType;

    void OnEnable() => PlayerMover.TurnEnded += OnPlayerTurnEnded;
    void OnDisable() => PlayerMover.TurnEnded -= OnPlayerTurnEnded;

    void Start() => UpdateUI();

    void OnPlayerTurnEnded(PlayerMover mover)
    {
        playerMover = mover;
        if (!grid) grid = mover.grid;
        if (!grid || grid.gridArray == null || vida <= 0) return;
        if (PlayerDead()) return;

        ResolveAttack();
        if (PlayerDead()) return;
        BossTurn();
    }

    bool PlayerDead() => playerMover.Central && playerMover.Central.vida <= 0;

    void BossTurn()
    {

        int typeOfAtack = Random.Range(0,30 + 1);
        int size = Random.Range(minSize, maxSize + 1);

        print(typeOfAtack + " roll of atack");
        
        Vector2Int center = playerMover.position;

        if (typeOfAtack < 7)
        {
            pendingType = AttackType.Square;   
        } else if (typeOfAtack < 14)
        {
            pendingType = AttackType.Ring;
        } else if (typeOfAtack < 20)
        {
            pendingType = AttackType.Cross;
        } else if (typeOfAtack < 26)
        {
            pendingType = AttackType.Diagonal;
        } else if (typeOfAtack <= 28)
        {
            pendingType = AttackType.Column;
        } else if (typeOfAtack <= 30)
        {
            pendingType = AttackType.Row;
        }
        
        foreach (Vector2Int p in GetPattern(pendingType, center, size))
        {
            if (!grid.IsWalkable(p.x, p.y)) {continue;}
            GridStat tile = grid.gridArray[p.x, p.y];
            if (!tile.dangerous && tile.walkable && !tile.heal) //Heal wtf
            {
                tile.SetWarning(warningColor);
                pendingTiles.Add(tile);
            }
        
        }

        print($"Boss prepares {pendingType} (size {size}) on {center}: {pendingTiles.Count} tiles");
    }

    void ResolveAttack()
    {
        if (pendingTiles.Count == 0) return;

        bool hit = false;
        foreach (GridStat tile in pendingTiles)
        {
            tile.ClearWarning();
            grid.DamageEffect(tile.x, tile.y);
            if (tile.x == playerMover.position.x && tile.y == playerMover.position.y) hit = true;
            else
            {
                tile.setDangerous();
            }
        }
        pendingTiles.Clear();

        if (hit && playerMover.Central) playerMover.Central.TakeDamage(damage);
    }

    HashSet<Vector2Int> GetPattern(AttackType type, Vector2Int c, int size)
    {
        HashSet<Vector2Int> cells = new HashSet<Vector2Int>();

        switch (type)
        {
            case AttackType.Square:
                for (int dx = -size; dx <= size; dx++)
                    for (int dy = -size; dy <= size; dy++)
                        cells.Add(c + new Vector2Int(dx, dy));
                break;

            case AttackType.Cross:
                for (int i = -size * 2; i <= size * 2; i++)
                {
                    cells.Add(c + new Vector2Int(i, 0));
                    cells.Add(c + new Vector2Int(0, i));
                }
                break;

            case AttackType.Row:
                for (int x = 0; x < grid.columns; x++)
                    for (int dy = 1 - size; dy <= size - 1; dy++)
                        cells.Add(new Vector2Int(x, c.y + dy));
                break;

            case AttackType.Column:
                for (int y = 0; y < grid.rows; y++)
                    for (int dx = 1 - size; dx <= size - 1; dx++)
                        cells.Add(new Vector2Int(c.x + dx, y));
                break;

            case AttackType.Diagonal:
                for (int i = -size * 2; i <= size * 2; i++)
                {
                    cells.Add(c + new Vector2Int(i, i));
                    cells.Add(c + new Vector2Int(i, -i));
                }
                break;

            case AttackType.Ring:
            {
                int r = size;
                for (int dx = -r; dx <= r; dx++)
                    for (int dy = -r; dy <= r; dy++)
                        if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) == r)
                            cells.Add(c + new Vector2Int(dx, dy));
                break;
            }
        }

        return cells;
    }

    public void TakeDamage(int amount)
    {
        vida = Mathf.Max(vida - amount, 0);
        UpdateUI();
        if (vida == 0) Die();
    }

    void Die()
    {
        foreach (GridStat tile in pendingTiles) tile.ClearWarning();
        pendingTiles.Clear();
        print("Boss defeated");
        enabled = false;
        //Posar a pantalla de victoria
    }

    void UpdateUI()
    {
        if (bossHealth) bossHealth.text = $"{vida}";
        //Averiguar com posar una barra de vida
    }
}