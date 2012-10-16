using System;
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
            Assert.IsNotNull(target.Server);
            Assert.IsTrue(target.Server.Port == 8125);
            Assert.IsTrue(target.Server.Address.Host == "localhost");
            Assert.IsTrue(target.ApplicationName == string.Concat("UnitTests", ".", Environment.MachineName));
        }

        [Test]
        public void StatsNotActiveOnBadUrl()
        {
            var target = new StatsdPipe("TBD");
            Assert.IsTrue(target.Active == false);
        }

        [Test]
        public void StatsExceptionOnEventFires()
        {
            int errorCount = 0;
            var target = new StatsdPipe("TBD", x => Interlocked.Increment(ref errorCount));
            Assert.AreEqual(false, target.Active);
            Assert.AreEqual(1, errorCount);
        }
        
        [Test]
        public void StatsPipeShouldReportNotActiveWhenNoConnectionExists()
        {
            var target = new StatsdPipe(null, null);
            Assert.IsTrue(target.Active == false);
        }

        [Test]
        public void StatsPipeShouldSendEvenWhenServerIsFake()
        {
            var target = new StatsdPipe("http://localhost:1234?application=UnitTests");
            target.Increment("fake");
        }

        [Test]
        public void StatsPipeShouldAddEnvironmentIfExists()
        {
            var s1 = new StatsdPipe("http://localhost:1234?application=UnitTests&environment=qa");
            var s2 = new StatsdPipe("http://localhost:1234?application=UnitTests");

            Assert.IsTrue(s1.ApplicationName.Contains(".qa"), "Should have .qa in the application name.");
            Assert.IsFalse(s2.ApplicationName.Contains(".qa"), "Should not have .qa in the application name.  Was " + s2.ApplicationName);
            Assert.IsTrue(s1.Active && s2.Active);
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

        [Test]
        public void EnsureActionsTimeProperly()
        {
            var stat = new StatsdPipeTiming();
            stat.TimeIt(() => Thread.Sleep(200), "Test");

            Assert.IsTrue(stat.ElapsedMilliseconds >= 200 && stat.ElapsedMilliseconds < 400);
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

