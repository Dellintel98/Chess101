using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MovementDirection
{
    None,
    ForwardF,
    BackwardB,
    LeftL,
    RightR,
    DiagonalFL,
    DiagonalFR,
    DiagonalBL,
    DiagonalBR
}

public class Square : MonoBehaviour
{
    [SerializeField] public GameObject potentialMoveMarkPrefab;

    private Vector2Int squarePosition;
    private string squarePositionCode;
    private string squareColorTag;
    private Color32 squareColor;
    private Color32 specialColor;
    private bool visibleHighlight;
    private bool visibleMoveMark;
    private GameObject myMoveMark;
    private SpriteRenderer spriteRenderer;
    private ChessPiece containedPiece;

    public void Setup(int row, int column)
    {
        visibleHighlight = false;
        specialColor = new Color32(0x1F, 0x7A, 0x8C, 0xFF); //#022b3a  #bfdbf7

        SetPositionOnBoard(row, column);
        CreatePotentialMoveMark();
        SetContainedPiece(null);
        SetColor();
    }

    private void SetPositionOnBoard(int row, int column)
    {
        squarePosition = new Vector2Int(row, column);

        char columnCode = (char)(column + 65);
        int rowCode = 8 - row;
        squarePositionCode = $"{columnCode}{rowCode} {squarePosition}";

        gameObject.name = "Square" + squarePositionCode;
    }

    private void CreatePotentialMoveMark()
    {
        myMoveMark = Instantiate(potentialMoveMarkPrefab, new Vector3(transform.position.x, transform.position.y, -1f), Quaternion.identity, transform) as GameObject;
        myMoveMark.GetComponent<SpriteRenderer>().sortingLayerName = "PiecesLayer";
        myMoveMark.GetComponent<SpriteRenderer>().color = specialColor;
        visibleMoveMark = false;
        myMoveMark.SetActive(visibleMoveMark);
    }

    private void SetMoveMarkColor(Color32 newColor)
    {
        myMoveMark.GetComponent<SpriteRenderer>().color = newColor;
    }

    public void SetPotentialMoveMarkVisibility()
    {
        visibleMoveMark = !visibleMoveMark;

        myMoveMark.SetActive(visibleMoveMark);
    }

    public void SetColor()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        int x = squarePosition.x + 1;
        int y = squarePosition.y;

        if ((x % 2 == 0 && y % 2 == 0) || (x % 2 != 0 && y % 2 != 0))
        {
            squareColorTag = "Dark";
            gameObject.tag = squareColorTag;
            squareColor = new Color32(0x52, 0x51, 0x51, 0xFF);
        }
        else
        {
            squareColorTag = "Light";
            gameObject.tag = squareColorTag;
            squareColor = new Color32(0xC6, 0xC6, 0xC6, 0xFF);
        }

