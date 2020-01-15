using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    [SerializeField] public Sprite[] pieceSprites;
    [SerializeField] public GameObject shadowPrefab;

    private Color32 pieceColor;
    private string myPlayerColorTag;
    //private string pieceState;
    private ChessBoard myBoard;
    private Square initialSquare;
    private Square currentSquare;
    private List<Square> potentialMoves = new List<Square>();
    private bool iMoved;
    private int moveCounter;
    private GameObject myShadow;
    private bool visibleShadow;

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializePiece(Square boardPosition, string playerColorTag, ChessBoard chessBoard)
    {
        int spriteIndex;

        myPlayerColorTag = playerColorTag;
        //pieceState = "alive";
        iMoved = false;
        moveCounter = 0;

        myBoard = chessBoard;
        initialSquare = boardPosition;
        currentSquare = boardPosition;

        boardPosition.SetContainedPiece(this);
        ComputePotentialMoves();

        spriteIndex = SetPieceTag();
        SetPieceSprite(spriteIndex);
        SetPieceColor();
        SetPieceSortingLayer();

        CreatePieceShadow();
    }

    private void CreatePieceShadow()
    {
        myShadow = Instantiate(shadowPrefab, new Vector3(transform.position.x, transform.position.y - 1.5f, 0f), Quaternion.identity, transform) as GameObject;
        myShadow.GetComponent<SpriteRenderer>().sortingLayerName = "PiecesLayer";
        visibleShadow = false;
        myShadow.SetActive(visibleShadow);
    }

    public void SetShadowVisibility()
    {
        visibleShadow = !visibleShadow;

        if (!visibleShadow)
        {
            transform.position = new Vector3(transform.position.x, myShadow.transform.position.y + 0.5f, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y + 1f, -5f);
        }

        myShadow.SetActive(visibleShadow);
    }

    private void SetPieceSortingLayer()
    {
        GetComponent<SpriteRenderer>().sortingLayerName = "PiecesLayer";
    }

    private void SetPieceColor()
    {
        if (myPlayerColorTag == "Dark")
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
        SetPieceName(y);

        return spriteIndex;
    }

    private void SetPieceName(int pieceIndex)
    {
        string index = $"{pieceIndex}";

        transform.name = $"{myPlayerColorTag}{transform.tag} ({index})";
    }

    private void SetPieceSprite(int spriteIndex)
    {
        GetComponent<SpriteRenderer>().sprite = pieceSprites[spriteIndex];
    }

    public string GetMyColorTag()
    {
        return myPlayerColorTag;
    }

    public void MovePiece()
    {
        Vector2 boardPosition = GetBoardPosition();

        gameObject.transform.position = new Vector3(boardPosition.x, boardPosition.y + 1f, -5f);
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

        if (iMoved)
        {
            moveCounter++;
        }
    }

    public int GetNumberOfMoves()
    {
        return moveCounter;
    }

    public void ShowPotentialMoves()
    {
        foreach(Square square in potentialMoves)
        {
            square.SetPotentialMoveMarkVisibility();
        }
    }

    public void RecomputePotentialMoves()
    {
        ComputePotentialMoves();
    }

    private void ComputePotentialMoves()
    {
        potentialMoves.Clear();

        switch (transform.tag)
        {
            case "Pawn":
                PotentialPawnMoves();
                break;
            case "Rook":
                PotentialRookMoves();
                ShowPotentialMoves();
                break;
            case "Knight":
                PotentialKnightMoves();
                break;
            case "Bishop":
                PotentialBishopMoves();
                break;
            case "Queen":
                PotentialQueenMoves();
                break;
            case "King":
                PotentialKingMoves();
                break;
            default:
                break;
        }
    }

    private void PotentialKingMoves()
    {
        
    }

    private void PotentialQueenMoves()
    {
        
    }

    private void PotentialBishopMoves()
    {
        
    }

    private void PotentialKnightMoves()
    {
        
    }

    private void PotentialRookMoves()
    {
        List<List<Vector3>> calculationsList = new List<List<Vector3>>();

        calculationsList.Add(CalculateRowOrColumnMoveCoordinates(7f, true, true));
        calculationsList.Add(CalculateRowOrColumnMoveCoordinates(7f, true, false));
        calculationsList.Add(CalculateRowOrColumnMoveCoordinates(7f, false, true));
        calculationsList.Add(CalculateRowOrColumnMoveCoordinates(7f, false, false));

        foreach (Square square in myBoard.board)
        {
            
            foreach(Vector3 calculation in calculationsList[0])
            {
                if(square.transform.position == calculation)
                {
                    potentialMoves.Add(square);
                }
            }

            
            foreach (Vector3 calculation in calculationsList[1])
            {
                if (square.transform.position == calculation)
                {
                    potentialMoves.Add(square);
                }
            }


            foreach (Vector3 calculation in calculationsList[2])
            {
                if (square.transform.position == calculation)
                {
                    potentialMoves.Add(square);
                }
            }

            
            foreach (Vector3 calculation in calculationsList[3])
            {
                if (square.transform.position == calculation)
                {
                    potentialMoves.Add(square);
                }
            }
        }
    }

    private void PotentialPawnMoves()
    {
        int myX = currentSquare.GetSquarePosition().x;
        int myY = currentSquare.GetSquarePosition().y;
        int signum;

        if(transform.tag == "Dark")
        {
            signum = -1;
        }
        else
        {
            signum = 1;
        }

        foreach(Square square in myBoard.board)
        {
            ChessPiece piece = square.GetContainedPiece();
            bool canIAdd = false;

            if (piece)
            {
                if (piece.GetMyColorTag() != myPlayerColorTag)
                {
                    if (square.GetSquarePosition().x == myX + (signum * 1))
                    {
                        canIAdd = true;
                    }
                    else if (moveCounter == 0 && square.GetSquarePosition().x == myX + (signum * 2))
                    {
                        canIAdd = true;
                    }
                    else if (square.GetSquarePosition().x == myX + signum * 1 && (square.GetSquarePosition().y == myY + 1 || square.GetSquarePosition().y == myY - 1))
                    {
                        canIAdd = true;
                    }
                }
            }
            else
            {
                if (square.GetSquarePosition().x == myX + (signum * 1))
                {
                    canIAdd = true;
                }
                else if (moveCounter == 0 && square.GetSquarePosition().x == myX + (signum * 2))
                {
                    canIAdd = true;
                }
                else if (square.GetSquarePosition().x == myX + signum * 1 && (square.GetSquarePosition().y == myY + 1 || square.GetSquarePosition().y == myY - 1))
                {
                    canIAdd = true;
                }
            }

            if (canIAdd)
            {
                potentialMoves.Add(square);
            }
        }
    }

    //private float CalculateRowMoveCoordinate(float numberOfMoves, bool moveForward)
    //{
    //    float signum = 1f;
    //    float row = transform.position.y;
    //    float newRow;
    //    float direction = 1f;

    //    if(myPlayerColorTag == "Dark")
    //    {
    //        signum = -1f;
    //    }

    //    if (!moveForward)
    //    {
    //        direction = -1f;
    //    }

    //    newRow = row + signum * numberOfMoves * 2f * direction;
        
    //    if(newRow > -8 && newRow < 8)
    //    {
    //        return newRow;
    //    }
    //    else
    //    {
    //        return 0;
    //    }
    //}

    private List<Vector3> CalculateRowOrColumnMoveCoordinates(float maxNumberOfMoves, bool moveForwardOrRight, bool isVerticalMove)
    {
        List<Vector3> calculations = new List<Vector3>();

        float signum = 1f;
        float coordinate;
        float newCoordinate;
        float direction = 1f;

        if (isVerticalMove)
        {
            coordinate = transform.position.y;
        }
        else
        {
            coordinate = transform.position.x;
        }

        if (myPlayerColorTag == "Dark")
        {
            signum = -1f;
        }

        if (!moveForwardOrRight)
        {
            direction = -1f;
        }

        for(float i = 0; i < maxNumberOfMoves; i++)
        {
            newCoordinate = coordinate + signum * maxNumberOfMoves * 2f * direction;

            if (newCoordinate > -8 && newCoordinate < 8)
            {
                if (isVerticalMove)
                {
                    calculations.Add(new Vector3(transform.position.x, newCoordinate, transform.position.z));
                }
                else
                {
                    calculations.Add(new Vector3(newCoordinate, transform.position.y, transform.position.z));
                }
            }
        }

        return calculations;
    }

    //private float CalculateColumnMoveCoordinate(float numberOfMoves, bool moveRight)
    //{
    //    float signum = 1f;
    //    float column = transform.position.x;
    //    float newColumn;
    //    float direction = 1f;

    //    if (myPlayerColorTag == "Dark")
    //    {
    //        signum = -1f;
    //    }

    //    if (!moveRight)
    //    {
    //        direction = -1f;
    //    }

    //    newColumn = column + signum * numberOfMoves * 2f * direction;

    //    if (newColumn > -8 && newColumn < 8)
    //    {
    //        return newColumn;
    //    }
    //    else
    //    {
    //        return 0;
    //    }
    //}
}
