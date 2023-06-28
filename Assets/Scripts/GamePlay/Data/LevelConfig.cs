using System;
using UnityEngine;

[Serializable]
public class LevelConfig
{
    public Vector2Int Size;
    public float TileScale = 1;
    public int BaseBombCount;
    public int ClickCount;
    public Transform StartPoint;
}
