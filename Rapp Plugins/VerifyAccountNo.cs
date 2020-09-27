using Microsoft.Xrm.Sdk;
using System;

namespace Rapp_Plugins
{
    public class VerifyAccountNo : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = new Lazy<IOrganizationService>(() => factory.CreateOrganizationService(context.UserId));

            if (context.InputParameters.ContainsKey("Target") && context.InputParameters["Target"] is Entity target)
            {
                if (!target.Contains("accountnumber"))
                {   // All good
                    return;
                }
                var accountnumber = target["accountnumber"] as string;

            }
        }
    }
}
