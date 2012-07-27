using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;

namespace StatsdNet
{
    /// <summary>
    /// Statistics tracking libary which uses a connection to a Statsd server to send application specific statistics to.
    /// The statsd receives UDP packets and so this library is essentially a fire and forget stats tracking system.  If
    /// the statsd server is down the stats will be lost but it will not affect the opperation of the executing program.
    /// 
    /// Stats do not have to be created before hand.  Any new stat will automatically get stored and created apon arrival.
    /// Therefore the ideal use of this library is to try and track as many meaningfull statistics as possible and create
    /// graphs over that data if needed at a later time.
    /// </summary>
    public class StatsdPipe : IDisposable
    {
        private const string STATS_APPLICATION_NAME = "application";
        private const string STATS_USE_MACHINE_NAME = "usemachinenamefolder";
        private const string STATS_CONNECTION_SETTING = "StatsdNet.Server";
        private UdpClient _udpClient;
        private readonly Random _random = new Random();

        /// <summary>
        /// The object representing the server url connection.
        /// </summary>
        public UrlBuilder Server { get; private set; }

        /// <summary>
        /// The application name to store all the stats under.
        /// </summary>
        public string ApplicationName { get; private set; }

        /// <summary>
        /// Indicates if stats will be sent to the server.
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// Indicates if all stats will be placed under the source machine name folder.
        /// </summary>
        public bool UseMachineNameFolder { get; private set; }

        /// <summary>
        /// Constructs the stats pipe using the default connection string from the config file.  ConnectionString=Move.Statsd.Server.
        /// </summary>
        public StatsdPipe()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[STATS_CONNECTION_SETTING];

            if (connectionString != null) Initialize(connectionString.ConnectionString);
        }

        /// <summary>
        /// Constructs the stats pipe using a given connection string to a statsd server.
        /// </summary>
        /// <param name="connectionString">Connection string in the format: http://server:port?application=MyAppName</param>
        public StatsdPipe(string connectionString)
        {
            Initialize(connectionString);
        }

        private void Initialize(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) return;

            Active = true;

            Server = new UrlBuilder(connectionString);

            ApplicationName = Server.GetParameterValue(STATS_APPLICATION_NAME);
            if (ApplicationName == null)
                throw new ConfigurationErrorsException(string.Format("The statsd connection string must contain a application querystring parameter.  Connection: {0}",
                                                  connectionString));

            var useMachineName = Server.GetParameterValue(STATS_USE_MACHINE_NAME);
            if (string.IsNullOrEmpty(useMachineName))
                UseMachineNameFolder = true;
            else
            {
                bool tempBool;
                UseMachineNameFolder = !bool.TryParse(useMachineName, out tempBool) || tempBool;
            }

            if (UseMachineNameFolder)
                ApplicationName = string.Concat(ApplicationName, ".", Environment.MachineName);

