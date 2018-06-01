using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NwLuaDebugHelper
{
    public class DecoderInput
    {
        public Dictionary<string, string> Meta = new Dictionary<string, string>();
        public string Port;
        public string Token;
        public string Payload;
    }
}
