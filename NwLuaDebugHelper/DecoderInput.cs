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

using System.Collections.Generic;

namespace NwLuaDebugHelper
{
    /// <summary>
    /// The DecoderInput object that holds deserialized elements from the JSON Input File
    /// </summary>
    public class DecoderInput
    {
        public Dictionary<string, string> Meta = new Dictionary<string, string>();
        public string Port;
        public List<string> Token;
        public string Payload;
    }
}
