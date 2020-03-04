using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessSet : MonoBehaviour
{
    [SerializeField] public GameObject piecePrefab;

    private int mySetIndex;
    private string myColorTag;
    private ChessBoard myBoard;
    private ChessPlayer myPlayer;
    private ChessPiece[,] pieceSet = new ChessPiece[2, 8]; // [0,] => Royalty row, [1,] => Pawn row
    private List<Square> potentialMoves = new List<Square>();
    private ChessSet enemyChessSet;

    public void CreatePieceSet(ChessBoard chessBoard, ChessPlayer chessPlayer, int setIndex, ChessSet opponent)
    {
        myBoard = chessBoard;
        myPlayer = chessPlayer;
        myColorTag = myPlayer.GetMyChosenColor();
        mySetIndex = setIndex;
        enemyChessSet = opponent;

        if (myColorTag == "Dark")
        {
            for (int row = 0; row <= 1; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Square currentSquare;
                    currentSquare = myBoard.board[row, column];
                    SetPiece(currentSquare, row, column, myColorTag);
                }
            }

            transform.name = "Dark Colored ChessSet";
        }
        else
        {
            for (int row = 7; row >= 6; row--)
            {
                for (int column = 0; column < 8; column++)
                {
                    Square currentSquare;
                    currentSquare = myBoard.board[row, column];
                    SetPiece(currentSquare, 7 - row, column, myColorTag);
                }
            }
            transform.name = "Light Colored ChessSet";
        }
    }

    private void SetPiece(Square currentSquare, int setRow, int setColumn, string playerColorTag)
    {
        Vector3 vector3 = currentSquare.transform.position;
        GameObject piece = Instantiate(piecePrefab, new Vector3(vector3.x, vector3.y, 0f), Quaternion.identity, gameObject.transform);
        pieceSet[setRow, setColumn] = piece.GetComponent<ChessPiece>();
        pieceSet[setRow, setColumn].InitializePiece(currentSquare, playerColorTag, myBoard, this);
    }

    public string GetColorTag()
    {
        return myColorTag;
    }

    public ChessPiece GetPieceByVector3(Vector3 rawPosition)
    {
        foreach (ChessPiece piece in pieceSet)
        {
            if (piece.transform.position == rawPosition)
            {
                return piece;
            }
        }

        return null;
    }

    public ChessPiece GetPieceByTagAndVector3(string pieceTag, Vector3 rawPosition)
    {
        ChessPiece piece = GetPieceByVector3(rawPosition);

        if (piece && piece.tag == pieceTag)
        {
            return piece;
        }

        return null;
    }

    public int GetMySetIndex()
    {
        return mySetIndex;
    }

    public bool IsMyPlayersTurn()
    {
        if(myPlayer.transform.tag == "Active")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetMyPlayersState(string state)
    {
        myPlayer.SetMyState(state);
    }

    public ChessSet GetMyEnemyChessSet()
    {
        return enemyChessSet;
    }

    public bool IsSquareAttackedByMyPieces(Square desiredSquare)
    {
        foreach(ChessPiece piece in pieceSet)
        {
            if (piece.GetPotentialMoves().Contains(desiredSquare))
            {
                if (piece.tag != "Pawn" && piece.tag != "King")
                {
                    return true;
                }
                else if (piece.tag == "Pawn")
                {
                    var pieceXPosition = piece.transform.position.x;

                    foreach (Square potentialMove in piece.GetPotentialMoves())
                    {
                        if(potentialMove.GetSquarePositionCode() == desiredSquare.GetSquarePositionCode())
                        {
                            if (potentialMove.transform.position.x == pieceXPosition)
                            {
                                return false;
                            }

                            return true;
                        }
                    }
                }
            }
        }
        
        return false;
    }

    public bool IsSquareAttackedByMyKing(Square desiredSquare)
    {
        foreach (ChessPiece piece in pieceSet)
        {
            if (piece.GetPotentialMoves().Contains(desiredSquare))
            {
                if (piece.tag == "King")
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsSquareAttackedByMyQueenRookBishop(Square desiredSquare)
    {
        foreach (ChessPiece piece in pieceSet)
        {
            if (piece.GetPotentialMoves().Contains(desiredSquare))
            {
                if (piece.tag != "Pawn" && piece.tag != "King" && piece.tag != "Knight")
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void RecomputeAllPotentialMoves()
    {
        foreach(ChessPiece piece in pieceSet)
        {
            piece.RecomputePotentialMoves();
        }
    }
}
