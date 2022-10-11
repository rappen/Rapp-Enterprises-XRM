using Common;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Diagnostics;
using System.Linq;

namespace Rapp_Plugins
{
    public class ListContactsFirstNames : RappPluginBase
    {
        public override string ExpectedEntity => "contact";

        public override string[] ExpectedMessages => new string[] { "Create", "Update" };

        /*
* Concatenate first names of all Contacts to
* the Description field of their parent Account.
*
* Triggered on Create and Update of Contact.
*/

        public override void Execute(RappContext rc)
        {
            var account = rc.Complete.GetParent(rc, "parentcustomerid", "name", "description");
            if (account != null)
            {
                account.TryGetAttributeValue("description", out string contactlist);

                var contacts = account.GetChildren(rc, "contact", "parentcustoemid", "firstname");

                var newcontactlist = contacts
                    .Where(c => c.Contains("firstname"))
                    .Select(c => c["firstname"] as string)
                    .OrderBy(f => f)
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
}