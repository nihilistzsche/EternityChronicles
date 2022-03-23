// DragonRange.cs in EternityChronicles/IronDragon
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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