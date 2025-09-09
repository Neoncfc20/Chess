using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PieceMovement;

public class PieceLocations : MonoBehaviour
{
    public static List<Piece> pieces = new List<Piece>();
    public static List<PieceMovement> movingPieces = new List<PieceMovement>();

    public class Piece
    {
        public Piece(string type, bool white, int x, int y)
        {
            Type = new PieceType(type);
            White = white;
            X = x;
            Y = y;
            CordX = TileToCoor(x);
            CordY = TileToCoor(y);
            PawnStart = true;

            string pieceImage = GetImageName(white, type);

            GameObject pieceObject = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/" + pieceImage + ".prefab", typeof(GameObject)) as GameObject;
            Object = Instantiate(pieceObject, Vector3.zero, Quaternion.identity);
            Object.transform.tag = "Piece";
            Object.transform.position = new Vector3(CordX, CordY,.5f);
            Object.AddComponent<BoxCollider2D>();
            Object.GetComponent<BoxCollider2D>().size = new Vector3(5.55f, 5.55f);
        }

        public PieceType Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool White { get; }
        public GameObject Object { get; set; }
        public float CordX { get; set; }
        public float CordY { get; set; }
        public bool PawnStart { get; set; }

    }

    public struct PieceType
    {
        public PieceType(string type)
        {
            Type = type;
            Mag = 0;
            Dirs = new List<int[]>();
            switch (type)
            {
                case "King":
                    Dirs = new List<int[]> { new int[] { 1, 0 }, new int[] { 1, 1 }, new int[] { 0, 1 }, new int[] { -1, 1 }, new int[] { -1, -1 }, new int[] { -1, 0 }, new int[] { 1, -1 }, new int[] { 0, -1 } };
                    Mag = 1;
                    break;
                case "Queen":
                    Dirs = new List<int[]> { new int[] { 1, 0 }, new int[] { 1, 1 }, new int[] { 0, 1 }, new int[] { -1, 1 }, new int[] { -1, -1 }, new int[] { -1, 0 }, new int[] { 1, -1 }, new int[] { 0, -1 } };
                    Mag = 8;
                    break;
                case "Rook":
                    Dirs = new List<int[]> { new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 0, -1 } };
                    Mag = 8;
                    break;
                case "Bishop":
                    Dirs = new List<int[]> { new int[] { 1, 1 }, new int[] { -1, 1 }, new int[] { -1, -1 }, new int[] { 1, -1 } };
                    Mag = 8;
                    break;
                case "Knight":
                    Dirs = new List<int[]> { new int[] { 1, 2 }, new int[] { -2, 1 }, new int[] { 2, 1 }, new int[] { -1, -2 }, new int[] { -2, -1 }, new int[] { -1, 2 }, new int[] { 1, -2 }, new int[] { 2, -1 } };
                    Mag = 1;
                    break;
                case "WhitePawn":
                    Dirs = new List<int[]> { new int[] { 0, 1 }, new int[] { 1, 1 }, new int[] { -1, 1 } };
                    Mag = 1;
                    break;
                case "BlackPawn":
                    Dirs = new List<int[]> { new int[] { 0, -1 }, new int[] { 1, -1 }, new int[] { -1, -1 } };
                    Mag = 1;
                    break;
            }
        }

        public string Type { get; }

