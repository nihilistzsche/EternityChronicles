using System.Collections.Generic;

namespace IronDragon.Builtins
{
    public class DragonRange
    {
        public DragonRange(int start, int end, bool inclusive = false)
        {
            Start = start;
            End = end;
            Inclusive = inclusive;
        }

        public int Start { get; }

        public int End { get; }

        public bool Inclusive { get; }

        public override int GetHashCode()
        {
            return Start * 0xFF / 0xCE + End * 0xEB / 0xFA + (Inclusive ? 0xFF3C : 0xCC4D);
        }

        public override bool Equals(object o)
        {
            if (o.GetType() != typeof(DragonRange)) return false;
            var r = o as DragonRange;
            return r.Start == Start && r.End == End && r.Inclusive == Inclusive;
        }

        public static implicit operator DragonArray(DragonRange range)
        {
            var l = new List<dynamic>();
            for (var i = range.Start; range.Inclusive ? i <= range.End : i < range.End; i++) l.Add(i);
            return new DragonArray(l);
        }
    }
}