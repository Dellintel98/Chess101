using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Square : MonoBehaviour
{
    [SerializeField] public GameObject potentialMoveMarkPrefab;

    private Vector2Int squarePosition;
    private string squarePositionCode;
    private string squareColorTag;
    private Color32 squareColor;
    private Color32 specialColor;
    private bool visibleHighlight;
    private bool visibleMoveMark;
    private GameObject myMoveMark;
    private SpriteRenderer spriteRenderer;
    private ChessPiece containedPiece;

    public void Setup(int row, int column)
    {
        visibleHighlight = false;
        specialColor = new Color32(0x1F, 0x7A, 0x8C, 0xFF); //#022b3a  #bfdbf7

        SetPositionOnBoard(row, column);
        CreatePotentialMoveMark();
        containedPiece = null;
        SetColor();
    }

    private void SetPositionOnBoard(int row, int column)
    {
        squarePosition = new Vector2Int(row, column);

        char columnCode = (char)(column + 65);
        int rowCode = 8 - row;
        squarePositionCode = $"{columnCode}{rowCode} {squarePosition}";

        gameObject.name = "Square" + squarePositionCode;
    }

    private void CreatePotentialMoveMark()
    {
        myMoveMark = Instantiate(potentialMoveMarkPrefab, new Vector3(transform.position.x, transform.position.y, -1f), Quaternion.identity, transform) as GameObject;
        myMoveMark.GetComponent<SpriteRenderer>().sortingLayerName = "PiecesLayer";
        myMoveMark.GetComponent<SpriteRenderer>().color = specialColor;
        visibleMoveMark = false;
        myMoveMark.SetActive(visibleMoveMark);
    }

    private void SetMoveMarkColor(Color32 newColor)
    {
        myMoveMark.GetComponent<SpriteRenderer>().color = newColor;
    }

    public void SetPotentialMoveMarkVisibility()
    {
        visibleMoveMark = !visibleMoveMark;

        myMoveMark.SetActive(visibleMoveMark);
    }

    public void SetColor()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        int x = squarePosition.x + 1;
        int y = squarePosition.y;

        if ((x % 2 == 0 && y % 2 == 0) || (x % 2 != 0 && y % 2 != 0))
        {
            squareColorTag = "Dark";
            gameObject.tag = squareColorTag;
            squareColor = new Color32(0x52, 0x51, 0x51, 0xFF);
        }
        else
        {
            squareColorTag = "Light";
            gameObject.tag = squareColorTag;
            squareColor = new Color32(0xC6, 0xC6, 0xC6, 0xFF);
        }

        spriteRenderer.color = squareColor;
    }

    public void HighlightSquare()
    {
        visibleHighlight = !visibleHighlight;

        if (visibleHighlight)
        {
            spriteRenderer.color = Color.Lerp(squareColor, specialColor, 0.5f);
        }
        else
        {
            spriteRenderer.color = squareColor;
        }
    }

    public bool IsHighlighted()
    {
        return visibleHighlight;
    }

    public void SetContainedPiece(ChessPiece piece)
    {
        containedPiece = piece;
    }

    public ChessPiece GetContainedPiece()
    {
        return containedPiece;
    }

    public Vector2Int GetSquarePosition()
    {
        return squarePosition;
    }

    public string GetSquarePositionCode()
    {
        return squarePositionCode;
    }
}
