using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using NLua;

namespace NwLuaDebugHelper
{
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

        public NwLuaDebugHelper(ObjectTranslator translator)
        {
            _translator = translator;
        }

        #endregion

        #region Private Methods

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

        private void ProcessDecoderInput(DecoderInput input)
        {
            _currentDecoderInput = input;
            _payLoad = new Payload(input.Payload);

            foreach (KeyValuePair<string, string> item in input.Meta)
            {
                CallBack callback = null;
                if (_callBacks.TryGetValue(item.Key, out callback))
                {
                    logger.Info($"Invoking Meta callback for {item.Key.ToString()}. Value: {item.Value.ToString()}");
                    callback.Function.Call(item.Value.ToString());
                }
            }
        }

        #endregion

        #region Public Methods

        public Payload GetPayload()
        {
            return _payLoad;
        }

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

        public void setCallbacks(LuaTable callBackTable)
        {
            _callBacks.Clear();
            foreach (KeyValuePair<Object, Object> element in callBackTable)
            {
                var callBack = new CallBack();
                callBack.Function = (LuaFunction)element.Value;
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
                else
                {
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
            }
        }

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

            // First do possible "Begin" Event
            if (_cbOnSessionBegin != null)
            {
                _cbOnSessionBegin.Call();
            }

            logger.Info($"Reading instructions from file {file}");
            var i = 0;
            foreach (var input in decoderInput)
            {
                i++;
                logger.Info($"Processing line {i}");
                ProcessDecoderInput(input);
            }
        }

        #endregion

    }
}
