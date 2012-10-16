using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StatsdNet
{
    public interface IStatsdPipe
    {
        /// <summary>
        /// The object representing the server url connection.
        /// </summary>
        UrlBuilder Server { get; }

        /// <summary>
        /// The application name to store all the stats under.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Indicates if stats will be sent to the server.
        /// </summary>
        bool Active { get; }

        /// <summary>
        /// Indicates if all stats will be placed under the source machine name folder.
        /// </summary>
        bool UseMachineNameFolder { get; }

        /// <summary>
        /// Records a generic gauge value stat.  Gauge is an absolute value to track as opposed to count per period.
        /// </summary>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="value">The absolute gauge value.  Does not decay over time.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <returns></returns>
        bool Gauge(string key, int value, double sampleRate = 1.0);

        /// <summary>
        /// Records length of execution of any given event at a certain time period.
        /// </summary>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="milliseconds">The amount of time in milliseconds this event took.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <returns></returns>
        bool Timing(string key, long milliseconds, double sampleRate = 1.0);

        /// <summary>
        /// Time and execute a function with the timing information sent to statsd.
        /// </summary>
        /// <param name="func">The function to execute and time.</param>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <returns></returns>
        T TimeIt<T>(Func<T> func, string key, double sampleRate = 1.0);

        /// <summary>
        /// Execute a function that returns a task and send the task timing information to statsd.
        /// </summary>
        /// <param name="func">The function to execute and time.</param>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <returns></returns>
        Task<T> TimeIt<T>(Func<Task<T>> func, string key, double sampleRate = 1.0);

        /// <summary>
        /// Execute a function that returns a task and send the task timing information to statsd.
        /// </summary>
        /// <param name="func">The function to execute and time.</param>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <typeparam name="T">The return type of the function.</typeparam>
        /// <returns></returns>
        Task TimeIt(Func<Task> func, string key, double sampleRate = 1.0);

        /// <summary>
        /// Time and execute an action with the timeing information sent to statsd.
        /// </summary>
        /// <param name="method">The action to execute and time.</param>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        void TimeIt(Action method, string key, double sampleRate = 1.0);

        /// <summary>
        /// Decrements a stat at a specific time period.
        /// </summary>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="magnitude">The amount to decrement by.  Will automatically convert to a negative value.</param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        bool Decrement(string key, int magnitude = -1, double sampleRate = 1.0);

        /// <summary>
        /// Decrements a set of stats by the default magnitude of -1 and sampleRate of 1.0
        /// </summary>
        /// <param name="keys">The unique stat name.  Use dot notation to create folder.</param>
        /// <returns></returns>
        bool Decrement(params string[] keys);

        /// <summary>
        /// Decrements a set of stats at a specific time period
        /// </summary>
        /// <param name="keys">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="magnitude"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        bool Decrement(IEnumerable<string> keys, int magnitude = -1, double sampleRate = 1.0);

        /// <summary>
        /// Increments a set of stats by the default magnitude of 1 and sampleRate of 1.0
        /// </summary>
        /// <param name="keys">The unique stat name.  Use dot notation to create folder.</param>
        /// <returns></returns>
        bool Increment(params string[] keys);

        /// <summary>
        /// Increments a stat at a specific time period
        /// </summary>
        /// <param name="key">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="magnitude"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        bool Increment(string key, int magnitude = 1, double sampleRate = 1.0);

        /// <summary>
        /// Increments a set of stats at a specific time period
        /// </summary>
        /// <param name="keys">The unique stat name.  Use dot notation to create folder.</param>
        /// <param name="magnitude"></param>
        /// <param name="sampleRate"></param>
        /// <returns></returns>
        bool Increment(IEnumerable<string> keys, int magnitude = 1, double sampleRate = 1.0);
    }
}
