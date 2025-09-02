using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PieceLocations : MonoBehaviour
{
    public static List<Piece> pieces = new List<Piece>();

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
            Object.AddComponent<PolygonCollider2D>();
            //Object.AddComponent<PieceLocations>();
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

    void Start() // Initiallizing all of the pieces
    {
        IntializeBoard();
    }

    /// <summary>
    /// Initializes the board.
    /// </summary>
    public static void IntializeBoard()
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
    /// Resets the board to its original state and starts a new game.
    /// </summary>
    public static void ResetGame()
    {
        foreach(Piece piece in pieces.ToListPooled())
        {
            Destroy(piece.Object);
            pieces.Remove(piece);
        }
        foreach(string pieceType in UIManagement.whiteRemovedPieces.Keys.ToListPooled())
        {
            UIManagement.whiteRemovedPieces[pieceType] = 0;
        }
        foreach (string pieceType in UIManagement.blackRemovedPieces.Keys.ToListPooled())
        {
            UIManagement.blackRemovedPieces[pieceType] = 0;
        }
        UIManagement.PrintRemovedPieces();

        PieceMovement.ResetGlobals();

        IntializeBoard();
    }
}
