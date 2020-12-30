using System.Xml.Linq;

namespace XDL
{
    public interface ICustomLoader {
        object CustomLoadObject(XElement element, object context);
    }
}
