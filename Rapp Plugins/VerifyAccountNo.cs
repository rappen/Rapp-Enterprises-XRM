using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace Rapp_Plugins
{
    public class VerifyAccountNo : IPlugin
    {
        /*
         * Verify that Account Number is numeric
         *
         * Triggered on Create of Account.
         */

        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);

            if (context.MessageName != "Create")
            {
                tracer.Trace($"Wrong message: {context.MessageName}");
                return;
            }
            if (context.Stage != 10)
            {
                tracer.Trace($"Wrong stage: {context.Stage}");
                return;
            }
            if (context.PrimaryEntityName != "account")
            {
                tracer.Trace($"Wrong entity: {context.PrimaryEntityName}");
                return;
            }
            if (!context.InputParameters.ContainsKey("Target") || !(context.InputParameters["Target"] is Entity target))
            {
                tracer.Trace("Target is not an entity.");
                return;
            }
            if (!target.Contains("accountnumber"))
            {   // All good
                tracer.Trace("Account Number not set.");
                return;
            }

            var accountnumber = target["accountnumber"] as string;

            if (accountnumber.Any(c => !char.IsDigit(c)))
            {
                throw new InvalidPluginExecutionException($"Account Number must be numeric. ({accountnumber})");
            }

            tracer.Trace($"Account Number is numeric: {accountnumber}");
        }
    }
}