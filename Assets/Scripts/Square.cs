using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Square : MonoBehaviour
{
    private Vector2Int squarePosition;
    private string squarePositionCode;
    private string squareColorTag;
    private Color32 squareColor;
    private string containedFigure;
    private SpriteRenderer spriteRenderer;

    public void Setup(int row, int column)
    {
        SetPositionOnBoard(row, column);
        SetContainedFigure("");
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
            squareColor = new Color32(0x70, 0x6F, 0x6F, 0xFF);
        }
        else
        {
            squareColorTag = "Light";
            gameObject.tag = squareColorTag;
            squareColor = new Color32(0xEE, 0xEE, 0xEE, 0xFF);
        }

        spriteRenderer.color = squareColor;
    }

    private void SetContainedFigure(string figureName)
    {
        containedFigure = figureName;
    }
}
