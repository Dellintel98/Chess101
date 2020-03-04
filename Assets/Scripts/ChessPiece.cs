using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    [SerializeField] public Sprite[] pieceSprites;
    [SerializeField] public GameObject shadowPrefab;
    [SerializeField] public GameObject promotionModalBoxPrefab;

    private Color32 pieceColor;
    private string myPlayerColorTag;
    //private string pieceState;
    private string myInitialTag;
    private ChessSet myChessSet;
    private ChessBoard myBoard;
    private Square initialSquare;
    private Square currentSquare;
    private Square previousSquare;
    private List<Square> potentialMoves = new List<Square>();
    private bool attackingEnemyKing;
    private bool defendingMyKing;
    private bool iMoved;
    private int moveCounter;
    private GameObject myShadow;
    private bool visibleShadow;
    private PromotionModalBox myModalBox;


    public void InitializePiece(Square boardPosition, string playerColorTag, ChessBoard chessBoard, ChessSet playerChessSet)
    {
        int spriteIndex;

        myPlayerColorTag = playerColorTag;
        //pieceState = "alive";
        iMoved = false;
        moveCounter = 0;
        attackingEnemyKing = false;
        defendingMyKing = false;

        myChessSet = playerChessSet;
        myBoard = chessBoard;
        initialSquare = boardPosition;
        currentSquare = boardPosition;
        previousSquare = boardPosition;

        boardPosition.SetContainedPiece(this);

        spriteIndex = SetInitialPieceTag();
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

    public void SetShadowVisibility(bool isCastling)
    {
        if (!isCastling)
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

    private int SetInitialPieceTag()
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

        SetPieceTag(pieceTag);
        myInitialTag = pieceTag;
        SetPieceName(y);

        return spriteIndex;
    }

    private void SetPieceTag(string pieceTag)
    {
        gameObject.tag = pieceTag;
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

    private void SetPieceSpriteBySpriteElement(Sprite pieceSprite)
    {
        GetComponent<SpriteRenderer>().sprite = pieceSprite;
    }

    public void ChangePawnToSelectedPiece(Sprite pieceSprite, string pieceTag)
    {
        SetPieceSpriteBySpriteElement(pieceSprite);
        SetPieceTag(pieceTag);
    }

    public string GetMyColorTag()
    {
        return myPlayerColorTag;
    }

    public ChessSet GetMyChessSet()
    {
        return myChessSet;
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

    public Square GetPreviousSquare()
    {
        return previousSquare;
    }

    public void SetCurrentSquare(Square newSquare)
    {
        currentSquare.SetContainedPiece(null);
        previousSquare = currentSquare;
        currentSquare = newSquare;

        if(currentSquare.GetContainedPiece() && currentSquare.GetContainedPiece().GetMyColorTag() != myPlayerColorTag)
        {
            Destroy(currentSquare.GetContainedPiece().gameObject);
        }
        else if(!currentSquare.GetContainedPiece() && transform.tag == "Pawn")
        {
            Square neighborSquare;

            if (myPlayerColorTag == "Light")
            {
                neighborSquare = myBoard.GetSquareByVector3(new Vector3(currentSquare.transform.position.x, currentSquare.transform.position.y - 2, 0f));
            }
            else
            {
                neighborSquare = myBoard.GetSquareByVector3(new Vector3(currentSquare.transform.position.x, currentSquare.transform.position.y + 2, 0f));
            }

            ChessPiece neighborPiece = neighborSquare.GetContainedPiece();

            if (neighborPiece && neighborPiece.GetMyColorTag() != myPlayerColorTag && neighborPiece.GetNumberOfMoves() == 1)
            {
                Destroy(neighborSquare.GetContainedPiece().gameObject);
                neighborSquare.SetContainedPiece(null);
            }
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

    public void ShowPotentialMoves(bool isCastling)
    {
        if (!isCastling)
        {
            foreach (Square square in potentialMoves)
            {
                square.SetPotentialMoveMarkVisibility();
            }
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
            defendingMyKing = false;
            attackingEnemyKing = false;
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
        float numberOfPossibleMoves = 1f;

        ChessPiece myFirstRook = null;
        ChessPiece mySecondRook = null;
        ChessPiece myFartherHorse = null;
        bool isKingSideCastlingPossible = true;
        bool isQueenSideCastlingPossible = true;

        if(moveCounter == 0)
        {
            myFirstRook = myBoard.GetSquareByVector3(new Vector3(7f, myY, 0f)).GetContainedPiece();
            mySecondRook = myBoard.GetSquareByVector3(new Vector3(-7f, myY, 0f)).GetContainedPiece();
            myFartherHorse = myBoard.GetSquareByVector3(new Vector3(-5f, myY, 0f)).GetContainedPiece();

            numberOfPossibleMoves = 2f;
        }

        List<Vector3> calculations = CalculateDiagonalMoveCoordinates(1f, true, true);
        calculations.AddRange(CalculateDiagonalMoveCoordinates(1f, false, true));
        calculations.AddRange(CalculateDiagonalMoveCoordinates(1f, true, false));
        calculations.AddRange(CalculateDiagonalMoveCoordinates(1f, false, false));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(1f, true, true));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(1f, false, true));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(numberOfPossibleMoves, true, false));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(numberOfPossibleMoves, false, false));

        foreach (Vector3 calculation in calculations)
        {
            Square square = myBoard.GetSquareByVector3(calculation);
            ChessPiece piece = square.GetContainedPiece();

            bool isMovePossible = IsSquareSafeForKing(square);

            if(isMovePossible && calculation.x == myX + 4 && isKingSideCastlingPossible)
            {
                if (myFirstRook && !piece && myFirstRook.GetNumberOfMoves() == 0)
                {
                    potentialMoves.Add(square);
                }
            }
            else if(isMovePossible && calculation.x == myX - 4 && isQueenSideCastlingPossible)
            {
                if (mySecondRook && !myFartherHorse && !piece && mySecondRook.GetNumberOfMoves() == 0)
                {
                    potentialMoves.Add(square);
                }
            }
            else if (isMovePossible && !piece)
            {
                potentialMoves.Add(square);
            }
            else if (isMovePossible && piece && piece.GetMyColorTag() != myPlayerColorTag)
            {
                potentialMoves.Add(square);
            }
            else if (!isMovePossible && !piece)
            {
                if (calculation.x == myX + 2)
                {
                    isKingSideCastlingPossible = false;
                }
                else if (calculation.x == myX - 2)
                {
                    isQueenSideCastlingPossible = false;
                }
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

        if (!CanIBeMoved())
        {
            return;
        }

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

        foreach (Vector3 calculation in calculations)
        {
            Square square = myBoard.GetSquareByVector3(calculation);
            ChessPiece piece = square.GetContainedPiece();

            if (square && !piece)
            {
                potentialMoves.Add(square);
            }
            else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag && piece.tag != "King")
            {
                potentialMoves.Add(square);
            }
            else if (square && piece && piece.GetMyColorTag() == myPlayerColorTag && piece.tag == "King")
            {
                defendingMyKing = true;
            }
            else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag && piece.tag == "King")
            {
                //Ovdje staviti kod koji postavlja da je protivnički kralj napadnut, na temelju čega će se na početku OnMouseDown ispitati je li šah, šahmat, remi i sl.
                attackingEnemyKing = true;
            }
        }

        if (!CanIBeMoved())
        {
            potentialMoves.Clear();
            return;
        }
    }

    private void PotentialRookMoves()
    {
        List<Vector3> calculations = CalculateRowOrColumnMoveCoordinates(7f, true, true);
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(7f, false, true));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(7f, true, false));
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(7f, false, false));

        if (!CanIBeMoved())
        {
            return;
        }

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

        if (!CanIBeMoved())
        {
            return;
        }

        foreach (Vector3 calculation in calculations)
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
                ChessPiece piece = null;

                if (square)
                {
                    piece = square.GetContainedPiece();
                }

                if (square && !piece)
                {
                    calculations.Add(position);
                }
                else if(square && piece && piece.GetMyColorTag() != myPlayerColorTag && piece.tag != "King")
                {
                    calculations.Add(position);
                    break;
                }
                else if(square && piece && piece.GetMyColorTag() == myPlayerColorTag)
                {
                    if(piece.tag == "King")
                    {
                        defendingMyKing = true;
                    }
                    break;
                }
                else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag && piece.tag == "King")
                {
                    //Ovdje staviti kod koji postavlja da je protivnički kralj napadnut, na temelju čega će se na početku OnMouseDown ispitati je li šah, šahmat, remi i sl.
                    attackingEnemyKing = true;
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
                ChessPiece piece = null;

                if (square)
                {
                    piece = square.GetContainedPiece();
                }

                if (square && !piece)
                {
                    calculations.Add(position);
                }
                else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag && piece.tag != "King")
                {
                    calculations.Add(position);
                    break;
                }
                else if (square && piece && piece.GetMyColorTag() == myPlayerColorTag)
                {
                    if (piece.tag == "King")
                    {
                        defendingMyKing = true;
                    }
                    break;
                }
                else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag && piece.tag == "King")
                {
                    //Ovdje staviti kod koji postavlja da je protivnički kralj napadnut, na temelju čega će se na početku OnMouseDown ispitati je li šah, šahmat, remi i sl.
                    attackingEnemyKing = true;
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

    public void ShowPawnPromotionModalBox()
    {
        CreatePawnPromotionModalBox();
    }

    public void DestroyPawnPromotionModalBox()
    {
        Destroy(myModalBox.gameObject);
    }

    private void CreatePawnPromotionModalBox()
    {
        float myYCoordinate;

        myYCoordinate = transform.position.y - 3f;

        if(myPlayerColorTag == "Dark")
        {
            myYCoordinate = transform.position.y + 3f;
        }

        GameObject modalBox = Instantiate(promotionModalBoxPrefab, new Vector3(transform.position.x, myYCoordinate, -5f), Quaternion.Euler(0, 0, 90f), transform);

        myModalBox = modalBox.GetComponent<PromotionModalBox>();
        myModalBox.GetComponent<SpriteRenderer>().sortingLayerName = "PiecesLayer";

        myModalBox.SetupPromotionalModalBox(this, pieceSprites);
    }

    public bool IsSquareSafeForKing(Square desiredMove)
    {
        ChessSet enemySet = myChessSet.GetMyEnemyChessSet();

        if (enemySet.IsSquareAttackedByMyPieces(desiredMove))
        {
            return false;
        }
        else if (enemySet.IsSquareAttackedByMyKing(desiredMove))
        {
            // Postaviti nešto kako se ne bi moglo pomicati na to polje al' da je dostupno u potential moves zbog prethodne provjere u if-u
            //return false;
            return true;
        }

        return true;
    }

    public bool CanIBeMoved()
    {
        ChessSet enemySet = myChessSet.GetMyEnemyChessSet();

        if (defendingMyKing && enemySet.IsSquareAttackedByMyQueenRookBishop(currentSquare))
        {
            return false;
        }

        return true;
    }

    public bool AmIAttackingEnemyKing()
    {
        return attackingEnemyKing;
    }

    public bool IsMyKingUnderAttack()
    {
        return false;
    }
}
