using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace StatsdNet
{
    /// <summary>
    /// Provides a framework for building a URI in stages.
    /// </summary>
    public class UrlBuilder
    {
        #region Private Members...
        private readonly Uri _sourceAddress;
        private readonly List<string> _commands = new List<string>();
        private readonly NameValueCollection _parameters;
        #endregion

        #region Public Properties...
        /// <summary>
        /// Get the port number from the url.
        /// </summary>
        public int Port { get { return _sourceAddress.Port; } }

        /// <summary>
        /// Get only the http://host name minus all path and port details.
        /// </summary>
        public string Server { get { return string.Format("{0}://{1}", _sourceAddress.Scheme, _sourceAddress.Host); } }

        /// <summary>
        /// Get the lowest first level path of the url.  ex:  http://domain:port/first
        /// </summary>
        public string RootServerPath { get { return string.Format("{0}://{1}{2}", _sourceAddress.Scheme, _sourceAddress.Authority, _sourceAddress.AbsolutePath); } }

        /// <summary>
        /// Get the full path without any query string parameters.  ex: http://domain:port/first/second/third
        /// </summary>
        public string LocalServerPath
        {
            get
            {
                var address = Address;
                return string.Format("{0}://{1}{2}", address.Scheme, address.Authority, address.LocalPath);
            }
        }

        /// <summary>
        /// The full Uri address built from all address parts.
        /// </summary>
        public Uri Address
        {
            get
            {
                return new Uri(BuildUrl());
            }
        }

        /// <summary>
        /// The collection of querystring parameters in this URL
        /// </summary>
        public NameValueCollection Parameters { get { return _parameters; } }
        #endregion

        #region Constructor...
        public UrlBuilder(string connectionString)
        {
            _sourceAddress = new Uri(connectionString);
            _parameters = System.Web.HttpUtility.ParseQueryString(_sourceAddress.Query);

            if (_sourceAddress == null)
                throw new Exception("The source Conncetion string did not properly parce into a Uri.  Source: " + connectionString);
        }
        #endregion

        #region Public Methods...
        /// <summary>
        /// Get the value from a given querystring key.
        /// </summary>
        /// <param name="key">The key of the querystring</param>
        /// <returns>The value of the key part of a querystring or null if it does not exist.</returns>
        public string GetParameterValue(string key)
        {
            return _parameters[key];
        }

        /// <summary>
        /// Added a directory level command ie: http://host:port/command/command/command
        /// </summary>
        /// <param name="command">The list of commands to add to the host.  Will be added in order received.</param>
        public void AddCommand(params string[] command)
        {
            foreach (var c in command)
            {
                _commands.Add(c);
            }
        }


        /// <summary>
        /// Add a querystring parameter to the Url
        /// </summary>
        /// <param name="key">The querystring key part key=value</param>
        /// <param name="value">The querystring value part key=value</param>
        /// <remarks>The value and key will automatically be URL encoded.</remarks>
        public void AddParameter(string key, string value)
        {
            _parameters.Add(key, value);
        }

        /// <summary>
        /// Gets a clean copy of the SolrUrl with the original base server path.
        /// </summary>
        /// <returns></returns>
        public UrlBuilder CloneBase()
        {
            return new UrlBuilder(RootServerPath);
        }
        #endregion

        #region Private Members...
        private string BuildUrl()
        {
            var url = RootServerPath;
            var commands = string.Join("/", _commands);
            if (string.IsNullOrEmpty(commands) == false)
                url = string.Format("{0}/{1}", url, commands);

            if (_parameters.Count > 0)
                return string.Format("{0}?{1}", url, _parameters);

            return url;
        }
        #endregion
    }
}
