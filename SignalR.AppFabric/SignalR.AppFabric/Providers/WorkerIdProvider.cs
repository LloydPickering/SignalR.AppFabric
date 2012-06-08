namespace SignalR.AppFabric.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using NewId;
    using NewId.Providers;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class WorkerIdProvider : IWorkerIdProvider
    {

        public byte[] GetWorkerId(int index)
        {
            //get the default implemented provider
            NetworkAddressWorkerIdProvider prov = new NetworkAddressWorkerIdProvider();
            byte[] result = prov.GetWorkerId(index);


            //and overwrite the first 4 bytes with the current thread ID (this is so that we can still be unique with web gardens which run on the same box)
            if (result.Length >= 4)
            {
                BitConverter.GetBytes(System.Threading.Thread.CurrentThread.ManagedThreadId).CopyTo(result, 0);
            }

            return result;
        }
    }
}
