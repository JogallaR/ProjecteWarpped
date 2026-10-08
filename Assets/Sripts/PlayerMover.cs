using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{

    public int manaThisTurn = 0;
    public int minRoll = 1;
    public int maxRoll = 6;
    public int movePoints;

    public GridBehaviour grid;
    public Vector2Int position;
    public float moveSpeed = 8f;
    public float heightOffset = 1.05f;

    bool moving;

    void Start()
    {
        transform.position = TileWorld(position);
        RollManaTurn();
    }

    void Update()
    {
        if (moving) return;

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null)
        {
            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                RollManaTurn();
                return;
            }

            Vector2Int dir = Vector2Int.zero;
            if (keyboard.wKey.wasPressedThisFrame) dir = Vector2Int.up;
            else if (keyboard.dKey.wasPressedThisFrame) dir = Vector2Int.right;
            else if (keyboard.sKey.wasPressedThisFrame) dir = Vector2Int.down;
            else if (keyboard.aKey.wasPressedThisFrame) dir = Vector2Int.left;

            if (dir != Vector2Int.zero)
            {
                Vector2Int target = position + dir;
                if (grid.IsWalkable(target.x, target.y))
                    TryMove(new List<GridStat> { grid.gridArray[position.x, position.y], grid.gridArray[target.x, target.y] });
                return;
            }
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GridStat tile = hit.collider.GetComponentInParent<GridStat>();
                if (tile)
                {
                    List<GridStat> path = grid.FindPath(position, new Vector2Int(tile.x, tile.y));
                    if (path.Count > 1)
                        TryMove(path);
                }
            }
        }
    }

    void RollManaTurn()
    {
        manaThisTurn = Random.Range(minRoll, maxRoll + 1);
        RollMovePoints();
    }

    void RollMovePoints()
    {
        movePoints = manaThisTurn * 3; //Mana to move = 3
        print($"New turn: You have {manaThisTurn} mana x {3} = {movePoints} move points");
        manaThisTurn = 0;
    }

    void TryMove(List<GridStat> path)
    {
        int steps = path.Count - 1;
        
        if (steps > movePoints)
        {
            if(movePoints == 0)
            {
                print("En your turn to get more moves");
            }
            else
            {
                print("Not enough mana: need {steps}, have {movePoints}");
            }
            
            return;
        }
        movePoints -= steps;
        StartCoroutine(FollowPath(path));
    }

    IEnumerator FollowPath(List<GridStat> path)
    {
        moving = true;
        foreach (GridStat tile in path)
        {
            Vector3 target = TileWorld(new Vector2Int(tile.x, tile.y));
            while ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
            position = new Vector2Int(tile.x, tile.y);
        }
        moving = false;
        print($"Move points left: {movePoints}");

        if (movePoints <= 0)
            print("Press space to end turn and get more moves");
    }

    Vector3 TileWorld(Vector2Int p)
    {
        return grid.GridToWorld(p.x, p.y) + Vector3.up * heightOffset;
    }
}