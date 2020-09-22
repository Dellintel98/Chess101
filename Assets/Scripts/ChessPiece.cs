using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    [SerializeField] public GameObject shadowPrefab;
    [SerializeField] public GameObject promotionModalBoxPrefab;

    private Color32 pieceColor;
    private string myPlayerColorTag;
    private string pieceState;
    private bool justBeenPromoted;
    private bool kingSideCastling;
    private bool queenSideCastling;
    private string myInitialTag;
    private ChessSet myChessSet;
    private ChessBoard myBoard;
    private Square initialSquare;
    private Square currentSquare;
    private Square previousSquare;
    private List<Square> potentialMoves = new List<Square>();
    private List<Square> mainPieceMoves = new List<Square>();   // Only for king and pawn
    private bool attackingEnemyKing;
    private bool defendingMyKing;
    private bool amIBlocked;
    private bool amIProtectedByMyPieces;
    private bool iMoved;
    private bool iCapturedEnemyPiece;
    private int moveCounter;
    private GameObject myShadow;
    private bool visibleShadow;
    private PromotionModalBox myModalBox;
    private bool isPromotionModalBoxOpen;
    private ChessGameplayManager myGame;
    private SpritePieceSet spritePieceSet = new SpritePieceSet();


    public void InitializePiece(Square boardPosition, string playerColorTag, ChessBoard chessBoard, ChessSet playerChessSet, Color32 pieceSetColor, SpritePieceSet spriteSet)
    {
        myPlayerColorTag = playerColorTag;
        pieceState = "Alive";
        iMoved = false;
        iCapturedEnemyPiece = false;
        justBeenPromoted = false;
        isPromotionModalBoxOpen = false;
        kingSideCastling = false;
        queenSideCastling = false;
        moveCounter = 0;
        attackingEnemyKing = false;
        defendingMyKing = false;
        amIBlocked = false;
        amIProtectedByMyPieces = false;

        myChessSet = playerChessSet;
        myBoard = chessBoard;
        initialSquare = boardPosition;
        currentSquare = boardPosition;
        previousSquare = boardPosition;

        boardPosition.SetContainedPiece(this);

        SetInitialPieceTag();
        spritePieceSet = spriteSet;
        SetPieceSprite();

        pieceColor = pieceSetColor;
        GetComponent<SpriteRenderer>().color = pieceColor;

        //SetPieceColor();
        SetPieceSortingLayer();

        CreatePieceShadow();
    }

    public string GetPieceState()
    {
        return pieceState;
    }

    public void SetPieceState(string newState)
    {
        if(newState == "Dead" || newState == "Promoted")
        {
            pieceState = newState;
        }
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

    private void SetInitialPieceTag()
    {
        int x = initialSquare.GetSquarePosition().x;
        int y = initialSquare.GetSquarePosition().y;
        string pieceTag;

        if (x == 7 || x == 0)
        {
            if (y == 4)
            {
                pieceTag = "King";
            }
            else if (y == 3)
            {
                pieceTag = "Queen";
            }
            else if (y == 2 || y == 5)
            {
                pieceTag = "Bishop";
            }
            else if (y == 1 || y == 6)
            {
                pieceTag = "Knight";
            }
            else
            {
                pieceTag = "Rook";
            }
        }
        else
        {
            pieceTag = "Pawn";
        }

        SetPieceTag(pieceTag);
        myInitialTag = pieceTag;
        SetPieceName(y);
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

    private void SetPieceSprite()
    {
        switch(tag)
        {
            case "Rook":
                GetComponent<SpriteRenderer>().sprite = spritePieceSet.rook;
                break;
            case "Knight":
                GetComponent<SpriteRenderer>().sprite = spritePieceSet.knight;
                break;
            case "Bishop":
                GetComponent<SpriteRenderer>().sprite = spritePieceSet.bishop;
                break;
            case "Queen":
                GetComponent<SpriteRenderer>().sprite = spritePieceSet.queen;
                break;
            case "King":
                GetComponent<SpriteRenderer>().sprite = spritePieceSet.king;
                break;
            case "Pawn":
                GetComponent<SpriteRenderer>().sprite = spritePieceSet.pawn;
                break;
        }
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
            currentSquare.GetContainedPiece().gameObject.SetActive(false);
            currentSquare.GetContainedPiece().SetPieceState("Dead");
            iCapturedEnemyPiece = true;
        }
        else if(!currentSquare.GetContainedPiece() && transform.tag == "Pawn")
        {
            if(Mathf.Abs(currentSquare.transform.position.y - previousSquare.transform.position.y) == 4f)
            {
                pieceState = "Possible EnPassant";
            }

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
                neighborSquare.GetContainedPiece().gameObject.SetActive(false);
                neighborSquare.GetContainedPiece().SetPieceState("Dead");
                neighborSquare.SetContainedPiece(null);
                iCapturedEnemyPiece = true;
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

            if (tag == "Pawn" && pieceState == "Possible EnPassant" && moveCounter > 1)
            {
                pieceState = "Alive";
            }
        }
    }

    public void IncreaseMoveCounter()
    {
        moveCounter++;
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

    public void ClearPotentialMovesIfDeadOrBlocked()
    {
        if (pieceState == "Dead")
        {
            potentialMoves.Clear();
            mainPieceMoves.Clear();
            attackingEnemyKing = false;
        }
        else if (amIBlocked)
        {
            potentialMoves.Clear();
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
            defendingMyKing = false;
            amIBlocked = false;
        }
            attackingEnemyKing = false;

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
        if (mainPieceMoves.Count > 0)
        {
            mainPieceMoves.Clear();
            defendingMyKing = false;
            attackingEnemyKing = false;
            amIBlocked = false;
        }

        float myX = transform.position.x;
        float myY = transform.position.y;

        if(myY % 2 == 0)
        {
            myY--;
        }

        float numberOfPossibleMoves = 1f;

        ChessPiece myFirstRook = null;
        ChessPiece mySecondRook = null;
        ChessPiece myFartherHorse = null;

        if(moveCounter == 0)
        {
            Square temporarySquare = myBoard.GetSquareByVector3(new Vector3(7f, myY, 0f));

            if (temporarySquare)
            {
                myFirstRook = temporarySquare.GetContainedPiece();
            }

            temporarySquare = myBoard.GetSquareByVector3(new Vector3(-7f, myY, 0f));

            if (temporarySquare)
            {
                mySecondRook = temporarySquare.GetContainedPiece();
            }

            temporarySquare = myBoard.GetSquareByVector3(new Vector3(-5f, myY, 0f));

            if (temporarySquare)
            {
                myFartherHorse = temporarySquare.GetContainedPiece();
            }
            //myFirstRook = myBoard.GetSquareByVector3(new Vector3(7f, myY, 0f)).GetContainedPiece();
            //mySecondRook = myBoard.GetSquareByVector3(new Vector3(-7f, myY, 0f)).GetContainedPiece();
            //myFartherHorse = myBoard.GetSquareByVector3(new Vector3(-5f, myY, 0f)).GetContainedPiece();

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

            if (isMovePossible && calculation.x == myX + 4)
            {
                Square temporarySquare = myBoard.GetSquareByVector3(new Vector3(myX + 2, myY, 0f));

                bool isKingSideCastlingPossible = IsSquareSafeForKing(temporarySquare);
                if (isKingSideCastlingPossible && myFirstRook && !piece && myFirstRook.GetNumberOfMoves() == 0)
                {
                    potentialMoves.Add(square);
                }
            }
            else if(isMovePossible && calculation.x == myX - 4)
            {
                Square temporarySquare = myBoard.GetSquareByVector3(new Vector3(myX - 2, myY, 0f));

                bool isQueenSideCastlingPossible = IsSquareSafeForKing(temporarySquare);
                if (isQueenSideCastlingPossible && mySecondRook && !myFartherHorse && !piece && mySecondRook.GetNumberOfMoves() == 0)
                {
                    potentialMoves.Add(square);
                }
            }
            else if (isMovePossible && !piece)
            {
                potentialMoves.Add(square);
                mainPieceMoves.Add(square);
            }
            else if (isMovePossible && piece && piece.GetMyColorTag() != myPlayerColorTag)
            {
                potentialMoves.Add(square);
                mainPieceMoves.Add(square);
            }
            else if(!isMovePossible && calculation.x != myX - 4 && calculation.x != myX + 4)
            {
                mainPieceMoves.Add(square);
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

        foreach (Vector3 calculation in calculations)
        {
            Square square = myBoard.GetSquareByVector3(calculation);
            ChessPiece piece = null;

            if (square)
            {
                piece = square.GetContainedPiece();
            }

            if (square && !piece)
            {
                potentialMoves.Add(square);
            }
            else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag && piece.tag != "King")
            {
                potentialMoves.Add(square);
            }
            else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag && piece.tag == "King")
            {
                attackingEnemyKing = true;
            }
            else if(square && piece && piece.GetMyColorTag() == myPlayerColorTag)
            {
                piece.SetPieceProtectionValue(protectionValue: true);
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
        if(mainPieceMoves.Count > 0)
        {
            mainPieceMoves.Clear();
            defendingMyKing = false;
            attackingEnemyKing = false;
            amIBlocked = false;
        }

        float numberOfPotentialForwardMoves = 2f;

        if(moveCounter > 0)
        {
            numberOfPotentialForwardMoves = 1f;
        }

        List<Vector3> calculations = new List<Vector3>();
        calculations.AddRange(CalculateRowOrColumnMoveCoordinates(numberOfPotentialForwardMoves, true, true));
        calculations.AddRange(CalculateDiagonalMoveCoordinates(1f, true, true));
        calculations.AddRange(CalculateDiagonalMoveCoordinates(1f, true, false));

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

                if(neighborPiece && neighborPiece.GetMyColorTag() != myPlayerColorTag && neighborPiece.GetNumberOfMoves() == 1 && neighborPiece.GetPieceState() == "Possible EnPassant")
                {
                    potentialMoves.Add(square);
                }
            }

            if(calculation.x != transform.position.x)
            {
                mainPieceMoves.Add(square);
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
            coordinate = transform.position.y;

            if(coordinate % 2 == 0)
            {
                coordinate--;
            }
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
                    float yCoordinate = transform.position.y;
                    if(yCoordinate % 2 == 0)
                    {
                        yCoordinate--;
                    }

                    position = new Vector3(newCoordinate, yCoordinate, 0f);
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
                    if(tag == "King" || tag == "Queen" || tag == "Rook")
                    {
                        piece.SetPieceProtectionValue(protectionValue: true);
                    }

                    break;
                }
                else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag && piece.tag == "King" && tag != "Pawn")
                {
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
        float yCoordinate = transform.position.y;
        float newXCoordinate;
        float newYCoordinate;
        float verticalDirection = 1f;
        float horizontalDirection = 1f;

        if(yCoordinate % 2 == 0)
        {
            yCoordinate--;
        }

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
                    if (tag == "King" || tag == "Queen" || tag == "Bishop" || tag == "Pawn")
                    {
                        piece.SetPieceProtectionValue(protectionValue: true);
                    }

                    break;
                }
                else if (square && piece && piece.GetMyColorTag() != myPlayerColorTag && piece.tag == "King")
                {
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
        float myY = transform.position.y;
        
        if(myY % 2 == 0)
        {
            myY--;
        }

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

    public void ShowPawnPromotionModalBox(ChessGameplayManager game)
    {
        isPromotionModalBoxOpen = true;
        CreatePawnPromotionModalBox();
        justBeenPromoted = true;
        myGame = game;
        myGame.SetPromotionInProcess();
    }

    public void DestroyPawnPromotionModalBox()
    {
        Destroy(myModalBox.gameObject);
        isPromotionModalBoxOpen = false;
        myChessSet.SetPawnPromotingState(false);
        myGame.SetPromotionProcessEnd();
        myGame.SetupPawnAfterPromotion(this);
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

        myModalBox.SetupPromotionalModalBox(this, spritePieceSet);
        myChessSet.SetPawnPromotingState(true);
    }

    public bool IsSquareSafeForKing(Square desiredMove)
    {
        ChessSet enemySet = myChessSet.GetMyEnemyChessSet();
        ChessPiece pieceOnDesiredMove = desiredMove.GetContainedPiece();

        if (enemySet.IsSquareAttackedByMyPieces(desiredMove, currentSquare))
        {
            return false;
        }
        else if (enemySet.IsSquareAttackedByMyKing(desiredMove))
        {
            return false;
        }
        else if (pieceOnDesiredMove)
        {
            if(pieceOnDesiredMove.GetMyColorTag() != myPlayerColorTag && pieceOnDesiredMove.AmIProtectedByMyPieces())
            {
                return false;
            }
        }

        return true;
    }

    public bool AmIAttackingEnemyKing()
    {
        return attackingEnemyKing;
    }

    public bool IsMyKingUnderAttack()
    {
        ChessSet enemySet = myChessSet.GetMyEnemyChessSet();

        return enemySet.AmIAttackingOpponentKing();
    }

    public void PromoteToKingDefender(bool promotionValue, ChessPiece attackerPiece=null)
    {
        Square temporarySquare = null;

        defendingMyKing = promotionValue;

        if (defendingMyKing)
        {
            if (attackerPiece && tag != "Pawn" && potentialMoves.Contains(attackerPiece.GetCurrentSquare()))
            {
                temporarySquare = attackerPiece.GetCurrentSquare();
            }
            else if(attackerPiece && tag == "Pawn" && mainPieceMoves.Contains(attackerPiece.GetCurrentSquare()))
            {
                temporarySquare = attackerPiece.GetCurrentSquare();
            }

            potentialMoves.Clear();

            if (temporarySquare)
            {
                potentialMoves.Add(temporarySquare);
            }
        }
    }

    public void SetBlockValueOfAPiece(bool blockValue)
    {
        amIBlocked = blockValue;
    }

    public bool AmIBlocked()
    {
        return amIBlocked;
    }

    public bool AmIDefendingMyKing()
    {
        return defendingMyKing;
    }

    public List<Square> GetMainPieceMoves()
    {
        return mainPieceMoves;
    }

    public void ClearMovesThatCannotProtectKing(List<Square> movesThatCanProtectKing)
    {
        if(tag != "King" && !amIBlocked)
        {
            potentialMoves.Clear();
            potentialMoves.AddRange(movesThatCanProtectKing);
        }
    }

    public void SetPieceProtectionValue(bool protectionValue)
    {
        amIProtectedByMyPieces = protectionValue;
    }

    public bool AmIProtectedByMyPieces()
    {
        return amIProtectedByMyPieces;
    }

    public bool DidICaptureEnemyPiece()
    {
        return iCapturedEnemyPiece;
    }

    public void ResetCaptureEnemyPiece()
    {
        iCapturedEnemyPiece = false;
    }

    public string GetMyInitialTag()
    {
        return myInitialTag;
    }

    public bool HaveIJustBeenPromoted()
    {
        return justBeenPromoted;
    }

    public void ResetPromotionState()
    {
        justBeenPromoted = false;
    }

    public bool GetCastling(string castlingType)
    {
        if (castlingType == "Kingside")
            return kingSideCastling;
        else if (castlingType == "Queenside")
            return queenSideCastling;
        else if (castlingType == "Either")
        {
            if (kingSideCastling)
                return kingSideCastling;
            else if (queenSideCastling)
                return queenSideCastling;
        }
        
        return false;
    }

    public void ResetCastling()
    {
        kingSideCastling = false;
        queenSideCastling = false;
    }

    public void SetCastling(string castlingType)
    {
        if (castlingType == "Kingside")
            kingSideCastling = true;
        else if (castlingType == "Queenside")
            queenSideCastling = true;
        else
            ResetCastling();
    }

    public bool GetStateOfPromotionModalBox()
    {
        return isPromotionModalBoxOpen;
    }

    public int GetNumberOfPotentialMoves()
    {
        return potentialMoves.Count;
    }
}
