using System;
using System.Collections.Generic;
using System.Text;

namespace DgnSharp
{
    public interface IElement
    {
        int Type { get; set; }
        int Level { get; set; }
        bool Deleted { get; set; }
        bool ComplexElement { get; set; }

        int WordToFollow { get; }
        byte[] RawContent { get; set; }
        byte[] GetRaw();
        void ReplaceText(string oldText, string newText);

        void ReplaceText(IEnumerable<(string oldText, string newText)> replacements);
    }
}
