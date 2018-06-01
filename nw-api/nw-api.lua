if nw then
	-- do not load this file if nw is already defined
	error(... .. ".lua should not be loaded as a parser/module file")
end

--[[

This file provides documentation for the NextGen parser API and is a working stub for use
in IDE's and validating basic syntax and structure using a stand-alone lua engine.

The only unrepresented functionality at this point is install decoder, is decoding,
transient meta, and packet id,

--]]

-- These enumerations define the list of callbacks the script can receive as a session is
-- being parsed (see setEvents).
-- NOTE: the values assigned here have no meaning and are just placeholders
nwevents = {}
nwevents.OnInit = "Nw.OnInit"					-- fired when the parsing engine is initialized, only once.
nwevents.OnStart = "Nw.OnStart"				-- fired when the system starts capture.
nwevents.OnStop = "Nw.OnStop"					-- fired when the system stops capture.
nwevents.OnReset = "Nw.OnReset"				-- fired each time a stream is found and parsing begins on that stream.
nwevents.OnSessionBegin = "Nw.OnSessionBegin"	-- fired at the beginning of each session parsed.
nwevents.OnSessionEnd = "Nw.OnSessionEnd"		-- fired at the end of each session parsed.
nwevents.OnStreamBegin = "Nw.OnStreamBegin"	-- fired at the beginning of each stream parsed.
nwevents.OnStreamEnd = "Nw.OnStreamEnd"		-- fired at the end of each stream parsed.

-- These enumerations define the list of possible data types that can be used for a index
-- key (see setKeys)
-- NOTE: the values assigned here have no meaning and are just placeholders
nwtypes ={}
nwtypes.Int8 = 0			-- A signed 8 bit number
nwtypes.UInt8 = 1			-- An unsigned 8 bit number
nwtypes.Int16 = 2			-- A signed 16 bit number
nwtypes.UInt16 = 3			-- An unsigned 16 bit number
nwtypes.Int32 = 4			-- A signed 32 bit number
nwtypes.UInt32 = 5			-- An unsigned 32 bit number
nwtypes.Int64 = 6			-- A signed 64 bit number
nwtypes.UInt64 = 7			-- An unsigned 64 bit number
nwtypes.UInt128 = 8			-- An unsigned 128 bit number
nwtypes.Float32 = 9			-- A 32 bit floating point number
nwtypes.Float64 = 10		-- A 64 bit floating point number
nwtypes.TimeT = 11			-- A 64 bit time representing the number of seconds from EPOCH
nwtypes.DayOfWeek = 12		-- A 8 bit integer representing the day of the week (0=Sunday, Saturday=6)
nwtypes.HourOfDay = 13		-- A 8 bit integer representing the hours of the day (0-23)
nwtypes.Binary = 14			-- Free form binary data (256 byte max)
nwtypes.Text = 15			-- Free form text (256 character max)
nwtypes.IPv4 = 16			-- A IPv4 address
nwtypes.IPv6 = 17			-- A IPv6 address
nwtypes.MAC = 18			-- A MAC address

nw = {}
nwlanguagekey = {}
nwpayload = {}

-- Netwitness Lua Debug Helper
import 'NwLuaDebugHelper'

nwldbhelper = NwLuaDebugHelper()

function nw.SetLogger(level, path)
	nwldbhelper:SetLogger(level, path)
end

function nw.Process(inputfile)
	nwldbhelper:Process(inputfile)
end


-- nwlanguagekey.create(name [, keyFormat [, description]])
-- keyFormat defaults to nwtype.Text and description is optional.
function nwlanguagekey.create(name, keyFormat, description)
	local result = {}
	result.name = name
	result.description = description and description or ""
	result.type = keyFormat and keyFormat or nwtypes.Text
	return result
end

-- returns the system default meta for path components (i.e. "filename", "directory", and
-- "extension").
function nwlanguagekey.getPathDefaults()
	return nwlanguagekey.create("filename"), nwlanguagekey.create("directory"), nwlanguagekey.create("extension")
end

