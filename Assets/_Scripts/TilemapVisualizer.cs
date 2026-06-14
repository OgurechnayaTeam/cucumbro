using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    private const string DungeonSortingLayerName = "Dungeon";
    private const string DungeonLayerName = "Dungeon";
    private const string WallLayerName = "Wall";
    private const int FloorSortingOrder = -20;
    private const int WallSortingOrder = -10;

    [SerializeField]
    private Tilemap floorTilemap, wallTilemap;
    [SerializeField]
    private TileBase floorTile, wallTop, wallSideRight, wallSiderLeft, wallBottom, wallFull, 
        wallInnerCornerDownLeft, wallInnerCornerDownRight, 
        wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft;

    private void Awake()
    {
        ConfigureSorting();
        EnsureWallCollider();
    }

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    internal void PaintSingleBasicWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;
        if (WallTypesHelper.wallTop.Contains(typeAsInt))
        {
            tile = wallTop;
        }else if (WallTypesHelper.wallSideRight.Contains(typeAsInt))
        {
            tile = wallSideRight;
        }
        else if (WallTypesHelper.wallSideLeft.Contains(typeAsInt))
        {
            tile = wallSiderLeft;
        }
        else if (WallTypesHelper.wallBottm.Contains(typeAsInt))
        {
            tile = wallBottom;
        }
        else if (WallTypesHelper.wallFull.Contains(typeAsInt))
        {
            tile = wallFull;
        }

        if (tile!=null)
            PaintSingleTile(wallTilemap, tile, position);
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePosition = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePosition, tile);
    }

    public Vector3 GetCellCenterWorld(Vector2Int position)
    {
        if (floorTilemap == null)
        {
            return new Vector3(position.x, position.y, 0f);
        }

        return floorTilemap.GetCellCenterWorld((Vector3Int)position);
    }

    public Vector2Int WorldToCell(Vector3 position)
    {
        if (floorTilemap == null)
        {
            return new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
        }

        Vector3Int cell = floorTilemap.WorldToCell(position);
        return new Vector2Int(cell.x, cell.y);
    }

    private void EnsureWallCollider()
    {
        if (wallTilemap == null || wallTilemap.TryGetComponent<TilemapCollider2D>(out _))
        {
            return;
        }

        wallTilemap.gameObject.AddComponent<TilemapCollider2D>().isTrigger = false;
    }

    public void Clear()
    {
        ConfigureSorting();
        EnsureWallCollider();
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
    }

    private void ConfigureSorting()
    {
        if (floorTilemap != null && floorTilemap.TryGetComponent<TilemapRenderer>(out var floorRenderer))
        {
            int dungeonLayer = LayerMask.NameToLayer(DungeonLayerName);
            if (dungeonLayer >= 0)
                floorTilemap.gameObject.layer = dungeonLayer;

            floorRenderer.sortingLayerName = DungeonSortingLayerName;
            floorRenderer.sortingOrder = FloorSortingOrder;
        }

        if (wallTilemap != null && wallTilemap.TryGetComponent<TilemapRenderer>(out var wallRenderer))
        {
            int wallLayer = LayerMask.NameToLayer(WallLayerName);
            if (wallLayer >= 0)
                wallTilemap.gameObject.layer = wallLayer;

            wallRenderer.sortingLayerName = DungeonSortingLayerName;
            wallRenderer.sortingOrder = WallSortingOrder;
        }
    }

    internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int typeASInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        if (WallTypesHelper.wallInnerCornerDownLeft.Contains(typeASInt))
        {
            tile = wallInnerCornerDownLeft;
        }
        else if (WallTypesHelper.wallInnerCornerDownRight.Contains(typeASInt))
        {
            tile = wallInnerCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownLeft;
        }
        else if (WallTypesHelper.wallDiagonalCornerDownRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerDownRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpRight.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpRight;
        }
        else if (WallTypesHelper.wallDiagonalCornerUpLeft.Contains(typeASInt))
        {
            tile = wallDiagonalCornerUpLeft;
        }
        else if (WallTypesHelper.wallFullEightDirections.Contains(typeASInt))
        {
            tile = wallFull;
        }
        else if (WallTypesHelper.wallBottmEightDirections.Contains(typeASInt))
        {
            tile = wallBottom;
        }

        if (tile != null)
            PaintSingleTile(wallTilemap, tile, position);
    }
}
