namespace MSharp.Framework.Services
{
    using System;
    using System.ComponentModel;
    using System.Threading;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IntegrationTestInjector
    {
        const int INJECTOR_AGENT_WAIT_INTERVALS = 10;// ms

        public static void Inject(Type serviceType, string request, string response)
        {
            var serviceKey = IntegrationManager.GetServiceKey(serviceType);

            while (true)
            {
                var queueItem = Database.Find<IIntegrationQueueItem>(i =>
                i.IntegrationService == serviceKey &&
                i.ResponseDate == null &&
                (request.IsEmpty() || i.Request == request));

                if (queueItem == null)
                {
                    Thread.Sleep(INJECTOR_AGENT_WAIT_INTERVALS);
                    continue;
                }

                Database.Update(queueItem, i =>
                {
                    i.Response = response;
                    i.ResponseDate = LocalTime.Now;
                });

                break;
            }
        }
    }
}
