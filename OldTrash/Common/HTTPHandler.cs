using System;
using System.Text;
using Common.Logging;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace Common
{
    public static class HTTPHandler
    {
        public static DateTime unixed = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        private static void SendJsonRequest(ref string json, ref Socket remoteSocket)
        {
            if (!string.IsNullOrEmpty(json))
            {
                string response = "HTTP/1.1 200\r\n";
                response += "Version: HTTP/1.1\r\n";
                response += "Content-Type: application/json\r\n";
                response += "Access-Control-Allow-Headers: *\r\n";
                response += "Access-Control-Allow-Origin: *\r\n";
                response += "Content-Length: " + (json.Length).ToString();
                response += "\r\n\r\n";
                response += json;
                if (remoteSocket.Connected)
                {
                    remoteSocket.Send(Encoding.ASCII.GetBytes(response));
                }
                Debug.Write(response);
                Logger.WriteLog("Return http 200 JSON response", LogLevel.Usual);
            }
        }
        private static void SendUTF8JsonRequest(ref string json, ref Socket remoteSocket)
        {
            if (!string.IsNullOrEmpty(json))
            {
                json = EncodeNonAsciiCharacters(ref json);
                string response = "HTTP/1.1 200\r\n";
                response += "Version: HTTP/1.1\r\n";
                response += "Content-Type: application/json\r\n";
                response += "Access-Control-Allow-Headers: *\r\n";
                response += "Access-Control-Allow-Origin: *\r\n";
                response += "Content-Length: " + (json.Length).ToString();
                response += "\r\n\r\n";
                response += json;
                if (remoteSocket.Connected)
                {
                    remoteSocket.Send(Encoding.ASCII.GetBytes(response));
                }
                Debug.Write(response);
                Logger.WriteLog("Return http 200 JSON response", LogLevel.Usual);
            }
        }
        private static void SendErrorJsonRequest(ref string json, ref Socket remoteSocket)
        {
            if (!string.IsNullOrEmpty(json))
            {
                string response = "HTTP/1.1 500\r\n";
                response += "Version: HTTP/1.1\r\n";
                response += "Content-Type: application/json\r\n";
                response += "Access-Control-Allow-Headers: *\r\n";
                response += "Access-Control-Allow-Origin: *\r\n";
                response += "Content-Length: " + json.Length.ToString();
                response += "\r\n\r\n";
                response += json;
                if (remoteSocket.Connected)
                {
                    remoteSocket.Send(Encoding.ASCII.GetBytes(response));
                }
                Debug.WriteLine(response);
                Logger.WriteLog("Return http 500 responce with JSON data.", LogLevel.Usual);
            }
        }
        public static bool RequiredJsonField(ref JObject json, ref string field_name, ref JTokenType field_type, ref Socket remoteSocket, ref dynamic value)
        {
            if (json == null)
            {
                Logger.WriteLog("Insert json is null, function CheckRequiredJsonField", LogLevel.Error);
                return false;
            }
            if (json.ContainsKey(field_name))
            {
                JToken token = json.GetValue(field_name);
                if (token.Type == field_type)
                {
                    value = token;
                    return true;
                }
                else
                {
                    Logger.WriteLog("Required field is not in correct format, field_name=" + field_name + " field_type=" + field_type, LogLevel.Error);
                    JsonAnswer(false, "Required field is not in correct format, field_name=" + field_name + " field_type=" + field_type, ref remoteSocket);
                    return false;
                }
            }
            else
            {
                Logger.WriteLog("Json does not contain required field, field_name=" + field_name, LogLevel.Error);
                JsonAnswer(false, "Json does not contain required field, field_name=" + field_name, ref remoteSocket);
                return false;
            }
        }
        public static bool OptionalJsonField(ref JObject json, ref string field_name, ref JTokenType field_type, ref dynamic value)
        {
            if (json == null)
            {
                Logger.WriteLog("Insert json is null, function CheckRequiredJsonField", LogLevel.Error);
                return false;
            }
            if (json.ContainsKey(field_name))
            {
                JToken token = json.GetValue(field_name);
                if (token.Type == field_type)
                {
                    value = token;
                    return true;
                }
                else
                {
                    Logger.WriteLog("Required field is not in correct format, field_name=" + field_name + " field_type=" + field_type, LogLevel.Error);
                    return false;
                }
            }
            else
            {
                Logger.WriteLog("Json does not contain required field, field_name=" + field_name, LogLevel.Error);
                return false;
            }
        }
        public static dynamic JsonRequest(ref string request, ref Socket remoteSocket)
        {
            JObject json = GetJsonFromRequest(ref request);
            if (json != null)
            {
                return json;
            }
            else
            {
                JsonAnswer(false, "Server can't define json object from request.", ref remoteSocket);
                Logger.WriteLog("Server can't define json object from request.", LogLevel.Error);
                return null;
            }
        }
        public static dynamic GetJsonFromRequest(ref string request)
        {
            if (!string.IsNullOrEmpty(request))
            {
                string json = "";
                int searchIndex = request.IndexOf("application/json", StringComparison.Ordinal);
                if (searchIndex == -1)
                {
                    Logger.WriteLog("Can not find \"application/json\" in request.", LogLevel.Error);
                    return null;
                }
                int indexFirstChar = request.IndexOf("{", searchIndex, StringComparison.Ordinal);
                if (indexFirstChar == -1)
                {
                    Logger.WriteLog("Can not find start json in request.", LogLevel.Error);
                    return null;
                }
                int indexLastChar = request.LastIndexOf("}", StringComparison.Ordinal);
                if (indexLastChar == -1)
                {
                    Logger.WriteLog("Can not find end json in request.", LogLevel.Error);
                    return null;
                }
                try
                {
                    json = request.Substring(indexFirstChar, indexLastChar - indexFirstChar + 1);
                    return JsonConvert.DeserializeObject<dynamic>(json);
                }
                catch (Exception e)
                {
                    Logger.WriteLog("Can not define json object in request. function GetJsonFromRequest(). Message: " + e.Message, LogLevel.Error);
                    return null;
                }
            }
            else
            {
                Logger.WriteLog("Insert request is null or empty, function GetJsonFromRequest", LogLevel.Error);
                return null;
            }
        }
        public static string GetContentDispositionValue(ref string request, string key)
        {
            if (string.IsNullOrEmpty(request))
            {
                Logger.WriteLog("Input value is null or empty, function FindValueContentDisposition()", LogLevel.Error);
                return null;
            }
            string findKey = "Content-Disposition: form-data; name=\"" + key + "\"";
            string boundary = GetBoundaryRequest(ref request);
            if (string.IsNullOrEmpty(boundary))
            {
                Logger.WriteLog("Can not get boundary from request, function FindValueContentDisposition", LogLevel.Error);
                return null;
            }
            boundary = "\r\n--" + boundary;
            if (request.Contains(findKey))
            {
                int searchKey = request.IndexOf(findKey, StringComparison.Ordinal) + findKey.Length;
                if (searchKey == -1)
                {
                    Logger.WriteLog("Can not find content-disposition key from request, function FindValueContentDisposition", LogLevel.Error);
                    return null;
                }
                int start_boundary = request.IndexOf(boundary, searchKey, StringComparison.Ordinal);
                if (start_boundary == -1)
                {
                    Logger.WriteLog("Can not end boundary from request, function FindValueContentDisposition", LogLevel.Error);
                    return null;
                }
                try
                {
                    for (int i = 0; i < start_boundary - searchKey; i++)
                    {
                        if (request[start_boundary - i] == '\n')
                        {
                            return request.Substring(start_boundary - i + 1, i - 1);
                        }
                    }
                    Logger.WriteLog("Can not define key value from request, function FindValueContentDisposition", LogLevel.Error);
                    return null;
                }
                catch
                {
                    Logger.WriteLog("Can not define key value from request, function FindValueContentDisposition", LogLevel.Error);
                    return null;
                }
            }
            else
            {
                Logger.WriteLog("Request does not contain find key, function FindValueContentDisposition", LogLevel.Error);
                return null;
            }
        }
        public static dynamic RequiredFormData(ref string request,ref Socket remoteSocket, string field_name, TypeCode field_type)
        {
            if (string.IsNullOrEmpty(request))
            {
                Logger.WriteLog("Input value is null or empty, function GetFormDataField()", LogLevel.Error);
                return null;
            }
            string field_value = GetContentDispositionValue(ref request, field_name);
            if (field_value != null)
            {
                switch(field_type)
                {
                    case TypeCode.Int32: 
                        int? field_int_value = ConvertSaveString(ref field_value, field_type);
                        if (field_int_value != null)
                        {
                            return field_int_value;
                        }
                        else
                        {
                            JsonAnswer(false, "Can not define field_type of value",ref remoteSocket);
                            return null;
                        }
                    case TypeCode.String: return field_value;
                    default:
                        JsonAnswer(false, "Can not define field_type of value", ref remoteSocket);
                        Logger.WriteLog("Can not define field_type of value, function CheckFormDataField", LogLevel.Error);
                        return null;
                }
            }
            else
            {
                JsonAnswer(false, "Can not define form-data value from request", ref remoteSocket);
                Logger.WriteLog("Can not define form-data value from request", LogLevel.Error);
                return null;
            }
        }
        public static string GetHeadParamRequest(ref string request, string key)
        {
            if (string.IsNullOrEmpty(request))
            {
                Logger.WriteLog("Input value is null or empty, function FindParamFromRequest()", LogLevel.Error);
                return null;
            }
            Regex urlParams = new Regex(@"[\?&](" + key + @"=([^&=#\s]*))", RegexOptions.Multiline);
            Match match = urlParams.Match(request);
            if (match.Success)
            {
                string value = match.Value;
                return value.Substring(key.Length + 2);
            }
            else
            {
                Logger.WriteLog("Can not define url parameter from request, function FindParamFromRequest", LogLevel.Error);
                return null;
            }
        }
        private static void SendIternalError(ref Socket remoteSocket)
        {
            string response = "";
            string responseBody = "<HTML>";
            responseBody += "<BODY>";
            responseBody += "<h1> 500 Internal Server Error...</h1>";
            responseBody += "</BODY></HTML>";
            response += "HTTP/1.1 500 \r\n";
            response += "Version: HTTP/1.1\r\n";
            response += "Content-Type: text/html; charset=utf-8\r\n";
            response += "Content-Length: " + (response.Length + responseBody.Length);
            response += "\r\n\r\n";
            response += responseBody;
            if (remoteSocket.Connected)
            {
                remoteSocket.Send(Encoding.ASCII.GetBytes(response));
            }
            Logger.WriteLog("HTTP 500 Error link response", LogLevel.Error);
        }
        public static dynamic ConvertSaveString(ref string value, TypeCode value_type)
        {
            if (string.IsNullOrEmpty(value))
            {
                Logger.WriteLog("Value is null or empty, function ConvertSaveString", LogLevel.Error);
                return null;
            }
            try
            {
                switch (value_type)
                {
                    case TypeCode.Int32: return Convert.ToInt32(value);
                    case TypeCode.Double: return Convert.ToDouble(value);
                    default: Logger.WriteLog("Can not define type of value, function ConvertSaveString()", LogLevel.Error);
                    return null;
                }
            }
            catch
            {
                Logger.WriteLog("Can not convert current value to type->" + value_type + ", function ConvertSaveString", LogLevel.Error);
                return null;
            }
        }
        public static string GetBoundaryRequest(ref string request)
        {
            int length = request.Length;
            bool exist = false;
            string boundary = "";
            int first = request.IndexOf("boundary=", StringComparison.Ordinal);
            if (first == -1)
            {
                Logger.WriteLog("Can not search boundary from request", LogLevel.Error);
                return null;
            }
            first += 9;                 // boundary=.Length
            while (!exist)
            {
                if (first < length)
                {
                    if (request[first] == '\r')
                    {
                        exist = true;
                    }
                    else
                    {
                        boundary += request[first];
                        ++first;
                    }
                }
                else exist = true;
            }
            return boundary;
        }
        public static string GetBearerToken(ref string request)
        {
            int length = request.Length;
            string token = "";
            int first = request.IndexOf("Authorization: Bearer ", StringComparison.Ordinal);
            if (first == -1)
            {
                Logger.WriteLog("Can not search start of bearer token from request.", LogLevel.Error);
                return null;
            }
            first += 22;                            //"Authorization: Bearer ".length
            int end = request.IndexOf('\r', first);
            if (end == -1)
            {
                Logger.WriteLog("Can not search end of bearer token from request.", LogLevel.Error);
                return null;
            }
            token = request.Substring(first, end - first);
            return token;
        }
        public static void JsonData(ref dynamic data, ref Socket remoteSocket)
        {
            string jsonAnswer = "{\r\n";
            jsonAnswer += "\"success\":true,\r\n";
            jsonAnswer += "\"data\":" + JsonConvert.SerializeObject(data) + "\r\n";
            jsonAnswer += "}";
            SendJsonRequest(ref jsonAnswer, ref remoteSocket);
        }
        public static void JsonUTF8Data(ref dynamic data, ref Socket remoteSocket)
        {
            string jsonAnswer = "{\r\n";
            jsonAnswer += "\"success\":true,\r\n";
            jsonAnswer += "\"data\":" + JsonConvert.SerializeObject(data) + "\r\n";
            jsonAnswer += "}";
            SendUTF8JsonRequest(ref jsonAnswer, ref remoteSocket);
        }
        public static void JsonAnswer(bool success, string message, ref Socket remoteSocket)
        {
            string jsonAnswer = "{\r\n";
            jsonAnswer += " \"success\":" + success.ToString().ToLower() + ",\r\n";
            jsonAnswer += "\"message\":\"" + message + "\"\r\n";
            jsonAnswer += "}";
            if (success)
            {
                SendJsonRequest(ref jsonAnswer, ref remoteSocket);
            }
            else
            {
                SendErrorJsonRequest(ref jsonAnswer, ref remoteSocket);
            }
        }
        public static string EncodeNonAsciiCharacters(ref string value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}