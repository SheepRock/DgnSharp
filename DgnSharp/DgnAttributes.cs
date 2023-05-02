using System;
using System.Collections.Generic;
using System.Text;

namespace DgnSharp
{
    public class DgnAttribute
    {
        public int Size { get; set; }
        public int Id { get; set; }
        public byte[] Content { get; set; }
        public int Header { get; set; }
    }
}
