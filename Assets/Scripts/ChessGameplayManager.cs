using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessGameplayManager : MonoBehaviour
{
    private int currentRound;
    private bool hasGameEnded;
    private ChessPlayer[] myPlayers = new ChessPlayer[2];
    private ChessSet[] mySets = new ChessSet[2];
    private ChessBoard myBoard;
    private ChessPiece activePiece;

    public void InitializeGame(ChessSet[] sets, ChessBoard chessBoard, ChessPlayer[] players)
    {
        hasGameEnded = false;
        currentRound = 1;
        myPlayers = players;
        mySets = sets;
        myBoard = chessBoard;
        activePiece = null;
    }

    /*private void OnMouseOver()
    {
        Vector2 currentPosition = GetBoardPosition();
        float currentX = currentPosition.x;
        float currentY = currentPosition.y;

        foreach (Square square in myBoard.board)
        {
            if (square.GetContainedPiece() && square.transform.position.x == currentX && square.transform.position.y == currentY)
            {
                //Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }
    }*/

    private void OnMouseDown()
    {
        if (hasGameEnded)
        {
            return;
        }

        Vector2 currentPosition = GetBoardPosition();
        float currentX = currentPosition.x;
        float currentY = currentPosition.y;

        foreach (Square square in myBoard.board)
        {
            float squareX = square.transform.position.x;
            float squareY = square.transform.position.y;

            if (!activePiece && square.GetContainedPiece() && squareX == currentX && squareY == currentY)
            {
                activePiece = square.GetContainedPiece();

                if (!activePiece.GetMyChessSet().IsMyPlayersTurn())
                {
                    activePiece = null;
                    break;
                }

                activePiece.GetMyChessSet().GetMyEnemyChessSet().RemoveHighlightOfSquares();

                if (!square.IsHighlighted())
                {
                    square.HighlightSquare(isCastling: false);
                }
                
                if (activePiece.HaveIMoved())
                {
                    activePiece.ResetMovementActivity();
                }

                activePiece.SetShadowVisibility(isCastling: false);

                if (!activePiece.AmIDefendingMyKing() && !activePiece.AmIBlocked() && !activePiece.IsMyKingUnderAttack())
                {
                    activePiece.RecomputePotentialMoves();
                }

                activePiece.ShowPotentialMoves(isCastling: false);
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
        if (activePiece)
        {
            bool moveCondition;
            Vector2 currentPosition = GetBoardPosition();
            float currentX = currentPosition.x;
            float currentY = currentPosition.y;

            float rawX = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y)).x;
            float rawY = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y)).y;

            CheckIfPieceIsOutOfBoard(rawX, rawY);
            if (!activePiece)
            {
                return;
            }

            currentX = PositionIfPieceIsBetweenSquares(rawX, currentX, true);
            currentY = PositionIfPieceIsBetweenSquares(rawY, currentY, false);

            Square square = myBoard.GetSquareByVector3(new Vector3(currentX, currentY, 0f));

            if (activePiece.GetPotentialMoves().Contains(square))
            {
                moveCondition = true;
            }
            else
            {
                moveCondition = false;
            }

            if (moveCondition && square.transform.position.x == currentX && square.transform.position.y == currentY)
            {
                SetupPieceAfterMovement(square, activePiece, isCastling: false);
            }

            ResetActivePieceIfThereIsNoPossibleMove(moveCondition);

            if (moveCondition)
            {
                int activePieceSetIndex = activePiece.GetMyChessSet().GetMySetIndex();
                int enemySetIndex = 1 - activePieceSetIndex;

                mySets[activePieceSetIndex].SetDefaultPiecesProtectionValue();
                mySets[activePieceSetIndex].RecomputeAllPotentialMoves();
                
                mySets[enemySetIndex].SetDefaultPiecesProtectionValue();
                mySets[enemySetIndex].RecomputeAllPotentialMoves();

                mySets[enemySetIndex].DetectKingDefenders();
                mySets[activePieceSetIndex].DetectKingDefenders();

                mySets[enemySetIndex].BlockOrUnblockPieces();

                mySets[activePieceSetIndex].CheckIfEnPassantHasNotBeenUsed(activePiece);
                mySets[activePieceSetIndex].GetMyPlayer().SetLastMovedPiece(activePiece);

                CheckGameState();
            }

            activePiece = null;
        }
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

            if (!isCastling)
            {
                //piece.RecomputePotentialMoves();
                SwitchPlayerTurn();
                IncreaseRoundNumber();
            }
        }

        if (!specialMoveSuccessful)
        {
            Debug.Log("Error - Greška");
            //Debug.Log("Provjeru raditi u funkciji SpecialMovementActive, u catch dijelu, a ne ovdje jer ovdje je false i kod normalnih pokreta kralja ili pijuna!=>");
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
            else
            {
                specialMoveSuccessful = true;
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
            else
            {
                specialMoveSuccessful = true;
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
        Square destinationSquare;
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
        float newX = rawWorldPosition.x;
        float newY = rawWorldPosition.y;
        int intX = (int) newX;
        int intY = (int) newY;

        float leftBoundary = 0f;
        float rightBoundary = 0f;
        float upperBoundary = 0f;
        float bottomBoundary = 0f;

        if (intX % 2 == 0)
        {
            if(intX > rawWorldPosition.x)
            {
                rightBoundary = intX;
                leftBoundary = rightBoundary - 2;
            }
            else
            {
                leftBoundary = intX;
                rightBoundary = leftBoundary + 2;
            }
        }
        else
        {
            newX = intX;
        }
        
        if (intY % 2 == 0)
        {
            if (intY > rawWorldPosition.y)
            {
                upperBoundary = intY;
                bottomBoundary = upperBoundary - 2;
            }
            else
            {
                bottomBoundary = intY;
                upperBoundary = bottomBoundary + 2;
            }
        }
        else
        {
            newY = intY;
        }
        
        if (newY != intY && newY < upperBoundary && newY > bottomBoundary)
        {
            if (newY < 0)
            {
                newY = intY - 1;
            }
            else
            {
                newY = intY + 1;
            }
        }
        
        if (newX != intX && newX < rightBoundary && newX > leftBoundary)
        {
            if(newX < 0)
            {
                newX = intX - 1;
            }
            else
            {
                newX = intX + 1;
            }
        }
        
        return new Vector2(newX, newY);
    }

    private void SwitchPlayerTurn()
    {
        if (activePiece)
        {
            int setIndex = activePiece.GetMyChessSet().GetMySetIndex();
            mySets[setIndex].SetMyPlayersState("Waiting");
            mySets[1 - setIndex].SetMyPlayersState("Active");
        }
    }

    private void IncreaseRoundNumber()
    {
        foreach(ChessPlayer player in myPlayers)
        {
            if(player.GetMyChosenColor() == "Light" && player.tag == "Active")
            {
                currentRound++;
            }
        }
    }

    private void CheckGameState()
    {
        ChessSet enemySet = activePiece.GetMyChessSet().GetMyEnemyChessSet();
        ChessPiece enemyKing = enemySet.GetPieceByTag("King");

        if (IsItCheck())
        {
            Debug.Log("Check");

            if (IsItCheckmate(enemySet, enemyKing))
            {
                Debug.Log("CHECKMATE!!!");
                hasGameEnded = true;
            }
            else
            {
                if (enemySet.CountPiecesThatAreAttackingEnemyKing() > 1)
                {
                    Debug.Log("BLOCK ALL PIECES EXCEPT A KING! counter > 1");
                }
                else if(enemySet.CheckIfAnyOfMyPiecesCanProtectKing())
                {
                    Debug.Log("My King can be protected! counter == 1 ...BLOCK ALL PIECES THAT CANNOT PROTECT THE KING!");
                }
                else
                {
                    Debug.Log("BLOCK ALL PIECES EXCEPT A KING! counter == 1");
                }
            }
        }
        else if (IsItRemi())
        {
            Debug.Log("REMI");
            hasGameEnded = true;
        }
    }

    private bool IsItCheck()
    {
        if (activePiece.GetMyChessSet().AmIAttackingOpponentKing())
        {
            return true;
        }

        return false;
    }

    private bool IsItCheckmate(ChessSet enemySet, ChessPiece enemyKing)
    {
        if (enemySet.CheckIfAnyOfMyPiecesCanProtectKing())
        {
            return false;
        }
        else if(enemyKing.GetPotentialMoves().Count == 0)
        {
            return true;
        }

        return false;
    }

    private bool IsItRemi()
    {
        return false;
    }
}
