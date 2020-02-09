using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessGameplayManager : MonoBehaviour
{
    //private int currentRound;
    //private bool isThereAWinner;
    private ChessSet[] mySets = new ChessSet[2];
    private ChessBoard myBoard;
    private ChessPiece activePiece;

    public void InitializeGame(ChessSet[] sets, ChessBoard chessBoard)
    {
        //isThereAWinner = false;
        //currentRound = 0;
        mySets = sets;
        myBoard = chessBoard;
        activePiece = null;
    }

    private void OnMouseDown()
    {
        Vector2 currentPosition = GetBoardPosition();
        float currentX = currentPosition.x;
        float currentY = currentPosition.y;

        foreach (Square square in myBoard.board)
        {
            if (!activePiece && square.GetContainedPiece() && square.transform.position.x == currentX && square.transform.position.y == currentY)
            {
                if (!square.IsHighlighted())
                {
                    square.HighlightSquare(isCastling:false);
                }
                activePiece = square.GetContainedPiece();

                if (activePiece.HaveIMoved())
                {
                    activePiece.ResetMovementActivity();
                }

                activePiece.SetShadowVisibility(isCastling:false);
                activePiece.RecomputePotentialMoves();
                activePiece.ShowPotentialMoves(isCastling:false);
                break;
            }
        }
    }

    private void OnMouseDrag()
    {
        if (activePiece)
        {
            activePiece.MovePiece();
        }
    }

    private void OnMouseUp()
    {
        bool moveCondition = false;
        float rawX = 0f;
        float rawY = 0f;
        Vector2 currentPosition = GetBoardPosition();
        float currentX = currentPosition.x;
        float currentY = currentPosition.y;

        if (activePiece)
        {
            rawX = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y)).x;
            rawY = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y)).y;
        }
        CheckIfPieceIsOutOfBoard(rawX, rawY);
        currentX = PositionIfPieceIsBetweenSquares(rawX, currentX, true);
        currentY = PositionIfPieceIsBetweenSquares(rawY, currentY, false);

        foreach (Square square in myBoard.board)
        {
            if (activePiece && activePiece.GetPotentialMoves().Contains(square))
            {
                moveCondition = true;
            }
            else
            {
                moveCondition = false;
            }

            if (activePiece && moveCondition && square.transform.position.x == currentX && square.transform.position.y == currentY)
            {
                SetupPieceAfterMovement(square, activePiece, isCastling:false);
                break;
            }
        }

        ResetActivePieceIfThereIsNoPossibleMove(moveCondition);
        activePiece = null;
    }

    private void SetupPieceAfterMovement(Square square, ChessPiece piece, bool isCastling)
    {
        bool specialMoveSuccessful = true;
        square.HighlightSquare(isCastling:false);
        piece.GetCurrentSquare().HighlightSquare(isCastling);
        piece.ShowPotentialMoves(isCastling);
        piece.SetCurrentSquare(square);
        piece.SetShadowVisibility(isCastling);
        piece.SnapPositionToCurrentSquare();

        if (piece.tag == "King" || piece.tag == "Pawn")
        {
            specialMoveSuccessful = SpecialMovementActive();
        }

        if (specialMoveSuccessful && piece.GetCurrentSquare() != piece.GetPreviousSquare())
        {
            piece.SetMovementActivity();
        }

        if (!specialMoveSuccessful)
        {

        }
    }

    private bool SpecialMovementActive()
    {
        bool specialMoveSuccessful = false;

        if(activePiece.tag == "King")
        {
            float previousXPosition = activePiece.GetPreviousSquare().transform.position.x;
            float currentXPosition = activePiece.GetCurrentSquare().transform.position.x;

            float differenceInPositions = currentXPosition - previousXPosition;

            if(activePiece.GetNumberOfMoves() == 0 && (differenceInPositions == 4 || differenceInPositions == -4))
            {
                try
                {
                    Castling(differenceInPositions);
                    specialMoveSuccessful = true;
                }
                catch
                {
                    specialMoveSuccessful = false;
                }
            }
        }
        else
        {
            float currentYPosition = activePiece.GetCurrentSquare().transform.position.y;

            if((currentYPosition == -7) || (currentYPosition == 7))
            {
                try
                {
                    PawnPromotion();
                    specialMoveSuccessful = true;
                }
                catch
                {
                    specialMoveSuccessful = false;
                }
            }
        }

        return specialMoveSuccessful;
    }

    private void PawnPromotion()
    {
        Exception PawnPromotionNotPossibleException = new Exception();

        try
        {
            activePiece.ShowPawnPromotionModalBox();
        }
        catch
        {
            throw PawnPromotionNotPossibleException;
        }
    }

    private void Castling(float differenceInPositions)
    {
        Exception CastlingNotPossibleException = new Exception();
        ChessPiece castlingRook;
        Square destinationSquare = null;
        int setIndex;
        float myX = activePiece.transform.position.x;
        float myY = activePiece.transform.position.y;
        float myZ = activePiece.transform.position.z;

        setIndex = activePiece.GetMyChessSet().GetMySetIndex();

        if (differenceInPositions == 4)
        {
            castlingRook = mySets[setIndex].GetPieceByTagAndVector3("Rook", new Vector3(myX + 2, myY, myZ));
            destinationSquare = myBoard.GetSquareByVector3(new Vector3(myX - 2, myY, myZ));
        }
        else if (differenceInPositions == -4)
        {
            castlingRook = mySets[setIndex].GetPieceByTagAndVector3("Rook", new Vector3(myX - 4, myY, myZ));
            destinationSquare = myBoard.GetSquareByVector3(new Vector3(myX + 2, myY, myZ));
        }
        else
        {
            throw CastlingNotPossibleException;
        }

        if (castlingRook)
        {
            if (castlingRook.GetNumberOfMoves() == 0)
            {
                SetupPieceAfterMovement(destinationSquare, castlingRook, isCastling: true);
            }
            else
            {
                throw CastlingNotPossibleException;
            }
        }
        else
        {
            throw CastlingNotPossibleException;
        } 
    }

    private void ResetActivePieceIfThereIsNoPossibleMove(bool moveCondition)
    {
        if (!moveCondition && activePiece)
        {
            activePiece.GetCurrentSquare().HighlightSquare(isCastling:false);
            activePiece.ShowPotentialMoves(isCastling:false);
            activePiece.SetShadowVisibility(isCastling:false);
            activePiece.SnapPositionToCurrentSquare();
        }
    }

    private float PositionIfPieceIsBetweenSquares(float rawCoordinate, float currentCoordinate, bool xDirection)
    {
        float distanceToFirstNeighborSquareCoordinate;
        float distanceToSecondNeighborSquareCoordinate;

        if (activePiece && currentCoordinate % 2 == 0)
        {
            float firstNeighborSquareCoordinate = currentCoordinate - 1;
            float secondNeighborSquareCoordinate = currentCoordinate + 1;

            if (xDirection)
            {
                distanceToFirstNeighborSquareCoordinate = Mathf.Abs(firstNeighborSquareCoordinate - rawCoordinate);
                distanceToSecondNeighborSquareCoordinate = Mathf.Abs(secondNeighborSquareCoordinate - rawCoordinate);
            }
            else
            {
                distanceToFirstNeighborSquareCoordinate = firstNeighborSquareCoordinate - rawCoordinate;
                distanceToSecondNeighborSquareCoordinate = secondNeighborSquareCoordinate - rawCoordinate;
            }

            if (distanceToFirstNeighborSquareCoordinate <= distanceToSecondNeighborSquareCoordinate)
            {
                currentCoordinate = firstNeighborSquareCoordinate;
            }
            else
            {
                currentCoordinate = secondNeighborSquareCoordinate;
            }

            if (currentCoordinate == -8)
            {
                currentCoordinate += 1;
            }
            else if (currentCoordinate == 8)
            {
                currentCoordinate -= 1;
            }
        }

        return currentCoordinate;
    }

    private void CheckIfPieceIsOutOfBoard(float rawX, float rawY)
    {
        if (activePiece && (rawY < -8 || rawY > 8 || rawX < -8 || rawX > 8))
        {
            activePiece.GetCurrentSquare().HighlightSquare(isCastling:false);
            activePiece.ShowPotentialMoves(isCastling:false);
            activePiece.SetShadowVisibility(isCastling:false);
            activePiece.SnapPositionToCurrentSquare();
            activePiece = null;
        }
    }

    private Vector2 GetBoardPosition()
    {
        Vector2 currentCursorPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 currentWorldPosition = Camera.main.ScreenToWorldPoint(currentCursorPosition);
        Vector2 currentBoardPosition = SnapToBoardGrid(currentWorldPosition);

        return currentBoardPosition;
    }

    private Vector2 SnapToBoardGrid(Vector2 rawWorldPosition)
    {
        float newX = Mathf.RoundToInt(rawWorldPosition.x);
        float newY = Mathf.RoundToInt(rawWorldPosition.y);

        return new Vector2(newX, newY);
    }
}