        spriteRenderer.color = squareColor;
    }

    public void HighlightSquare()
    {
        visibleHighlight = !visibleHighlight;

        if (visibleHighlight)
        {
            spriteRenderer.color = Color.Lerp(squareColor, specialColor, 0.5f);
        }
        else
        {
            spriteRenderer.color = squareColor;
        }
    }

    public bool IsHighlighted()
    {
        return visibleHighlight;
    }

    public void SetContainedPiece(ChessPiece piece)
    {
        containedPiece = piece;
    }

    public ChessPiece GetContainedPiece()
    {
        return containedPiece;
    }

    public Vector2Int GetSquarePosition()
    {
        return squarePosition;
    }

    public string GetSquarePositionCode()
    {
        return squarePositionCode;
    }


    //Vidit zašto ne radi i bloka igra nakon dodavanja funkcije ispod
    public bool CanYouGetToMe(ChessPiece activePiece, ChessBoard myBoard)
    {
        int pieceRow = activePiece.GetCurrentSquare().GetSquarePosition().x;
        int pieceColumn = activePiece.GetCurrentSquare().GetSquarePosition().y;
        int myRow = squarePosition.x;
        int myColumn = squarePosition.y;
        string pieceColor = activePiece.GetMyColorTag();

        MovementDirection direction;

        if (pieceRow == myRow && pieceColumn != myColumn)
        {
            //if(Mathf.Abs(pieceColumn - myColumn) > maxNumberOfMoves)
            //{
            //    return false;
            //}

            if(pieceColumn > myColumn)
            {
                direction = (pieceColor == "Light") ? MovementDirection.LeftL : MovementDirection.RightR;
            }
            else
            {
                direction = (pieceColor == "Light") ? MovementDirection.RightR : MovementDirection.LeftL;
            }
        }
        else if (pieceRow != myRow && pieceColumn == myColumn)
        {
            if (pieceRow > myRow)
            {
                direction = (pieceColor == "Light") ? MovementDirection.BackwardB : MovementDirection.ForwardF;
            }
            else
            {
                direction = (pieceColor == "Light") ? MovementDirection.ForwardF : MovementDirection.BackwardB;
            }
        }
        else if (pieceRow != myRow && pieceColumn != myColumn)
        {
            if (pieceRow > myRow && pieceColumn > myColumn)
            {
                direction = (pieceColor == "Light") ? MovementDirection.DiagonalBL : MovementDirection.DiagonalFR;
            }
            else if (pieceRow > myRow && pieceColumn < myColumn)
            {
                direction = (pieceColor == "Light") ? MovementDirection.DiagonalBR : MovementDirection.DiagonalFL;
            }
            else if (pieceRow < myRow && pieceColumn > myColumn)
            {
                direction = (pieceColor == "Light") ? MovementDirection.DiagonalFL : MovementDirection.DiagonalBR;
            }
            else
            {
                direction = (pieceColor == "Light") ? MovementDirection.DiagonalFR : MovementDirection.DiagonalBL;
            }
        }
        else
        {
            direction = MovementDirection.None;
        }

        switch (direction)
        {
            case MovementDirection.ForwardF:
                for (int i = pieceRow; (pieceColor == "Light") ? i >= myRow : i <= myRow; i = (pieceColor == "Light") ? i-- : i++)
                {
                    if(i == pieceRow)
                    {
                        continue;
                    }

                    Square square = myBoard.board[i, pieceColumn];
                    ChessPiece piece = square.GetContainedPiece();

                    if (piece && i != myRow)
                    {
                        return false;
                    }
                    else if(piece && i == myRow && piece.GetMyColorTag() == pieceColor)
                    {
                        return false;
                    }
                }
                break;
            case MovementDirection.BackwardB:
                for (int i = pieceRow; (pieceColor == "Light") ? i <= myRow : i >= myRow; i = (pieceColor == "Light") ? i++ : i--)
                {
                    if (i == pieceRow)
                    {
                        continue;
                    }

                    Square square = myBoard.board[i, pieceColumn];
                    ChessPiece piece = square.GetContainedPiece();

                    if (piece && i != myRow)
                    {
                        return false;
                    }
                    else if (piece && i == myRow && piece.GetMyColorTag() == pieceColor)
                    {
                        return false;
                    }
                }
                break;
            case MovementDirection.RightR:
                for (int i = pieceColumn; (pieceColor == "Light") ? i <= myColumn : i >= myColumn; i = (pieceColor == "Light") ? i++ : i--)
                {
                    if (i == pieceColumn)
                    {
                        continue;
                    }

                    Square square = myBoard.board[pieceRow, i];
                    ChessPiece piece = square.GetContainedPiece();

                    if (piece && i != myColumn)
                    {
                        return false;
                    }
                    else if (piece && i == myColumn && piece.GetMyColorTag() == pieceColor)
                    {
                        return false;
                    }
                }
                break;
            case MovementDirection.LeftL:
                for (int i = pieceColumn; (pieceColor == "Light") ? i >= myColumn : i <= myColumn; i = (pieceColor == "Light") ? i-- : i++)
                {
                    if (i == pieceColumn)
                    {
                        continue;
                    }

                    Square square = myBoard.board[pieceRow, i];
                    ChessPiece piece = square.GetContainedPiece();

                    if (piece && i != myColumn)
                    {
                        return false;
                    }
                    else if (piece && i == myColumn && piece.GetMyColorTag() == pieceColor)
                    {
                        return false;
                    }
                }
                break;
            case MovementDirection.DiagonalFR:
                for (int i = pieceRow; (pieceColor == "Light") ? i >= myRow : i <= myRow; i = (pieceColor == "Light") ? i-- : i++)
                {
                    if(i == pieceRow)
                    {
                        continue;
                    }

                    int numberOfMoves = (pieceColor == "Light") ? pieceRow - i : i - pieceRow;

                    Square square = myBoard.board[i, pieceColumn + numberOfMoves];
                    ChessPiece piece = square.GetContainedPiece();

                    if (piece && i != myRow)
                    {
                        return false;
                    }
                    else if (piece && i == myRow && piece.GetMyColorTag() == pieceColor)
                    {
                        return false;
                    }
                }
                break;
            case MovementDirection.DiagonalFL:
                for (int i = pieceRow; (pieceColor == "Light") ? i >= myRow : i <= myRow; i = (pieceColor == "Light") ? i-- : i++)
                {
                    if (i == pieceRow)
                    {
                        continue;
                    }

                    int numberOfMoves = (pieceColor == "Light") ? pieceRow - i : i - pieceRow;

                    Square square = myBoard.board[i, pieceColumn - numberOfMoves];
                    ChessPiece piece = square.GetContainedPiece();

                    if (piece && i != myRow)
                    {
                        return false;
                    }
                    else if (piece && i == myRow && piece.GetMyColorTag() == pieceColor)
                    {
                        return false;
                    }
                }
                break;
            case MovementDirection.DiagonalBR:
                for (int i = pieceRow; (pieceColor == "Light") ? i <= myRow : i >= myRow; i = (pieceColor == "Light") ? i++ : i--)
                {
                    if (i == pieceRow)
                    {
                        continue;
                    }

                    int numberOfMoves = (pieceColor == "Light") ? i - pieceRow : pieceRow - i;

                    Square square = myBoard.board[i, pieceColumn + numberOfMoves];
                    ChessPiece piece = square.GetContainedPiece();

                    if (piece && i != myRow)
                    {
                        return false;
                    }
                    else if (piece && i == myRow && piece.GetMyColorTag() == pieceColor)
                    {
                        return false;
                    }
                }
                break;
            case MovementDirection.DiagonalBL:
                for (int i = pieceRow; (pieceColor == "Light") ? i <= myRow : i >= myRow; i = (pieceColor == "Light") ? i++ : i--)
                {
                    if (i == pieceRow)
                    {
                        continue;
                    }

                    int numberOfMoves = (pieceColor == "Light") ? i - pieceRow : pieceRow - i;

                    Square square = myBoard.board[i, pieceColumn - numberOfMoves];
                    ChessPiece piece = square.GetContainedPiece();

                    if (piece && i != myRow)
                    {
                        return false;
                    }
                    else if (piece && i == myRow && piece.GetMyColorTag() == pieceColor)
                    {
                        return false;
                    }
                }
                break;
            default:
                break;
        }

        return true;
    }
}
