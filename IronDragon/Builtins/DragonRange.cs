// DragonRange.cs in EternityChronicles/IronDragon
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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