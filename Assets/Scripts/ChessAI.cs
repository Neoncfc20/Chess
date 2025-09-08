using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using static PieceLocations;
using static PieceMovement;

public class ChessAI : MonoBehaviour
{
    private int maxDepth;

    public ChessAI(int depth = 3)
    {
        maxDepth = depth;
    }

    /// <summary>
    /// Shuffles the list of pieces to get different move outcomes for the AI.
    /// </summary>
    /// <return>The shuffled list of pieces</return>
    public static List<Piece> ShufflePieces()
    {
        List<Piece> inputPieces = new List<Piece>(pieces);
        int count = inputPieces.Count;
        int last = count - 1;
        for (int i = 0; i < last; ++i)
        {
            int r = UnityEngine.Random.Range(i, count);
            Piece tmp = inputPieces[i];
            inputPieces[i] = inputPieces[r];
            inputPieces[r] = tmp;
        }
        return inputPieces;
    }

    /// <summary>
    /// Shuffles the list of moves to get different move outcomes for the AI.
    /// </summary>
    /// <return>The shuffled list of moves</return>
    public static List<int[]> ShuffleMoves(List<int[]> moves)
    {
        List<int[]> inputMoves = new List<int[]>(moves);
        int count = inputMoves.Count;
        int last = count - 1;
        for (int i = 0; i < last; ++i)
        {
            var r = UnityEngine.Random.Range(i, count);
            int[] tmp = inputMoves[i];
            inputMoves[i] = inputMoves[r];
            inputMoves[r] = tmp;
        }
        return inputMoves;
    }

    /// <summary>
    /// Gets and sets a move for the AI to make.
    /// </summary>
    public static void GetBestMove()
    {
        int mat = 100;
        int[] finalMove = { 0, 0 };
        List<Piece> shuffledPieces = ShufflePieces();
        Piece finalPiece = null;
        foreach (Piece piece in shuffledPieces)
        {
            if (!piece.White)
            {
                List<int[]> moves = GetMoves(piece);
                List<int[]> shuffledMoves = ShuffleMoves(moves);
                foreach (int[] move in shuffledMoves)
                {
                    List<Piece> tempPieces = new List<Piece>(pieces);
                    Piece attackCheck = GetPiece(move[0], move[1]);
                    // Check to see if the piece is moving onto another piece

                    if (attackCheck != null)
                    {
                        tempPieces.Remove(attackCheck);
                    }

                    int updatedMat = UIManagement.CalculateMaterial(tempPieces, out _);
                    if (updatedMat < mat)
                    {
                        finalPiece = piece;
                        finalMove = move;
                    }
                }
            }

        }

        Piece attacked = GetPiece(finalMove[0], finalMove[1]);

        // Check to see if the piece is moving onto another piece

        if (attacked != null)
        {
            Destroy(attacked.Object); // Removing the attacked piece
            pieces.Remove(attacked);
        }

        CheckCastleStatus(finalPiece, attacked); // Removing Castling Ability if the Rook or Pawn was moved

        if (finalPiece.Type.Type == "King" && Math.Abs(finalMove[0] - finalPiece.X) == 2)
        {
            MoveCastleRook(finalPiece, finalMove[0]); // Performing castling
        }

        finalPiece.X = finalMove[0]; // Setting globals to move the piece
        finalPiece.Y = finalMove[1];
        pieceMoving = true;
        selectedObject = finalPiece.Object;
        start = finalPiece.Object.transform.position;
        end = new Vector3(TileToCoor(finalMove[0]), TileToCoor(finalMove[1]), .5f);

        if (finalPiece.Type.Type.Contains("Pawn")) // Adding a condition for pawn specialties
        {
            PromotionCheck(finalPiece);
            finalPiece.PawnStart = false; // Moved off its starting square
        }

        if (attacked != null)
        {
            UIManagement.AddRemovedPiece(attacked); // Removing the attacked piece if there was one
        }

        RemoveMoves(); // Removing the circles from the board

        Checkmate(); // Checks to see if checkmate was achieved
    }
}
