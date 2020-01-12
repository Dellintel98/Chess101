using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    [SerializeField] public Sprite[] pieceSprites;

    private string pieceTag;
    private Color32 pieceColor;
    //private string pieceState;
    private Square initialSquare;
    private Square currentSquare;
    private SpriteRenderer spriteRenderer;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializePiece(Square boardPosition, string playerColorTag)
    {
        int spriteIndex;

        //pieceState = "alive";
        initialSquare = boardPosition;
        currentSquare = boardPosition;

        boardPosition.SetContainedPiece(this);

        spriteIndex = SetPieceTag();
        SetPieceSprite(spriteIndex);

        SetPieceColor(playerColorTag);
    }

    private void SetPieceColor(string playerColorTag)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerColorTag == "Dark")
        {
            pieceColor = new Color32(0x00, 0x00, 0x00, 0xFF);
        }
        else
        {
            pieceColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        }

        spriteRenderer.color = pieceColor;
    }

    private int SetPieceTag()
    {
        int x = initialSquare.GetSquarePosition().x;
        int y = initialSquare.GetSquarePosition().y;
        int spriteIndex;

        if (x == 7 || x == 0)
        {
            if (y == 4)
            {
                pieceTag = "King";
                spriteIndex = 4;
            }
            else if (y == 3)
            {
                pieceTag = "Queen";
                spriteIndex = 3;
            }
            else if (y == 2 || y == 5)
            {
                pieceTag = "Bishop";
                spriteIndex = 2;
            }
            else if (y == 1 || y == 6)
            {
                pieceTag = "Knight";
                spriteIndex = 1;
            }
            else
            {
                pieceTag = "Rook";
                spriteIndex = 0;
            }
        }
        else
        {
            pieceTag = "Pawn";
            spriteIndex = 5;
        }

        gameObject.tag = pieceTag;
        
        return spriteIndex;
    }

    private void SetPieceSprite(int spriteIndex)
    {
        GetComponent<SpriteRenderer>().sprite = pieceSprites[spriteIndex];
    }

    //private void OnMouseDown()
    //{
    //    spriteRenderer = GetComponent<SpriteRenderer>();
    //    pieceColor = new Color32(0x80, 0x00, 0x00, 0xFF);
    //    spriteRenderer.color = pieceColor;
    //}
}
