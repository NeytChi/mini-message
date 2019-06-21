using System;
using System.IO;
using System.Text;
using Common.Logging;
using Common.NDatabase;
using System.Diagnostics;
using Common.NDatabase.FileData;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Common.FileSystem
{
    public static class LoaderFile
    {
        private static Random random = new Random();
        public static string CurrentDirectory = Directory.GetCurrentDirectory();
        public static string PathToFiles = "/files/";
        private static Regex ContentDispositionPattern = new Regex("Content-Disposition: form-data;" +
                                                            " name=\"(.*)\"; filename=\"(.*)\"\r\n" +
                                                            "Content-Type: (.*)\r\n\r\n", RegexOptions.Compiled);
        private static string[] availableExtentions = { "image", "video", "audio", "application" };
        public static string DailyDirectory;

        public static string DomenName = Config.Domen;
        public static string DailyPath;
        public static DateTime CurrentTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1);

        /// <summary>
        /// Save file by specific path and get filled FileD structure data by that.
        /// <summary>
        public static bool SaveFile(ref FileD fileD, string RelativePath)
        {
            ChangeDailyPath();
            if (!Directory.Exists(CurrentDirectory + RelativePath + DailyPath))
            {
                Directory.CreateDirectory(CurrentDirectory + RelativePath + DailyPath);
            }
            if (!File.Exists(CurrentDirectory + RelativePath + DailyPath + fileD.file_name))
            {
                fileD.file_path = RelativePath + DailyPath;
                fileD.file_fullpath = CurrentDirectory + RelativePath + DailyPath;
                CreateFileBinary(ref fileD.file_name, ref fileD.file_fullpath, ref fileD.data);
                Database.file.AddFile(fileD);
                Logger.WriteLog("Create file. file_name->" + fileD.file_name + " Relative path->" + RelativePath, LogLevel.Usual);
                return true;
            }
            else
            {
                Logger.WriteLog("Can't create file. file_name->" + fileD.file_name + " Relative path->" + RelativePath, LogLevel.Warning);
                return false;
            }
        }
        /// <summary>
        /// Change daily path to save files in daily new folder. That need to save file without override another file.
        /// <summary>
        private static void ChangeDailyPath()
        {
            if (CurrentTime.Day != DateTime.Now.Day)
            {
                CurrentTime = DateTime.Now;
                DailyPath = CurrentTime.Day + "-" + CurrentTime.Month + "-" + CurrentTime.Year + "/";
            }
        }
        /// <summary>
        /// Multis the loading files. Get files from ascii request, create files in common folder and get all information about this files.
        /// </summary>
        /// <returns>The loading.</returns>
        /// <param name="AsciiRequest">ASCII request.</param>
        /// <param name="buffer">Buffer.</param>
        /// <param name="bytes">Bytes.</param>
        /// <param name="count_files">Count files.</param>
        public static FileD[] LoadingFiles(ref string AsciiRequest, ref byte[] buffer, ref int bytes, ref int count_files)
        {
            bool endRequest = false;
            int last_position = 0;
            List<Match> dispositionsAscii = new List<Match>();
            List<Match> boundariesAscii = new List<Match>();
            FileD[] files = new FileD[count_files];
            if (CheckHeadersFileRequest(ref AsciiRequest))
            {
                string EndBoundaryAscii = GetBoundary(ref AsciiRequest);
                Regex endBoundaryPattern = new Regex(EndBoundaryAscii);
                while (!endRequest)
                {
                    Match contentFile = ContentDispositionPattern.Match(AsciiRequest, last_position);
                    if (contentFile.Success && boundariesAscii.Count < count_files)
                    {
                        last_position = contentFile.Index + contentFile.Length;
                        Match boundary = endBoundaryPattern.Match(AsciiRequest, last_position);
                        if (boundary.Success)
                        {
                            dispositionsAscii.Add(contentFile);
                            boundariesAscii.Add(boundary);
                        }
                    }
                    else
                    {
                        endRequest = true;
                    }
                }
                for (int i = 0; i < dispositionsAscii.Count; i++)
                {
                    Match disposition = dispositionsAscii[i];
                    Match boundaries = boundariesAscii[i];
                    byte[] fileBuffer = GetFileBufferByPositions(ref buffer, ref AsciiRequest, ref disposition, ref boundaries);
                    if (fileBuffer != null)
                    {
                        FileD file = new FileD();
                        if (CreateFileByInfo(ref disposition, ref file))
                        {
                            file.data = fileBuffer;
                            files[i] = file;
                        }
                    }
                    else { Logger.WriteLog("Can not create file from request, file_count=" + i, LogLevel.Error); }
                }
            }
            else 
            {
                Logger.WriteLog("Request doesnot has required request fields.", LogLevel.Error);
                return null;
            }
            Logger.WriteLog("Get files from request. From request loaded " + files.Length + " file(s).", LogLevel.Usual);
            return files;
        }
        public static bool LoadingFile(ref string AsciiRequest, ref byte[] buffer, ref int bytes, ref FileD file)
        {
            string log_text = null;
            int last_position = 0;
            if (CheckHeadersFileRequest(ref AsciiRequest))
            {
                string EndBoundaryAscii = GetBoundary(ref AsciiRequest);
                Regex endBoundaryPattern = new Regex(EndBoundaryAscii);
                Match disposition = ContentDispositionPattern.Match(AsciiRequest, last_position);
                if (disposition.Success)
                {
                    last_position = disposition.Index + disposition.Length;
                    Match boundary = endBoundaryPattern.Match(AsciiRequest, last_position);
                    if (boundary.Success)
                    {
                        byte[] fileBuffer = GetFileBufferByPositions(ref buffer, ref AsciiRequest, ref disposition, ref boundary);
                        if (fileBuffer != null)
                        {
                            if (CreateFileByInfo(ref disposition, ref file))
                            {
                                file.data = fileBuffer;
                                Logger.WriteLog("Get file from request. file.file_id->" + file.file_id, LogLevel.Usual);
                                return true;
                            }
                            else { log_text = "Can not create file with buffer by positions."; }
                        }
                        else { log_text = "Can not get file's buffer by positions."; }
                    }
                    else { log_text = "Can not define boundary of request."; }
                } 
                else { log_text = "Can not define content-disposition of request."; }
            }
            else { log_text = "Request doesn't has required request's fields."; }
            Logger.WriteLog(log_text, LogLevel.Warning);
            return false;
        }
        private static byte[] GetFileBufferByPositions(ref byte[] buffer,ref string AsciiRequest,ref Match start,ref Match end)
        {
            try
            {
                byte[] binRequestPart = Encoding.ASCII.GetBytes(AsciiRequest.Substring(0, start.Index + start.Length));
                byte[] binBoundary = Encoding.ASCII.GetBytes(AsciiRequest.Substring(start.Index + start.Length, end.Index - start.Index - start.Length));
                int fileLength = end.Index - start.Index - start.Length;
                byte[] binFile = new byte[fileLength];
                Array.Copy(buffer, binRequestPart.Length, binFile, 0, fileLength);
                return binFile;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                Logger.WriteLog(e.Message, LogLevel.Error);
                return null;
            }
        }
        public static bool CheckHeadersFileRequest(ref string request)
        {
            if (string.IsNullOrEmpty(request))
            {
                Logger.WriteLog("Input value is null or empty, function CheckHeadersFileRequest()", LogLevel.Error);
                return false;
            }
            if (request.Contains("Content-Type: multipart/form-data") || request.Contains("content-type: multipart/form-data"))
            {
                if (request.Contains("boundary="))
                {
                    if (request.Contains("Connection: keep-alive") || request.Contains("connection: keep-alive"))
                    {
                        return true;
                    }
                    else 
                    {
                        Logger.WriteLog("Can not find (connection: keep-alive) in request, function CheckHeadersFileRequest", LogLevel.Error);
                        return false; 
                    }
                }
                else 
                {
                    Logger.WriteLog("Can not find (boundary=) in request, function CheckHeadersFileRequest", LogLevel.Error);
                    return false; 
                }
            }
            else
            {
                Logger.WriteLog("Can not find (Content-Type: multipart/form-data) in request, function CheckHeadersFileRequest", LogLevel.Error);
                return false;
            }
        }
        public static bool CreateFileByInfo(ref Match disposition, ref FileD file)
        {
            if (disposition == null)
            {
                Logger.WriteLog("Input value is null, function CreateFileByInfo()", LogLevel.Error);
                return false;
            }
            string ContentType = GetContentType(disposition.Value);
            file.file_extension = GetFileExtention(ContentType);
            file.file_type = GetFileType(disposition.Value);
            file.file_last_name = GetFileName(disposition.Value);
            file.file_name = random.Next(0, 2146567890).ToString();
            file.file_path = GetPathFromExtention(file.file_extension);
            file.file_fullpath = file.file_path + file.file_name;
            Logger.WriteLog("Create file info by disposition.", LogLevel.Usual);
            return true;
        }
        public static string GetPathFromExtention(string extention)
        {
            switch (extention)
            {
                case "image":
                    Directory.CreateDirectory(CurrentDirectory + PathToFiles + DailyDirectory + "Images/");
                    return PathToFiles + DailyDirectory + "Images/";
                case "video":
                    Directory.CreateDirectory(CurrentDirectory + PathToFiles + DailyDirectory + "Videos/");
                    return PathToFiles + DailyDirectory + "Videos/";
                case "audio":
                    Directory.CreateDirectory(CurrentDirectory + PathToFiles + DailyDirectory + "Audios/");
                    return PathToFiles + DailyDirectory + "Audios/";
                default: 
                    return PathToFiles;
            }
        }
        private static string GetFileExtention(string disposition)
        {
            int position;
            for (int i = 0; i < availableExtentions.Length; i++)
            {
                if (disposition.Contains(availableExtentions[i]))
                {
                    position = disposition.IndexOf(availableExtentions[i] + "/", StringComparison.Ordinal);
                    if (position != -1)
                    {
                        position = (availableExtentions[i] + "/").Length;
                        Logger.WriteLog("Get file extention.", LogLevel.Usual);
                        return disposition.Substring(position);
                    }
                }
            }
            Logger.WriteLog("Can not get type of file disposition, function GetFileExtention()", LogLevel.Error);
            return null;
        }
        private static string GetFileType(string disposition)
        {
            int position;
            for (int i = 0; i < availableExtentions.Length; i++)
            {
                if (disposition.Contains(availableExtentions[i]))
                {
                    position = disposition.IndexOf(availableExtentions[i] + "/", StringComparison.Ordinal);
                    if (position != -1)
                    {
                        Logger.WriteLog("Get file type. file.file_type->" + availableExtentions[i], LogLevel.Usual);
                        return availableExtentions[i];
                    }
                }
            }
            Logger.WriteLog("Can not get type of file disposition, function GetFileType()", LogLevel.Error);
            return null;
        }
        private static string GetFileName(string disposition)
        {
            int first, end;
            first = disposition.IndexOf("filename=\"", StringComparison.Ordinal);
            if (first == -1)
            {
                Logger.WriteLog("Can not get start of name file, function GetFileName()", LogLevel.Error);
                return null;
            }
            first += 11;                                                        //"filename=\""
            end = disposition.IndexOf("\"", first, StringComparison.Ordinal);
            if (end == -1)
            {
                Logger.WriteLog("Can not get end of name file, function GetFileName()", LogLevel.Error);
                return null;
            }
            string filename = disposition.Substring(first, (end - first));
            Logger.WriteLog("Get file name from disposition request", LogLevel.Error);
            return filename;
        }
        private static string GetContentType(string disposition)
        {
            int length = disposition.Length;
            bool exist = false;
            string contentType = "";
            int first = (disposition.IndexOf("Content-Type: ", StringComparison.Ordinal));
            if (first == -1)
            {
                Logger.WriteLog("Can not find (Content-Type) start in disposition request, function GetContentType()", LogLevel.Error);
                return null;
            }
            first += 14;    //"Content-Type: "
            while (!exist)
            {
                if (first < length)
                {
                    if (disposition[first] == '\r')
                    {
                        exist = true;
                    }
                    else
                    {
                        contentType += disposition[first];
                        ++first;
                    }
                }
                else
                {
                    exist = true;
                }
            }
            return contentType;
        }
        /// <summary>
        /// Creates the file binary.
        /// </summary>
        /// <returns><c>true</c>, if file binary was created, <c>false</c> otherwise.</returns>
        /// <param name="fileName">File name.</param>
        /// <param name="pathToSave">Path to save.</param>
        /// <param name="byteArray">Byte array.</param>
        public static bool CreateFileBinary(ref string fileName,ref string pathToSave, ref byte[] byteArray)
        {
            try
            {
                using (Stream fileStream = new FileStream(pathToSave + fileName, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(byteArray, 0, byteArray.Length);
                    fileStream.Close();
                }
                Logger.WriteLog("Get file from request. File name " + fileName, LogLevel.Usual);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error with CreateFileBinary. Message: " + e.Message);
                Logger.WriteLog("Error with CreateFileBinary. Message: " + e.Message, LogLevel.Error);
                return false;
            }
        }
        private static string GetBoundary(ref string request)
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
            first += 9;                                     // boundary=.Length
            while (!exist)
            {
                if (first <= length)
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
                else
                {
                    exist = true;
                }
            }
            boundary = "--" + boundary;
            return boundary;
        }
        public static string SearchPathToFile(string nameFile, string startSearchFolder)
        {
            string findPathFile = "";
            string pathCurrent = startSearchFolder;
            string[] files = Directory.GetFiles(pathCurrent);
            foreach (string file in files)
            {
                if (file == pathCurrent + "/" + nameFile) 
                { 
                    return file; 
                }
            }
            string[] folders = Directory.GetDirectories(pathCurrent);
            foreach (string folder in folders)
            {
                FileAttributes attr = File.GetAttributes(folder);
                if (attr.HasFlag(FileAttributes.Directory))
                {
                    findPathFile = SearchPathToFile(nameFile, folder);
                }
            }
            return findPathFile;
        }
        public static bool DeleteFile(FileD file)
        {
            bool deleted = false;
            if (File.Exists(CurrentDirectory + file.file_path + file.file_name))
            {
                File.Delete(CurrentDirectory + file.file_path + file.file_name);
                deleted = Database.file.DeleteById(file.file_id);
                if (deleted == false)
                {
                    Logger.WriteLog("Database does not contain file with id=" + file.file_id, LogLevel.Usual);
                }
                Logger.WriteLog("Delete file id=" + file.file_id, LogLevel.Usual);
                return true;
            }
            else
            {
                Logger.WriteLog("Input file->"  + file.file_path + file.file_name + " not exists, function DeleteFile", LogLevel.Error);
                return false;
            }
        }
        //public static void ChangeDailyPath()
        //{
        //    DailyDirectory = Convert.ToString((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds) + "/";
        //    Logger.WriteLog("Change daily path to->" + DailyDirectory, LogLevel.Usual);
        //}
    }
}