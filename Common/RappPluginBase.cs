using Microsoft.Xrm.Sdk;
using Rappen.CDS.Canary;
using System;
using System.Linq;

namespace Common
{
    public abstract class RappPluginBase : IPlugin
    {

        public abstract string[] ExpectedMessages { get; }
        public abstract string ExpectedEntity { get; }

        public void Execute(IServiceProvider serviceProvider)
        {
            var rc = new RappContext(serviceProvider);
            rc.TraceContext(rc.Context, true, true, true, true, rc.Service);

            if (ExpectedMessages?.Length > 0 && !ExpectedMessages.Contains(rc.Context.MessageName))
            {
                rc.Trace($"Wrong message: {rc.Context.MessageName}");
                return;
            }
            if (!string.IsNullOrEmpty(ExpectedEntity) && !ExpectedEntity.Equals(rc.Context.PrimaryEntityName))
            {
                rc.Trace($"Wrong entity: {rc.Context.PrimaryEntityName}");
                return;
            }
            Execute(rc);
        }

        public abstract void Execute(RappContext rc);
    }
}
