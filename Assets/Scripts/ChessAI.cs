/* using System;
using ChessDotNet;
using System.Linq;
using System.Collections.Generic;
using static PieceLocations;


namespace Assembly_CSharp
{
    public class ChessAI
    {
        private int maxDepth;

        public ChessAI(int depth = 3)
        {
            maxDepth = depth;
        }

        public Move GetBestMove(ChessGame game, Player player)
        {
            int bestValue = int.MinValue;
            Move bestMove = null;
            foreach (var move in game.GetValidMoves(player))
            {
                game.MakeMove(move, true);
                int value = Minimax(game, maxDepth - 1, int.MinValue, int.MaxValue, player == Player.White ? Player.Black : Player.White);
                game.Undo();
                if (value > bestValue)
                {
                    bestValue = value;
                    bestMove = move;
                }
            }
            return bestMove;
        }

        private int Minimax(ChessGame game, int depth, int alpha, int beta, Player current)
        {
            if (depth == 0 || game.IsDraw() || game.IsCheckmated(Player.White) || game.IsCheckmated(Player.Black))
                return EvaluateBoard(game);

            if (current == Player.White)
            {
                int maxEval = int.MinValue;
                foreach (var move in game.GetValidMoves(current))
                {
                    game.MakeMove(move, true);
                    int eval = Minimax(game, depth - 1, alpha, beta, Player.Black);
                    game.Undo();
                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha) break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;
                foreach (var move in game.GetValidMoves(current))
                {
                    game.MakeMove(move, true);
                    int eval = Minimax(game, depth - 1, alpha, beta, Player.White);
                    game.Undo();
                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha) break;
                }
                return minEval;
            }
        }

        private int EvaluateBoard(ChessGame game)
        {
            // Simple material count
            int score = 0;
            foreach (var sq in game.GetBoard())
            {
                if (sq?.PieceType == null) continue;
                int val = PieceValue(sq.PieceType.Value);
                score += (sq.Side == Player.White) ? val : -val;
            }
            return score;
        }

        private int PieceValue(PieceType pt)
        {
            return pt switch
            {
                PieceType.Pawn => 100,
                PieceType.Knight => 320,
                PieceType.Bishop => 330,
                PieceType.Rook => 500,
                PieceType.Queen => 900,
                PieceType.King => 20000,
                _ => 0
            };
        }
    }
}
*/