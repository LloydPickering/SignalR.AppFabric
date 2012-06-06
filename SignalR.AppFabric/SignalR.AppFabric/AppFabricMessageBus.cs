namespace SignalR.AppFabric
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationServer.Caching;
    using ProtoBuf;
    using SignalR;

    /// <summary>
    /// Message Bus for SignalR using AppFabric
    /// </summary>
    public class AppFabricMessageBus : IMessageBus, IIdGenerator<String>
    {
        private readonly InProcessMessageBus<String> _bus;
        private DataCache _dc = null;
        private readonly String _eventKey;
        private TimeSpan _timeout;

        public AppFabricMessageBus(IDependencyResolver resolver, String eventKey, TimeSpan cachecleanup, DataCache dc)
        {
            _eventKey = eventKey;
            _bus = new InProcessMessageBus<String>(resolver, this);
            _timeout = cachecleanup;

            _dc = dc;
            _dc.AddCacheLevelCallback(DataCacheOperations.AddItem, CacheCallback);
        }
        private void CacheCallback(String cachename, String region, String key, DataCacheItemVersion itemVersion, DataCacheOperations OperationId, DataCacheNotificationDescriptor nd)
        {
            Object o = null;
            while (o == null)
            {
                try
                {
                    o = _dc.Get(key);
                }
                catch { }
            }
            Byte[] b = o as Byte[];
            if (b != null)
                OnMessage(key, b);
            else
                Debug.WriteLine("Error with key: " + key);
        }
        private void OnMessage(String key, Byte[] data)
        {
            try
            {
                var message = Message.Deserialize(data);

                if (message.EventKey == _eventKey)
                {
                    try
                    {
                        ConvertFromString(key.Remove(0, _eventKey.Length));
                    }
                    catch (Exception) { }
                }

                // Save it to the local buffer
                _bus.Send(message.ConnectionId, message.EventKey, message.Value).Catch();
            }
            catch (Exception)//ex)
            {
                // TODO: Handle this better
                //Debug.WriteLine(ex.Message);
            }
        }
        #region IIdGenerator<long> members
        public string ConvertFromString(String value)
        {
            return value;
        }

        public string ConvertToString(String value)
        {
            return value;
        }

        private Int64 ticks = 0;
        private Int32 sequence = 0;
        private Int32 _threadID = Thread.CurrentThread.ManagedThreadId;
        private String computername = Environment.MachineName;
        public String GetNext()
        {
            if (DateTime.Now.Ticks == ticks)
            {
                sequence++;
            }
            else
            {
                ticks = DateTime.Now.Ticks;
                sequence = 0;
            }
            return ticks.ToString("00000000000000000000") + sequence.ToString("0000") + computername + _threadID;
        }
        #endregion

        #region IMessageBus members
        public Task<MessageResult> GetMessages(IEnumerable<String> eventKeys, String lastmessageid, CancellationToken timeoutToken)
        {
            return _bus.GetMessages(eventKeys, lastmessageid, timeoutToken);
        }
        public Task Send(String source, String eventKey, Object value)
        {
            var message = new Message(source, eventKey, value);
            String key = GetNext();
            _dc.Add(eventKey + key, message.GetBytes(), _timeout);
            return TaskAsyncHelper.Empty;
        }
        #endregion
        [ProtoContract]
        private class Message
        {
            public Message()
            {
            }

            public Message(string connectionId, string eventKey, object value)
            {
                ConnectionId = connectionId;
                EventKey = eventKey;
                Value = value.ToString();
            }

            [ProtoMember(1)]
            public string ConnectionId { get; set; }

            [ProtoMember(2)]
            public string EventKey { get; set; }

            [ProtoMember(3)]
            public string Value { get; set; }

            public byte[] GetBytes()
            {
                using (var ms = new MemoryStream())
                {
                    Serializer.Serialize(ms, this);
                    return ms.ToArray();
                }
            }

            public static Message Deserialize(byte[] data)
            {
                using (var ms = new MemoryStream(data))
                {
                    return Serializer.Deserialize<Message>(ms);
                }
            }
        }
    }
}
