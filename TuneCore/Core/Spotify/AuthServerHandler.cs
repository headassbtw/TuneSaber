using System.Reflection;
using System.Threading;
using System.Web;
using System.Globalization;
using System.Text;
using System;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.WebSockets;
using EmbedIO.WebApi;
using EmbedIO.Actions;
using EmbedIO.Routing;
using EmbedIO.Net;
//using SpotifyAPI.Web.Auth;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;

namespace TuneSaber
{
    public class AuthObjects
    {
        public AuthObjects(string accessCode, string refreshCode, int expirationSeconds)
        {
            this.AccessCode = accessCode;
            this.RefreshCode = refreshCode;
            this.ExpirationSeconds = expirationSeconds;
        }

        public string AccessCode { get; set; }
        public int ExpirationSeconds { get; set; }
        public string RefreshCode { get; set; }
    }
}

namespace SpotifyAPI.Web
{
    /// <summary>
    ///   Ensure input parameters
    /// </summary>
    internal static class Ensure
    {
        /// <summary>
        ///   Checks an argument to ensure it isn't null.
        /// </summary>
        /// <param name = "value">The argument value to check</param>
        /// <param name = "name">The name of the argument</param>
        public static void ArgumentNotNull(object value, string name)
        {
            if (value != null)
            {
                return;
            }

            throw new ArgumentNullException(name);
        }

        /// <summary>
        ///   Checks an argument to ensure it isn't null or an empty string
        /// </summary>
        /// <param name = "value">The argument value to check</param>
        /// <param name = "name">The name of the argument</param>
        public static void ArgumentNotNullOrEmptyString(string value, string name)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return;
            }

            throw new ArgumentException("String is empty or null", name);
        }

        public static void ArgumentNotNullOrEmptyList<T>(IEnumerable<T> value, string name)
        {
            if (value != null && value.Any())
            {
                return;
            }

            throw new ArgumentException("List is empty or null", name);
        }
    }
}
namespace TuneSaber
{
    public interface IAuthServer : IDisposable
    {
        event Func<object, TuneSaber.AuthObjects, Task> AuthorizationCodeReceived;

        //event Func<object, ImplictGrantResponse, Task> ImplictGrantReceived;
        event Func<object, string, Task> Poger;
        Task Start();
        Task Stop();

        Uri BaseUri { get; }
    }
}
namespace TuneSaber.Core.Spotify.AuthServerHandler
{
    public class EmbedIOAuthServer : IAuthServer
    {
        public event Func<object, AuthObjects, Task>? AuthorizationCodeReceived;
        //public event Func<object, ImplictGrantResponse, Task>? ImplictGrantReceived;
        public event Func<object, string, Task> Poger;

        private const string AssetsResourcePath = "TuneSaber.Core.Spotify.auth_assets.main.js";
        private const string DefaultResourcePath = "TuneSaber.Core.Spotify.default_site";

        private CancellationTokenSource? _cancelTokenSource;
        private readonly WebServer _webServer;


        public EmbedIOAuthServer(Uri baseUri, int port)
          : this(baseUri, port, Assembly.GetExecutingAssembly(), DefaultResourcePath) { }



        public EmbedIOAuthServer(Uri baseUri, int port, Assembly resourceAssembly, string resourcePath)
        {
            Plugin.Log.Notice("web module starting");
            SpotifyAPI.Web.Ensure.ArgumentNotNull(baseUri, nameof(baseUri));

            BaseUri = baseUri;
            Port = port;
            Task.Delay(100);
            try
            {

                _webServer = new WebServer(port)
              .WithModule(new ActionModule("/", HttpVerbs.Post, (ctx) =>
              {
                  var query = ctx.Request.QueryString;
                  var error = query["error"];
                  if (error != null)
                  {
                      Plugin.Log.Critical(("AuthException: " + (error, query["state"]).ToString()));
                      
                  }

                  //var requestType = query.Get("request_type");
                  //Plugin.Log.Notice("type is " + requestType);
                  
                  /*if (requestType == "custom")
                  {
                      Poger?.Invoke(this, "progress?");
                      
                      /*AuthorizationCodeReceived?.Invoke(this, new AuthObjects(query["access_token"]!, query["refresh_token"]!, int.Parse(query["expires_in"]!))
                      {
                          AccessCode = query["access_token"],
                          RefreshCode = query["refresh_token"],
                          ExpirationSeconds = int.Parse(query["expires_in"])
                      }); ;
                  }*/
                   
                  return ctx.SendStringAsync("OK", "text/plain", Encoding.UTF8);
              }))
              .WithEmbeddedResources("/auth_assets", Assembly.GetExecutingAssembly(), AssetsResourcePath)
              .WithEmbeddedResources(baseUri.AbsolutePath, resourceAssembly, resourcePath);
                
            }
            catch(Exception e) { Plugin.Log.Critical(e.ToString()); }

            
        }


        

        public Uri BaseUri { get; }
        public int Port { get; }

        public Task Start()
        {
            _cancelTokenSource = new CancellationTokenSource();
            _webServer.Start(_cancelTokenSource.Token);

            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _cancelTokenSource?.Cancel();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _webServer?.Dispose();
            }
        }
    }
}