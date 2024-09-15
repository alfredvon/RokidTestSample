using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public static readonly float moveTolerance = 0.05f;

    [SerializeField] UnitAnimator animator;
    [SerializeField] float moveSpeed = 1f;

    public Tile CurrentTile { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsAttacking { get; private set; }

    List<Tile> movePath;
    List<Tile> movableTiles;
    List<Tile> attackableTiles;
    Character character;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        Tile tile = GridManager.Instance.GetTileWithWorldPosition(transform.position);
        if (tile != null) 
        {
            tile.PlaceUnit(this, true);
        }
        character = GetComponent<Character>();
        character.SetParentUnit(this);
        character.SetCharacterState(CharacterState.Generate);
    }

    public void SetCurrentTile(Tile current_tile)
    {
        CurrentTile = current_tile;
    }

    public void SetMovableTiles(List<Tile> tiles)
    {
        if (tiles == null || tiles.Count <= 0)
            return;
        movableTiles = tiles;
    }

    public bool IsInMovableTiles(Tile tile)
    {
        if (movableTiles == null)
            return false;
        foreach (Tile t in movableTiles) 
        {
            if (tile.Position == t.Position)
                return true;
        }
        return false;
    }

    public List<Tile> GetMovableTiles()
    {
        return movableTiles;
    }

    public void SetAttackableTiles(List<Tile> tiles)
    { 
        attackableTiles = tiles;
    }

    public List<Tile> GetAttackableTiles()
    { 
        return attackableTiles;    
    }

    public bool IsInAttackableTiles(Tile tile)
    {
        if (attackableTiles == null)
            return false;
        foreach (Tile t in attackableTiles)
        {
            if (tile.Position == t.Position)
                return true;
        }
        return false;
    }

    public float GetMovePoints()
    {
        return character.MovePoints;
    }

    public void Move(List<Tile> path)
    {
        if (IsMoving == false && movePath == null) 
        {
            ChangeCharacterState(CharacterState.Move);
            character.TurnMoveDone = true;
            IsMoving = true;
            movePath = path;
            animator?.StartMoving();
            RotateUnit(movePath[0].WorldPosition);
        }
    }

    public void Attack(Tile target_tile)
    {
        if (IsAttacking == false)
        {
            character.TurnAttackDone = true;
            ChangeCharacterState(CharacterState.Attack);
            IsAttacking = true;
            RotateUnit(target_tile.WorldPosition);
            animator?.Attack();
            StartCoroutine(StopAttackCoroutine());
        }
    }

    IEnumerator StopAttackCoroutine()
    {
        yield return new WaitForSeconds(character.AttackInterval);
        IsAttacking = false;
        animator?.StopAttack();
        ChangeCharacterState(CharacterState.AttackEnd);
    }

    public Character GetCharacter()
    {
        return character;
    }

    public bool IsEqual(Unit other_unit)
    { 
        if (other_unit == null)
            return false;
        return character.ID == other_unit.GetCharacter().ID;
    }

    private void RotateUnit(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0f;
        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void ChangeCharacterState(CharacterState state)
    {
        character.SetCharacterState(state);
    }

    private void Update()
    {
        if (movePath != null && movePath.Count > 0)
        {
            Tile nextTile = movePath[0];
            transform.position = Vector3.MoveTowards(transform.position, nextTile.WorldPosition, moveSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, nextTile.WorldPosition) < moveTolerance)
            {
                transform.position = nextTile.WorldPosition;
                movePath.RemoveAt(0);
                //move complete
                if (movePath.Count <= 0)
                {
                    movePath = null;
                    CurrentTile.RemoveUnit();
                    CurrentTile = nextTile;
                    CurrentTile.PlaceUnit(this);
                    foreach (Tile tile in movableTiles)
                        tile.ShowMoveHighlight(false);
                    movableTiles = null;
                    animator?.StopMoving();
                    IsMoving = false;
                    ChangeCharacterState(CharacterState.ReadyForAttack);
                }
                else
                    RotateUnit(movePath[0].WorldPosition);
            }
        }
    }
}
