using System;
using System.Collections.Generic;
using System.Linq;

namespace Hexapawn
{
    class Program
    {
        static void Main(string[] args)
        {
            var Player1 = new Player("White");
            var Player2 = new Computer("Black");

        newRound:
            Player1.ResetPawns();
            Player2.ResetPawns();

            Board board = new Board(Player1, Player2);

            string orig = "";
            string dest = "";

            Console.WriteLine($"NEW GAME_______\n");
            Console.WriteLine($"Player1 : {Player1.Score}\nPlayer2 : {Player2.Score}\n");
            Console.WriteLine($"Current losingStates Computer :");
            Console.WriteLine($"{string.Join("\n", Player2.losingStates)}\n");
            board.Draw();

            foreach (var round in Enumerable.Range(1, 8))
            {
            //Player move
            Orig:
                Console.Write($"Which pawn you want to move ? ");
                Console.Write($"[{string.Join(",", Player1.GetMovablePawns(board).Select(x => x.getPosition(board)))}] : ");
                orig = Console.ReadLine();

                if (board.GetPawn(orig) == null)
                {
                    Console.WriteLine("Nothing there!");
                    goto Orig;
                }
                if (board.GetPawn(orig).Color != "White")
                {
                    Console.WriteLine("That's not yours!");
                    goto Orig;
                }
                if (board.GetPawn(orig).getValidMoves(board).Count() == 0)
                {
                    Console.WriteLine("No moves for that one");
                    goto Orig;
                }

            Dest:
                Console.Write($"Where to ? ('x' to go back) ");
                Console.Write($"[{string.Join(",", board.GetPawn(orig).getValidMoves(board).ToList())}] : ");
                dest = Console.ReadLine();
                if (dest == "x")
                {
                    goto Orig;
                }
                if (!board.GetPawn(orig).getValidMoves(board).Contains(dest))
                {
                    Console.WriteLine("not a valid move!");
                    goto Dest;
                }

                var hit1 = board.Move(orig, dest);
                if (hit1 is Pawn)
                {
                    Player2.Pawns.Remove(hit1);
                }
                board.Draw();

                //check win
                if (dest.EndsWith("3") || Player2.GetMovablePawns(board).Count == 0)
                {
                    Console.WriteLine("You Have Won");
                    Player1.Score++;
                    goto newRound;
                }
                if (Player2.losingStates.Count > 0 && Player2.Steps > 0)
                {
                    Player2.losingStates.RemoveAt(Player2.losingStates.Count - 1);
                }

                //Computer Move
                var hit2 = Player2.Play(board);
                if (hit2 is Pawn)
                {
                    Player1.Pawns.Remove(hit2);
                }
                board.Draw();

                //check win
                if (Player2.Pawns.Where(x => x.getPosition(board).EndsWith("1")).Count() > 0 || Player1.GetMovablePawns(board).Count == 0)
                {
                    Console.WriteLine("The Computer Has Won");
                    Player2.Score++;
                    goto newRound;
                }
                Player2.losingStates.Add(board.GetCode());
                Player2.Steps++;
            }
        }
    }

    class Computer : Player
    {
        public List<string> losingStates { get; set; }
        public Computer(string Color) : base(Color)
        {
            this.losingStates = new List<string>() { };
        }

        private Random rnd = new Random();

        public Pawn Play(Board board)
        {
            var goodMoves = new List<List<string>> { };

            var movablePawns = this.GetMovablePawns(board);
            foreach (var p in movablePawns)
            {
                var possibleDest = p.getValidMoves(board);
                foreach (var d in possibleDest)
                {
                    Board btemp = new Board(board);
                    var orgLoc = p.getPosition(btemp);
                    btemp.Move(p.getPosition(btemp), d);

                    if (!losingStates.Contains(btemp.GetCode()))
                    {
                        goodMoves.Add(new List<string> { orgLoc, d });
                    }
                }
            }
            if (goodMoves.Count == 0)
            {
                //this should never happen!!
                Console.WriteLine("Computer cannot find a good move!! Errrrorrr quitquit!@#");
            }
            var r = rnd.Next(goodMoves.Count);

            Console.WriteLine($"Computer moved {goodMoves[r][0]} to {goodMoves[r][1]}");
            var hit = board.Move(goodMoves[r][0], goodMoves[r][1]);

            return hit;
        }
    }

    class Player
    {
        public List<Pawn> Pawns { get; set; }

        public string Color { get; set; }
        public int Score { get; set; }
        public int Steps { get; set; }
        public Player(string Color)
        {
            this.Color = Color;
            this.Pawns = new List<Pawn>() { };
            this.Score = 0;
            this.Steps = 0;
            foreach (var x in Enumerable.Range(1, 3))
            {
                Pawns.Add(new Pawn(x, this.Color));
            }
        }

        public List<Pawn> GetMovablePawns(Board board)
        {
            return this.Pawns.Where(x => x.getValidMoves(board).Count() > 0).ToList();
        }

        public void ResetPawns()
        {
            this.Pawns.Clear();
            foreach (var x in Enumerable.Range(1, 3))
            {
                Pawns.Add(new Pawn(x, this.Color));
            }
            this.Steps = 0;
        }
    }

