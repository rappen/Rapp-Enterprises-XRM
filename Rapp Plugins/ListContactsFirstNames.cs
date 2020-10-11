using Common;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace Rapp_Plugins
{
    public class ListContactsFirstNames : RappPluginBase
    {
        public override string[] ExpectedMessages => new string[] { "Create", "Update" };

        public override string ExpectedEntity => "contact";

        public override void Execute(RappContext rc)
        {
            if (rc.GetAttributeValue("parentcustomerid", new EntityReference()).Id.Equals(Guid.Empty))
            {
                return;
            }

            var account = rc.Target.GetParent(rc, "parentcustomerid", "accountid", "name", "description");
            var contacts = account.GetChildren(rc, "contact", "parentcustomerid", "firstname");

            var newcontactlist = contacts
                .Where(c => c.Contains("firstname"))
                .Select(c => c["firstname"] as string)
                .Distinct();

            account.TryGetAttributeValue("description", out string contactlist);
            if (!newcontactlist.Equals(contactlist))
            {
                var updaccount = new Entity("account", account.Id);
                updaccount["description"] = newcontactlist;
                rc.Update(updaccount);
            }
        }
    }
}