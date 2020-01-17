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
        if (potentialMoves.Count > 0)
        {
            potentialMoves.Clear();
        }

        switch (transform.tag)
        {
            case "Pawn":
                PotentialPawnMoves();
                break;
            case "Rook":
                PotentialRookMoves();
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
        PotentialRookMoves();
        PotentialBishopMoves();
    }

    private void PotentialBishopMoves()
    {
        
    }

    private void PotentialKnightMoves()
    {
        
    }

    private void PotentialRookMoves()
    {
        List<Vector3> calculations = CalculateRowOrColumnMoveCoordinates(7f, true, true);
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(7f, false, true));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(7f, true, false));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(7f, false, false));

        foreach (Square square in myBoard.board)
        {
            ChessPiece piece = square.GetContainedPiece();

            if (!piece || (piece && piece.GetMyColorTag() != myPlayerColorTag))
            {
                if (calculations.Contains(square.transform.position))
                {
                    potentialMoves.Add(square);
                }
            }
        }
    }

    private void PotentialPawnMoves()
    {
        float numberOfPotentialForwardMoves = 2f;

        //  Ovaj brojač treba prebaciti na višu razinu, u ChessGameplayManager,
        //  jer broj poteza ovisi o igri i pojedinim setovima figura, a ne o samim figurama
        if(moveCounter > 0)
        {
            numberOfPotentialForwardMoves = 1f;
        }

        List<Vector3> calculations = CalculateRowOrColumnMoveCoordinates(numberOfPotentialForwardMoves, true, true);
        //Dodaj izračun za dijagonalno kretanje

        foreach (Square square in myBoard.board)
        {
            ChessPiece piece = square.GetContainedPiece();

            if (!piece || (piece && piece.GetMyColorTag() != myPlayerColorTag))
            {
                if (calculations.Contains(square.transform.position))
                {
                    potentialMoves.Add(square);
                }
            }

            // !!!!!  Dodaj način izbacivanja polja iz liste mogućih poteza, 
            // !!!!!  ako se između figure koja se pomiče i gledanog polja nalazi figura iste boje

            //if (piece && piece.GetMyColorTag() == myPlayerColorTag)
            //{
            //    break;
            //}
        }
    }

    private List<Vector3> CalculateRowOrColumnMoveCoordinates(float maxNumberOfMoves, bool moveForwardOrRight, bool isVerticalMove)
    {
        List<Vector3> calculations = new List<Vector3>();

        float signum = 1f;
        float coordinate;
        float newCoordinate;
        float direction = 1f;

        if (isVerticalMove)
        {
            coordinate = transform.position.y - 1;
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

        for(float numberOfMoves = 1; numberOfMoves <= maxNumberOfMoves; numberOfMoves++)
        {
            newCoordinate = coordinate + signum * numberOfMoves * 2f * direction;

            if (newCoordinate > -8 && newCoordinate < 8)
            {
                if (isVerticalMove)
                {
                    calculations.Add(new Vector3(transform.position.x, newCoordinate, 0f));
                }
                else
                {
                    calculations.Add(new Vector3(newCoordinate, transform.position.y - 1, 0f));
                }
            }
        }

        return calculations;
    }
}
