using Common;
using Microsoft.Xrm.Sdk;

namespace Rapp_Plugins
{
    public class VerifyAccountNo : RappPluginBase
    {
        public override string[] ExpectedMessages => new string[] { "Create" };

        public override string ExpectedEntity => "account";

        public override void Execute(RappContext rc)
        {
            var accountnumber = rc.GetAttributeValue("accountnumber", string.Empty);
            if (!string.IsNullOrWhiteSpace(accountnumber) &&
                !int.TryParse(accountnumber, out _))
            {
                throw new InvalidPluginExecutionException("Account Number must be numeric.");
            }
        }
    }
}
