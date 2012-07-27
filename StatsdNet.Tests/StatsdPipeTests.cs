using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace StatsdNet.Tests
{
    public class StatsdPipeTests
    {
        [Test]
        public void StatsdPipeConstructsFromDefaultTest()
        {
            var target = new StatsdPipe();
            Assert.IsTrue(target.Server.Port == 8125);
            Assert.IsTrue(target.Server.Address.Host == "localhost");
            Assert.IsTrue(target.ApplicationName == string.Concat(Environment.MachineName, ".", "UnitTests"));
        }

        [Test]
        [ExpectedException(typeof(ConfigurationErrorsException))]
        public void StatsExceptionOnMissingApplicationName()
        {
            var target = new StatsdPipe("http://localhost:8125?group=Statsd");
            Assert.IsTrue(target.Active == false);
        }
        
        [Test]
        public void StatsPipeShouldReportNotActiveWhenNotConnectionExists()
        {
            var target = new StatsdPipe(null);
            Assert.IsTrue(target.Active == false);
        }

        [Test]
        public void StatsPipeShouldSendEvenWhenServerIsFake()
        {
            var target = new StatsdPipe("http://localhost:1234?application=UnitTests");
            target.Increment("fake");
        }

        [Test]
        public void StatsdUnitTestRunRecordInStatsdServer()
        {
            var target = new StatsdPipe();
            target.Increment("StatsPipeTest");
        }

        [Test]
        public void EnsureMachineNameInConnectionStringIsHandled()
        {
            var target = new StatsdPipe("http://localhost:1234?application=UnitTests");
            Assert.IsTrue(target.UseMachineNameFolder);

            target = new StatsdPipe("http://localhost:1234?application=UnitTests&usemachinenamefolder=gibberish");
            Assert.IsTrue(target.UseMachineNameFolder);

            target = new StatsdPipe("http://localhost:1234?application=UnitTests&usemachinenamefolder=false");
            Assert.IsFalse(target.UseMachineNameFolder);

            target = new StatsdPipe("http://localhost:1234?application=UnitTests&usemachinenamefolder=true");
            Assert.IsTrue(target.UseMachineNameFolder);

            target = new StatsdPipe("http://localhost:1234?application=UnitTests&UseMachineNameFolder=false");
            Assert.IsFalse(target.UseMachineNameFolder);
        }

        [Test]
        public void EnsureAsyncFunctionsTimeProperly()
        {
            var stat = new StatsdPipeTiming();
            stat.TimeIt(() => Task.Factory.StartNew(() => Thread.Sleep(100)), "Test");
            Thread.Sleep(200);
            Assert.IsTrue(stat.ElapsedMilliseconds >= 100 && stat.ElapsedMilliseconds < 200);
        }

        [Test]
        public void EnsureAsyncTFunctionsTimeProperly()
        {
            var stat = new StatsdPipeTiming();
            stat.TimeIt(() => Task.Factory.StartNew(() => { Thread.Sleep(100); return true; }), "Test");
            Thread.Sleep(200);
            Assert.IsTrue(stat.ElapsedMilliseconds >= 100 && stat.ElapsedMilliseconds < 200);
        }
    }

    public class StatsdPipeTiming : StatsdPipe
    {
        public long ElapsedMilliseconds { get; set; }

        public override bool Timing(string key, long milliseconds, double sampleRate = 1.0)
        {
            ElapsedMilliseconds = milliseconds;
            return true;
        }
    }
}

