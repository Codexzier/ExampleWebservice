using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleWebservice
{
    public class WebServer
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly Func<HttpListenerRequest, string> _responderMethod;

        public WebServer(string urlWithPort, Func<HttpListenerRequest, string> method)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Need Windows 7 or higher");
            }

            if (string.IsNullOrEmpty(urlWithPort))
            {
                throw new ArgumentException("Check URI prefixes are required");
            }

            if (method == null)
            {
                throw new ArgumentException("responder method required");
            }

            this._listener.Prefixes.Add(urlWithPort);
            this._responderMethod = method;
            this._listener.Start();
        }

        public void Run()
        {
            Console.WriteLine("Webserver running...");

            Task.Run(() =>
            {
                while (this._listener.IsListening)
                {
                    _ = ThreadPool.QueueUserWorkItem(this.WriteResponse,
                        this.FilterRequests());
                }
            });
        }

        private void WriteResponse(object? c)
        {
            if (!(c is HttpListenerContext ctx))
            {
                return;
            }

            var rstr = this._responderMethod(ctx.Request);
            var buf = Encoding.UTF8.GetBytes(rstr);
            ctx.Response.ContentLength64 = buf.Length;
            ctx.Response.OutputStream.Write(buf, 0, buf.Length);

            ctx.Response.OutputStream.Close();
        }

        private HttpListenerContext FilterRequests()
        {
            var context = this._listener.GetContext();

            if (context.Request.RawUrl.Equals("/favicon.ico"))
            {
                return null;
            }

            return context;
        }

        public void Stop()
        {
            this._listener.Stop();
            this._listener.Close();
        }
    }
}
