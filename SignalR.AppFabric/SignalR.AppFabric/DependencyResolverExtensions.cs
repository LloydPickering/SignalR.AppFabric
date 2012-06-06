namespace SignalR.AppFabric
{
    using System;
    using Microsoft.ApplicationServer.Caching;
    using SignalR;

    public static class DependencyResolverExtensions
    {
        public static IDependencyResolver UseAppFabric(this IDependencyResolver resolver, String eventKey, TimeSpan cacheRecycle, DataCache dc)
        {
            var bus = new Lazy<AppFabricMessageBus>(() => new AppFabricMessageBus(resolver, eventKey, cacheRecycle, dc));
            resolver.Register(typeof(IMessageBus), () => bus.Value);

            return resolver;
        }
    }
}
