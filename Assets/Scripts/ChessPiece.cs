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
        //Vector2 currentBoardPosition = SnapToBoardGrid(currentWorldPosition);

        return currentWorldPosition;
    }

    public Square GetCurrentSquare()
    {
        return currentSquare;
    }

    public Square GetInitialSquare()
    {
        return initialSquare;
    }

    public void SetCurrentSquare(Square newSquare)
    {
        currentSquare.SetContainedPiece(null);
        currentSquare = newSquare;

        if(currentSquare.GetContainedPiece() && currentSquare.GetContainedPiece().GetMyColorTag() != myPlayerColorTag)
        {
            Destroy(currentSquare.GetContainedPiece().gameObject); // Odrediti pravila jedenja
        }

        currentSquare.SetContainedPiece(this);
    }

    public void SnapPositionToCurrentSquare()
    {
        transform.position = currentSquare.transform.position;
    }

    public bool HaveIMoved()
    {
        return iMoved;
    }

    public void SetMovementActivity()
    {
        ResetMovementActivity();

        if (iMoved)
        {
            moveCounter++;
        }
    }

    public void ResetMovementActivity()
    {
        iMoved = !iMoved;
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

    public List<Square> GetPotentialMoves()
    {
        return potentialMoves;
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
        float myX = transform.position.x;
        float myY = transform.position.y - 1;

        ChessPiece myFirstRook = null;
        ChessPiece mySecondRook = null;
        ChessPiece myFartherHorse = null;

        if(moveCounter == 0)
        {
            myFirstRook = myBoard.GetSquareByVector3(new Vector3(7f, myY, 0f)).GetContainedPiece();
            mySecondRook = myBoard.GetSquareByVector3(new Vector3(-7f, myY, 0f)).GetContainedPiece();
            myFartherHorse = myBoard.GetSquareByVector3(new Vector3(-5f, myY, 0f)).GetContainedPiece();
        }

        List<Vector3> calculations = CalculateDiagonalMoveCoordinates(1f, true, true);
        calculations.AddRange(CalculateDiagonalMoveCoordinates(1f, false, true));
        calculations.AddRange(CalculateDiagonalMoveCoordinates(1f, true, false));
        calculations.AddRange(CalculateDiagonalMoveCoordinates(1f, false, false));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(1f, true, true));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(1f, false, true));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(2f, true, false));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(2f, false, false));

        foreach (Vector3 calculation in calculations)
        {
            Square square = myBoard.GetSquareByVector3(calculation);
            ChessPiece piece = square.GetContainedPiece();

            if(calculation.x == myX + 4)
            {
                if (myFirstRook && !piece && myFirstRook.GetNumberOfMoves() == 0)
                {
                    potentialMoves.Add(square);
                }
            }
            else if(calculation.x == myX - 4)
            {
                if (mySecondRook && !myFartherHorse && !piece && mySecondRook.GetNumberOfMoves() == 0)
                {
                    potentialMoves.Add(square);
                }
            }
            else if (!piece || (piece && piece.GetMyColorTag() != myPlayerColorTag))
            {
                potentialMoves.Add(square);
            }
        }
    }

    private void PotentialQueenMoves()
    {
        PotentialRookMoves();
        PotentialBishopMoves();
    }

    private void PotentialBishopMoves()
    {
        List<Vector3> calculations = CalculateDiagonalMoveCoordinates(7f, true, true);
        calculations.AddRange(CalculateDiagonalMoveCoordinates(7f, false, true));
        calculations.AddRange(CalculateDiagonalMoveCoordinates(7f, true, false));
        calculations.AddRange(CalculateDiagonalMoveCoordinates(7f, false, false));

        foreach (Vector3 calculation in calculations)
        {
            Square square = myBoard.GetSquareByVector3(calculation);
            ChessPiece piece = square.GetContainedPiece();

            if (!piece || (piece && piece.GetMyColorTag() != myPlayerColorTag))
            {
                potentialMoves.Add(square);
            }
        }
    }

    private void PotentialKnightMoves()
    {
        List<Vector3> calculations = CalculateLShapedMoveCoordinates();

        foreach(Vector3 calculation in calculations)
        {
            Square square = myBoard.GetSquareByVector3(calculation);
            ChessPiece piece = square.GetContainedPiece();

            if (square && !piece)
            {
                potentialMoves.Add(square);
            }
            else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag)
            {
                potentialMoves.Add(square);
            }
        }
    }

    private void PotentialRookMoves()
    {
        List<Vector3> calculations = CalculateRowOrColumnMoveCoordinates(7f, true, true);
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(7f, false, true));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(7f, true, false));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(7f, false, false));

        foreach (Vector3 calculation in calculations)
        {
            Square square = myBoard.GetSquareByVector3(calculation);
            ChessPiece piece = square.GetContainedPiece();

            if (!piece || (piece && piece.GetMyColorTag() != myPlayerColorTag))
            {
                potentialMoves.Add(square);
            }
        }
    }

    private void PotentialPawnMoves()
    {
        float numberOfPotentialForwardMoves = 2f;

        if(moveCounter > 0)
        {
            numberOfPotentialForwardMoves = 1f;
        }

        List<Vector3> calculations = new List<Vector3>();
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(numberOfPotentialForwardMoves, true, true));
        calculations.AddRange(CalculateDiagonalMoveCoordinates(1f, true, true));
        calculations.AddRange(CalculateDiagonalMoveCoordinates(1f, true, false));

        foreach(Vector3 calculation in calculations)
        {
            Square square = myBoard.GetSquareByVector3(calculation);
            ChessPiece piece = square.GetContainedPiece();

            if (!piece && calculation.x == transform.position.x)
            {
                potentialMoves.Add(square);
            }
            else if (piece && calculation.x != transform.position.x)
            {
                potentialMoves.Add(square);
            }
            else if (!piece && calculation.x != transform.position.x)
            {
                Square neighborSquare;

                if(myPlayerColorTag == "Light")
                {
                    neighborSquare = myBoard.GetSquareByVector3(new Vector3(calculation.x, calculation.y - 2, calculation.z));
                }
                else
                {
                    neighborSquare = myBoard.GetSquareByVector3(new Vector3(calculation.x, calculation.y + 2, calculation.z));
                }

                ChessPiece neighborPiece = neighborSquare.GetContainedPiece();

                if(neighborPiece && neighborPiece.GetMyColorTag() != myPlayerColorTag && neighborPiece.GetNumberOfMoves() == 1)
                {
                    potentialMoves.Add(square);
                }
            }
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
                Vector3 position;

                if (isVerticalMove)
                {
                    position = new Vector3(transform.position.x, newCoordinate, 0f);
                }
                else
                {
                    position = new Vector3(newCoordinate, transform.position.y - 1, 0f);
                }

                Square square = myBoard.GetSquareByVector3(position);
                ChessPiece piece = square.GetContainedPiece();

                if (square && !piece)
                {
                    calculations.Add(position);
                }
                else if(square && piece && piece.GetMyColorTag() != myPlayerColorTag)
                {
                    calculations.Add(position);
                    break;
                }
                else if(square && piece && piece.GetMyColorTag() == myPlayerColorTag)
                {
                    break;
                }
            }
        }

        return calculations;
    }

    private List<Vector3> CalculateDiagonalMoveCoordinates(float maxNumberOfMoves, bool moveForward, bool moveRight)
    {
        List<Vector3> calculations = new List<Vector3>();

        float signum = 1f;
        float xCoordinate = transform.position.x;
        float yCoordinate = transform.position.y - 1;
        float newXCoordinate;
        float newYCoordinate;
        float verticalDirection = 1f;
        float horizontalDirection = 1f;

        if (myPlayerColorTag == "Dark")
        {
            signum = -1f;
        }

        if (!moveForward)
        {
            verticalDirection = -1f;
        }

        if (!moveRight)
        {
            horizontalDirection = -1f;
        }

        for (float numberOfMoves = 1; numberOfMoves <= maxNumberOfMoves; numberOfMoves++)
        {
            newXCoordinate = xCoordinate + signum * numberOfMoves * 2f * horizontalDirection;
            newYCoordinate = yCoordinate + signum * numberOfMoves * 2f * verticalDirection;

            if (newXCoordinate > -8 && newXCoordinate < 8 && newYCoordinate > -8 && newYCoordinate < 8)
            {
                Vector3 position = new Vector3(newXCoordinate, newYCoordinate, 0f);

                Square square = myBoard.GetSquareByVector3(position);
                ChessPiece piece = square.GetContainedPiece();

                if (square && !piece)
                {
                    calculations.Add(position);
                }
                else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag)
                {
                    calculations.Add(position);
                    break;
                }
                else if (square && piece && piece.GetMyColorTag() == myPlayerColorTag)
                {
                    break;
                }
            }
        }

        return calculations;
    }

    private List<Vector3> CalculateLShapedMoveCoordinates()
    {
        List<Vector3> calculations = new List<Vector3>();
        List<Vector3> potentialCalculations = new List<Vector3>();

        float myX = transform.position.x;
        float myY = transform.position.y - 1;

        potentialCalculations.Add(new Vector3(myX + 2, myY + 4, 0f));
        potentialCalculations.Add(new Vector3(myX + 4, myY + 2, 0f));
        potentialCalculations.Add(new Vector3(myX + 4, myY - 2, 0f));
        potentialCalculations.Add(new Vector3(myX + 2, myY - 4, 0f));
        potentialCalculations.Add(new Vector3(myX - 2, myY - 4, 0f));
        potentialCalculations.Add(new Vector3(myX - 4, myY - 2, 0f));
        potentialCalculations.Add(new Vector3(myX - 4, myY + 2, 0f));
        potentialCalculations.Add(new Vector3(myX - 2, myY + 4, 0f));

        foreach(Vector3 calculation in potentialCalculations)
        {
            if(calculation.x > -8 && calculation.x < 8 && calculation.y > -8 && calculation.y < 8)
            {
                calculations.Add(calculation);
            }
        }

        return calculations;
    }
}
