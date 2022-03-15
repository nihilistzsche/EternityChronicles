using System.Collections.Generic;
using System.Security.Cryptography;

namespace DragonMUD.Data.Character
{
    public class Account : BaseObject
    {
        public string Name { get; set; }

        public SHA256 Password { get; set; }

        public Dictionary<string, Character> Characters { get; set; }
    }
}