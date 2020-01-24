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

        if (activePiece)
        {
            rawX = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y)).x;
            rawY = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y)).y;
        }

        Vector2 currentPosition = GetBoardPosition();
        float currentX = currentPosition.x;
        float currentY = currentPosition.y;

        if (activePiece && (rawY < -8 || rawY > 8 || rawX < -8 || rawX > 8))
        {
            activePiece.GetCurrentSquare().HighlightSquare();
            activePiece.ShowPotentialMoves();
            activePiece.SetShadowVisibility();
            activePiece.SnapPositionToCurrentSquare();
            activePiece = null;
        }

        if (activePiece && currentX % 2 == 0)
        {
            float firstNeighborSquareX = currentX - 1;
            float secondNeighborSquareX = currentX + 1;
            
            if(Mathf.Abs(firstNeighborSquareX - rawX) <= Mathf.Abs(secondNeighborSquareX - rawX))
            {
                currentX = firstNeighborSquareX;
            }
            else
            {
                currentX = secondNeighborSquareX;
            }

            if(currentX == -8)
            {
                currentX += 1;
            }
            else if(currentX == 8)
            {
                currentX -= 1;
            }
        }

        if (activePiece && currentY % 2 == 0)
        {
            float firstNeighborSquareY = currentY - 1;
            float secondNeighborSquareY = currentY + 1;

            if (firstNeighborSquareY - rawY <= secondNeighborSquareY - rawY)
            {
                currentY = firstNeighborSquareY;
            }
            else
            {
                currentY = secondNeighborSquareY;
            }

            if (currentY == -8)
            {
                currentY += 1;
            }
            else if (currentY == 8)
            {
                currentY -= 1;
            }
        }

        foreach (Square square in myBoard.board)
        {
            if(activePiece && activePiece.GetPotentialMoves().Contains(square))
            {
                moveCondition = true;
            }
            else
            {
                moveCondition = false;
            }

            if (activePiece && moveCondition && square.transform.position.x == currentX && square.transform.position.y == currentY)
            {
                square.HighlightSquare();
                activePiece.GetCurrentSquare().HighlightSquare();
                activePiece.ShowPotentialMoves();
                activePiece.SetCurrentSquare(square);
                activePiece.SetShadowVisibility();

                if(activePiece.GetCurrentSquare() != activePiece.GetInitialSquare())
                {
                    activePiece.SetMovementActivity();
                }

                activePiece.SnapPositionToCurrentSquare();
                break;
            }
        }

        if (!moveCondition && activePiece)
        {
            activePiece.GetCurrentSquare().HighlightSquare();
            activePiece.ShowPotentialMoves();
            activePiece.SetShadowVisibility();
            activePiece.SnapPositionToCurrentSquare();
        }

        activePiece = null;
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

    private bool IsValidMove()
    {


        return false;
    }
}