    class Pawn
    {
        public int Id { get; set; }
        public string Color { get; set; }

        public Pawn(int Id, string Color)
        {
            this.Id = Id;
            this.Color = Color;
        }

        //Statics
        static public Dictionary<string, string> validMovesWhite = new Dictionary<string, string>()
        {
            { "A1", "A2" }, { "B1", "B2" }, { "C1", "C2" }, { "A2", "A3" }, { "B2", "B3" }, { "C2", "C3" }
        };
        static public Dictionary<string, string[]> validHitsWhite = new Dictionary<string, string[]>()
        {
            { "A1", new string[] {"B2" } }, { "B1", new string[] {"A2", "C2" } }, { "C1", new string[] {"B2"} },
            { "A2", new string[] {"B3"} }, { "B2", new string[] {"A3", "C3" } }, { "C2", new string[] {"B3"} }
        };
        static public Dictionary<string, string> validMovesBlack = new Dictionary<string, string>()
        {
            { "A3", "A2" }, { "B3", "B2" }, { "C3", "C2" }, { "A2", "A1" }, { "B2", "B1" }, { "C2", "C1" }
        };
        static public Dictionary<string, string[]> validHitsBlack = new Dictionary<string, string[]>()
        {
            { "A3", new string[] {"B2" } }, { "B3", new string[] {"A2", "C2" } }, { "C3", new string[] {"B2"} },
            { "A2", new string[] {"B1"} }, { "B2", new string[] {"A1", "C1" } }, { "C2", new string[] {"B1"} }
        };

        public string getPosition(Board board)
        {
            return board.state.FirstOrDefault(x => x.Value == this).Key;
        }

        public List<string> getValidMoves(Board board)
        {
            var moves = new List<string>();

            if (this.Color == "White")
            {
                if (this.getPosition(board).EndsWith("3"))
                {
                    return moves;
                }
                if (board.GetPawn(validMovesWhite[this.getPosition(board)]) == null)
                {
                    moves.Add(validMovesWhite[this.getPosition(board)]);
                }
                validHitsWhite[this.getPosition(board)].Where(x => board.GetPawn(x) != null && board.GetPawn(x).Color == "Black").ToList().ForEach(y => moves.Add(y));
            }
            else
            {
                if (this.getPosition(board).EndsWith("1"))
                {
                    return moves;
                }
                if (board.GetPawn(validMovesBlack[this.getPosition(board)]) == null)
                {
                    moves.Add(validMovesBlack[this.getPosition(board)]);
                }
                validHitsBlack[this.getPosition(board)].Where(x => board.GetPawn(x) != null && board.GetPawn(x).Color == "White").ToList().ForEach(y => moves.Add(y));
            }
            return moves;
        }
    }

    class Board
    {
        static string[] Tiles = { "A1", "B1", "C1", "A2", "B2", "C2", "A3", "B3", "C3" };
        public Dictionary<string, Pawn> state;

        //new board with pawns in starting positions
        public Board(Player P1, Player P2)
        {
            state = new Dictionary<string, Pawn>()
            {
                { "A1", P1.Pawns[0] },  { "B1", P1.Pawns[1] },  { "C1", P1.Pawns[2] },
                { "A2", null}, { "B2", null}, { "C2", null},
                { "A3", P2.Pawns[0] },  { "B3", P2.Pawns[1] },  { "C3", P2.Pawns[2] }
            };
        }

        //copy board
        public Board(Board existing)
        {
            this.state = new Dictionary<string, Pawn>(existing.state);
        }

        public string GetCode()
        {
            var code = "";
            foreach (var t in Tiles)
            {
                code += GetPiece(t) != " " ? GetPiece(t) : ".";
            }
            return code;
        }

        public Pawn GetPawn(string pos)
        {
            return state.ContainsKey(pos) ? state[pos] : null;
        }

        public Pawn Move(string orig, string dest)
        {
            var hit = state[dest];
            state[dest] = state[orig];
            state[orig] = null;
            return hit;
        }

        public string GetPosition(Pawn pawn)
        {
            return state.FirstOrDefault(x => x.Value == pawn).Key;
        }

        public string GetPiece(string pos)
        {
            return state[pos] is Pawn ? state[pos].Color.Substring(0, 1) : " ";
        }

        public void Draw()
        {
            int y = 0;
            Console.WriteLine($"   ┌───┬───┬───┐");
            Console.WriteLine($" 3 │ { this.GetPiece("A3") } │ { this.GetPiece("B3") } │ { this.GetPiece("C3") } │");
            Console.WriteLine($"   ├───┼───┼───┤");
            Console.WriteLine($" 2 │ { this.GetPiece("A2") } │ { this.GetPiece("B2") } │ { this.GetPiece("C2") } │");
            Console.WriteLine($"   ├───┼───┼───┤");
            Console.WriteLine($" 1 │ { this.GetPiece("A1") } │ { this.GetPiece("B1") } │ { this.GetPiece("C1") } │");
            Console.WriteLine($"   └───┴───┴───┘");
            Console.WriteLine($"     A   B   C  ");
            Console.WriteLine($"\n");
        }
    }
}