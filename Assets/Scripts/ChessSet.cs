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
    private List<string> livingPiecesList = new List<string>();
    private ChessSet enemyChessSet;
    private bool amIPromotingAPawn;

    public void CreatePieceSet(ChessBoard chessBoard, ChessPlayer chessPlayer, int setIndex, ChessSet opponent, Color32[] pieceSetColors, SpritePieceSet spriteSet)
    {
        myBoard = chessBoard;
        myPlayer = chessPlayer;
        myColorTag = myPlayer.GetMyChosenColor();
        mySetIndex = setIndex;
        enemyChessSet = opponent;
        amIPromotingAPawn = false;

        if (myColorTag == "Dark")
        {
            for (int row = 0; row <= 1; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    Square currentSquare;
                    currentSquare = myBoard.board[row, column];
                    SetPiece(currentSquare, row, column, myColorTag, pieceSetColors[1], spriteSet);
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
                    SetPiece(currentSquare, 7 - row, column, myColorTag, pieceSetColors[0], spriteSet);
                }
            }
            transform.name = "Light Colored ChessSet";
        }
    }

    private void SetPiece(Square currentSquare, int setRow, int setColumn, string playerColorTag, Color32 pieceColorSet, SpritePieceSet spriteSet)
    {
        Vector3 vector3 = currentSquare.transform.position;
        GameObject piece = Instantiate(piecePrefab, new Vector3(vector3.x, vector3.y, 0f), Quaternion.identity, gameObject.transform);
        pieceSet[setRow, setColumn] = piece.GetComponent<ChessPiece>();
        pieceSet[setRow, setColumn].InitializePiece(currentSquare, playerColorTag, myBoard, this, pieceColorSet, spriteSet);
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

    public ChessPiece GetPieceByTag(string pieceTag)
    {
        foreach(ChessPiece piece in pieceSet)
        {
            if(piece.tag == pieceTag)
            {
                return piece;
            }
        }

        return null;
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

    public ChessPlayer GetMyPlayer()
    {
        return myPlayer;
    }

    public ChessSet GetMyEnemyChessSet()
    {
        return enemyChessSet;
    }

    public ChessPiece[,] GetMyPieceSet()
    {
        return pieceSet;
    }

    public bool AmIPromotingAPawn()
    {
        return amIPromotingAPawn;
    }

    public void SetPawnPromotingState(bool value)
    {
        amIPromotingAPawn = value;
    }

    public bool IsSquareAttackedByMyPieces(Square desiredSquare, Square currentSquare)
    {
        foreach(ChessPiece piece in pieceSet)
        {
            if (piece.GetPieceState() == "Dead") continue;

            if (piece.tag == "Pawn" && piece.GetMainPieceMoves().Contains(desiredSquare))
            {
                var pieceXPosition = piece.transform.position.x;

                if (desiredSquare.transform.position.x != pieceXPosition)
                {
                    return true;
                }
            }
            else if (piece.GetPotentialMoves().Contains(desiredSquare))
            {
                if (piece.tag != "Pawn" && piece.tag != "King")
                {
                    return true;
                }
            }
            else
            {
                if (currentSquare && piece.AmIAttackingEnemyKing())
                {
                    if (piece.tag == "Rook" || piece.tag == "Bishop" || piece.tag == "Queen")
                    {
                        if(piece.GetCurrentSquare().name == desiredSquare.name && !piece.AmIProtectedByMyPieces())
                        {
                            return false;
                        }
                        else if(Are3OnSameLine(enemyPiece:piece, myKingPosition:currentSquare, myDesiredSquare:desiredSquare))
                        {
                            return true;
                        }
                    }
                }
            }
        }
        
        return false;
    }

    public void DetectKingDefenders()
    {
        ChessPiece king = GetPieceByTag("King");
        foreach (ChessPiece enemyPiece in enemyChessSet.GetMyPieceSet())
        {
            if (enemyPiece.isActiveAndEnabled && king && enemyPiece.tag != "Pawn" && enemyPiece.tag != "King" && enemyPiece.tag != "Knight")
            {
                foreach(ChessPiece piece in pieceSet)
                {
                    if(piece.isActiveAndEnabled && piece.tag != "King" && enemyPiece.GetPotentialMoves().Contains(piece.GetCurrentSquare()))
                    {
                        if (Are3OnSameLine(enemyPiece:enemyPiece, myKingPosition:king.GetCurrentSquare(), myPiece:piece))
                        {
                            piece.PromoteToKingDefender(true, enemyPiece);
                        }
                        else if (piece.AmIDefendingMyKing())
                        {
                            piece.PromoteToKingDefender(false);
                        }
                    }
                }
            }
        }
    }

    public bool AmIAttackingOpponentKing()
    {
        foreach(ChessPiece piece in pieceSet)
        {
            if (piece.isActiveAndEnabled && piece.AmIAttackingEnemyKing())
            {
                return true;
            }
        }

        return false;
    }

    public bool Are3OnSameLine(ChessPiece enemyPiece, Square myKingPosition, ChessPiece myPiece=null, Square myDesiredSquare=null)
    {
        if(myPiece || myDesiredSquare)
        {
            float attackingPieceX = enemyPiece.transform.position.x;
            float attackingPieceY = enemyPiece.transform.position.y;
            if (attackingPieceY % 2 == 0)
            {
                attackingPieceY--;
            }

            float kingX = myKingPosition.transform.position.x;
            float kingY = myKingPosition.transform.position.y;

            float desiredX;
            float desiredY;
            if (!myPiece && myDesiredSquare)
            {
                desiredX = myDesiredSquare.transform.position.x;
                desiredY = myDesiredSquare.transform.position.y;
            }
            else
            {
                desiredX = myPiece.transform.position.x;
                desiredY = myPiece.transform.position.y;
                
                if (desiredY % 2 == 0)
                {
                    desiredY--;
                }
            }

            if(attackingPieceY == kingY && kingY == desiredY)
            {
                return true;
            }
            else if(attackingPieceX == kingX && kingX == desiredX)
            {
                return true;
            }

            float kLineDirection;
            float yAddition;

            kLineDirection = (attackingPieceY - kingY) / (attackingPieceX - kingX);
            yAddition = kingY - (kingX * kLineDirection);

            float resultingY = Mathf.RoundToInt((kLineDirection * desiredX) + yAddition);

            if (resultingY == desiredY)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public bool IsSquareBetweenKingAndAttacker(Square attackingPieceSquare, Square myKingSquare, Square squareToTest)
    {
        float attackingX = attackingPieceSquare.transform.position.x;
        float attackingY = attackingPieceSquare.transform.position.y;

        float kingX = myKingSquare.transform.position.x;
        float kingY = myKingSquare.transform.position.y;

        float squareToTestX = squareToTest.transform.position.x;
        float squareToTestY = squareToTest.transform.position.y;

        if(squareToTestY > attackingY && squareToTestY > kingY)
        {
            return false;
        }
        else if (squareToTestY < attackingY && squareToTestY < kingY)
        {
            return false;
        }
        
        if (squareToTestX > attackingX && squareToTestX > kingX)
        {
            return false;
        }
        else if (squareToTestX < attackingX && squareToTestX < kingX)
        {
            return false;
        }

        return true;
    }

    public bool IsSquareAttackedByMyKing(Square desiredSquare)
    {
        foreach (ChessPiece piece in pieceSet)
        {
            if (piece.tag == "King")
            {
                if (piece.GetMainPieceMoves().Contains(desiredSquare))
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
            if(piece.isActiveAndEnabled)
            {
                piece.RecomputePotentialMoves();
            }
            else
            {
                piece.ClearPotentialMovesIfDeadOrBlocked();
            }
        }
    }

    public void RemoveHighlightOfSquares()
    {
        if (myPlayer.GetLastMovedPiece())
        {
            Square currentSquare = myPlayer.GetLastMovedPiece().GetCurrentSquare();

            if (currentSquare.IsHighlighted())
            {
                currentSquare.HighlightSquare(isCastling: false);
            }
        }
    }

    public void CheckIfEnPassantHasNotBeenUsed(ChessPiece currentActivePiece)
    {
        if (currentActivePiece)
        {
            ChessPiece myLastMovedPiece = GetMyPlayer().GetLastMovedPiece();
            ChessPiece enemyLastMovedPiece = enemyChessSet.GetMyPlayer().GetLastMovedPiece();

            if (enemyLastMovedPiece && myLastMovedPiece && enemyLastMovedPiece.tag == "Pawn" && enemyLastMovedPiece.GetPieceState() == "Possible EnPassant" && currentActivePiece.name != myLastMovedPiece.name)
            {
                enemyLastMovedPiece.IncreaseMoveCounter();
            }
        }
    }

    public bool CheckIfAnyOfMyPiecesCanProtectKing()
    {
        int counter = 0;

        foreach (ChessPiece piece in pieceSet)
        {
            if (piece.isActiveAndEnabled && piece.tag != "King" && !piece.AmIBlocked())
            {
                counter++;
            }
        }

        if(counter > 0)
        {
            return true;
        }

        return false;
    }

    public void BlockOrUnblockPieces()
    {
        ChessPiece enemyAttackingMyKing = enemyChessSet.GetPieceThatIsAttackingEnemyKing();
        ChessPiece myKing = GetPieceByTag("King");
        List<Square> movesList = new List<Square>();
        //Također srediti ono vezano uz pijune i remi i ostalo

        if (enemyAttackingMyKing)
        {
            foreach (ChessPiece piece in pieceSet)
            {
                if(piece.tag != "King")
                {
                    piece.SetBlockValueOfAPiece(true);
                }

                foreach (Square potentialMove in enemyAttackingMyKing.GetPotentialMoves())
                {
                    if(Are3OnSameLine(enemyAttackingMyKing, myKing.GetCurrentSquare(), myDesiredSquare: potentialMove))
                    {
                        if (piece.isActiveAndEnabled && piece.tag != "King" && piece.GetPotentialMoves().Contains(potentialMove))
                        {
                            if (!movesList.Contains(potentialMove) && IsSquareBetweenKingAndAttacker(enemyAttackingMyKing.GetCurrentSquare(), myKing.GetCurrentSquare(), potentialMove))
                            {
                                piece.SetBlockValueOfAPiece(false);
                                movesList.Add(potentialMove);
                            }
                        }
                    }
                }

                if (piece.isActiveAndEnabled && piece.tag != "King" && piece.GetPotentialMoves().Contains(enemyAttackingMyKing.GetCurrentSquare()))
                {
                    piece.SetBlockValueOfAPiece(false);

                    if (!movesList.Contains(enemyAttackingMyKing.GetCurrentSquare()))
                    {
                        movesList.Add(enemyAttackingMyKing.GetCurrentSquare());
                    }
                }

                piece.ClearPotentialMovesIfDeadOrBlocked();
                piece.ClearMovesThatCannotProtectKing(movesList);

                movesList.Clear();
            }
        }
    }

    public ChessPiece GetPieceThatIsAttackingEnemyKing()
    {
        if (CountPiecesThatAreAttackingEnemyKing() == 1)
        {
            foreach (ChessPiece piece in pieceSet)
            {
                if (piece.isActiveAndEnabled && piece.AmIAttackingEnemyKing())
                {
                    return piece;
                }
            }
        }
        return null;
    }

    public int CountPiecesThatAreAttackingEnemyKing()
    {
        int counter = 0;
        foreach(ChessPiece piece in pieceSet)
        {
            if (piece.isActiveAndEnabled && piece.AmIAttackingEnemyKing())
            {
                counter++;
            }
        }

        return counter;
    }

    public List<string> GetNamesOfPiecesThatCanProtectKing()
    {
        List<string> protectors = new List<string>();
        ChessPiece myKing = GetPieceByTag("King");
        ChessPiece enemyAttackingMyKing = enemyChessSet.GetPieceThatIsAttackingEnemyKing();

        if (enemyAttackingMyKing)
        {
            foreach (Square potentialMove in enemyAttackingMyKing.GetPotentialMoves())
            {
                if (myKing.GetPotentialMoves().Contains(potentialMove))
                {
                    foreach (ChessPiece piece in pieceSet)
                    {
                        if (piece.isActiveAndEnabled && piece.tag != "King" && piece.GetPotentialMoves().Contains(potentialMove))
                        {
                            protectors.Add(piece.name);
                        }
                        else if (piece.isActiveAndEnabled && piece.tag != "King" && piece.GetPotentialMoves().Contains(enemyAttackingMyKing.GetCurrentSquare()))
                        {
                            protectors.Add(piece.name);
                        }
                    }
                }
            }
        }

        return protectors;
    }

    public List<string> GetNamesOfPiecesThatCannotProtectKing()
    {
        List<string> piecesNames = new List<string>();
        var kingProtectors = GetNamesOfPiecesThatCanProtectKing();

        foreach(ChessPiece piece in pieceSet)
        {
            if(piece.isActiveAndEnabled && piece.tag != "King" && !kingProtectors.Contains(piece.name))
            {
                piecesNames.Add(piece.name);
            }
        }

        return piecesNames;
    }

    public List<string> GetAllPiecesNamesExceptKings()
    {
        List<string> piecesNames = new List<string>();

        foreach (ChessPiece piece in pieceSet)
        {
            if (piece.isActiveAndEnabled && piece.tag != "King")
            {
                piecesNames.Add(piece.name);
            }
        }

        return piecesNames;
    }

    public void SetDefaultPiecesProtectionValue()
    {
        foreach(ChessPiece piece in pieceSet)
        {
            if (piece.isActiveAndEnabled)
            {
                piece.SetPieceProtectionValue(false);
            }
        }
    }

    public bool AreThereAnyPotentialMoves()
    {
        foreach(ChessPiece piece in pieceSet)
        {
            if (piece.isActiveAndEnabled && piece.GetNumberOfPotentialMoves() != 0)
            {
                return true;
            }
        }

        return false;
    }

    public List<string> GetListOfAllLivingPieces()
    {
        if(livingPiecesList.Count > 0)
            livingPiecesList.Clear();

        foreach(ChessPiece piece in pieceSet)
        {
            if(piece.isActiveAndEnabled)
            {
                livingPiecesList.Add(piece.tag);
            }
        }

        return livingPiecesList;
    }
}
