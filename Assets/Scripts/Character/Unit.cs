using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public static readonly float moveTolerance = 0.05f;

    [SerializeField] UnitAnimator animator;
    [SerializeField] float moveSpeed = 1f;

    public Tile CurrentTile { get; private set; }

    List<Tile> movePath;
    public void SetCurrentTile(Tile currentTile)
    {
        CurrentTile = currentTile;
    }

    public void Move(List<Tile> path)
    {
        if (movePath == null) 
        {
            movePath = path;
            animator?.StartMoving();
            RotateUnit(movePath[0].WorldPosition);
        }
    }

    private void RotateUnit(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0f;
        transform.rotation = Quaternion.LookRotation(direction);
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
                if (movePath.Count <= 0)
                {
                    movePath = null;
                    CurrentTile.RemoveUnit();
                    CurrentTile = nextTile;
                    CurrentTile.PlaceUnit(this);
                    animator?.StopMoving();
                }
                else
                    RotateUnit(movePath[0].WorldPosition);
            }
        }
    }
}
