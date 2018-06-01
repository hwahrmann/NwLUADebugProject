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

using NLua;

namespace NwLuaDebugHelper
{
    /// <summary>
    /// The Callback Object holding the Type and the Function to be invoked
    /// </summary>
    public class CallBack
    {
        public Enums.CallbackType callBackType;
        public string Token;
        public LuaFunction Function;
    }
}
