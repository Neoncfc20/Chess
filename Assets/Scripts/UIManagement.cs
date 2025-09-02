using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManagement : MonoBehaviour
{
    public static Dictionary<string, int> whiteRemovedPieces = new Dictionary<string, int>
    {
        {"WhitePawn", 0 },
        {"Knight", 0 },
        {"Bishop", 0 },
        {"Rook", 0 },
        {"Queen", 0 }
    };
    public static Dictionary<string, int> blackRemovedPieces = new Dictionary<string, int>
    {
        {"BlackPawn", 0 },
        {"Knight", 0 },
        {"Bishop", 0 },
        {"Rook", 0 },
        {"Queen", 0 }
    };
    public static Dictionary<string, int> materialAmounts = new Dictionary<string, int>
    {
        {"WhitePawn", 1 },
        {"BlackPawn", 1 },
        {"Knight", 3 },
        {"Bishop", 3 },
        {"Rook", 5 },
        {"Queen", 9 },
        {"King", 0 }
    };

    public static Dictionary<string, int> whiteSprites = new Dictionary<string, int>
    {
        {"WhitePawn", 0 },
        {"Knight", 1 },
        {"Bishop", 2 },
        {"Rook", 3 },
        {"Queen", 4 }
    };

    public static Dictionary<string, int> blackSprites = new Dictionary<string, int>
    {
        {"BlackPawn", 5 },
        {"Knight", 6 },
        {"Bishop", 7 },
        {"Rook", 8 },
        {"Queen", 9 }
    };

    /// <summary>
    /// Adds the removed piece to the dictionary then reprints the pieces.
    /// </summary>
    /// <param name="piece">The piece being removed.</param>
    public static void AddRemovedPiece(PieceLocations.Piece piece)
    {
        if (piece.White)
        {
            whiteRemovedPieces[piece.Type.Type]++;
        }
        else
        {
            blackRemovedPieces[piece.Type.Type]++;
        }

        PrintRemovedPieces();

    }

    /// <summary>
    /// Prints the removed pieces to the screen as well as the material difference.
    /// </summary>
    public static void PrintRemovedPieces()
    {
        string leftPieces = string.Empty;
        string rightPieces = string.Empty;
        foreach (string piece in blackRemovedPieces.Keys)
        {
            for (int i = 0; i < blackRemovedPieces[piece]; i++) 
            {
                leftPieces += "<sprite=" + blackSprites[piece].ToString() + ">";
                if (i == blackRemovedPieces[piece] - 1)
                {
                    leftPieces += " ";
                }
            }
        }
        foreach (string piece in whiteRemovedPieces.Keys)
        {
            for (int i = 0; i < whiteRemovedPieces[piece]; i++)
            {
                rightPieces += "<sprite=" + whiteSprites[piece].ToString() + ">";
                if (i == whiteRemovedPieces[piece] - 1)
                {
                    rightPieces += " ";
                }
            }
        }

        int material = CalculateMaterial(out bool white);

        if (material != 0)
        {
            if (white)
            {
                leftPieces += "+" + material.ToString();
            }
            else
            {
                rightPieces += "+" + material.ToString();
            }
        }

        GameObject leftText = GameObject.Find("Left Removed Pieces");
        leftText.GetComponent<TMP_Text>().text = leftPieces ;
        GameObject rightText = GameObject.Find("Right Removed Pieces");
        rightText.GetComponent<TMP_Text>().text = rightPieces;
    }

    /// <summary>
    /// Returns the difference in material left on the board.
    /// </summary>
    /// <param name="white">If white has more material the value is true.</param>
    /// <return>The difference in material.</return>
    public static int CalculateMaterial(out bool white)
    {
        int materialDifference = 0;
        foreach(PieceLocations.Piece piece in PieceLocations.pieces) // Loops over every piece
        {
            if (piece.White)
            {
                materialDifference += materialAmounts[piece.Type.Type]; // Adds material for white pieces
            }
            else
            {
                materialDifference -= materialAmounts[piece.Type.Type]; // Subtracts material for black pieces
            }
        }

        if (materialDifference < 0)
        {
            white = false;
        }
        else
        {
            white = true;
        } 
        return Math.Abs(materialDifference);
    }

    public static void StartGame()
    {
        SceneManager.LoadScene("ChessBoard");
    }

}
