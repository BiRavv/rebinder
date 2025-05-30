using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace rebinderBackend.FrontendConnection
{
    public static class Fetch
    {

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
        ///     When the frontend delivers POST "address@type" it performs "action"
        ///     (Return 0 at the end of the function)
        /// </summary>
        /// <param name="address">The input from the frontend is like "address@type"</param>
        /// <param name="type">The input from the frontend is like "address@type"</param>
        /// <param name="action">When it gets the input from the frontend, this method is performed.</param>
        /// <typeparam name="T">Type of the return of "action"</typeparam>
        public static void Listen<T>(string address, string type, Func<T> action)
        {
            Task.Run(() =>
            {
                while (_listenersRunning)
                {
                    var context = LocalServer.getListener().GetContext();
                    string body = new StreamReader(context.Request.InputStream).ReadToEnd().Trim();
                    context.Response.StatusCode = 200;
                    context.Response.Close();

                    if (body == $"{address}@{type}")
                    {
                        Console.WriteLine("Listening...");
                        action();
                    }
                }
            });
        }
    }
}