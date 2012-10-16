using System;
using System.Diagnostics;

namespace StatsdNet.Extensions
{

    /// <summary>
    /// Set of Statsd extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Will record the time it take to execute the given action into statsd.
        /// </summary> 
        /// <param name="method">The function to execute.</param>
        /// <param name="statsdPipe">The statsd pipe to write the timing information to.</param>
        /// <param name="key">The key to store the information under.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T TimeIt<T>(this Func<T> method, StatsdPipe statsdPipe, string key, double sampleRate = 1.0)
        {
            var sw = Stopwatch.StartNew();
            var result = method();
            statsdPipe.Timing(key, sw.ElapsedMilliseconds, sampleRate);
            return result;
        }

        /// <summary>
        /// Will record the time it take to execute the given action into statsd.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="statsdPipe">The statsd pipe to write the timing information to.</param>
        /// <param name="key">The key to store the information under.</param>
        /// <param name="sampleRate">The ratio of how often this value is sampled.</param>
        public static void TimeIt(this Action action, StatsdPipe statsdPipe, string key, double sampleRate = 1.0)
        {
            var sw = Stopwatch.StartNew();
            action();
            statsdPipe.Timing(key, sw.ElapsedMilliseconds, sampleRate);
        }
    }
}
