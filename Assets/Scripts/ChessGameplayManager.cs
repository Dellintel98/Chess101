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
                    square.HighlightSquare();
                }
                activePiece = square.GetContainedPiece();

                if (activePiece.HaveIMoved())
                {
                    activePiece.ResetMovementActivity();
                }

                activePiece.SetShadowVisibility();
                activePiece.RecomputePotentialMoves();
                activePiece.ShowPotentialMoves();
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
                SetupActivePieceAfterMovement(square);
                break;
            }
        }

        ResetActivePieceIfThereIsNoPossibleMove(moveCondition);
        activePiece = null;
    }

    private void SetupActivePieceAfterMovement(Square square)
    {
        square.HighlightSquare();
        activePiece.GetCurrentSquare().HighlightSquare();
        activePiece.ShowPotentialMoves();
        activePiece.SetCurrentSquare(square);
        activePiece.SetShadowVisibility();
        activePiece.SnapPositionToCurrentSquare();

        if (activePiece.tag == "King" || activePiece.tag == "Pawn")
        {
            SpecialMovementActive();
        }

        if (activePiece.GetCurrentSquare() != activePiece.GetPreviousSquare())
        {
            activePiece.SetMovementActivity();
        }
    }

    private void SpecialMovementActive()
    {
        if(activePiece.tag == "King")
        {
            float previousXPosition = activePiece.GetPreviousSquare().transform.position.x;
            float currentXPosition = activePiece.GetCurrentSquare().transform.position.x;

            float differenceInPositions = Mathf.Abs(currentXPosition - previousXPosition);

            if(activePiece.GetNumberOfMoves() == 0 && differenceInPositions == 4)
            {
                Castling();
            }
        }
        else
        {
            float currentYPosition = activePiece.GetCurrentSquare().transform.position.y;

            if((currentYPosition == -7) || (currentYPosition == 7))
            {
                PawnPromotion();
            }
        }
    }

    private void PawnPromotion()
    {
        activePiece.ShowPawnPromotionModalBox();
    }

    private void Castling()
    {
        throw new NotImplementedException();
    }

    private void ResetActivePieceIfThereIsNoPossibleMove(bool moveCondition)
    {
        if (!moveCondition && activePiece)
        {
            activePiece.GetCurrentSquare().HighlightSquare();
            activePiece.ShowPotentialMoves();
            activePiece.SetShadowVisibility();
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
            activePiece.GetCurrentSquare().HighlightSquare();
            activePiece.ShowPotentialMoves();
            activePiece.SetShadowVisibility();
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
