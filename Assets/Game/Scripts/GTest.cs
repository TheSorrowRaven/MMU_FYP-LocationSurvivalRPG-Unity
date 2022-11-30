using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
/// <summary>
/// For testing only, make sure to comment out codes instead of deleting
/// </summary>
public class GTest : MonoBehaviour
{
    //public GameSettings GameSettings;

    //public Grid grid;

    //private List<Vector3> positions = new();


    //private void Update()
    //{
    //    float size = (float)(GameSettings.UnityUnitsDistanceQueryRadius);
    //    grid.cellSize = new(size, size, size);

    //    positions.Clear();
    //    var cell = grid.WorldToCell(transform.position);
    //    var tCell = cell;

    //    positions.Add(grid.CellToWorld(tCell));
    //    tCell.x += 1;
    //    positions.Add(grid.CellToWorld(tCell));
    //    tCell.x -= 1;
    //    tCell.y += 1;
    //    positions.Add(grid.CellToWorld(tCell));
    //    tCell = cell;
    //    tCell.x += 1;
    //    tCell.y += 1;
    //    positions.Add(grid.CellToWorld(tCell));
    //}

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    foreach(var position in positions)
    //    {
    //        Gizmos.DrawWireSphere(position, (float)GameSettings.UnityUnitsDistanceQueryRadius);
    //    }
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, (float)GameSettings.UnityUnitsDistanceQueryRadius);
    //}


}
