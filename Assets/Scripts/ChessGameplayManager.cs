using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessGameplayManager : MonoBehaviour
{
    private int currentRound;
    private bool isThereAWinner;
    private ChessSet[] mySets = new ChessSet[2];
    private ChessBoard myBoard;
    private ChessPiece activePiece;

    public void InitializeGame(ChessSet[] sets, ChessBoard chessBoard)
    {
        isThereAWinner = false;
        currentRound = 0;
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
            if (square.GetContainedPiece() && square.transform.position.x == currentX && square.transform.position.y == currentY)
            {
                if (!square.IsHighlighted())
                {
                    square.HighlightSquare();
                }
                activePiece = square.GetContainedPiece();
                activePiece.SetShadowVisibility();
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
        Vector2 currentPosition = GetBoardPosition();
        float currentX = currentPosition.x;
        float currentY = currentPosition.y;

        foreach (Square square in myBoard.board)
        {
            if (activePiece && square.transform.position.x == currentX && square.transform.position.y == currentY)
            {
                square.HighlightSquare();
                activePiece.GetCurrentSquare().HighlightSquare();
                activePiece.ShowPotentialMoves();

                activePiece.SetCurrentSquare(square);
                activePiece.RecomputePotentialMoves();
                
                activePiece.SetShadowVisibility();
                activePiece.transform.position = new Vector3(activePiece.transform.position.x, activePiece.transform.position.y, 0f);
                activePiece.SetMovementActivity();
                break;
            }
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

    




    //____________________________________________________________________
    //--------------------------------------------------------------------
    //--------------------------------------------------------------------
    private void GameLoop(ChessSet[] sets, ChessBoard chessBoard)
    {
        while (!isThereAWinner)
        {
            currentRound++;

            Debug.Log("Round: " + currentRound);

            Round(sets, chessBoard);

            if (currentRound == 4)
            {
                isThereAWinner = true;
            }
        }
    }

    private void Round(ChessSet[] sets, ChessBoard chessBoard)
    {
        for (int i = 0; i < 2; i++)
        {
            Turn(sets[i]);
        }
    }

    private void Turn(ChessSet activeSet)
    {
        //while (!activeSet.HaveIMovedAPiece())
        //{
        //    Debug.Log("Still did not move a piece.");
        //}

        Debug.Log("Yes! Finally you moved a piece!");
    }

    private Square[] GetPotentialSquares()
    {
        return new Square[0];
    }
}
