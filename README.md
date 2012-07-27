StatsdNet
=========

Simple client to send UDP stats packets to a Statsd server using the Etsy pattern.

### Summary
This client provides methods for sending statistical data to a Statsd server.  It has been heavily modified from the C# example provided by the Etsy Statsd project found [here (etsy/statsd)](https://github.com/etsy/statsd/).  Some of the modifications include: automatic server url loading from the app config, standardized key folder structure, required application name for all keys, common defaults for parameters, expanded exception handling and more.

### Methods
#### Gauge
Records a static value.  Used for tracking static state data at a given point of time.  Examples of this could be amount of threads being using at a given moment.
####Timing 
Records a timed event in milliseconds.  Obviously, this is useful for tracking the length of time it takes to execute any set of actions.  Example could be timing calls to an external service or really any time sensitive section of code.
####Increment / Decrement
Records the magnitude of an event at any given time.  Useful for tracking counts of an event occuring at any given time.  Examples could be incrementing for each exception encountered or user logins.

####TimeIt
Provides a nice wrapper for sending Timing information for any Action, Function or Task.  

	StatsdPipe.TimeIt(() => 
		{
			DoSomeWorkHere();
			TheTimeItMethodWillTimeTheseMethodsTogether();
		}, "SomeWorkTimedMS");

Note that TimeIt will also work with Task based methods by subscribing to the ContinueWith method to record the time to complete the task.  This is however, not the most accurate time measurement as the ContinueWith method is subject to task scheduling.  This method is useful for keeping the main code path cleaner and easier to read by removing the Stopwatch creation and timing buzz code.

### Default Configuration Options and Opinions
1. Requires a Url with the format: http://server:port?application=name
2. All stats key names are stored in the format: ApplicationName.MachineName.Key
Note: MachineName can be turned off through a [usemachinenamefolder=false] parameter in the Url.
3. The client by default will look for the Url in the app.config under the connectionStrings section with the name: StatsdNet.Server.
4. If no Url is provided in the app.config and no Url is passed to the constructor then the client will deactivate and not attempt to send stats packets.
