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

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using NLog;
using NLua;

namespace NwLuaDebugHelper
{
    /// <summary>
    /// The main class, which interacts with the Lua Interpreter. It has implementation of most of the nw-api functions.
    /// </summary>
    public class NwLuaDebugHelper
    {
        #region Variables

        private ObjectTranslator _translator;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private const string DEFAULT_LOGFILE = "NwLuaDebugHelper.log";
        private Dictionary<string, CallBack> _callBacks = new Dictionary<string, CallBack>();

        private DecoderInput _currentDecoderInput = null;
        private Payload _payLoad = null;

        // Special callbacks
        private LuaFunction _cbOnInit = null;
        private LuaFunction _cbOnStart = null;
        private LuaFunction _cbOnStop = null;
        private LuaFunction _cbOnReset = null;
        private LuaFunction _cbOnSessionBegin = null;
        private LuaFunction _cbOnSessionEnd = null;
        private LuaFunction _cbOnStreamBegin = null;
        private LuaFunction _cbOnStreamEnd = null;

        #endregion

        #region ctor

        /// <summary>
        /// Main constructor invoked out of nw-api.
        /// the paramegter points to the trabslator holding a reference to the LUA Interpreter.
        /// </summary>
        /// <param name="translator"></param>
        public NwLuaDebugHelper(ObjectTranslator translator)
        {
            _translator = translator;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Deserializes a JSON object, which was read from the input file.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="input"></param>
        /// <returns></returns>
        private IEnumerable<T> DeserializeObjects<T>(string input)
        {
            JsonSerializer serializer = new JsonSerializer();
            using (var strreader = new StringReader(input))
            using (var jsonreader = new JsonTextReader(strreader))
            {
                jsonreader.SupportMultipleContent = true;
                while (jsonreader.Read())
                {
                    yield return serializer.Deserialize<T>(jsonreader);
                }
            }
        }

        /// <summary>
        /// Analyses the Input line and then invokes the respective LUA Function as set via Callbacks from the parser
        /// </summary>
        /// <param name="input"></param>
        private void ProcessDecoderInput(DecoderInput input)
        {
            _currentDecoderInput = input;
            _payLoad = new Payload(input.Payload);

            // Process Meta Callbacks
            if (input.Meta != null)
            {
                foreach (KeyValuePair<string, string> item in input.Meta)
                {
                    CallBack callback = null;
                    if (_callBacks.TryGetValue(item.Key, out callback))
                    {
                        logger.Debug($"Invoking Meta callback for {item.Key.ToString()}. Value: {item.Value.ToString()}");
                        callback.Function.Call(item.Value.ToString());
                    }
                }
            }

            // Process Token Callbacks
            foreach (KeyValuePair<string, CallBack> item in _callBacks)
            {
                CallBack callback = item.Value;
                if (callback.callBackType == Enums.CallbackType.Token)
                {
                    string token = callback.Token;
                    bool match = false;
                    int first, last = 0;
                    if (token.StartsWith("^"))
                    {
                        match = _payLoad.tostring().StartsWith(token.Substring(1));
                        first = 1;
                        last = token.Length - 1;
                    }
                    else if (token.EndsWith("$"))
                    {
                        match = _payLoad.tostring().EndsWith(token.Substring(0, token.Length - 1));
                        first = _payLoad.len() - token.Length + 2;
                        last = _payLoad.len();
                    }
                    else
                    {
                        first = _payLoad.tostring().IndexOf(token);
                        if (first > -1)
                        {
                            first = first + 1;
                            last = first + token.Length - 1;
                            match = true;
                        }
                    }

                    if (match)
                    {
                        logger.Debug($"Invoking Token callback for {token}.");
                        callback.Function.Call(token, first, last);
                    }
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// NW-API: nw.getPayload()
        /// Returns a Payload object
        /// </summary>
        /// <returns></returns>
        public Payload GetPayload()
        {
            return _payLoad;
        }

        /// <summary>
        /// Allows setting of the LogLevel and LogFile
        /// </summary>
        /// <param name="loglevel"></param>
        /// <param name="path"></param>
        public void SetLogger(string loglevel, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = DEFAULT_LOGFILE;
            }

            var folder = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var level = LogLevel.FromString(loglevel);
            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget() { FileName = path, Name = "logfile" };
            config.LoggingRules.Add(new NLog.Config.LoggingRule("*", level, logfile));
            NLog.LogManager.Configuration = config;
            logger.Info("Set new Logging configuration");
        }

        /// <summary>
        /// Writes to the log file using the specified Log Level
        /// </summary>
        /// <param name="level"></param>
        /// <param name="message"></param>
        public void WriteLog(string level, string message)
        {
            switch (level)
            {
                case "Debug":
                    logger.Debug(message);
                    break;
                case "Warn":
                    logger.Warn(message);
                    break;
                case "Info":
                    logger.Info(message);
                    break;
                case "Failure":
                    logger.Fatal(message);
                    break;
            }
        }

        /// <summary>
        /// NW-API: parser:setCallbacks
        /// 
        /// Sets the callbacks as specified in the Parser
        /// </summary>
        /// <param name="callBackTable"></param>
        public void setCallbacks(LuaTable callBackTable)
        {
            _callBacks.Clear();
            foreach (KeyValuePair<Object, Object> element in callBackTable)
            {
                var callBack = new CallBack();
                callBack.Function = (LuaFunction)element.Value;
                // Are we looking at Meta Callbacks
                if (element.Key is LuaTable)
                {
                    callBack.callBackType = Enums.CallbackType.Meta;

                    // Can't find an easy way of getting an element from ICollection
                    int i = 0;
                    foreach (string key in (element.Key as LuaTable).Keys)
                    {
                        if (key == "name")
                        {
                            break;
                        }
                        i++;
                    }
                    int j = 0;
                    foreach (var value in (element.Key as LuaTable).Values)
                    {
                        if (i == j)
                        {
                            callBack.Token = (string)value;
                            break;
                        }
                        j++;
                    }
                    logger.Info($"Registering callback for {callBack.Token}");
                    _callBacks.Add(callBack.Token, callBack);
                }
                else if (element.Key.ToString().StartsWith("Nw."))
                {
                    // Check for specific "nwevents"
                    if (element.Key.ToString() == "Nw.OnInit")
                    {
                        _cbOnInit = callBack.Function;
                    }
                    else if (element.Key.ToString() == "Nw.OnStart")
                    {
                        _cbOnStart = callBack.Function;
                    }
                    else if (element.Key.ToString() == "Nw.OnStop")
                    {
                        _cbOnStop = callBack.Function;
                    }
                    else if (element.Key.ToString() == "Nw.OnReset")
                    {
                        _cbOnReset = callBack.Function;
                    }
                    else if (element.Key.ToString() == "Nw.OnSessionBegin")
                    {
                        _cbOnSessionBegin = callBack.Function;
                    }
                    else if (element.Key.ToString() == "Nw.OnSessionEnd")
                    {
                        _cbOnSessionEnd = callBack.Function;
                    }
                    else if (element.Key.ToString() == "Nw.OnStreamBegin")
                    {
                        _cbOnStreamBegin = callBack.Function;
                    }
                    else if (element.Key.ToString() == "Nw.OnStreamEnd")
                    {
                        _cbOnStreamEnd = callBack.Function;
                    }
                    logger.Info($"Registering callback for {element.Key.ToString()}");
                }
                else
                {
                    // Token Match
                    callBack.callBackType = Enums.CallbackType.Token;
                    callBack.Token = element.Key.ToString();
                    _callBacks.Add(callBack.Token, callBack);
                }
            }
        }

        /// <summary>
        /// The "main" loop, which starts reading the input file and invokes the callbacks based on the json input
        /// </summary>
        /// <param name="inputFile"></param>
        public void Process(string inputFile)
        {
            var file = inputFile;
            if (string.IsNullOrEmpty(inputFile))
            {
                file = "decoder.json";
            }

            var json = "";
            try
            {
                StreamReader sr = new StreamReader(file);
                json = sr.ReadToEnd();

            }
            catch (FileNotFoundException)
            {
                logger.Error($"Decoder Input File could not be found: {file}");
                return;
            }
            catch (IOException ex)
            {
                logger.Error($"Exception reading Decoder Input file: {ex.Message} {ex.StackTrace}");
                return;
            }

            var decoderInput = new List<DecoderInput>();
            try
            {
                decoderInput = DeserializeObjects<DecoderInput>(json).ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"Exception processing Decoder Input file: {ex.Message} {ex.StackTrace}");
                return;
            }

            // First do possible "Init" and "Start" Events
            if (_cbOnInit != null)
            {
                _cbOnInit.Call();
            }
            if (_cbOnStart != null)
            {
                _cbOnStart.Call();
            }

            logger.Info($"Reading instructions from file {file}");
            var i = 0;
            foreach (var input in decoderInput)
            {
                // Execute the Begin Events
                if (_cbOnSessionBegin != null)
                {
                    _cbOnSessionBegin.Call();
                }
                if (_cbOnStreamBegin != null)
                {
                    _cbOnStreamBegin.Call();
                }

                i++;
                logger.Info($"Processing line {i}");
                ProcessDecoderInput(input);

                // Execute the Begin Events
                if (_cbOnStreamEnd != null)
                {
                    _cbOnStreamEnd.Call();
                }
                if (_cbOnSessionEnd != null)
                {
                    _cbOnSessionEnd.Call();
                }
            }

            if (_cbOnReset != null)
            {
                _cbOnReset.Call();
            }
            if (_cbOnStop != null)
            {
                _cbOnStop.Call();
            }
        }

        #endregion

    }
}
