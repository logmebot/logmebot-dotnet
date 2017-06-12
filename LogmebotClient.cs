using Logmebot.Net.Models;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Script.Serialization;

namespace Logmebot.Net
{
    public class LogmebotClient
    {
        private const string SessionStateKey = "Logmebot.Net.Client.State";
        


        /// <summary>
        /// Logmebot base uri
        /// </summary>
        public string BasePath { get; set; } = "http://www.logmebot.com/";


        /// <summary>
        /// Logmebot logon page
        /// </summary>
        public string LogonEndpoint
        {
            get { return BasePath + "account/Logon"; }
        }

        /// <summary>
        /// Logmebot endpoint that return the accesstoken
        /// </summary>
        public string TokenEndpoint
        {
            get { return BasePath + "oauth/AccessToken"; }
        }

        /// <summary>
        /// Logmebot endpoint that return the info about the logged user
        /// according to logon request scope
        /// </summary>
        public string MeEndpoint
        {
            get { return BasePath + "oauth/Me"; }
        }

        private string clientId = "";
        /// <summary>
        /// registered app unique identifier on logmebot.com
        /// </summary>
        public string ClientId
        {
            get { return clientId; }
        }

        private string clientSecret = "";
        /// <summary>
        /// oauth secret key used to obtain AccessToken. 
        /// Always keep safe this code
        /// </summary>
        public string ClientSecret
        {
            get { return clientSecret; }
        }

        private string callbackUri = "";
        /// <summary>
        /// your application callback url. 
        /// The same you set during app creation
        /// </summary>
        public string CallbackUri
        {
            get { return callbackUri; }
        }

        private string scope = "email";
        /// <summary>
        /// Include one or more scope values (space-separated) to request additional levels of access.
        /// </summary>
        public string Scope
        {
            set { scope = value; }
            get { return scope; }
        }

        /// <summary>
        /// (recommended)
        /// The state parameter serves two functions. When the user is redirected back to your app, whatever value 
        /// you include as the state will also be included in the redirect. This gives your app a chance to persist 
        /// data between the user being directed to the authorization server and back again, such as using the state 
        /// parameter as a session key. This may be used to indicate what action in the app to perform after authorization 
        /// is complete, for example, indicating which of your app’s pages to redirect to after authorization. 
        /// This also serves as a CSRF protection mechanism. 
        /// When the user is redirected back to your app, double check that the state value matches what you 
        /// set it to originally. This will ensure an attacker can’t intercept the authorization flow.
        /// </summary>
        public string State
        {
            get
            {
                string res = "";
                
                if (HttpContext.Current.Session[SessionStateKey] != null)
                {
                    res = HttpContext.Current.Session[SessionStateKey].ToString();
                }
                return res;
            }
        }

        private string accessToken = "";
        public string AccessToken
        {
            get { return accessToken; }
        }

        /// <summary>
        /// init app settings using web.config
        /// </summary>
        public LogmebotClient()
        {
            string clientId = getConfigValue("LogMeBotClientId");
            string clientSecret = getConfigValue("LogMeBotClientSecret");
            string callbackUri = getConfigValue("LogMeBotCallbackuri");

            init(clientId, clientSecret, callbackUri);
        }

        /// <summary>
        /// init app using  parameters values
        /// </summary>
        public LogmebotClient(string clientId, string clientSecret, string callbackUri)
        {
            init(clientId, clientSecret, callbackUri);
        }


        /// <summary>
        /// redir to logmebot auth page
        /// </summary>
        public void LogOn(string state = "")
        {
            string url = GetLogOnUri(state);
            HttpContext.Current.Response.Redirect(url);
        }

