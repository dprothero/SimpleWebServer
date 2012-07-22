using System;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MB.Web
{
    /// <summary>
    /// Simple http server which listens at a provided address and port.
    /// </summary>
    public class SimpleHttpListener
    {
        private HttpListener _listener;
        private bool _isRunning;
        private bool _isStopping;
        private string _prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleHttpListener"/> class.
        /// </summary>
        /// <param name="prefix">The htpp address prefix.</param>
        public SimpleHttpListener(string prefix)
        {
            _prefix = prefix;
        }

        /// <summary>
        /// Starts the http server. Internally, one async task is used for all requests.
        /// </summary>
        public void Start()
        {
            if (_isRunning) return;
            CheckIfHttpListenerIsSupported();
            CheckForMissingPrefix();
            TryStartListener();
            Listen();
        }

        private static void CheckIfHttpListenerIsSupported()
        {
            if (!HttpListener.IsSupported)
                throw new Exception("ERROR! Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
        }

        private void CheckForMissingPrefix()
        {
            if (_prefix == null)
                throw new Exception("ERROR! prefix missing");
        }

        private void TryStartListener()
        {
            try
            {
                StartListener();
            }
            catch (HttpListenerException ex)
            {
                _isRunning = false;
                throw new Exception(String.Format("ERROR! http listener at {0} could not be started\nmake sure the user has the rights for this port or run as administrator", _prefix), ex);
            }
        }

        private void StartListener()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(_prefix);
            _listener.Start();

            _isRunning = true;
        }

        private void Listen()
        {
            if (_isRunning)
                CreateListenTask();
        }

        private void CreateListenTask()
        {
            Trace.WriteLine("started at " + _prefix);
            Task.Factory.StartNew(() => { DoListenTask(); } );
        }

        private void DoListenTask()
        {
            while (!_isStopping)
                WaitForAndHandleRequest();

            StopListener();
        }

        private void WaitForAndHandleRequest()
        {
            var context = _listener.GetContext();
            var request = context.Request;
            var response = context.Response;

            if (ShowDebugOutput)
                WriteDebugInfo(request);
            FireOnReceivedRequest(request, response);
        }

        private static void WriteDebugInfo(HttpListenerRequest request)
        {
            Debug.WriteLine(String.Format("KeepAlive: {0}", request.KeepAlive));
            Debug.WriteLine(String.Format("Local end point: {0}", request.LocalEndPoint.ToString()));
            Debug.WriteLine(String.Format("Remote end point: {0}", request.RemoteEndPoint.ToString()));
            Debug.WriteLine(String.Format("Is local? {0}", request.IsLocal));
            Debug.WriteLine(String.Format("HTTP method: {0}", request.HttpMethod));
            Debug.WriteLine(String.Format("Protocol version: {0}", request.ProtocolVersion));
            Debug.WriteLine(String.Format("Is authenticated: {0}", request.IsAuthenticated));
            Debug.WriteLine(String.Format("Is secure: {0}", request.IsSecureConnection));
        }

        private void FireOnReceivedRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if (OnReceivedRequest != null)
                OnReceivedRequest(request, response);
        }

        private void StopListener()
        {
            _isRunning = false;
            _isStopping = false;
            _listener.Close();

            Trace.WriteLine("stopped");

            FireOnStopped();
        }

        private void FireOnStopped()
        {
            if (OnStopped != null)
                OnStopped();
        }

        /// <summary>
        /// Stops the http server.
        /// </summary>
        public void Stop()
        {
            if (!_isRunning || _isStopping) return;
            _isStopping = true;
        }

        /// <summary>
        /// Gets or sets the on OnReceivedRequest callback action.
        /// </summary>
        public Action<HttpListenerRequest, HttpListenerResponse> OnReceivedRequest 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the OnStopped callback action.
        /// </summary>
        public Action OnStopped 
        { 
            get; 
            set;
        }

        /// <summary>
        /// If True, SimpleHttpListener outputs additional debugging info for each request.
        /// </summary>
        public Boolean ShowDebugOutput
        {
            get;
            set;
        }
    }
}

