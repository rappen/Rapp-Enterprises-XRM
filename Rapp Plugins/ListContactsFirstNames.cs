using Microsoft.Xrm.Sdk;
using Rappen.XRM.RappSack;
using System.Linq;

namespace Rapp_Plugins
{
    public class ListContactsFirstNames : RappSackPlugin
    {
        public override string NeedEntity => "contact";
        public override string[] NeedMessages => new[] { "Create", "Update", "Delete" };
        public override string[] NeedAttributes => new[] { "parentcustomerid" };

        public override void Execute()
        {
            var contact = ContextEntity[ContextEntityType.Complete];

            var account = contact.GetParent(this, "parentcustomerid", "accountid", "name", "description");

            var contactlist = account.AttributeValue("description", string.Empty);

            var contacts = account.GetChildren(this, "contact", "parentcustomerid", true, "firstname");

            var newcontactlist = contacts.Entities
                .Select(c => c.AttributeValue("firstname", string.Empty))
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