-- nw.createParser(name, description [, appType])
-- Creates a new instance of a parser. This function must be called as the first line of
-- the script so that other parts of the script can reference the parser object. The name
-- is the system identifier given to the parser. Usually with no spaces. The desc can be
-- free text. The appType is used by the system to identify the application type as meta
-- is created.
function nw.createParser(name, description, appType)

	local parser = {}

	-- parser:setKeys(keysTable [, fileMeta, dirMeta, extMeta])
	-- Registers which keys the parser will create meta data for. The first parameter is a
	-- list of LanguageKeys and the path parameters are individual LanguageKeys.
	-- Example:
	--	parser:setKeys({nwlanguagekey.create("alert")})
	function parser:setKeys(keysTable, fileMeta, dirMeta, extMeta) end

	-- parser:setPorts(portsList)
	-- Registers which ports this parser would like to be notified on. When the system
	-- identifies a port it will fire the event if that port matches on what was
	-- registered. The first parameter is a table where the key is a port number and the
	-- value is a locally defined function callback. The setPorts function should be
	-- called in the script AFTER the call to createParser() and AFTER the callback
	-- function definition. The callback function will have a single argument (the port
	-- number) and any results will be discarded.
	-- Example:
	--	parser:setPorts({80 = function(port) end})
	function parser:setPorts(portsList) end

	-- parser:setTokens(tokensTable)
	-- Registers which tokens this parser would like to be notified on. When the system
	-- identifies a token it will fire the event if that token matches what was
	-- registered. The first parameter is a table where the key is a token string and the
	-- value is a locally defined function callback. The setTokens function should be
	-- called in the script AFTER the call to createParser() and AFTER the callback
	-- function definition.
	-- To match the token at line start or packet break, add '^' to the beginning of the
	-- token (e.g. "^USER"). To match the token at line end add a '$' to the end of the
	-- token (e.g. "USER$"). Use '%' to escape if necessary, where '%c' -> 'c' for any
	-- character c (e.g. "%^abc%1%%23%$" -> "^abc1%23$").
	-- The callback will pass the token index defined in self.tokens (e.g.
	-- self.tokens[token]) and the index of the first and last characters of the token
	-- occur in the payload (i.e. token, first,last). Any results from the callback are
	-- discarded.
	-- Example:
	--	parser:setTokens({["GET /"] = function(token, first, last) end})
	function parser:setTokens(tokensTable) end

	-- parser:setEvents(eventsTable)
	-- Registers which system events this parser would like to be notified on. When the
	-- system hits a state it will fire the event that was registered. The only parameter
	-- is a table keyed by LanguageKeys and the value is a locally defined function
	-- callback. The setEvents function should be called in the script AFTER the call to
	-- createParser() and AFTER the callback function definition.  The callback receives
	-- no arguments and any results are discarded.
	-- Example:
	--	parser:setEvents({[nwtypes.OnSessionBegin] = function() end})
	function parser:setEvents(eventsTable) end

	-- parser:setMeta(metaTable)
	-- Registers meta fields this parser would like to be notified on. When the system
	-- creates a meta value of the specified types, it will fire the event that was
	-- registered. The first parameter is a table where the key is an event enumeration
	-- and the value is a locally defined function callback. The setEvents function should
	-- be called in the script AFTER the call to createParser() and AFTER the callback
	-- function definition. The callback will pass the meta index defined in self.meta
	-- (e.g. self.meta[LanguageKey]) and the value created.
	-- Example:
	--	parser:setEvents({[nwlanguagekey.create("alert")] = function(meta, value) end})
	function parser:setMeta(metaTable) end

	-- parser:setCallbacks(callbacksTable)
	-- A single function for performing port, token, event, and meta callback
	-- registration.  The setCallbacks function should be called in the script AFTER the
	-- call to createParser() and AFTER the callback function definition.  Refer to the
	-- appropriate function for callback signatures.
	-- Example:
	--	parser:setCallbacks({
	--		[80]						= function(port) end,					-- port
	--		["GET /"]					= function(token, first, last) end		-- token
	--		[nwevents.OnSessionBegin]	= function() end, 						-- event
	--		[nwlanguagekey.create("alert")]	= function(value) end				-- meta
	--	})
	function parser:setCallbacks(callbacksTable) 
		nwldbhelper:setCallbacks(callbacksTable)
	end

	-- parser:createPathMeta(value [, fileMeta [, dirMeta [, extMeta]]])
	-- Create path meta from a single value.  The meta types can be omitted if specified
	-- by the call to setKeys.
	function parser:createPathMeta(value, fileMeta, dirMeta, extMeta) end

	return parser
end

-- Logs a debug message to the logging system. Expects the first parameter to be the
-- message.
function nw.logDebug(msg) 
	nwldbhelper:WriteLog("Debug", msg)
end

-- Logs a informational message to the logging system. Expects the first parameter to be
-- the message.
function nw.logInfo(msg) 
	nwldbhelper:WriteLog("Info", msg)
end

-- Logs a warning message to the logging system. Expects the first parameter to be the
-- message.
function nw.logWarning(msg) 
	nwldbhelper:WriteLog("Warn", msg)
end

-- Logs a failure message to the logging system. Expects the first parameter to be the
-- message.
function nw.logFailure(msg) 
	nwldbhelper:WriteLog("Failure", msg)
