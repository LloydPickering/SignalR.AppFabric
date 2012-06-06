SignalR.AppFabric
=================

A SignalR Message Bus implemented using Windows Server App Fabric.

This Message Bus is largely based on the SignalR.Redis code.

Usage:

1. Setup an AppFabric cache for SignalR. You must configure the cache to allow notifications (More info here: http://msdn.microsoft.com/en-us/library/ee808091(v=azure.10).aspx )
2. Instantiate a DataCache using DataCacheFactory. You can either do this with webconfig setup and the default constructor, or, you can create a DataCacheFactoryconfiguration object.
3. In your Application_Start() call: SignalR.GlobalHost.DependencyResolver.UseAppFabric

Notes:
don't forget to alter the notification poll time of your Cache object. Default is 300seconds, which is way too long for most uses. We are using signalr for near realtime stuff, so we need to poll the cache multiple times every second.
I instantiate the cache in code rather than using web.config as we have our DataCacheNotificationProperties polling time to 100ms, the web.config xml key is specified in (int)seconds.

