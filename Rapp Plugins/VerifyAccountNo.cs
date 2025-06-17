using Microsoft.Xrm.Sdk;
using Rappen.XRM.RappSack;
using System.Linq;

namespace Rapp_Plugins
{
    public class VerifyAccountNo : RappSackPlugin
    {
        /*
        * Verify that Account Number is numeric
        *
        * Triggered on Create of Account.
        */
        public override string NeedEntity => "account";
        public override string NeedMessage => "Create";
        public override int NeedStage => 10;
        public override string[] NeedAttributes => new[] { "accountnumber" };
        public override bool NeedThrowIfNotMatch => true;

        public override void Execute()
        {
            var accountnumber = Target.AttributeValue("accountnumber", string.Empty);

            if (accountnumber.Any(c => !char.IsDigit(c)))
            {
                throw new InvalidPluginExecutionException($"Account Number must be numeric. ({accountnumber})");
            }

            Trace($"Account Number is numeric: {accountnumber}");
        }
    }
}