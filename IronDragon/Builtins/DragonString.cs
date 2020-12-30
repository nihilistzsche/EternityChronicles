using System.Text;
using IronDragon.Runtime;

namespace IronDragon.Builtins {
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    [DragonExport("String")]
    public class DragonString {
        public DragonString() {
            _internal = new StringBuilder();
        }

        public DragonString(string @string) {
            _internal = new StringBuilder(@string);
        }

        private StringBuilder _internal { get; }

        public override int GetHashCode() {
            return _internal.ToString().GetHashCode();
        }

        public override string ToString() {
            return _internal.ToString();
        }

        public override bool Equals(object obj) {
            if (obj is DragonString) {
                return ((DragonString) obj)._internal.ToString() == _internal.ToString();
            }
            if (obj is string) {
                return (string) obj == _internal.ToString();
            }
            return false;
        }

        [DragonExport("<<")]
        public void StringAdd(dynamic val) {
            _internal.Append((string) val);
        }

        public static implicit operator string(DragonString s) {
            return s._internal.ToString();
        }

        public static implicit operator DragonString(string s) {
            return new(s);
        }
    }
}