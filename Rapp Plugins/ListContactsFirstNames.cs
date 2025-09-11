using Common;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace Rapp_Plugins
{
    public class ListContactsFirstNames : JRPlugin
    {
        public override string TriggerEntity => "contact";

        public override string[] TriggerMessages => new string[] { "Create", "Update" };

        public override void Execute()
        {
            if (!(complete.GetParent(this, "parentcustomerid", "accountid", "name", "description") is Entity account))
            {
                Trace("Contact has no parent account.");
                return;
            }

            account.TryGetAttributeValue("description", out string contactlist);
            var contacts = account.GetChildren(this, "contact", "parentcustomerid", true, "firstname");

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