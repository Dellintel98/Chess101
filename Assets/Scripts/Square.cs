using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    private Vector2Int squarePosition;
    private string squarePositionCode;
    private string squareColorTag;
    private Color32 squareColor;
    private SpriteRenderer spriteRenderer;
    private ChessPiece containedPiece;

    public void Setup(int row, int column)
    {
        SetPositionOnBoard(row, column);
        SetContainedPiece(null);
        SetColor();
    }

    private void SetPositionOnBoard(int row, int column)
    {
        squarePosition = new Vector2Int(row, column);

        char columnCode = (char)(column + 65);
        int rowCode = 8 - row;
        squarePositionCode = $"{columnCode}{rowCode}";

        gameObject.name = "Square" + squarePositionCode;
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
