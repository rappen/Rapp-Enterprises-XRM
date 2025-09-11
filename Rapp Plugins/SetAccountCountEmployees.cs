using Common;
using Microsoft.Xrm.Sdk;

namespace Rapp_Plugins
{
    public class SetAccountCountEmployees : JRPlugin
    {
        public override string TriggerEntity => "contact";
        public override string[] TriggerMessages => new string[] { "Create", "Update", "Delete" };

        public override void Execute()
        {
            CountEmployees(this, preImage);
            CountEmployees(this, target);
        }

        private static void CountEmployees(JRPlugin jr, Entity contact)
        {
            var account = contact.GetParent(jr, "parentcustomerid", "accountid", "name", "numberofemployees");
            if (account == null)
            {
                jr.Trace("Contact has no parent account.");
                return;
            }
            var oldemployees = account.GetAttributeValue<int>("numberofemployees");

            var contacts = account.GetChildren(jr, "contact", "parentcustomerid", true, "contactid");
            var newemployees = contacts.Entities.Count;

            if (!newemployees.Equals(oldemployees))
            {
                var updaccount = account.Vanilla();
                updaccount["numberofemployees"] = newemployees;
                jr.Update(updaccount);
            }
        }
    }
}