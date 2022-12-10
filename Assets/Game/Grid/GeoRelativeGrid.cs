using Mapbox.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Move POI Cluster to use this instead

public class GeoRelativeGrid : MonoBehaviour
{
    private static G G => G.Instance;


    [SerializeField] private Grid grid;

    public Vector3 CellSize => grid.cellSize;

    [System.NonSerialized] private Vector2Int gridOffset;

    public void MapUpdated()
    {
        AlignGrid();
    }

    private void AlignGrid()
    {
        Vector3 zero = G.GeoToWorld(0, 0);
        gridOffset = new((int)(zero.x / grid.cellSize.x), (int)(zero.z / grid.cellSize.y));
        Vector3 pos = new(zero.x % grid.cellSize.x, 0, zero.z % grid.cellSize.y);
        grid.transform.localPosition = pos;
    }

    // Returns the same cell with the same geo coordinates no matter where the player is
    public Vector2Int GeoToCell(Vector2d geo)
    {
        Vector3 world = G.GeoToWorld(geo);
        Vector2Int cell = WorldToCell(world);
        return cell;
    }

    public Vector2Int WorldToCell(Vector3 pos)
    {
        Vector2Int cell = (Vector2Int)grid.WorldToCell(pos) - gridOffset;
        return cell;
    }

    public Vector2 CellToWorld(Vector2Int cell)
    {
        Vector3Int actualCell = (Vector3Int)(cell + gridOffset);
        Vector3 world = grid.CellToWorld(actualCell);
        return new(world.x, world.z);
    }


}
