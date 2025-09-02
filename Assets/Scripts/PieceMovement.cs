using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using System;
using UnityEngine.UIElements;

public class PieceMovement : MonoBehaviour
{
    static PieceLocations.Piece selectedPiece; // The currently selected piece
    GameObject selectedObject; // The selected object on the screen (Piece, Square, Circle)
    static GameObject openPopup; // A game object to track the popup currently open on the screen
    static List<GameObject> circles = new List<GameObject>(); // A list storing all the circles that appear for a player's potential moves
    static bool clicking = false; // Makes sure the user can't perform multiple inputs while the code is running
    static bool whiteMove = true; // Determines which player's move it is

    // A set of four booleans to determine if castling is legal for either player for both directions
    static bool whiteCastleLeft = true;
    static bool whiteCastleRight = true;
    static bool blackCastleLeft = true;
    static bool blackCastleRight = true;


    // Update is called once per frame
    void Update()
    {
        GameObject piece;
        GameObject circle;

        if (!clicking && Input.GetMouseButtonDown(0)) // Makes sure the game is ready for another input
        {
            clicking = true; // Sets the state to clicking

            // Set of code to get the object the user clicked on
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            worldPoint.z = Camera.main.transform.position.z;
            Ray ray = new Ray(worldPoint, new Vector3(0, 0, 1));
            RaycastHit2D hit = Physics2D.GetRayIntersection(ray);

            if (hit.collider) // Checks if the user clicked on anything
            {
                if (hit.collider.tag == "Piece") // If the player clicks on a piece
                {
                    RemoveMoves(); // Removes any circles on the screen
                    piece = hit.transform.gameObject; 
                    selectedObject = piece;
                    Vector3 coor = piece.transform.position;
                    selectedPiece = PieceLocations.GetPieceCord(coor.x, coor.y);
                    if (selectedPiece == null) { return; }
                    if (selectedPiece.White != whiteMove || openPopup != null) // Checking to make sure it's the correct player's turn
                    {                                                            // And that there is no popup open
                        return;
                    }
                    List<int[]> moves = GetMoves(selectedPiece); // Determines every potential move for the selcted piece
                    SetMoves(moves); // Puts those moves on the board
                }
                else if (hit.collider.tag == "Move") // If the player selects a circle (spot to move a piece to)
                {
                    circle = hit.transform.gameObject;
                    Vector3 coor = circle.transform.position;

                    PieceLocations.Piece attacked = PieceLocations.GetPiece(PieceLocations.CoorToTile(coor.x), PieceLocations.CoorToTile(coor.y));
                        
                    // Check to see if the piece is moving onto another piece

                    if (attacked != null) 
                    {
                        Destroy(attacked.Object); // Removing the attacked piece
                        PieceLocations.pieces.Remove(attacked);
                    }

                    CheckCastleStatus(selectedPiece, attacked); // Removing Castling Ability if the Rook or Pawn was moved

                    if (selectedPiece.Type.Type == "King" && Math.Abs(PieceLocations.CoorToTile(coor.x) - selectedPiece.X) == 2)
                    {
                        MoveCastleRook(selectedPiece, PieceLocations.CoorToTile(coor.x)); // Performing castling
                    }

                    selectedPiece.X = PieceLocations.CoorToTile(coor.x); // Moving the piece
                    selectedPiece.Y = PieceLocations.CoorToTile(coor.y);
                    selectedObject.transform.position = coor;

                    if (selectedPiece.Type.Type.Contains("Pawn")) // Adding a condition for pawn specialties
                    {
                        PromotionCheck(selectedPiece);
                        selectedPiece.PawnStart = false; // Moved off its starting square
                    }

                    if (attacked != null)
                    {
                        UIManagement.AddRemovedPiece(attacked); // Removing the attacked piece if there was one
                    }

                    RemoveMoves(); // Removing the circles from the board
                    whiteMove = !whiteMove; // Changing whose move it is

                    Checkmate(); // Checks to see if checkmate was achieved
                }
                else
                {
                    return;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            clicking = false; // Gets out of the clicking state
        }
    }

    /// <summary>
    /// Returns a list of coordinates with all potential moves.
    /// </summary>
    /// <param name="piece">The piece being moved.</param>
    /// <return>A list of coordinates with all potential moves</return>
    public static List<int[]> GetMoves(PieceLocations.Piece piece)
    {
        List<int[]> moves = new List<int[]>();
        int originalX = piece.X;
        int originalY = piece.Y;
        int magnitude = piece.Type.Mag;

        if (piece.Type.Type == "King" && !InCheck(piece.White)) // Gets extra moves for the king if castling is still legal
        {
            if (CastleLogic(piece.White, out List<int[]> castleMoves))
            {
                moves.AddRange(castleMoves);
            }
        }

        if (piece.Type.Type.Contains("Pawn") && piece.PawnStart) // Adding a condition for pawn moving off their starting square
        {
            magnitude = 2;
        }
        foreach (int[] dir in piece.Type.Dirs) // Each piece has a list of directions and magnitudes, this loop uses them to determine all legal moves.
        {
            for (int i = 1; i <= magnitude; i++)
            {
                int tileX = dir[0] * i + piece.X;
                int tileY = dir[1] * i + piece.Y;

                if (Blocked(tileX, tileY, out bool white)) // Checking to see if a piece is blocking a movement lane
                {
                    if (white != piece.White && (!piece.Type.Type.Contains("Pawn") || (dir[0] != 0 && i == 1))) { // If the piece blocking is the same type exclude the option
                        if (MoveLogic(tileX, tileY, piece, originalX, originalY, out int[] move))
                        {
                            moves.Add(move);
                        }
                    }
                    break;
                }
                else { if (!OB(tileX, tileY)) {
                        if ((!piece.Type.Type.Contains("Pawn") || dir[0] == 0) && MoveLogic(tileX, tileY, piece, originalX, originalY, out int[] move))
                        {
                            moves.Add(move);
                        }
                    } else { break; } 
                }
            }
        }

        return moves;
    }

    /// <summary>
    /// Returns if the move is legal.
    /// </summary>
    /// <param name="tileX">X position of the move (Board).</param>
    /// <param name="tileY">Y position of the move (Board).</param>
    /// <param name="piece">The piece being moved.</param>
    /// <param name="originalX">X position of the piece before the move (Board).</param>
    /// <param name="originalY">Y position of the piece before the move (Board).</param>
    /// <return>If the move is legal</return>
    public static bool MoveLogic(int tileX, int tileY, PieceLocations.Piece piece, int originalX, int originalY, out int[] move )
    {
        move = null;
        PieceLocations.Piece attacked = PieceLocations.GetPiece(tileX, tileY);
        if (attacked != null && attacked.White != piece.White)
        {
            PieceLocations.pieces.Remove(attacked);
        }
        piece.X = tileX; piece.Y = tileY;
        if (!InCheck(piece.White))
        {
            move = new int[] { tileX, tileY };
            piece.X = originalX; piece.Y = originalY;
            if (attacked != null)
            {
                PieceLocations.pieces.Add(attacked);
            }
            return true;
        }
        else { 
            piece.X = originalX; piece.Y = originalY;
            if (attacked != null)
            {
                PieceLocations.pieces.Add(attacked);
            }
        }
        
        return false;
    }

    /// <summary>
    /// Returns if a piece can attack the provided square
    /// </summary>
    /// <param name="piece">The piece being moved.</param>
    /// <param name="x">X value (Board)</param>
    /// <param name="y">Y value (Board)</param>
    /// <return>If the piece can attack the square.</return>
    public static bool CanAttackSquare(PieceLocations.Piece piece, int x, int y)
    {
        foreach (int[] dir in piece.Type.Dirs)
        {
            for (int i = 1; i <= piece.Type.Mag; i++)
            {
                int tileX = dir[0] * i + piece.X;
                int tileY = dir[1] * i + piece.Y;

                if (tileX == x && tileY == y)
                {
                    return true;
                }

                if (Blocked(tileX, tileY, out bool white) || OB(tileX, tileY)) // Checking to see if a piece is blocking a movement lane or the piece has gone off the grid
                {
                    break;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Checks to see if a piece is being blocked by another piece.
    /// </summary>
    /// <param name="x">The x position of the potential move (Board).</param>
    /// <param name="y">The y position of the potential move (Board).</param>
    /// <param name="white">If the piece blocking is white.</param>
    /// <return>If a piece is on the spot.</return>
    public static bool Blocked(int x, int y, out bool white)
    {
        white = true;
        PieceLocations.Piece piece = PieceLocations.GetPiece(x, y);
        if (piece == null) { return false;}
        else
        {
            white = piece.White;
            return true;
        }
    }

    /// <summary>
    /// Checks to see if a move would be outside the board.
    /// </summary>
    /// <param name="x">The x position of the potential move (Board).</param>
    /// <param name="y">The y position of the potential move (Board).</param>
    /// <return>If the piece is inbounds.</return>
    public static bool OB(int x, int y)
    {
        if (x > 7 || x < 0 || y > 7 || y < 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Puts circles on the board to denote possible moves.
    /// </summary>
    /// <param name="moves">All possible moves.</param>
    public static void SetMoves(List<int[]> moves)
    {
        
        foreach (int[] move in moves)
        {
            float x = PieceLocations.TileToCoor(move[0]);
            float y = PieceLocations.TileToCoor(move[1]);

            GameObject prefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/SelectCircle.prefab", typeof(GameObject)) as GameObject;
            GameObject circle = Instantiate(prefab, Vector3.zero, Quaternion.identity);

            circle.transform.position = new Vector3(x,y,-1);
            circle.transform.localScale = new Vector3(.5f, .5f);
            circle.AddComponent<PolygonCollider2D>();
            circle.transform.tag = "Move";
            circles.Add(circle);
        }
    }

    /// <summary>
    /// Removes all of the circles on the board.
    /// </summary>
    public static void RemoveMoves()
    {
        foreach (GameObject circle in circles)
        {
            Destroy(circle);
        }
        circles.Clear();
    }

    /// <summary>
    /// Checks to see if a king is in check or would be.
    /// </summary>
    /// <param name="white">The player checking for</param>
    /// <return>If the king is in check.</return>
    public static bool InCheck(bool white)
    {
        PieceLocations.Piece king = null;
        foreach (PieceLocations.Piece piece in PieceLocations.pieces) // Finding the correct king piece
        {
            if (piece.White == white && piece.Type.Type == "King")
            {
                king = piece;
                break;
            } 
        }
        return !SquareSafe(king.X, king.Y, white);
    }

    /// <summary>
    /// Checks to see if a particular square is in danger.
    /// </summary>
    /// <param name="x">X value (Board)</param>
    /// <param name="y">Y value (Board)</param>
    /// <param name="white">The opposite color of the attacking pieces</param>
    /// <return>If the square is safe.</return>
    public static bool SquareSafe(int x, int y, bool white)
    {
        foreach (PieceLocations.Piece piece in PieceLocations.pieces) // See if any pieces can attack the king
        {
            if (piece.White != white)
            {
                if (CanAttackSquare(piece, x, y))
                {
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Checks to see if a pawn needs to be promoted and opens the popup if so.
    /// </summary>
    /// <param name="piece">The piece being checked</param>
    public static void PromotionCheck(PieceLocations.Piece piece)
    {
        if (piece.White && piece.Y == 7)
        {
            GameObject whitePromotion = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/White Promotion Popup.prefab", typeof(GameObject)) as GameObject;
            GameObject promotionPopup = Instantiate(whitePromotion);
            openPopup = promotionPopup;
        }
        else if (!piece.White && piece.Y == 0)
        {
            GameObject blackPromotion = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Black Promotion Popup.prefab", typeof(GameObject)) as GameObject;
            GameObject promotionPopup = Instantiate(blackPromotion);
            openPopup = promotionPopup;
        }
    }

    /// <summary>
    /// Promotes the selected piece from the popup.
    /// </summary>
    /// <param name="pieceType">The type of piece being promoted to</param>
    public static void PromotionHandler(string pieceType)
    {
        Destroy(selectedPiece.Object);
        PieceLocations.pieces.Remove(selectedPiece);
        PieceLocations.pieces.Add(new PieceLocations.Piece(pieceType, selectedPiece.White, selectedPiece.X, selectedPiece.Y));
        Destroy(openPopup);
        openPopup = null;
        UIManagement.PrintRemovedPieces();
    }

    /// <summary>
    /// Checks to see if castling is legal.
    /// </summary>
    /// <param name="white">If the color of the king is white</param>
    /// <param name="castles">The list of valid castle moves</param>
    /// <return>If there is a legal castle.</return>
    public static bool CastleLogic(bool white, out List<int[]> castles)
    {
        castles = new List<int[]>();
        if (white)
        {
            if (whiteCastleLeft && PieceLocations.GetPiece(1, 0) == null && PieceLocations.GetPiece(2, 0) == null && PieceLocations.GetPiece(3, 0) == null
                && SquareSafe(1, 0, white) && SquareSafe(2, 0, white) && SquareSafe(3, 0, white) && SquareSafe(0, 0, white)) {
                castles.Add(new int[] { 2, 0 });
            }
            if (whiteCastleRight && PieceLocations.GetPiece(5, 0) == null && PieceLocations.GetPiece(6, 0) == null
                && SquareSafe(5, 0, white) && SquareSafe(6, 0, white) && SquareSafe(7, 0, white))
            {
                castles.Add(new int[] { 6, 0 });
            }
        }
        else
        {
            if (blackCastleLeft && PieceLocations.GetPiece(1, 7) == null && PieceLocations.GetPiece(2, 7) == null && PieceLocations.GetPiece(3, 7) == null
                && SquareSafe(1, 7, white) && SquareSafe(2, 7, white) && SquareSafe(3, 7, white) && SquareSafe(0, 7, white))
            {
                castles.Add(new int[] { 2, 7 });
            }
            if (blackCastleRight && PieceLocations.GetPiece(5, 7) == null && PieceLocations.GetPiece(6, 7) == null
                && SquareSafe(5, 7, white) && SquareSafe(6, 7, white) && SquareSafe(7, 7, white))
            {
                castles.Add(new int[] { 6, 7 });
            }
        }
        if (castles.Count > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes castling status for rook and king moves.
    /// </summary>
    /// <param name="piece">The piece being moved.</param>
    /// <param name="attacked">The piece potentially being removed.</param>
    public static void CheckCastleStatus(PieceLocations.Piece piece, PieceLocations.Piece attacked)
    {
        if (selectedPiece.Type.Type == "King") // Moved the king
        {
            if (piece.White)
            {
                whiteCastleLeft = false;
                whiteCastleRight = false;
            }
            else
            {
                blackCastleLeft = false;
                blackCastleRight = false;
            }
        }
        else if (selectedPiece.Type.Type == "Rook") // Moved a rook
        {
            if (piece.White)
            {
                if (piece.Y == 0)
                {
                    if (piece.X == 0)
                    {
                        whiteCastleLeft = false;
                    }
                    else if (piece.X == 7)
                    {
                        whiteCastleRight = false;
                    }
                }
            }
            else
            {
                if (piece.Y == 7)
                {
                    if (piece.X == 0)
                    {
                        blackCastleLeft = false;
                    }
                    else if (piece.X == 7)
                    {
                        blackCastleRight = false;
                    }
                }
            }
        }

        if (attacked != null && attacked.Type.Type == "Rook") // Removing castling ability if the rook was captured
        {
            if (attacked.X == 0 && attacked.Y == 0)
            {
                whiteCastleLeft = false;
            }
            else if (attacked.X == 7 && attacked.Y == 0)
            {
                whiteCastleRight = false;
            }
            else if (attacked.X == 0 && attacked.Y == 7)
            {
                blackCastleLeft = false;
            }
            else if (attacked.X == 7 && attacked.Y == 7)
            {
                blackCastleRight = false;
            }
        }
    }

    /// <summary>
    /// Moves the rook during a castle.
    /// </summary>
    /// <param name="king">The king being castled.</param>
    /// <param name="x">The x position the king is moving to (Board).</param>
    public static void MoveCastleRook(PieceLocations.Piece king, int x)
    {
        PieceLocations.Piece rook = null;
        if (king.White)
        {
            if (x == 2)
            {
                rook = PieceLocations.GetPiece(0, 0);
                rook.X = 3;
                rook.Y = 0;
                rook.Object.transform.position = new Vector3(PieceLocations.TileToCoor(rook.X), PieceLocations.TileToCoor(rook.Y));
            }
            else
            {
                rook = PieceLocations.GetPiece(7, 0);
                rook.X = 5;
                rook.Y = 0;
                rook.Object.transform.position = new Vector3(PieceLocations.TileToCoor(rook.X), PieceLocations.TileToCoor(rook.Y));
            }
        }
        else
        {
            if (x == 2)
            {
                rook = PieceLocations.GetPiece(0, 7);
                rook.X = 3;
                rook.Y = 7;
                rook.Object.transform.position = new Vector3(PieceLocations.TileToCoor(rook.X), PieceLocations.TileToCoor(rook.Y));
            }
            else
            {
                rook = PieceLocations.GetPiece(7, 7);
                rook.X = 5;
                rook.Y = 7;
                rook.Object.transform.position = new Vector3(PieceLocations.TileToCoor(rook.X), PieceLocations.TileToCoor(rook.Y));
            }
        }
    }

    /// <summary>
    /// Checks to see if there has been a checkmate.
    /// </summary>
    public static void Checkmate()
    {
        foreach(PieceLocations.Piece piece in PieceLocations.pieces.ToListPooled())
        {
            if (piece.White == whiteMove)
            {
                if (GetMoves(piece).Count != 0) 
                {
                    return;
                }
            }
        }
        openPopup = null;
        CheckmatePopup(whiteMove);
    }

    /// <summary>
    /// Opens the checkmate popup.
    /// </summary>
    /// <param name="white">The side that got checkmated.</param>
    public static void CheckmatePopup(bool white)
    {
        if (white)
        {
            GameObject popup = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Checkmate Popup Black.prefab", typeof(GameObject)) as GameObject;
            GameObject checkmatePopup = Instantiate(popup);
            openPopup = checkmatePopup;
        }
        else
        {
            GameObject popup = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Checkmate Popup White.prefab", typeof(GameObject)) as GameObject;
            GameObject checkmatePopup = Instantiate(popup);
            openPopup = checkmatePopup;
        }
    }

    /// <summary>
    /// Resets all globals to restart the game.
    /// </summary>
    public static void ResetGlobals()
    {
        Destroy(openPopup);
        whiteMove = true;
        whiteCastleLeft = true;
        whiteCastleRight = true;
        blackCastleLeft = true;
        blackCastleRight = true;
        openPopup = null;
    }
}
                          
