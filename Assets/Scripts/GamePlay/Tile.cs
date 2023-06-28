using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Tile : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer render;
    [SerializeField]
    private TextMeshPro numberTxt;
    [SerializeField]
    private Sprite pressedImg;
    [SerializeField]
    private Color noticeColor;
    [SerializeField]
    private Collider2D collider;

    public EType Type { get; private set; }

    public enum EType
    {
        Empty,
        Adjacent,
        Bomb
    }

    public Vector2Int Coordinate { get; private set; }

    public bool Opened { get; private set; }

    public void Init(Vector2Int coordinate)
    {
        Coordinate = coordinate;
        numberTxt.enabled = false;
        Opened = false;
    }
    public void SetToEmpty()
    {
        Type = EType.Empty;
        numberTxt.enabled = false;
    }

    public void SetToBomb()
    {
        Type = EType.Bomb;
    }

    public void SetToAdjacent()
    {
        Type = EType.Adjacent;
    }

    public void ShowNumber(int number)
    {
        numberTxt.text = number.ToString();
        numberTxt.enabled = true;
    }

    public void Open()
    {
        render.sprite = pressedImg;
        //TODO:check status
        switch (Type)
        {
            case EType.Adjacent:
                numberTxt.enabled = true;
                break;
        }
        Opened = true;
    }

    public void Explode()
    {
        render.enabled = false;
        numberTxt.enabled = false;
        Opened = true;
        collider.enabled = false;
    }
}