end

-- Get the application type for the current session, may be zero if not yet assigned.
-- Returns the app type.
function nw.getAppType() end

-- Sets the application type (e.g. 21 for FTP) for the current session.
function nw.setAppType(appType) end

-- returns the current transport protocol (udp/tcp), source port, and destination port.
function nw.getTransport() end

-- returns the current network protocol (ipv4/ipv6)
function nw.getNetworkProtocol() end

-- nw.createMeta(key, value [, j])
-- Creates meta of type key (e.g. self.keys.username) with the provided value.
-- value can be be of any lua value type or payload, but must be an index if the optional
-- index j is provided.
-- Strings and payloads are coerced to numeric types as if the had a textual
-- representation of the value (e.g. a payload referencing "123" will be coerced to the
-- numeric 123).
function nw.createMeta(key, value, j) end

-- decode base64 encoded value into a string
function nw.base64Decode(value) end

-- Get a string representation of the network (ipv4/ipv6) source address
function nw.getSessionSource() end

-- Get a string representation of the network (ipv4/ipv6) destination address
function nw.getSessionDestination() end

-- Get a string representation of the network (ipv4/ipv6) source and destination addresses
-- Example:
--	local src, dst = nw.getSessionAddresses()
function nw.getSessionAddresses() end

-- Get the source and destination ports (tcp/udp)
-- Example:
--	local src, dst = nw.getSessionPorts()
function nw.getSessionPorts() end

-- Get the number of streams, packets, bytes and payload bytes for the current session.
-- Example:
--	local streams, packets, bytes, pBytes = nw.getSessionStats()
function nw.getSessionStats() end

-- Get the number of packets, bytes, payload bytes, retransmitted packets, and
-- retransmitted payload bytes for the current stream.
-- Example:
--	local packets, bytes, pBytes, rPackets, rBytes = nw.getStreamStats()
function nw.getStreamStats() end

-- Returns true if the current stream is the request stream of the session.
function nw.isRequestStream() end

-- Returns true if the current stream is the response stream of the session.
function nw.isResponseStream() end

-- nw.getPayload([i [, j]])
-- Returns a payload object for the current stream that can be manipulated as a lua string.
-- If there is no current stream (e.g. init, start/stop, reset) the result will be nil.
-- i and j are optional, defaulting to 1 and -1 respectively.
-- WARINING:
--		A payload object is only valid for the life of a current session. Accessing a
--		saved payload object in a subsequent session will result in an error.
function nw.getPayload(i, j)
	--local payload = {}
	--setmetatable(payload, nwpayload)
	--return payload
	return nwldbhelper:GetPayload()
end

-- nwpayload.byte(p [, i [, j]])
-- same as string.byte(s, [,i [, j]]) with i defaulting to 1 and j defaulting to i.
function nwpayload.byte(p, i, j) end

-- nwpayload.short(p [, i [, j]])
-- similar to nwpayload.byte except 2 byte, big endian values are returned.  no value is
-- returned for last (j - i + 1) % 2 bytes
function nwpayload.short(p, i, j) end

-- nwpayload.int(p [, i [, j]])
-- similar to nwpayload.byte except 4 byte, big endian values are returned.  no value is
-- returned for last (j - i + 1) % 4 bytes
function nwpayload.int(p, i, j) end

-- nwpayload.tostring(p [, i [, j]])
-- similar to nwpayload.sub with the result being the bytes at the referenced indices
-- copied into a lua string instead of a payload.  i defaults to 1 and j to -1.
function nwpayload.tostring(p, i, j) end

-- nwpayload.find(p, pattern [, i [, j]])
-- Similar to string.find
-- The pattern is always considered a literal (as if the plain parameter of string.find
-- were set to true). The i is the starting position of the find (inclusive) and the j is
-- the 1 based terminating index of the find (inclusive), the default value is -1.
function nwpayload.find(p, pattern, i, j) end

-- Same as string.len(s) (see http://www.lua.org/manual/5.1/manual.html#pdf-string.len)
function nwpayload.len(p) end

-- nwpayload.sub(p, i [, j])
-- Same as string.sub(p) (see http://www.lua.org/manual/5.1/manual.html#pdf-string.sub)
-- The returned value, if not nil, is a lua string and has no dependencies on the payload
-- object
function nwpayload.sub(p, i, j) end

-- return the index into the stream of the start of the payload object
function nwpayload.pos(p) end

-- determine if the payload or lua string contains the same value
function nwpayload.equal(p, value) end

-- get the payload object for the first packet in the current payload
function nwpayload.getPacketPayload(p) end

-- get the payload object for the packet following the begining of the current payload
function nwpayload.getNextPacketPayload(p) end
