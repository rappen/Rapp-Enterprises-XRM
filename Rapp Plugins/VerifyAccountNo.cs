using Common;
using Microsoft.Xrm.Sdk;
using System.Linq;

namespace Rapp_Plugins
{
    public class VerifyAccountNo : JRPlugin
    {
        public override string TriggerEntity => "account";

        public override string[] TriggerMessages => new string[] { "Create" };

        public override int ExecutionOrder => 10;

        public override void Execute()
        {
            var accountnumber = complete.GetAttributeValue<string>("accountnumber");

            if (accountnumber.Any(c => !char.IsDigit(c)))
            {
                throw new InvalidPluginExecutionException($"Account Number must be numeric. ({accountnumber})");
            }

            Trace($"Account Number is numeric: {accountnumber}");
        }
    }
}