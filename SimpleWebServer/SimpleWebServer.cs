using System;
using System.IO;
using System.Net;
using System.Diagnostics;

namespace MB.Web
{
    /// <summary>
    /// Simple web server. Serves the contents of a specified
    /// directory to an address plus port.
    /// </summary>
    public class SimpleWebServer
    {
        private string _rootDir;
        private SimpleHttpListener _listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleWebServer"/> class.
        /// Does not start the server automatically.
        /// If no file name is provided, index.html is returned (if it exists).
        /// </summary>
        /// <param name="prefix">The http address prefix.</param>
        /// <param name="rootDir">The root directory to serve the files from.</param>
        public SimpleWebServer(string prefix, string rootDir)
        {
            _rootDir = rootDir;

            _listener = new SimpleHttpListener(prefix)
            {
                OnReceivedRequest = OnReceivedRequest,
            };  
        }

        /// <summary>
        /// Starts the web server.
        /// </summary>
        public void Start()
        {
            _listener.Start();
        }

        private void OnReceivedRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            var fileName = GetFileNameFromUrlPath(request.Url.AbsolutePath);
            response.ContentType = MimeHelper.GetMimeType(fileName);
            TryServeFile(response, fileName);
        }

        private string GetFileNameFromUrlPath(string fileName)
        {
            Trace.WriteLine("request: " + fileName);
            fileName = StripLeadingSlash(fileName);
            fileName = GetDefaultDocument(fileName);
            fileName = Path.Combine(_rootDir, fileName);
            return fileName;
        }

        private static string StripLeadingSlash(string fileName)
        {
            return fileName.Substring(1);
        }

        private static string GetDefaultDocument(string fileName)
        {
            // if no filename is given, use index.html
            if (string.IsNullOrEmpty(fileName))
                fileName = "index.html";
            else
            {
                var parts = fileName.Split('/');
                if (parts.Length > 0 && string.IsNullOrEmpty(parts[parts.Length - 1]))
                    fileName = Path.Combine(fileName, "index.html");
            }
            return fileName;
        }

        private static void TryServeFile(HttpListenerResponse response, string fileName)
        {
            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(fileName, FileMode.Open);
                ServeFileStream(response, fileStream);
            }
            catch
            {
                Trace.WriteLine("error serving file");
                response.StatusCode = 404;
            }
            finally
            {
                if (fileStream != null)
                    fileStream.Close();
                response.OutputStream.Close();
            }
        }

        private static void ServeFileStream(HttpListenerResponse response, FileStream fileStream)
        {
            var buffer = new byte[1024 * 16];
            int nbytes;

            while ((nbytes = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                response.OutputStream.Write(buffer, 0, nbytes);
        }
    }
}

