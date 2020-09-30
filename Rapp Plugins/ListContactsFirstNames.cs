using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Rapp_Plugins
{
    public class ListContactsFirstNames : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);

            if (context.MessageName != "Create" || context.MessageName != "Update")
            {
                tracer.Trace($"Wrong message: {context.MessageName}");
                return;
            }
            if (context.PrimaryEntityName != "contact")
            {
                tracer.Trace($"Wrong entity: {context.PrimaryEntityName}");
                return;
            }

            if (context.InputParameters.ContainsKey("Target") && context.InputParameters["Target"] is Entity target && target.Contains("parentcustomerid"))
            {
                var accountref = target["parentcustomerid"] as EntityReference;
                if (accountref.Id.Equals(Guid.Empty))
                {
                    if (context.PreEntityImages.Count > 0 && context.PreEntityImages[context.PreEntityImages.Keys.First()] is Entity preimage && preimage.Contains("parentcustomerid"))
                    {
                        accountref = preimage["parentcustomerid"] as EntityReference;
                    }
                }
                if (accountref.Id.Equals(Guid.Empty))
                {
                    return;
                }
                var account = service.Retrieve("account", accountref.Id, new ColumnSet("accountid", "name", "description"));
                account.TryGetAttributeValue("description", out string contactlist);
                tracer.Trace($"Retrieved account: {account["name"]}");

                var query = new QueryExpression("contact");
                query.ColumnSet = new ColumnSet("firstname");
                query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, accountref.Id);
                query.AddOrder("firstname", OrderType.Ascending);
                var contacts = service.RetrieveMultiple(query);
                var newcontactlist = contacts.Entities
                    .Where(c => c.Contains("firstname"))
                    .Select(c => c["firstname"] as string)
                    .Distinct();

                if (!newcontactlist.Equals(contactlist))
                {
                    var updaccount = new Entity("account", account.Id);
                    updaccount["description"] = newcontactlist;
                    service.Update(updaccount);
                }
            }
        }
    }
}
