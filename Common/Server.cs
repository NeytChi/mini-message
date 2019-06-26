using System;
using System.Net;
using System.Text;
using Common.Routing;
using Common.Logging;
using System.Threading;
using Common.NDatabase;
using System.Net.Sockets;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Common
{
    public class Server
    {
        public bool request_view = true;
        public int port = 8020;
        public string ip = "127.0.0.1";
        public string domen = "(none)";
        private Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private Regex contentlength = new Regex("ength: [0-9]*", RegexOptions.Compiled);
        private readonly string OPTIONS = "OPTIONS";

        public void InitListenSocket()
        {
            Router.AddRoute(new Route("GET", "logs", HttpLogs));
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            socket.Bind(iPEndPoint);
            socket.Listen(1000);
            Logger.WriteLog("Server run. Host_Port=" + ip + ":" + port, LogLevel.Usual);
            while (true)
            {
                Socket handleSocket = socket.Accept();
                Thread thread = new Thread(() => ReceivedSocketData(ref handleSocket))
                {
                    IsBackground = true
                };
                thread.Start();
            }
        }
        private void ReceivedSocketData(ref Socket handleSocket)
        {
            byte[] buffer = new byte[1096];
            int bytes = 0;
            string request = "";
            int ContentLength = 0;
            for (; ; )
            {
                if (buffer.Length < bytes + 300)
                {
                    Array.Resize(ref buffer, bytes + 2000);
                }
                else
                {
                    bytes += handleSocket.Receive(buffer, bytes, 60, SocketFlags.None);
                }
                if (bytes > 200 && bytes < 1000 && buffer.Length == 1096)
                {
                    request = Encoding.ASCII.GetString(buffer, 0, bytes);
                    if (request.Contains("content-length:") || request.Contains("Content-Length:"))
                    {
                        ContentLength = GetRequestContentLenght(ref request);
                        if (ContentLength > 0 && ContentLength < 210000000)
                        {
                            Array.Resize(ref buffer, ContentLength + bytes);
                        }
                        else if (ContentLength > 210000000) handleSocket.Close();
                    }
                }
                if (handleSocket.Available == 0 && bytes >= ContentLength) { break; }
                if (handleSocket.Available == 0 && bytes < ContentLength)
                {
                    if ((handleSocket.Poll(10000, SelectMode.SelectRead) && (handleSocket.Available == 0)) || !handleSocket.Connected)
                    {
                        handleSocket.Close();
                        Logger.WriteLog("Remote socket was disconnected.", LogLevel.Usual);
                        break;
                    }
                }
                if (bytes > 210000000)
                {
                    HttpIternalServerError(ref handleSocket);
                    handleSocket.Close();
                    break;
                }
            }
            if (handleSocket.Connected)
            {
                request = Encoding.ASCII.GetString(buffer, 0, bytes);
                HttpRequest objRequest = new HttpRequest(ref handleSocket, ref request, ref bytes, ref buffer);
                RouteRequest(ref objRequest);
            }
            if (handleSocket.Connected) { handleSocket.Close(); }
        }
        private void RouteRequest(ref HttpRequest request)
        {
            if (request_view)
            {
                Debug.WriteLine("Request:");
                Debug.WriteLine(request.data);
            }
            request.method = GetMethodRequest(ref request.data);
            if (request.method != null)
            {
                if (request.method == OPTIONS)
                {
                    HttpOptions(ref request.remoteSocket);
                }
                else
                {
                    request.url = FindURLRequest(ref request.data, ref request.method);
                    if (request.url != null)
                    {
                        Route route = Router.GetRoute(ref request.url);
                        if (route != null)
                        {
                            if (route.route_method == request.method)
                            {
                                route.action(ref request);
                            }
                            else { HttpErrorUrl(ref request.remoteSocket); }
                        }
                        else { HttpErrorUrl(ref request.remoteSocket); }
                    }
                    else { HttpErrorUrl(ref request.remoteSocket); }
                }
            }
            else { HttpErrorUrl(ref request.remoteSocket); }
        }
        /// <summary>
        /// Finds the URL in request.
        /// </summary>
        /// <returns>The URLR equest.</returns>
        /// <param name="request">Request.</param>
        /// <param name="method">Method.</param>
        public string FindURLRequest(ref string request, ref string method)
        {
            string url = GetBetween(ref request, method + " /", " HTTP/1.");
            if (string.IsNullOrEmpty(url)) { return null; }
            int questionUrl = url.IndexOf('?', 1);
            if (questionUrl == -1)
            {
                if (url[url.Length - 1] != '/')
                {
                    return url.ToLower();                                       // handle this pattern url -> /User || /User/Profile
                }
                else
                {
                    return url.Remove(url.Length - 1).ToLower();                // handle this pattern url -> /User/ || /User/Profile/
                }
            }
            else
            {
                if (url[questionUrl - 1] == '/')                                // handle this pattern url -> /User/Profile/?id=1111 -> /User/Profile/
                {
                    return url.Substring(0, questionUrl - 1).ToLower();         // handle this pattern url -> User/Profile - return
                }
                else
                {
                    Logger.WriteLog("Can not define pattern of url, function FindURLRequest()", LogLevel.Error);
                    return null;                                                // Don't handle this pattern url -> /User?id=1111 and /User/Profile?id=9999 
                }
            }
        }
        public string GetMethodRequest(ref string request)
        {
            if (!string.IsNullOrEmpty(request))
            {
                if (request.Length < 10)
                {
                    Logger.WriteLog("Can not define method of request, input request have not enough characters, function GetMethodRequest()", LogLevel.Error);
                    return null;
                }
                for (int i = 0; i < 10; i++)
                {
                    if (request[i] == '\t' || request[i] == ' ')
                    {
                        return request.Substring(0, i);
                    }
                }
                Logger.WriteLog("Can not define method of request, request does not contains tab or space, function GetMethodRequest()", LogLevel.Error);
                return null;
            }
            else
            {
                Logger.WriteLog("Input request is null or empty, function GetMethodRequest", LogLevel.Error);
                return null;
            }
        }
        public string GetBetween(ref string source, string start, string end)
        {
            if (!string.IsNullOrEmpty(source))
            {
                if (source.Contains(start) && source.Contains(end))
                {
                    int Start = source.IndexOf(start, 0, StringComparison.Ordinal) + start.Length;
                    if (Start == -1)
                    {
                        Logger.WriteLog("Can not find start of source, function GetBetween()", LogLevel.Error);
                        return null;
                    }
                    int End = source.IndexOf(end, Start, StringComparison.Ordinal);
                    if (End == -1)
                    {
                        Logger.WriteLog("Can not find end of source, function GetBetween()", LogLevel.Error);
                        return null;
                    }
                    return source.Substring(Start, End - Start);
                }
                else
                {
                    Logger.WriteLog("Source does not contains search values, function GetBetween()", LogLevel.Error);
                    return null;
                }
            }
            else
            {
                Logger.WriteLog("Source is null or empty, function GetBetween()", LogLevel.Error);
                return null;
            }
        }
        private void HttpOptions(ref Socket remoteSocket)
        {
            string response = "HTTP/1.1 200 OK\r\n" +
                              "Access-Control-Allow-Methods: POST, GET, OPTIONS\r\n" +
                              "Access-Control-Allow-Headers: *\r\n" +
                              "Access-Control-Allow-Origin: *\r\n" +
                              "Vary: Accept-Encoding, Origin\r\n" +
                              "Content-Encoding: gzip\r\n" +
                              "Content-Length: 0\r\n" +
                              "Keep-Alive: timeout=300\r\n" +
                              "Connection: Keep-Alive\r\n" +
                              "Content-Type: multipart/form-data\r\n\r\n";
            if (remoteSocket.Connected)
            {
                remoteSocket.Send(Encoding.ASCII.GetBytes(response));
            }
            Logger.WriteLog("HTTP Response " + remoteSocket.AddressFamily + " - OPTIONS", LogLevel.Usual);
        }
        private void HttpLogs(ref HttpRequest request)
        {
            string response = "";
            byte[] logs = Logger.ReadMassiveLogs();
            string start = "<HTML><BODY>Logs massive information:<br><hr>";
            string end = "</BODY></HTML>";
            response = "HTTP/1.1 200 OK\r\n" +
                       "Version: HTTP/1.1\r\n" +
                       "Content-Type: text/html; charset=utf-8\r\n" +
                       "Content-Length: " + (start.Length + end.Length + logs.Length) +
                       "\r\n\r\n" +
                        start;
            byte[] answerstart = Encoding.ASCII.GetBytes(response);
            byte[] answerend = Encoding.ASCII.GetBytes(end);
            byte[] requestbyte = new byte[logs.Length + answerstart.Length + answerend.Length];
            answerstart.CopyTo(requestbyte, 0);
            logs.CopyTo(requestbyte, answerstart.Length);
            answerend.CopyTo(requestbyte, answerstart.Length + logs.Length);
            if (request.remoteSocket.Connected)
            {
                request.remoteSocket.Send(requestbyte);
            }
            Logger.WriteLog("Get http logs data.", LogLevel.Usual);
        }
        private void HttpIternalServerError(ref Socket remoteSocket)
        {
            string response = "";
            string responseBody = "";
            responseBody = string.Format("<HTML>" +
                                         "<BODY>" +
                                         "<h1> 500 Internal Server Error !..</h1>" +
                                         "</BODY></HTML>");
            response = "HTTP/1.1 500 \r\n" +
                       "Version: HTTP/1.1\r\n" +
                       "Content-Type: text/html; charset=utf-8\r\n" +
                       "Content-Length: " + (response.Length + responseBody.Length) +
                       "\r\n\r\n" +
                     responseBody;
            if (remoteSocket.Connected)
            {
                remoteSocket.Send(Encoding.ASCII.GetBytes(response));
            }
            Debug.Write(response);
            Logger.WriteLog("HTTP 400 Error link response", LogLevel.Error);
        }
        private void HttpErrorUrl(ref Socket remoteSocket)
        {
            string response = "";
            string responseBody = "";
            responseBody = string.Format("<HTML>" + "<BODY>" +
                                         "<h1>error url...</h1>" +
                                         "</BODY>" + "</HTML>");
            response = "HTTP/1.1 400 \r\n" +
                       "Version: HTTP/1.1\r\n" +
                       "Content-Type: text/html; charset=utf-8\r\n" +
                       "Content-Length: " + (responseBody.Length) +
                       "\r\n\r\n" +
                       responseBody;
            if (remoteSocket.Connected)
            {
                remoteSocket.Send(Encoding.ASCII.GetBytes(response));
            }
            Logger.WriteLog("HTTP 400 Error link response", LogLevel.Warning);
        }
        /// <summary>
        /// Gets value "content lenght" from request.
        /// </summary>
        /// <returns>The request content lenght.</returns>
        /// <param name="request">Picie of request.</param>
        public int GetRequestContentLenght(ref string request)
        {
            try
            {
                Match resultContentLength = contentlength.Match(request);
                if (resultContentLength.Success)
                {
                    return Convert.ToInt32(resultContentLength.Value.Substring("ength: ".Length)) + resultContentLength.Index + resultContentLength.Length;
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                Logger.WriteLog("Error function GetRequestContentLenght(), exception with converting to int value", LogLevel.Error);
                return 0;
            }
        }
        public void Dispose()
        {
            Logger.Dispose();
            socket.Close();
            Database.connection.Close();
        }
    }
}
