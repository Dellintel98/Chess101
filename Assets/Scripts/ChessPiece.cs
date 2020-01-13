using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    [SerializeField] public Sprite[] pieceSprites;
    [SerializeField] public GameObject shadowPrefab;

    private Color32 pieceColor;
    //private string pieceState;
    private Square initialSquare;
    private Square currentSquare;
    private bool iMoved;
    private GameObject myShadow;
    private bool visibleShadow;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializePiece(Square boardPosition, string playerColorTag)
    {
        int spriteIndex;

        //pieceState = "alive";
        iMoved = false;

        initialSquare = boardPosition;
        currentSquare = boardPosition;

        boardPosition.SetContainedPiece(this);

        spriteIndex = SetPieceTag();
        SetPieceSprite(spriteIndex);

        SetPieceColor(playerColorTag);

        SetPieceSortingLayer();

        CreatePieceShadow();
    }

    private void CreatePieceShadow()
    {
        myShadow = Instantiate(shadowPrefab, new Vector3(transform.position.x, transform.position.y - (float)1.5, 0f), Quaternion.identity, transform) as GameObject;
        myShadow.GetComponent<SpriteRenderer>().sortingLayerName = "PiecesLayer";
        visibleShadow = false;
        myShadow.SetActive(visibleShadow);
    }

    public void SetShadowVisibility()
    {
        visibleShadow = !visibleShadow;
        myShadow.SetActive(visibleShadow);

        if (!visibleShadow)
        {
            transform.position = new Vector3(transform.position.x, myShadow.transform.position.y + (float)0.5, transform.position.z);
        }
    }

    private void SetPieceSortingLayer()
    {
        GetComponent<SpriteRenderer>().sortingLayerName = "PiecesLayer";
    }

    private void SetPieceColor(string playerColorTag)
    {
        if (playerColorTag == "Dark")
        {
            pieceColor = new Color32(0x00, 0x00, 0x00, 0xFF);
        }
        else
        {
            pieceColor = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
        }

        GetComponent<SpriteRenderer>().color = pieceColor;
    }

    private int SetPieceTag()
    {
        int x = initialSquare.GetSquarePosition().x;
        int y = initialSquare.GetSquarePosition().y;
        int spriteIndex;
        string pieceTag;

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

    //private void OnMouseUp()
    //{
    //    Vector3 position = gameObject.transform.position;
    //    gameObject.transform.position = new Vector3(position.x, position.y, 0f);
    //    SetMovementActivity();
    //}

    public void MovePiece()
    {
        Vector2 boardPosition = GetBoardPosition();

        gameObject.transform.position = new Vector3(boardPosition.x, boardPosition.y + 1, -5f);
    }

    private Vector2 GetBoardPosition()
    {
        Vector2 currentCursorPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 currentWorldPosition = Camera.main.ScreenToWorldPoint(currentCursorPosition);
        Vector2 currentBoardPosition = SnapToBoardGrid(currentWorldPosition);

        return currentBoardPosition;
    }

    public Square GetCurrentSquare()
    {
        return currentSquare;
    }

    public void SetCurrentSquare(Square newSquare)
    {
        currentSquare.SetContainedPiece(null);
        currentSquare = newSquare;
        currentSquare.SetContainedPiece(this);
    }

    private Vector2 SnapToBoardGrid(Vector2 rawWorldPosition)
    {
        float newX = Mathf.RoundToInt(rawWorldPosition.x);
        float newY = Mathf.RoundToInt(rawWorldPosition.y);

        return new Vector2(newX, newY);
    }

    public bool HaveIMoved()
    {
        return iMoved;
    }

    public void SetMovementActivity()
    {
        iMoved = !iMoved;
    }
}