        /// <summary>
        /// retrieve the access token
        /// </summary>
        /// <returns></returns>
        public string GetAccessToken(string code, string state)
        {
            int statusCode = 0;

            try
            {
                if (state != this.State)
                    throw new LogMeBotException(401, "Invalid state");

                var accessTokenResult = new AccessTokenResult();
                var serializer = new JavaScriptSerializer();

                string reqUri = (TokenEndpoint
                    + "?client_id={client_id}"
                    + "&redirect_uri={redirect_uri}"
                    + "&client_secret={client_secret}"
                    + "&code={code}")
                    .Replace("{client_id}", this.ClientId)
                    .Replace("{redirect_uri}", this.CallbackUri)
                    .Replace("{client_secret}", this.ClientSecret)
                    .Replace("{code}", code);

                HttpWebRequest wrLogon = (HttpWebRequest)WebRequest.Create(reqUri);
                wrLogon.AllowAutoRedirect = false;
                wrLogon.KeepAlive = true;
                HttpWebResponse retreiveResponse = (HttpWebResponse)wrLogon.GetResponse();
                statusCode = (int)retreiveResponse.StatusCode;
                Stream objStream = retreiveResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                string json = objReader.ReadToEnd();
                retreiveResponse.Close();

                accessTokenResult = serializer.Deserialize<AccessTokenResult>(json);
                accessToken = accessTokenResult.access_token;
                //SaveToken();
            }
            catch (WebException wex)
            {
                HttpWebResponse wrs = (HttpWebResponse)wex.Response;
                throw new LogMeBotException((int)wrs.StatusCode, wex.Message);
            }
            catch (Exception ex)
            {
                throw new LogMeBotException(statusCode, ex.Message);
            }
            return accessToken;
        }

        /// <summary>
        /// Returns info about the logged user
        /// according to logon request scope
        /// </summary>
        public MeResult GetMe(string accessToken)
        {
            int statusCode = 0;
            var res = new MeResult();
            var serializer = new JavaScriptSerializer();

            try
            {

                string reqUri = (MeEndpoint
                    + "?access_token={access_token}")
                    .Replace("{access_token}", accessToken);

                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(reqUri);
                wr.AllowAutoRedirect = false;
                wr.KeepAlive = true;
                HttpWebResponse retreiveResponse = (HttpWebResponse)wr.GetResponse();
                statusCode = (int)retreiveResponse.StatusCode;
                Stream objStream = retreiveResponse.GetResponseStream();
                StreamReader objReader = new StreamReader(objStream);
                string json = objReader.ReadToEnd();
                retreiveResponse.Close();

                res = serializer.Deserialize<MeResult>(json);
            }
            catch (WebException wex)
            {
                HttpWebResponse wrs = (HttpWebResponse)wex.Response;
                throw new LogMeBotException((int)wrs.StatusCode, wex.Message);
            }
            catch (Exception ex)
            {
                throw new LogMeBotException(statusCode, ex.Message);
            }
            return res;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">anti forgery state. new Guid if empty</param>
        /// <returns></returns>
        public string GetLogOnUri(string state = "")
        {
            if (string.IsNullOrEmpty(state))
                state = Guid.NewGuid().ToString();

            HttpContext.Current.Session.Remove(SessionStateKey);
            HttpContext.Current.Session.Add(SessionStateKey, state);

            string url = (LogonEndpoint
                + "?response_type=code"
                + "&client_id={clientId}"
                + "&scope={scope}"
                + "&state={state}"
                + "&redirect_uri={callbackUri}")
                .Replace("{clientId}", clientId)
                .Replace("{scope}", scope)
                .Replace("{state}", state)
                .Replace("{callbackUri}", callbackUri);
            return url;
        }

        private string getConfigValue(string key, string defaultValue = "")
        {
            string res = defaultValue;
            if (ConfigurationManager.AppSettings[key] != null)
                res = ConfigurationManager.AppSettings[key];
            return res;
        }

        private void init(string clientId, string clientSecret, string callbackUri)
        {
            if (string.IsNullOrEmpty(clientId)
                || string.IsNullOrEmpty(clientSecret)
                || string.IsNullOrEmpty(callbackUri))
            {
                throw new ArgumentException("Invalid Logmebot app settings");
            }

            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.callbackUri = callbackUri;
        }
    }
}
