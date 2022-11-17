using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Kalmia_Game.Model.Reversi
{
    internal enum DiscColor : int
    {
        Black = 1,
        Empty = 0,
        White = -1
    }

    internal enum BoardCoordinate : int
    {
        A1, B1, C1, D1, E1, F1, G1, H1,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A8, B8, C8, D8, E8, F8, G8, H8,
        Pass, Null
    }

    internal class GameResult
    {
        public DiscColor Winner { get; set; } = DiscColor.Empty;
        public bool GameOver { get; set; } = false;
        public bool Draw { get; set; } = false;
        public int DiscDiff { get; set; } = 0;
    }

    struct Move
    {
        public DiscColor Player { get; set; }
        public BoardCoordinate Coord { get; set; }

        public Move(DiscColor player, BoardCoordinate coord) { this.Player = player; this.Coord = coord; }
    }

    internal class Position : IEnumerable<DiscColor>
    {
        public const int BOARD_SIZE = 8;
        public const int SQUARE_NUM = 64;
        public static ReadOnlySpan<(DiscColor color, BoardCoordinate coord)> CrossCoordinates => new (DiscColor, BoardCoordinate)[]
        {
            (DiscColor.Black, BoardCoordinate.E4), (DiscColor.Black, BoardCoordinate.D5),
            (DiscColor.White, BoardCoordinate.D4), (DiscColor.White, BoardCoordinate.E5)
        };

        public DiscColor SideToMove { get; private set; }
        public bool CanPass => CalculateMobility(this.SideToMove).Length == 0;
        public ReadOnlyCollection<Move> MoveHistroy => new(this.moveHistroy);

        public DiscColor this[BoardCoordinate coord] => this.discs[(int)coord];

        DiscColor[] discs = new DiscColor[SQUARE_NUM];
        List<Move> moveHistroy = new();

        public Position()
        {
            this.discs[(int)BoardCoordinate.D4] = this.discs[(int)BoardCoordinate.E5] = DiscColor.White;
            this.discs[(int)BoardCoordinate.E4] = this.discs[(int)BoardCoordinate.D5] = DiscColor.Black;
            this.SideToMove = DiscColor.Black;
        }

        public Position(Position pos)
        {
            this.discs = (DiscColor[])pos.discs.Clone();
            this.SideToMove = pos.SideToMove;
            lock (pos.MoveHistroy)
                this.moveHistroy = new List<Move>(pos.moveHistroy);
        }

        public static BoardCoordinate StringToBoardCoordinate(string coordStr)
        {
            coordStr = coordStr.ToLower();
            if (coordStr == "pass")
                return BoardCoordinate.Pass;

            var x = coordStr[0] - 'a';
            var y = coordStr[1] - '1';
            if (x < 0 || y < 0 || x >= BOARD_SIZE || y >= BOARD_SIZE)
                return BoardCoordinate.Null;
            return (BoardCoordinate)(x + y * BOARD_SIZE);
        }

        public int CountDiscs(DiscColor color)
        {
            var count = 0;
            for (var coord = 0; coord < SQUARE_NUM; coord++)
                if (this.discs[coord] == color)
                    count++;
            return count;
        }

        public int CountEmptySquares()
        {
            var count = 0;
            for (var coord = 0; coord < SQUARE_NUM; coord++)
                if (this.discs[coord] == DiscColor.Empty)
                    count++;
            return count;
        }

        public BoardCoordinate[]? Update(BoardCoordinate move)
        {
            if (!IsLegalMove(move))
                return null;

            BoardCoordinate[] flipped;
            if (move != BoardCoordinate.Pass)
            {
                this.discs[(int)move] = this.SideToMove;
                flipped = FlipDiscs(this.SideToMove, move);
            }
            else
                flipped = Array.Empty<BoardCoordinate>();

            this.moveHistroy.Add(new Move(this.SideToMove, move));
            this.SideToMove = (DiscColor)(-(int)this.SideToMove);
            return flipped;
        }

        public bool IsLegalMove(BoardCoordinate move) => GetNextMoves().Contains(move);

        public BoardCoordinate[] GetNextMoves()
        {
            var mobility = CalculateMobility(this.SideToMove);
            if (mobility.Length == 0)
            {
                var oppMobility = CalculateMobility((DiscColor)(-(int)this.SideToMove));
                mobility = (oppMobility.Length != 0) ? new BoardCoordinate[] { BoardCoordinate.Pass } : Array.Empty<BoardCoordinate>();
            }
            return mobility;
        }

        public GameResult GetGameResult()
        {
            if (GetNextMoves().Length != 0)
                return new GameResult();

            (var bCount, var wCount) = (CountDiscs(DiscColor.Black), CountDiscs(DiscColor.White));
            if (bCount > wCount)
                return new GameResult { Winner = DiscColor.Black, DiscDiff = bCount - wCount, GameOver = true };

            if (bCount < wCount)
                return new GameResult { Winner = DiscColor.White, DiscDiff = wCount - bCount, GameOver = true };

            return new GameResult { Draw = true, GameOver = true };
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
                return false;

            if (obj is Position pos)
                return pos.SideToMove == this.SideToMove && pos.SequenceEqual(this);

            return false;
        }

        public override int GetHashCode() => base.GetHashCode();    // 警告を抑えるためのコード.

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("  ");
            for (var i = 0; i < BOARD_SIZE; i++)
                sb.Append($"{(char)('A' + i)} ");

            for (var y = 0; y < BOARD_SIZE; y++)
            {
                sb.Append($"\n{y + 1} ");
                for (var x = 0; x < BOARD_SIZE; x++)
                {
                    var disc = this.discs[x + y * BOARD_SIZE];
                    if (disc == DiscColor.Black)
                        sb.Append("X ");
                    else if (disc == DiscColor.White)
                        sb.Append("O ");
                    else
                        sb.Append(". ");
                }
            }
            return sb.ToString();
        }

        public IEnumerator<DiscColor> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        BoardCoordinate[] CalculateMobility(DiscColor color)
        {
            var mobility = new List<BoardCoordinate>();
            for (var pos = 0; pos < this.discs.Length; pos++)
            {
                if (this.discs[pos] != DiscColor.Empty)
                    continue;
                if (CheckMobility(color, (BoardCoordinate)pos))
                    mobility.Add((BoardCoordinate)pos);
            }
            return mobility.ToArray();
        }

        bool CheckMobility(DiscColor color, BoardCoordinate coord)
        {
            return CheckMobility(color, coord, 1, 0)
                || CheckMobility(color, coord, -1, 0)
                || CheckMobility(color, coord, 0, 1)
                || CheckMobility(color, coord, 0, -1)
                || CheckMobility(color, coord, 1, 1)
                || CheckMobility(color, coord, -1, -1)
                || CheckMobility(color, coord, -1, 1)
                || CheckMobility(color, coord, 1, -1);
        }

        bool CheckMobility(DiscColor color, BoardCoordinate coord, int dirX, int dirY)
        {
            var oppColor = (DiscColor)(-(int)color);
            (int x, int y) = ((int)coord % BOARD_SIZE, (int)coord / BOARD_SIZE);

            (int nextX, int nextY) = (x + dirX, y + dirY);
            Func<int, int, bool> outOfRange = (x, y) => nextX < 0 || nextX >= BOARD_SIZE || nextY < 0 || nextY >= BOARD_SIZE;
            if (outOfRange(nextX, nextY) || this.discs[nextX + nextY * BOARD_SIZE] != oppColor)
                return false;

            do
            {
                nextX += dirX;
                nextY += dirY;
            } while (!outOfRange(nextX, nextY) && this.discs[nextX + nextY * BOARD_SIZE] == oppColor);

            if (!outOfRange(nextX, nextY) && this.discs[nextX + nextY * BOARD_SIZE] == color)
                return true;
            return false;
        }

        BoardCoordinate[] FlipDiscs(DiscColor color, BoardCoordinate coord)
        {
            var flipped = new List<BoardCoordinate>();
            FlipDiscs(color, coord, 1, 0, flipped);
            FlipDiscs(color, coord, -1, 0, flipped);
            FlipDiscs(color, coord, 0, 1, flipped);
            FlipDiscs(color, coord, 0, -1, flipped);
            FlipDiscs(color, coord, 1, 1, flipped);
            FlipDiscs(color, coord, -1, -1, flipped);
            FlipDiscs(color, coord, -1, 1, flipped);
            FlipDiscs(color, coord, 1, -1, flipped);
            return flipped.ToArray();
        }

        void FlipDiscs(DiscColor color, BoardCoordinate coord, int dirX, int dirY, List<BoardCoordinate> flipped)
        {
            var oppColor = (DiscColor)(-(int)color);
            (int x, int y) = ((int)coord % BOARD_SIZE, (int)coord / BOARD_SIZE);

            if (!CheckMobility(color, coord, dirX, dirY))
                return;

            (int nextX, int nextY) = (x + dirX, y + dirY);
            while (nextX >= 0 && nextX < BOARD_SIZE && nextY >= 0 && nextY < BOARD_SIZE && this.discs[nextX + nextY * BOARD_SIZE] == oppColor)
            {
                var flippedPos = nextX + nextY * BOARD_SIZE;
                this.discs[flippedPos] = color;
                flipped.Add((BoardCoordinate)flippedPos);
                nextX += dirX;
                nextY += dirY;
            }
        }

        public struct Enumerator : IEnumerator<DiscColor>
        {
            DiscColor[] discs;
            int coord;

            public DiscColor Current { get; private set; }

            object IEnumerator.Current { get { return this.Current; } }

            public Enumerator(Position pos)
            {
                this.discs = pos.discs;
                this.coord = -1;
                this.Current = DiscColor.Empty;
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (++this.coord == SQUARE_NUM)
                    return false;
                this.Current = discs[this.coord];
                return true;
            }

            public void Reset()
            {
                this.coord = -1;
            }
        }
    }
}
