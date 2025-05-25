using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rappen.XRM.RappSack;
using System;
using System.Linq;

namespace Rapp_Plugins
{
    public class ListContactsFirstNames : RappSackPlugin
    {
        public override string NeedEntity => "contact";
        public override string[] NeedMessages => new[] { "Create", "Update" };
        public override string[] NeedAttributes => new[] { "parentcustomerid" };

        public override void Execute()
        {
            var Target = ContextEntity[ContextEntityType.Complete];
            var accountref = Target["parentcustomerid"] as EntityReference;
            if (accountref.Id.Equals(Guid.Empty))
            {
                Trace("No parentcustomerid, just exit.");
                return;
            }

            var account = Retrieve("account", accountref.Id, new ColumnSet("accountid", "name", "description"));

            account.TryGetAttributeValue("description", out string contactlist);

            var query = new QueryExpression("contact");
            query.ColumnSet = new ColumnSet("firstname");
            query.Criteria.AddCondition("parentcustomerid", ConditionOperator.Equal, accountref.Id);
            query.AddOrder("firstname", OrderType.Ascending);

            var contacts = RetrieveMultiple(query);

            var newcontactlist = contacts.Entities
                .Where(c => c.Contains("firstname"))
                .Select(c => c["firstname"] as string)
                .Distinct();

            if (!newcontactlist.Equals(contactlist))
            {
                var updaccount = new Entity("account", account.Id);
                updaccount["description"] = newcontactlist;
                Update(updaccount);
            }
        }
    }
}