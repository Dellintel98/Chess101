using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessSet : MonoBehaviour
{
    [SerializeField] public GameObject piecePrefab;

    private int mySetIndex;
    private string chessSetColorTag;
    private ChessBoard myBoard;
    private ChessPiece[,] pieceSet = new ChessPiece[2, 8]; // [0,] => Royalty row, [1,] => Pawn row
    private List<Square> potentialMoves = new List<Square>();

    public void CreatePieceSet(ChessBoard chessBoard, string playerColorTag, int setIndex)
    {
        myBoard = chessBoard;
        chessSetColorTag = playerColorTag;
        mySetIndex = setIndex;

        if (chessSetColorTag == "Dark")
        {
            for (int row = 0; row <= 1; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Square currentSquare;
                    currentSquare = myBoard.board[row, column];
                    SetPiece(currentSquare, row, column, playerColorTag);
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
                    SetPiece(currentSquare, 7 - row, column, playerColorTag);
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
        return chessSetColorTag;
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
}
