using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public partial class GamePlayManager : MonoBehaviour
{
    public UnityEvent PassEvent;
    public UnityEvent FailEvent;

    [SerializeField]
    private LevelConfig levelConfig; 
    [SerializeField]
    private Tile tileSample;
    [SerializeField]
    private TextMeshProUGUI countTxt;

    private Dictionary<Vector2Int, Tile> map = new();

    private int count;

    private readonly Vector2Int[] adjacentCoors = new Vector2Int[]
    {
        new Vector2Int(-1, 1), new Vector2Int(0, 1), new Vector2Int(1, 1),
        new Vector2Int(-1, 0), new Vector2Int(1, 0),
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
    };

    private readonly Vector2Int[] nextCoors = new Vector2Int[]
    {
        new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, -1)
    };

    private readonly Vector2Int[] xAdjacentCoors = new Vector2Int[]
    {
        new Vector2Int(-1, 1), new Vector2Int(1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1)
    };

    public void Init()
    {
        genMap(levelConfig);
        updateCount(levelConfig.ClickCount);
    }

    private void genMap(LevelConfig levelConfig)
    {
        var size = levelConfig.Size;
        var tileScale = levelConfig.TileScale;

        List<Vector2Int> coordinateCopy = new();

        #region gen tiles
        for (int i = 0; i < size.x; i++)
        {
            for(int j = 0; j < size.y; j++)
            {
                var pos = levelConfig.StartPoint.position;
                pos.x += i * tileScale;
                pos.y += j * tileScale;
                var tile = GameObject.Instantiate<Tile>(tileSample, pos, Quaternion.identity);
                tile.transform.localScale *= tileScale;
                Vector2Int coordinate = new Vector2Int(i, j);
                tile.Init(coordinate);
                map.Add(coordinate, tile);
                coordinateCopy.Add(coordinate);
            }
        }
        #endregion

        HashSet<Vector2Int> bombCoordinates = new();

        #region set bombs
        for (int i = 0; i < levelConfig.BaseBombCount; i++)
        {
            var coordinate = coordinateCopy[Random.Range(0, coordinateCopy.Count)];
            map[coordinate].SetToBomb();
            coordinateCopy.Remove(coordinate);
            bombCoordinates.Add(coordinate);
        }
        #endregion

        #region set adjacent
        foreach (var bombCoor in bombCoordinates)
        {
            foreach(var adjCoor in adjacentCoors)
            {
                var checkCoor = bombCoor + adjCoor;

                if(map.TryGetValue(checkCoor, out var tile)
                    && tile.Type == Tile.EType.Empty)
                {
                    tile.SetToAdjacent();
                }
            }
        }
        #endregion
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && count > 0 && !EventSystem.current.IsPointerOverGameObject())
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null && hit.collider.TryGetComponent<Tile>(out var tile) && !tile.Opened)
            {
                switch (tile.Type)
                {
                    case Tile.EType.Bomb:
                        map.Remove(tile.Coordinate);
                        explode(tile);
                        break;
                    default:
                        expand(tile);
                        break;
                }

                updateCount(--count);

                if (map.Count == 0)
                {
                    PassEvent?.Invoke();
                }
                else if (count == 0)
                {
                    FailEvent?.Invoke();
                }

                //Debug.Log("Target Position: " + hit.collider.gameObject.transform.position);
            }
        }
    }

    private void expand(Tile centerTile)
    {
        centerTile.Open();

        if (centerTile.Type == Tile.EType.Adjacent
            && checkAdjacentBombs(centerTile) > 0)
        {
            return;
        }

        foreach (var adjCoor in adjacentCoors)
        {
            var checkCoor = centerTile.Coordinate + adjCoor;

            if (!map.TryGetValue(checkCoor, out var tile)
                || tile.Type == Tile.EType.Bomb
                || tile.Opened)
            {
                continue;
            }

            expand(tile);
        }
    }

    private void explode(Tile centerTile)
    {
        map.Remove(centerTile.Coordinate);
        centerTile.Explode();

        foreach (var adjCoor in nextCoors)
        {
            var checkCoor = centerTile.Coordinate + adjCoor;

            if (!map.TryGetValue(checkCoor, out var tile)
                || !tile.Opened)
            {
                continue;
            }

            explode(tile);
        }

        if(centerTile.Type == Tile.EType.Bomb)
        {
            foreach (var adjCoor in xAdjacentCoors)
            {
                var checkCoor = centerTile.Coordinate + adjCoor;

                if (map.TryGetValue(checkCoor, out var tile)
                    && tile.Opened
                    && tile.Type == Tile.EType.Adjacent)
                {
                    checkAdjacentBombs(tile);
                }
            }
        }
    }

    private void updateCount(int count)
    {
        this.count = count;
        countTxt.text = count.ToString();
    }

    private int checkAdjacentBombs(Tile centerTile)
    {
        int bombCount = 0;

        foreach (var adjCoor in adjacentCoors)
        {
            var checkCoor = centerTile.Coordinate + adjCoor;

            if (map.TryGetValue(checkCoor, out var tile)
                && tile.Type == Tile.EType.Bomb)
            {
                bombCount++;
            }
        }

        if (bombCount > 0)
        {
            centerTile.ShowNumber(bombCount);
        }
        else
        {
            centerTile.SetToEmpty();
        }

        return bombCount;
    }
}
