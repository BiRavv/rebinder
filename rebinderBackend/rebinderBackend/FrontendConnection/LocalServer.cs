using System;
using System.Net;

namespace rebinderBackend.FrontendConnection
{
    public static class LocalServer
    {

        private static HttpListener LISTENER = new();

        public static HttpListener getListener()
        {
            if (LISTENER != null) return LISTENER;
            throw new NullReferenceException();
        }

    static LocalServer()
        {
            getListener().Prefixes.Add("http://localhost:3102/");
            getListener().Start();

            Console.WriteLine("C# Server Running on http://localhost:3102/");
        }
    }
}