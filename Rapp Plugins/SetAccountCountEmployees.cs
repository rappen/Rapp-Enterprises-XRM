using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Diagnostics;
using System.Linq;

namespace Rapp_Plugins
{
    public class SetAccountCountEmployees : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);

            if (context.PrimaryEntityName != "contact")
            {
                tracer.Trace($"Wrong entity: {context.PrimaryEntityName}");
                return;
            }
            if (context.MessageName != "Create" || context.MessageName != "Update" || context.MessageName != "Delete")
            {
                tracer.Trace($"Wrong message: {context.MessageName}");
                return;
            }
            if (!context.InputParameters.ContainsKey("Target") ||
                !(context.InputParameters["Target"] is Entity target) ||
                !target.Contains("parentcustomerid"))
            {
                tracer.Trace("Target is not an entity or does not contain parentcustomerid.");
                return;
            }

            var customerRef = target["parentcustomerid"] as EntityReference;
            var newAccountRef = customerRef?.LogicalName == "account" ? customerRef : null;
            var oldAccountRef = (EntityReference)null;

            if (context.PreEntityImages.Count > 0 &&
                 context.PreEntityImages[context.PreEntityImages.Keys.First()] is Entity preimage &&
                 preimage.Contains("parentcustomerid"))
            {
                var oldCustomerRef = preimage["parentcustomerid"] as EntityReference;
                oldAccountRef = oldCustomerRef?.LogicalName == "account" ? oldCustomerRef : null;
            }

            CountEmployees(service, tracer, oldAccountRef);
            CountEmployees(service, tracer, newAccountRef);
        }

        private void CountEmployees(IOrganizationService service, ITracingService tracer, EntityReference accountRef)
        {
            if (accountRef == null || accountRef.Id.Equals(Guid.Empty))
            {
                tracer.Trace("No account reference, just exit.");
                return;
            }
            var sw = Stopwatch.StartNew();
            var account = service.Retrieve("account", accountRef.Id, new ColumnSet("accountid", "name", "numberofemployees"));
            sw.Stop();
            tracer.Trace($"Retrieved account: {account["name"]} in {sw.ElapsedMilliseconds} ms");

            account.TryGetAttributeValue("numberofemployees", out int oldemployees);

            var query = new QueryExpression("contact");
            query.ColumnSet = new ColumnSet("fullname");
            query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, accountRef.Id);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            sw.Restart();
            var contacts = service.RetrieveMultiple(query);
            sw.Stop();
            tracer.Trace($"Retrieved {contacts.Entities.Count} contacts in {sw.ElapsedMilliseconds} ms");

            var newemployees = contacts.Entities.Count;

            if (!newemployees.Equals(oldemployees))
            {
                var updaccount = new Entity("account", account.Id);
                updaccount["numberofemployees"] = newemployees;
                sw.Restart();
                service.Update(updaccount);
                sw.Stop();
                tracer.Trace($"Updated account: {account["name"]} with {newemployees} contacts in {sw.ElapsedMilliseconds} ms");
            }
        }
    }
}