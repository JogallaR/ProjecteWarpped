using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class PlayerMover : MonoBehaviour
{
<<<<<<< HEAD
    //Moves per mana
    const int MovesPerMana = 3;
=======
    public static event Action<PlayerMover> TurnEnded;

    //Moves per mana
    const int MovesPerMana = 2;
>>>>>>> origin/main

    //TextmeshPro mana
    [SerializeField] TMP_Text MoveText;

    [SerializeField] CentralPlayer central;
<<<<<<< HEAD
=======
    public CentralPlayer Central => central;
>>>>>>> origin/main

    public int manaThisTurn = 0;
    public int minRoll = 2;
    public int maxRoll = 6;
    //Mana aquest punts
    public int movePoints;

<<<<<<< HEAD
    public bool effectdo = true;
=======
    public bool effectDo = true;
>>>>>>> origin/main

    public GridBehaviour grid;
    public Vector2Int position;
    public float moveSpeed = 8f;
    public float heightOffset = 0.05f;

<<<<<<< HEAD
=======
    public bool smartWalk = false;

>>>>>>> origin/main
    bool moving;
    //Referencia publica
    public bool IsMoving => moving;


    void Awake()
    {
        if (!central) central = GetComponentInParent<CentralPlayer>();
    }

<<<<<<< HEAD
 public void Init(GridBehaviour gridRef, TMP_Text moveText, Vector2Int spawnTile)
{
    grid = gridRef;
    MoveText = moveText;
    position = spawnTile;
    transform.position = TileWorld(position);
}
=======
    public void Init(GridBehaviour gridRef, TMP_Text moveText, Vector2Int spawnTile)
    {
        grid = gridRef;
        MoveText = moveText;
        position = spawnTile;
        transform.position = TileWorld(position);
    }
>>>>>>> origin/main

    void Start()
    {
        //Creacio grid
        transform.position = TileWorld(position);
        EndTurn();
    }

    void Update()
    {

        if (moving) return;

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        if (keyboard != null)
        {
<<<<<<< HEAD
=======

             if (keyboard.cKey.wasPressedThisFrame)
            {
                if (smartWalk)
                { smartWalk = false;}
                else if (!smartWalk)
                {
                    smartWalk = true;
                }
            }
            ;

>>>>>>> origin/main
            Vector2Int dir = Vector2Int.zero;
            if (keyboard.upArrowKey.wasPressedThisFrame) dir = Vector2Int.down;
            else if (keyboard.rightArrowKey.wasPressedThisFrame) dir = Vector2Int.left;
            else if (keyboard.downArrowKey.wasPressedThisFrame) dir = Vector2Int.up;
            else if (keyboard.leftArrowKey.wasPressedThisFrame) dir = Vector2Int.right;

            if (dir != Vector2Int.zero)
            {
                Vector2Int target = position + dir;
                if (grid.IsWalkable(target.x, target.y))
                {
                    if (grid.gridArray[position.x, position.y].dangerous && movePoints > 0)
                    {
                        //grid.DamageEffect(position.x,position.y);
                        central.Heal(1);
                    }
                    TryMove(new List<GridStat> { grid.gridArray[position.x, position.y], grid.gridArray[target.x, target.y] });
                }
                return;
            }
<<<<<<< HEAD
=======

>>>>>>> origin/main
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GridStat tile = hit.collider.GetComponentInParent<GridStat>();
                if (tile)
                {
<<<<<<< HEAD
                    List<GridStat> path = grid.Safewalk(position, new Vector2Int(tile.x, tile.y));
                    //List<GridStat> path = grid.FindPath(position, new Vector2Int(tile.x, tile.y));
=======
                    List<GridStat> path;
                    
                    if (smartWalk)
                    {
                        path = grid.Safewalk(position, new Vector2Int(tile.x, tile.y));    
                    } else
                    {
                        path = grid.FindPath(position, new Vector2Int(tile.x, tile.y));    
                    }
>>>>>>> origin/main
                    if (path.Count > 1)
                        TryMove(path);
                }
            }
        }
    }

    public void EndTurn()
    {
<<<<<<< HEAD
        GameStats.torns++;
=======
>>>>>>> origin/main
        manaThisTurn = Random.Range(minRoll, maxRoll + 1);
        movePoints = manaThisTurn * MovesPerMana;
        print($"New turn: {manaThisTurn} mana x {MovesPerMana} = {movePoints} move points");
        UpdateUI();
<<<<<<< HEAD
=======
        grid.chekcDisableDamage();
        TurnEnded?.Invoke(this);
    }

    public void changePathLogic()
    {
        if (smartWalk)
                { smartWalk = false;}
                else if (!smartWalk)
                {
                    smartWalk = true;
                }
    }

    public Vector2 GetPosition()
    {
        return position;
>>>>>>> origin/main
    }

    void UpdateUI()
    {
        if (MoveText) MoveText.text = $"{movePoints}";
    }

    void TryMove(List<GridStat> path)
    {
        int steps = path.Count - 1;

        if (steps > movePoints)
        {
            if (movePoints == 0)
                print("End your turn to get more moves");
            else
                print($"Not enough mana: need {steps}, have {movePoints}");
            return;
        }

        movePoints -= steps;
<<<<<<< HEAD
        GameStats.passos += steps;
=======
>>>>>>> origin/main
        UpdateUI();
        StartCoroutine(FollowPath(path));
    }

    IEnumerator FollowPath(List<GridStat> path)
    {
        moving = true;
        foreach (GridStat tile in path)
        {
            if (tile.dangerous)
            {
                print(tile + " dangerous");
                central.TakeDamage(1);

<<<<<<< HEAD
                if (effectdo)
                {
                    grid.DamageEffect(tile.x,tile.y);
                    effectdo = false;    
                } else
                {
                    effectdo = true;
=======
                if (effectDo)
                {
                    grid.DamageEffect(tile.x,tile.y);
                    effectDo = false;    
                } else
                {
                    effectDo = true;
>>>>>>> origin/main
                }
                //Comp
            }
            if (tile.heal)
            {
                print(tile + " heal");
<<<<<<< HEAD
                central.Heal(1);
=======
                central.Heal(5);
>>>>>>> origin/main
                grid.UseHeal(tile.x, tile.y);
            }

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
            print("Press Enter to end turn and get more moves");
    }

    Vector3 TileWorld(Vector2Int p)
    {
        try {
            return grid.GridToWorld(p.x, p.y) + Vector3.up * heightOffset;
        } catch (Exception)
        {
            //Assuming you are on the menu
            return new Vector3(0,0,0);
        }
    }
}