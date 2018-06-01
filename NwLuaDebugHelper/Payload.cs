#region Copyright (C) 2018 Helmut Wahrmann

/* 
 *  Copyright (C) 2018 Helmut Wahrmann
 *  https://github.com/hwahrmann/NwLUADebugProject
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 3, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

#endregion

namespace NwLuaDebugHelper
{
    /// <summary>
    /// The Payload object, which is passed back to the LUA Interpreter
    /// </summary>
    public class Payload
    {
        #region Variables

        private string _payload = null;

        #endregion

        #region ctor
        
        /// <summary>
        /// Construct the Payload object using the inout ayload string
        /// </summary>
        /// <param name="payload"></param>
        public Payload(string payload)
        {
            _payload = payload;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the Payload as a string
        /// </summary>
        /// <returns></returns>
        public string tostring()
        {
            return _payload;
        }

        /// <summary>
        /// Find the given string somewhere in the payload
        /// </summary>
        /// <param name="searchString"></param>
        /// <returns></returns>
        public string find(string searchString)
        {
            return find(searchString, 1, -1);
        }

        /// <summary>
        /// Find the given string from the starting position to the end of the payload
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public string find(string searchString, int start)
        {
            return find(searchString, start, -1);
        }

        /// <summary>
        /// Find the given string between the given range
        /// </summary>
        /// <param name="searchString"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the Length of the Payload
        /// </summary>
        /// <returns></returns>
        public int len()
        {
            return _payload.Length;
        }

        #endregion
    }
}
