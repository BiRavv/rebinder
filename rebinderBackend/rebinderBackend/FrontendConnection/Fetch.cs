using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace rebinderBackend.FrontendConnection
{
    public static class Fetch
    {
        public static List<Func<string, string>> Listeners = new();


        // Here you can stop and start all listeners if needed (probably not needed)
        private static Boolean _listenersRunning = true;
        public static void StopAll()
        {
            _listenersRunning = false;
        }
        public static void StartAll()
        {
            _listenersRunning = true;
        }

        /// <summary>
        /// A Listener added here will get all the POST in from the frontend.
        /// </summary>
        /// <param name="listener">The function, that runs after anything comes from the frontend. It returns the response. If the response is null, it won't respond.</param>
        public static void AddListener(Func<string, string> listener)
        {
            Listeners.Add(listener);
        }
        
        public static void Listen()
        {
            Task.Run(() =>
            {
                while (_listenersRunning)
                {
                    var context = LocalServer.getListener().GetContext();
                    string body = new StreamReader(context.Request.InputStream).ReadToEnd().Trim();

                    if (_mainContext == null)
                    {
                        throw new Exception("No thread context available");
                    }

                    string responseText = null;

                    // Use a ManualResetEventSlim to wait for the main context thread to run the listener and get the result
                    using (var waitHandle = new ManualResetEventSlim(false))
                    {
                        _mainContext.Post(_ =>
                        {
                            foreach (var listener in Listeners)
                            {
                                string result = listener(body);
                                if (result != null)
                                {
                                    responseText = result;
                                    break;
                                }
                            }

                            waitHandle.Set();
                        }, null);

                        // Wait for the main thread to finish processing
                        waitHandle.Wait();
                    }

                    if (responseText == null)
                    {
                        // No response
                        context.Response.StatusCode = 200;
                        context.Response.Close();
                        continue;
                    }

                    context.Response.StatusCode = 200;
                    byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                    context.Response.ContentType = "text/plain";
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.Close();
                }
            });
        }
       
        // This if for the action() to run on main thread
        private static SynchronizationContext? _mainContext;
        public static void Init(SynchronizationContext context)
        {
            _mainContext = context;
        }

    }
}