using System;
using Common.Logging;
using Newtonsoft.Json;
using Common.NDatabase;
using Common.FileSystem;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using Common.NDatabase.FileData;
using System.Collections.Generic;

namespace Common.Routing
{
    public class HttpRequest
    {
        public Socket remoteSocket;
        public string data;
        public byte[] buffer;
        public int bytes;

        public string method;
        public string url;
        public string BearerToken;

        public JObject json;
        public List<FileD> files;

        public HttpRequest(ref Socket remoteSocket, ref string data, ref int bytes, ref byte[] buffer)
        {
            this.remoteSocket = remoteSocket;
            this.data = data;
            this.bytes = bytes;
            this.buffer = buffer;
        }
        /// <summary>
        /// Saves the file by ref file_name, file_path and data. Set this paramaters if you want to change full_path, file_name and data of file.
        /// </summary>
        /// <param name="file">File.</param>
        public void SaveFile(ref FileD file)
        {
            LoaderFile.CreateFileBinary(ref file.file_name, ref file.file_path, ref file.data);
            Database.file.AddFile(file); 
        }
        public bool GetFileRequest(ref FileD file)
        {
            if (LoaderFile.LoadingFile(ref data, ref buffer, ref bytes, ref file))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool GetFilesRequest(ref FileD[] files, int count)
        {
            files = LoaderFile.LoadingFiles(ref data, ref buffer, ref bytes, ref count);
            if (files != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void JsonInContentDisposition(string field_name)
        {
            if (json == null)
            {
                string str_json = HTTPHandler.GetContentDispositionValue(ref data, field_name);
                try
                {
                    json = JsonConvert.DeserializeObject<dynamic>(str_json);
                }
                catch (Exception e)
                {
                    Logger.WriteLog(e.Message, LogLevel.Error);
                }
            }
        }
        public string FormField(string field_name)
        {
            string value = HTTPHandler.GetContentDispositionValue(ref data, field_name);
            if (value == null)
            {
                value = "";
            }
            return value;
        }
        public dynamic RequiredJsonField(string field_name, JTokenType type)
        {
            dynamic value = null;
            if (json == null)
            {
                json = HTTPHandler.JsonRequest(ref data, ref remoteSocket);
            }
            if (json != null)
            {
                if (HTTPHandler.RequiredJsonField(ref json, ref field_name, ref type, ref remoteSocket, ref value))
                {
                    return value;
                }
                else
                {
                    ReturnedValue(ref value, type);
                    return value;
                }
            }
            else
            {
                ReturnedValue(ref value, type);
                return value;
            }
        }
        public dynamic OptionalJsonField(string field_name, JTokenType type)
        {
            dynamic value = null;
            if (json == null)
            {
                json = HTTPHandler.JsonRequest(ref data, ref remoteSocket);
            }
            if (json != null)
            {
                if (HTTPHandler.OptionalJsonField(ref json, ref field_name, ref type, ref value))
                {
                    return value;
                }
                else
                {
                    ReturnedValue(ref value, type);
                    return value;
                }
            }
            else
            {
                ReturnedValue(ref value, type);
                return value;
            }
        }
        private void ReturnedValue(ref dynamic value, JTokenType type)
        {
            switch (type)
            {
                case JTokenType.Integer:
                case JTokenType.Float:
                    value = -1;
                    break;
                case JTokenType.Boolean: value = false;
                    break;
                default: return;
            }
        }
        public string HeadParameter(string field_name)
        {
            string value = HTTPHandler.GetHeadParamRequest(ref data, field_name);
            if (value == null)
            {
                value = "";
            }
            return value;
        }
        public string GetBearerToken()
        {
            BearerToken = HTTPHandler.GetBearerToken(ref data);
            return BearerToken;
        }
        public void ResponseJsonData(dynamic data)
        {
            HTTPHandler.JsonData(ref data, ref remoteSocket);
        }
        public void ResponseJsonUTF8Data(dynamic data)
        {
            HTTPHandler.JsonUTF8Data(ref data, ref remoteSocket);
        }
        public void ResponseJsonAnswer(bool success, string message)
        {
            HTTPHandler.JsonAnswer(success, message, ref remoteSocket);
        }
    }
}