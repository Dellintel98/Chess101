﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromotionModalBox : MonoBehaviour
{
    [SerializeField] public GameObject promotionPieceObjectPrefab;

    private Sprite[] myPieceSprites;
    private PromotionPieceElement[] myPromotionPieces;
    private ChessPiece myChessPiece;
    private Color32 myPieceColor;


    public void SetupPromotionalModalBox(ChessPiece chessPiece, Sprite[] sprites)
    {
        myChessPiece = chessPiece;
        myPromotionPieces = new PromotionPieceElement[4];
        myPieceSprites = new Sprite[4];

        for(int i = 0; i < 4; i++)
        {
            myPieceSprites[i] = sprites[i];
        }

        transform.GetComponent<SpriteRenderer>().color = Color.grey;
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
        myPromotionPieces[index].Setup(myPieceSprites[spriteIndex], myPieceColor);
    }

    private void OnMouseOver()
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
}
