using System;
using IronDragon.Parser;
using Antlr4.Runtime;

namespace IronDragon.Lexer
{
    public class DragonTokenFactory : ITokenFactory
    {
        internal DragonParser Parser { private get; set; }

        public IToken Create(int type, string text)
        {
            return new DragonToken<string>(type, text);
        }

        public IToken Create(Tuple<ITokenSource, ICharStream> source, int type, string text, int channel, int start, int stop, int line, int charPositionInLine)
        {
            return Create(type, text);
        }

        public int GetTokenFromName(string name)
        {
            var klass = typeof(DragonParser);
            try
            {
                var field = klass.GetField(name);

                return (int) field.GetValue(Parser);
            }
            catch (Exception)
            {
                return -2;
            }
        }
    }
}