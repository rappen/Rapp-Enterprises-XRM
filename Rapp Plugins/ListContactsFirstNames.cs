using Common;
using System.Linq;

namespace Rapp_Plugins
{
    /*
     * Concatenate first names of all Contacts to
     * the Description field of their parent Account.
     *
     * Triggered on Create and Update of Contact.
    */

    public class ListContactsFirstNames : RappPluginBase
    {
        public override string ExpectedEntity => "contact";

        public override string[] ExpectedMessages => new[] { "Create", "Update" };

        public override void Execute(RappContext rc)
        {
            var account = rc.Complete.GetParent(rc, "parentcustomerid", "accountid", "name", "description");
            if (account == null)
            {
                return;
            }

            var contacts = account.GetChildren(rc, "contact", "parentcustomerid", "firstname");

            var newcontactlist = contacts.Entities
                .Where(c => c.Contains("firstname"))
                .Select(c => c["firstname"] as string)
                .OrderBy(f => f)
                .Distinct();

            account.TryGetAttributeValue("description", out string contactlist);
            if (!newcontactlist.Equals(contactlist))
            {
                var updaccount = account.Clone();
                updaccount["description"] = newcontactlist;
                rc.Update(updaccount);
            }
        }
    }
}