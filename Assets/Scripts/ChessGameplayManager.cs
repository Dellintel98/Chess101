using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChessGameplayManager : MonoBehaviour
{
    [SerializeField] public Button exportButton;
    [SerializeField] public GameObject chessManagerObject;
    [SerializeField] public GameObject scoreSheetObject;
    private int currentRound;
    private bool hasGameEnded;
    private ChessPlayer[] myPlayers = new ChessPlayer[2];
    private ChessSet[] mySets = new ChessSet[2];
    private ChessBoard myBoard;
    private ChessPiece activePiece;
    private ScoreSheet myScoreSheet;
    [HideInInspector] public string terminationString;
    [HideInInspector] public bool isPromotionInProcess;
    private int offerDrawCounter;
    private string colorTagOfLastDrawOfferer;
    private int lightPlayer50MoveCounter;
    private int darkPlayer50MoveCounter;
    private bool exportEnabled { get; set; }

    public void InitializeGame(ChessSet[] sets, ChessBoard chessBoard, ChessPlayer[] players)
    {
        hasGameEnded = false;
        currentRound = 1;
        myPlayers = players;
        mySets = sets;
        myBoard = chessBoard;
        activePiece = null;
        isPromotionInProcess = false;
        offerDrawCounter = 0;
        colorTagOfLastDrawOfferer = "";
        lightPlayer50MoveCounter = 0;
        darkPlayer50MoveCounter = 0;
        myScoreSheet = scoreSheetObject.GetComponent<ScoreSheet>();
        exportEnabled = false;

        foreach (ChessPlayer player in myPlayers)
        {
            player.SetMyGame(this);
        }
    }

    private void OnMouseDown()
    {
        if (hasGameEnded)
        {
            return;
        }

        foreach(ChessSet set in mySets)
        {
            if(set.AmIPromotingAPawn())
            {
                return;
            }
        }

        foreach(ChessPlayer player in myPlayers)
        {
            if (!player.HasGameStarted() && player.GetMyState() == "Active")
                player.GameHasStarted();
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
                activePiece.ResetPromotionState();
                if (activePiece.GetCastling("Either"))
                    activePiece.ResetCastling();

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

            activePiece.ResetCaptureEnemyPiece();
            if (moveCondition && square.transform.position.x == currentX && square.transform.position.y == currentY)
            {
                SetupPieceAfterMovement(square, activePiece, isCastling: false);
            }

            ResetActivePieceIfThereIsNoPossibleMove(moveCondition);

            if (moveCondition && !isPromotionInProcess)
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

    public void SetupPawnAfterPromotion(ChessPiece promotedPiece)
    {
        activePiece = promotedPiece;

        if(activePiece)
        {
            SwitchPlayerTurn();
            IncreaseRoundNumber();

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

            if (!isCastling && !isPromotionInProcess)
            {
                //piece.RecomputePotentialMoves();
                //Debug.Log("Started waiting...");
                //yield return new WaitWhile(() => isPromotionInProcess);
                //Debug.Log("Waiting has finished.");


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
        bool specialMoveSuccessful;

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
            activePiece.ShowPawnPromotionModalBox(this);
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
            activePiece.SetCastling("Kingside");
        }
        else if (differenceInPositions == -4)
        {
            castlingRook = mySets[setIndex].GetPieceByTagAndVector3("Rook", new Vector3(myX - 4, myY, myZ));
            destinationSquare = myBoard.GetSquareByVector3(new Vector3(myX + 2, myY, myZ));
            activePiece.SetCastling("Queenside");
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
        if (activePiece && activePiece.GetMyChessSet().GetMyPlayer().tag == "Active")
        {
            int setIndex = activePiece.GetMyChessSet().GetMySetIndex();
            mySets[setIndex].SetMyPlayersState("Waiting");
            mySets[setIndex].GetMyPlayer().AddTimeIncrement();
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
        if (hasGameEnded) return;

        ChessSet enemySet = activePiece.GetMyChessSet().GetMyEnemyChessSet();
        ChessPiece enemyKing = enemySet.GetPieceByTag("King");
        string enemyColorTag = enemySet.GetColorTag();
        string myColorTag = activePiece.GetMyColorTag();
        //if(activePiece.GetMyChessSet().GetMyPlayer().tag == "Lost")
        //{
        //    hasGameEnded = true;
        //    Debug.Log($"Game over: {myColorTag} colored player flagged! {enemyColorTag} colored player won!");
        //}
        bool isCheck = IsItCheck();
        bool isCheckmate = IsItCheckmate(enemySet, enemyKing);
        bool isRemi = IsItRemi();
        bool isCheckFunctionArgument;
        bool isCheckmateFunctionArgument;

        if (isCheck && isCheckmate)
            isCheckFunctionArgument = false;
        else if (isCheck && !isCheckmate)
            isCheckFunctionArgument = true;
        else
            isCheckFunctionArgument = false;

        if (isCheck)
            isCheckmateFunctionArgument = isCheckmate;
        else
            isCheckmateFunctionArgument = false;

        hasGameEnded = false;
        if (isCheck)
        {
            Debug.Log($"Check => Attacker: {myColorTag} - Defender: {enemyColorTag}");

            if (isCheckmate)
            {
                Debug.Log($"CHECKMATE!!! => Attacker: {myColorTag} - Defender: {enemyColorTag}");
                hasGameEnded = true;
                activePiece.GetMyChessSet().GetMyPlayer().SetMyState("Won");
                activePiece.GetMyChessSet().GetMyEnemyChessSet().GetMyPlayer().SetMyState("Lost");
                terminationString = "won by checkmate";
            }
            else
            {
                if (enemySet.CountPiecesThatAreAttackingEnemyKing() > 1)
                {
                    Debug.Log($"BLOCK ALL PIECES EXCEPT A KING! counter > 1 => Attacker: {myColorTag} - Defender: {enemyColorTag}");
                }
                else if(enemySet.CheckIfAnyOfMyPiecesCanProtectKing())
                {
                    Debug.Log($"My King can be protected! counter == 1 ...BLOCK ALL PIECES THAT CANNOT PROTECT THE KING! => Attacker: {myColorTag} - Defender: {enemyColorTag}");
                }
                else
                {
                    Debug.Log($"BLOCK ALL PIECES EXCEPT A KING! counter == 1 => Attacker: {myColorTag} - Defender: {enemyColorTag}");
                }
            }
        }
        else if (isRemi)
        {
            Debug.Log($"REMI => Attacker: {myColorTag} - Defender: {enemyColorTag}");
            hasGameEnded = true;
            activePiece.GetMyChessSet().GetMyPlayer().SetMyState("Remi");
            activePiece.GetMyChessSet().GetMyEnemyChessSet().GetMyPlayer().SetMyState("Remi");
        }

        string winner = (hasGameEnded) ? ((isRemi) ? "None" : myColorTag) : "None";

        int gameRound = (myColorTag == "Dark") ? currentRound - 1 : currentRound;

        myScoreSheet.SetNewMove(activePiece.GetMyInitialTag(), activePiece.tag, activePiece.GetPreviousSquare().GetSquarePositionCode(), 
            activePiece.GetCurrentSquare().GetSquarePositionCode(), gameRound, isCheckFunctionArgument, isCheckmateFunctionArgument, activePiece.GetCastling("Kingside"), 
            activePiece.GetCastling("Queenside"), activePiece.DidICaptureEnemyPiece(), activePiece.HaveIJustBeenPromoted(), winner, isRemi, hasGameEnded, false, false);
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
        if(!hasGameEnded)
        {
            ChessSet mySet = activePiece.GetMyChessSet();
            ChessSet myEnemySet = mySet.GetMyEnemyChessSet();

            if(IsItStalemate())
            {
                terminationString = "Draw by stalemate";
                return true;
            }
            if (IsItADeadPosition(mySet, myEnemySet, false))
            {
                terminationString = "Draw by insufficient material";
                return true;
            }
            if (Is50MoveRuleApplicable())
            {
                terminationString = "Draw by enforcing a 50 move rule";
                return true;
            }
            if (IsItThreeFoldRepetition())
            {
                terminationString = "Draw by threefold repetition";
                return true;
            }
        }

        return false;
    }

    private bool IsItStalemate()
    {
        ChessSet myEnemySet = activePiece.GetMyChessSet().GetMyEnemyChessSet();

        bool isCheck = IsItCheck();

        if (isCheck)
            return false;

        return !myEnemySet.AreThereAnyPotentialMoves();
    }

    private bool IsItThreeFoldRepetition()
    {
        bool isA3XDraw = myScoreSheet.CheckIsItAThreefoldRepetition(activePiece.GetMyInitialTag(), activePiece.tag, activePiece.GetPreviousSquare().GetSquarePositionCode(),
            activePiece.GetCurrentSquare().GetSquarePositionCode(), false, false, activePiece.DidICaptureEnemyPiece(), activePiece.HaveIJustBeenPromoted());

        return isA3XDraw;
    }

    private bool Is50MoveRuleApplicable()
    {
        if(GetCurrentlyActivePlayer().GetMyChosenColor() == "Light")
        {
            if (activePiece.tag == "Pawn" || activePiece.DidICaptureEnemyPiece())
                lightPlayer50MoveCounter = 0;
            else
                lightPlayer50MoveCounter++;
        }
        else
        {
            if (activePiece.tag == "Pawn" || activePiece.DidICaptureEnemyPiece())
                darkPlayer50MoveCounter = 0;
            else
                darkPlayer50MoveCounter++;
        }

        if (lightPlayer50MoveCounter == 50 && darkPlayer50MoveCounter == 50)
            return true;

        return false;
    }

    public bool IsItADeadPosition(ChessSet mySet, ChessSet myEnemySet, bool isItTimeout)
    {
        List<string> myAlivePieces = mySet.GetListOfAllLivingPieces();
        List<string> enemyAlivePieces = myEnemySet.GetListOfAllLivingPieces();

        if(myAlivePieces.Count == 1 && enemyAlivePieces.Count == 1)
        {
            terminationString = isItTimeout ? "Draw by timeout vs insufficient material" : "";
            return true;
        }
        else if(myAlivePieces.Count == 1 && enemyAlivePieces.Count == 2)
        {
            if(enemyAlivePieces.Contains("King") && (enemyAlivePieces.Contains("Knight") || enemyAlivePieces.Contains("Bishop")))
            {
                terminationString = isItTimeout ? "Draw by timeout vs insufficient material" : "";

                return true;
            }
        }
        else if (enemyAlivePieces.Count == 1 && myAlivePieces.Count == 2)
        {
            if (myAlivePieces.Contains("King") && (myAlivePieces.Contains("Knight") || myAlivePieces.Contains("Bishop")))
            {
                terminationString = isItTimeout ? "Draw by timeout vs insufficient material" : "";

                return true;
            }
        }
        else if (myAlivePieces.Count == 2 && enemyAlivePieces.Count == 2)
        {
            if (myAlivePieces.Contains("King") && myAlivePieces.Contains("Bishop") && enemyAlivePieces.Contains("King") && enemyAlivePieces.Contains("Bishop"))
            {
                string myBishopSquareColor = mySet.GetPieceByTag("Bishop").GetCurrentSquare().tag;
                string enemyBishopSquareColor = mySet.GetPieceByTag("Bishop").GetCurrentSquare().tag;

                if (myBishopSquareColor == enemyBishopSquareColor)
                {
                    terminationString = isItTimeout ? "Draw by timeout vs insufficient material" : "";

                    return true;
                }
            }
        }
        else if (enemyAlivePieces.Count == 1 && myAlivePieces.Count >= 3 && isItTimeout)
        {
            terminationString = isItTimeout ? "Draw by timeout vs insufficient material" : "";

            return true;
        }

        return false;
    }

    public void SetPromotionInProcess()
    {
        isPromotionInProcess = true;
    }

    public void SetPromotionProcessEnd()
    {
        isPromotionInProcess = false;
    }

    public void SetFlaggingEndScore(string winner, bool isADraw)
    {
        hasGameEnded = true;
        myScoreSheet.SetNewMove("", "", "", "", currentRound, false, false, false, false, false, false, winner, isADraw, hasGameEnded, false, false);
    }

    private ChessPlayer GetCurrentlyActivePlayer()
    {
        foreach(ChessPlayer player in myPlayers)
        {
            if (player.tag == "Active")
                return player;
        }

        return null;
    }

    public void OfferADraw()
    {
        if(!hasGameEnded)
        {
            offerDrawCounter++;

            ChessPlayer me = GetCurrentlyActivePlayer();
            string myColorTag = me.GetMyChosenColor();

            if (offerDrawCounter == 1)
            {
                Debug.Log($"Draw was offered by {myColorTag} colored player.");
                colorTagOfLastDrawOfferer = myColorTag;
            }
            if (offerDrawCounter == 2)
            {
                if (colorTagOfLastDrawOfferer == myColorTag)
                {
                    offerDrawCounter = 1;
                }
                else
                {
                    Debug.Log($"Draw was accepted by {myColorTag} colored player.");
                    hasGameEnded = true;
                    myScoreSheet.SetNewMove("", "", "", "", currentRound, false, false, false, false, false, false, "None", true, hasGameEnded, false, true);
                    terminationString = "Game drawn by agreement";
                    offerDrawCounter = 0;
                }
            }
        }
    }

    public void Resign()
    {
        if(!hasGameEnded)
        {
            ChessPlayer me = GetCurrentlyActivePlayer();
            string myColorTag = me.GetMyChosenColor();
            string myEnemyColorTag = me.GetMyEnemyColorTag();

            hasGameEnded = true;
            myScoreSheet.SetNewMove("", "", "", "", currentRound, false, false, false, false, false, false, myEnemyColorTag, false, hasGameEnded, true, false);
            Debug.Log($"{myColorTag} colored player resigned. {myEnemyColorTag} colored player won.");
            terminationString = "won by resignation";
        }
    }

    public void ExportGame()
    {
        if (!hasGameEnded)
            return;

        ChessManager manager = chessManagerObject.GetComponent<ChessManager>();
        string[] pgnHeading = manager.ExportGameInPGNFormat();

        if(pgnHeading != null)
            myScoreSheet.ExportGameAsPGN(pgnHeading[0], pgnHeading[1], pgnHeading[2], pgnHeading[3], pgnHeading[4], pgnHeading[5], pgnHeading[6], terminationString);
    }

    public bool HasGameEnded()
    {
        return hasGameEnded;
    }

    // Update is called once per frame
    private void Update()
    {
        if(hasGameEnded && !exportEnabled)
        {
            exportButton.interactable = true;
            exportEnabled = true;
        }
    }
}
