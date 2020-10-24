using Common;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace Rapp_Plugins
{
    public class ListContactsFirstNames : RappPluginBase
    {
        public override string ExpectedEntity => "contact";

        public override string[] ExpectedMessages => new string[] { "Create", "Udpate" };

        /* 
        * Concatenate first names of all Contacts to 
        * the Description field of their parent Account.
        * 
        * Triggered on Create and Update of Contact.
        */
        public override void Execute(RappContext rc)
        {
            var accountref = rc.GetAttributeValue("parentcustomerid", new EntityReference());
            if (accountref.Id.Equals(Guid.Empty))
            {
                return;
            }

            var account = rc.Target.GetParent(rc, "parentcustomerid", "accountid", "name", "description");

            account.TryGetAttributeValue("description", out string contactlist);
            var contacts = account.GetChildren(rc, "contact", "parentcustomerid", "firstname");

            var newcontactlist = contacts
                .Where(c => c.Contains("firstname"))
                .Select(c => c["firstname"] as string)
                .Distinct();

            if (!newcontactlist.Equals(contactlist))
            {
                var updaccount = new Entity("account", account.Id);
                updaccount["description"] = newcontactlist;
                rc.Update(updaccount);
            }
        }
    }
}
