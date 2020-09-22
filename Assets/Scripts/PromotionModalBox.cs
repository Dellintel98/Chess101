using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromotionModalBox : MonoBehaviour
{
    [SerializeField] public GameObject promotionPieceObjectPrefab;

    private Sprite[] myPieceSprites;
    private PromotionPieceElement[] myPromotionPieces;
    private ChessPiece myChessPiece;
    private Color32 myPieceColor;
    private Color32 mySpecialColor = new Color32(0xDB, 0xDB, 0xDB, 0xFF);


    public void SetupPromotionalModalBox(ChessPiece chessPiece, SpritePieceSet sprites)
    {
        myChessPiece = chessPiece;
        myPromotionPieces = new PromotionPieceElement[4];
        myPieceSprites = new Sprite[4];

        myPieceSprites[0] = sprites.rook;
        myPieceSprites[1] = sprites.knight;
        myPieceSprites[2] = sprites.bishop;
        myPieceSprites[3] = sprites.queen;

        transform.GetComponent<SpriteRenderer>().color = mySpecialColor;
        myPieceColor = myChessPiece.transform.GetComponent<SpriteRenderer>().color;

        SetupPromotionPieces();
    }

    private void SetupPromotionPieces()
    {
        float promotionPieceYCoordinate;

        for (int i = 0; i < 4; i++)
        {
            promotionPieceYCoordinate = (transform.position.y - 3f) + i * 2;

            InstantiatePromotionPieces(i, promotionPieceYCoordinate);
        }
    }

    private void InstantiatePromotionPieces(int index, float pieceYCoordinate)
    {
        GameObject promotionPieceObject;
        int spriteIndex = index;

        if (myChessPiece.GetMyColorTag() == "Dark")
        {
            spriteIndex = 3 - index;
        }

        promotionPieceObject = Instantiate(promotionPieceObjectPrefab, new Vector3(transform.position.x, pieceYCoordinate, -6f), Quaternion.identity, transform);
        myPromotionPieces[index] = promotionPieceObject.GetComponent<PromotionPieceElement>();
        myPromotionPieces[index].Setup(myPieceSprites[spriteIndex], myPieceColor, myChessPiece, this);
    }

    public void OnHover()
    {
        Vector2 currentCursorPosition = GetCursorGridPosition();
        Vector2 hoveredPromotionPiece;

        for (int i = 0; i < 4; i++)
        {
            hoveredPromotionPiece = new Vector2(myPromotionPieces[i].transform.position.x, myPromotionPieces[i].transform.position.y);

            if (hoveredPromotionPiece == currentCursorPosition)
            {
                myPromotionPieces[i].Highlight();
            }
            else
            {
                myPromotionPieces[i].RemoveHighlight();
            }
        }
    }

    private Vector2 GetCursorGridPosition()
    {
        Vector2 currentCursorPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 currentWorldPosition = Camera.main.ScreenToWorldPoint(currentCursorPosition);
        Vector2 currentGridPosition = SnapToGrid(currentWorldPosition);

        return currentGridPosition;
    }

    private Vector2 SnapToGrid(Vector2 rawWorldPosition)
    {
        float newX = Mathf.RoundToInt(rawWorldPosition.x);
        float newY = Mathf.RoundToInt(rawWorldPosition.y);

        return new Vector2(newX, newY);
    }

    public string GetTagEqualToPieceSprite(Sprite pieceSprite)
    {
        string pieceTag = "";

        for (int i = 0; i < 4; i++)
        {
            if(myPieceSprites[i] == pieceSprite)
            {
                switch (i)
                {
                    case 0:
                        pieceTag = "Rook";
                        break;
                    case 1:
                        pieceTag = "Knight";
                        break;
                    case 2:
                        pieceTag = "Bishop";
                        break;
                    case 3:
                        pieceTag = "Queen";
                        break;
                }
            }
        }

        return pieceTag;
    }
}