            _udpClient = new UdpClient(Server.Address.Host, Server.Port);
        }

        /// <summary>
        /// Records a generic gauge value stat.  Gauge is an absolute value to track as opposed to count per period.
        /// </summary>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="value">The absolute gauge value.  Does not decay over time.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <returns></returns>
        public bool Gauge(string key, int value, double sampleRate = 1.0)
        {
            if (key == null) throw new ArgumentNullException("key");
            return Send(sampleRate, String.Format("{0}:{1:d}|g", key.Replace(":", ""), value));
        }

        /// <summary>
        /// Records length of execution of any given event at a certain time period.
        /// </summary>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="milliseconds">The amount of time in milliseconds this event took.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <returns></returns>
        public virtual bool Timing(string key, long milliseconds, double sampleRate = 1.0)
        {
            if (key == null) throw new ArgumentNullException("key");
            return Send(sampleRate, String.Format("{0}:{1:d}|ms", key.Replace(":", ""), milliseconds));
        }

        /// <summary>
        /// Time and execute a function with the timing information sent to statsd.
        /// </summary>
        /// <param name="func">The function to execute and time.</param>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <returns></returns>
        public T TimeIt<T>(Func<T> func, string key, double sampleRate = 1.0)
        {
            var sw = Stopwatch.StartNew();
            var result = func();
            Timing(key, sw.ElapsedMilliseconds, sampleRate);
            return result;
        }

        /// <summary>
        /// Execute a function that returns a task and send the task timing information to statsd.
        /// </summary>
        /// <param name="func">The function to execute and time.</param>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <returns></returns>
        public Task<T> TimeIt<T>(Func<Task<T>> func, string key, double sampleRate = 1.0)
        {
            var sw = Stopwatch.StartNew();
            var result = func();
            result.ContinueWith(x => Timing(key, sw.ElapsedMilliseconds, sampleRate));
            return result;
        }

        /// <summary>
        /// Execute a function that returns a task and send the task timing information to statsd.
        /// </summary>
        /// <param name="func">The function to execute and time.</param>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <returns></returns>
        public Task TimeIt(Func<Task> func, string key, double sampleRate = 1.0)
        {
            var sw = Stopwatch.StartNew();
            var result = func();
            result.ContinueWith(x => Timing(key, sw.ElapsedMilliseconds, sampleRate));
            return result;
        }

        /// <summary>
        /// Time and execute an action with the timeing information sent to statsd.
        /// </summary>
        /// <param name="method">The action to execute and time.</param>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        public void TimeIt(Action method, string key, double sampleRate = 1.0)
        {
            var sw = Stopwatch.StartNew();
            method();
            Timing(key, sw.ElapsedMilliseconds, sampleRate);
        }

        /// <summary>
        /// Decrements a stat at a specific time period.
        /// </summary>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="magnitude">The amount to decrement by.  Will automatically convert to a negative value.</param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public bool Decrement(string key, int magnitude = -1, double sampleRate = 1.0)
        {
            if (key == null) throw new ArgumentNullException("key");
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(key, magnitude, sampleRate);
        }

        /// <summary>
        /// Decrements a set of stats by the default magnitude of -1 and sampleRate of 1.0
        /// </summary>
        /// <param name="keys">The unique stat name.  Use dot notation to create folder.</param>
        /// <returns></returns>
        public bool Decrement(params string[] keys)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            return Increment(keys);
        }

        /// <summary>
        /// Decrements a set of stats at a specific time period
        /// </summary>
        /// <param name="keys">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="magnitude"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public bool Decrement(IEnumerable<string> keys, int magnitude = -1, double sampleRate = 1.0)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            magnitude = magnitude < 0 ? magnitude : -magnitude;
            return Increment(keys, magnitude, sampleRate);
        }

        /// <summary>
        /// Increments a set of stats by the default magnitude of 1 and sampleRate of 1.0
        /// </summary>
        /// <param name="keys">The unique stat name.  Use dot notation to create folder.</param>
        /// <returns></returns>
        public bool Increment(params string[] keys)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            return Increment(keys);
        }

        /// <summary>
        /// Increments a stat at a specific time period
        /// </summary>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="magnitude"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public bool Increment(string key, int magnitude = 1, double sampleRate = 1.0)
        {
            if (key == null) throw new ArgumentNullException("key");
            return Increment(new[] { key }, magnitude, sampleRate);
        }

        /// <summary>
        /// Increments a set of stats at a specific time period
        /// </summary>
        /// <param name="keys">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="magnitude"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        public bool Increment(IEnumerable<string> keys, int magnitude = 1, double sampleRate = 1.0)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            return Send(sampleRate, keys.Select(key => String.Format("{0}:{1}|c", key.Replace(":", ""), magnitude)).ToArray());
        }

        protected bool Send(String stat, double sampleRate)
        {
            return Send(sampleRate, stat);
        }

        protected bool Send(double sampleRate, params string[] statKeys)
        {
            if (Active == false) return false;

            var retval = false; // didn't send anything
            var groupStats = statKeys.Select(x => string.Concat(ApplicationName, ".", x));
            if (sampleRate < 1.0)
            {
                foreach (var stat in groupStats)
                {
                    if (_random.NextDouble() <= sampleRate)
                    {
                        var statFormatted = String.Format("{0}|@{1:f}", stat, sampleRate);
                        if (DoSend(statFormatted))
                        {
                            retval = true;
                        }
                    }
                }
            }
            else
            {
                foreach (var stat in groupStats)
                {
                    if (DoSend(stat))
                    {
                        retval = true;
                    }
                }
            }

            return retval;
        }

        protected bool DoSend(string stat)
        {
            var data = Encoding.Default.GetBytes(stat);

            _udpClient.Send(data, data.Length);
            return true;
        }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                if (_udpClient != null)
                {
                    _udpClient.Close();
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("Exception occured in the statdspipe dispose method.  Exception ignored.  Ex:{0}", ex));
            }
        }

        #endregion
    }
}