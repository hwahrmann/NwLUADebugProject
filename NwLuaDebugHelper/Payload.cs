using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NwLuaDebugHelper
{
    public class Payload
    {
        #region Variables

        private string _payload = null;

        #endregion

        #region ctor
        
        public Payload(string payload)
        {
            _payload = payload;
        }

        #endregion

        public string tostring()
        {
            return _payload;
        }

        public string find(string searchString)
        {
            return find(searchString, 1, -1);
        }

        public string find(string searchString, int start, int end)
        {
            int pos = -1;
            if (end == -1)
            {
                pos = _payload.IndexOf(searchString, start - 1);
            }
            else
            {
                pos = _payload.IndexOf(searchString, start - 1, end - 1);
            }
            return pos == -1 ? null:pos.ToString();
        }

        public int len()
        {
            return _payload.Length;
        }
    }
}