        public List<int[]> Dirs { get; }
        public int Mag { get; }

    }

    public class PieceMovement
    {
        public PieceMovement(Vector3 start, Vector3 end, GameObject piece) 
        {
            Start = start;
            End = end;
            Piece = piece;
            MoveCount = 0;
        }

        public Vector3 Start { get; set; } // The starting location of a piece move
        public Vector3 End { get; set; } // The ending location of a piece move
        public float MoveCount { get; set; } // The count of how far the piece has moved
        public GameObject Piece { get ; set; } // The object being moved
    }

    private void Start()
    {
        InitializeBoard();
    }

    /// <summary>
    /// Initializes the board.
    /// </summary>
    public static void InitializeBoard()
    {
        int[] rows = { 0, 1, 6, 7 };
        foreach (int row in rows)
        {
            bool white = false;
            if ((row == 0) || (row == 1))
            {
                white = true;
            }
            if ((row == 1) || (row == 6))
            {
                for (int column = 0; column < 8; column++)
                {
                    if (white)
                    {
                        pieces.Add(new Piece("WhitePawn", white, column, row));
                    }
                    else
                    {
                        pieces.Add(new Piece("BlackPawn", white, column, row));
                    }
                }
            }
            else
            {
                pieces.Add(new Piece("Rook", white, 0, row));
                pieces.Add(new Piece("Knight", white, 1, row));
                pieces.Add(new Piece("Bishop", white, 2, row));
                pieces.Add(new Piece("Queen", white, 3, row));
                pieces.Add(new Piece("King", white, 4, row));
                pieces.Add(new Piece("Bishop", white, 5, row));
                pieces.Add(new Piece("Knight", white, 6, row));
                pieces.Add(new Piece("Rook", white, 7, row));
            }

        }
    }

    /// <summary>
    /// Checks to see if a piece is at a coor location.
    /// </summary>
    /// <param name="x">The x position of the piece (Coor).</param>
    /// <param name="y">The y position of the piece (Coor).</param>
    /// <return>Returns the piece if found.</return>
    public static Piece GetPieceCord(float x, float y)
    {
        foreach (Piece piece in pieces)
        {
            if (piece.X == CoorToTile(x) && piece.Y == CoorToTile(y))
            {
                return piece;
            }
        }
        return null;
    }

    /// <summary>
    /// Checks to see if a piece is at a board location.
    /// </summary>
    /// <param name="x">The x position of the piece (Board).</param>
    /// <param name="y">The y position of the piece (Board).</param>
    /// <return>Returns the piece if found.</return>
    public static Piece GetPiece(int x, int y)
    {
        foreach (Piece piece in pieces)
        {
            if (piece.X == x && piece.Y == y)
            {
                return piece;
            }
        }
        return null;
    }

    /// <summary>
    /// Converts a coor value into a board value.
    /// </summary>
    /// <param name="coor">The position of the piece (Coor).</param>
    /// <return>Returns converted value.</return>
    public static int CoorToTile(double coor)
    {
        return (int) (coor + 3.5);
    }

    /// <summary>
    /// Converts a board value into a coor value.
    /// </summary>
    /// <param name="tile">The position of the piece (Board).</param>
    /// <return>Returns converted value.</return>
    public static float TileToCoor(int tile)
    {
        return (float) (tile - 3.5);
    }

    /// <summary>
    /// Gets the name of the piece prefab.
    /// </summary>
    /// <param name="white">True if the piece is white.</param>
    /// <param name="type">The type of piece.</param>
    /// <return>The name of the piece prefab</return>
    public static string GetImageName(bool white, string type)
    {
        if (type.Contains("Pawn")) { return type; }
        if (white){ return "White" + type; }
        else { return "Black" + type; }
    }

    /// <summary>
    /// Fixes all of the variable to their initial state.
    /// </summary>
    public static void FixVariables()
    {
        foreach (Piece piece in pieces.ToListPooled())
        {
            Destroy(piece.Object);
            pieces.Remove(piece);
        }
        foreach (string pieceType in UIManagement.whiteRemovedPieces.Keys.ToListPooled())
        {
            UIManagement.whiteRemovedPieces[pieceType] = 0;
        }
        foreach (string pieceType in UIManagement.blackRemovedPieces.Keys.ToListPooled())
        {
            UIManagement.blackRemovedPieces[pieceType] = 0;
        }
        UIManagement.PrintRemovedPieces();

        ResetGlobals();
    }

    /// <summary>
    /// Moves a piece and displays the movement over a second period.
    /// </summary>
    public static void MovePiece()
    {
        foreach (PieceMovement mp in movingPieces)
        {
            float t = mp.MoveCount / 75.0f;
            mp.Piece.transform.position = Vector3.Lerp(mp.Start, mp.End, t);

            if (mp.MoveCount == 75)
            {
                movingPieces.Remove(mp);
                Checkmate(); // Checks to see if checkmate was achieved
            }
            mp.MoveCount++;
        }
        if (movingPieces.Count == 0)
        {
            pieceMoving = false;
        }
    }

    /// <summary>
    /// Gets the name of the piece prefab.
    /// </summary>
    /// <return>If stalemate should be called due to a lack of material on both sides.</return>
    public static bool StalemateOnMaterial()
    {
        int whiteMaterial = 0;
        int blackMaterial = 0;
        foreach (Piece piece in pieces) // Loops over every piece
        {
            if (piece.White)
            {
                if (piece.Type.Type == "WhitePawn") // Returning false if white has a pawn remaining
                {
                    return false;
                }
                whiteMaterial += UIManagement.materialAmounts[piece.Type.Type]; // Adds material for white pieces
            }
            else
            {
                if (piece.Type.Type == "BlackPawn") // Returning false if black has a pawn remaining
                {
                    return false;
                }
                blackMaterial -= UIManagement.materialAmounts[piece.Type.Type]; // Subtracts material for black pieces
            }
        }
        if (whiteMaterial >= 5 || blackMaterial >= 5) 
        {
            return false;
        }
        return true;
    }
}
