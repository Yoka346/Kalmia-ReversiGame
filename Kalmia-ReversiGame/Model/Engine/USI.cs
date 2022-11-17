using System.Text;

using Kalmia_Game.Model.Reversi;

namespace Kalmia_Game.Model.Engine
{
    internal static class USI
    {
        public static string ToSfenString(this Position pos)
        {
            var sb = new StringBuilder();
            foreach (var disc in pos)
                sb.Append(
                   disc switch
                   {
                       DiscColor.Black => 'X',
                       DiscColor.White => 'O',
                       _ => '-'
                   });
            sb.Append((pos.SideToMove == DiscColor.Black) ? 'B' : 'W');
            return sb.ToString();
        }
    }
}
