using Microsoft.Xrm.Sdk;
using Rappen.CDS.Canary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public abstract class RappPluginBase : IPlugin
    {
        public abstract string ExpectedEntity { get; }
        public abstract string[] ExpectedMessages { get; }

        public void Execute(IServiceProvider serviceProvider)
        {
            var rc = new RappContext(serviceProvider);
            rc.TraceContext(rc.context);

            if (!string.IsNullOrEmpty(ExpectedEntity) && ExpectedEntity != rc.context.PrimaryEntityName)
            {
                rc.Trace($"Wrong entity: {rc.context.PrimaryEntityName}");
                return;
            }
            if (ExpectedMessages?.Length > 0 && !ExpectedMessages.Contains(rc.context.MessageName))
            {
                rc.Trace($"Wrong message: {rc.context.MessageName}");
                return;
            }

            Execute(rc);
        }

        public abstract void Execute(RappContext rc);
    }
}