using Common;
using Microsoft.Xrm.Sdk;
using System;

namespace Rapp_Plugins
{
    public class VerifyAccountNo : RappPluginBase
    {
        public override string ExpectedEntity => "account";

        public override string[] ExpectedMessages => new string[] { "Create" };

        /*
* Verify that Account Number is numeric
*
* Triggered on Create of Account.
*/

        public override void Execute(RappContext rc)
        {
            if (rc.Target?.Contains("accountnumber") != true)
            {   // All good
                return;
            }
            var accountnumber = rc.Target["accountnumber"] as string;

            if (!int.TryParse(accountnumber, out int numericnumber))
            {
                throw new InvalidPluginExecutionException("Account Number must be numeric.");
            }
        }
    }
}