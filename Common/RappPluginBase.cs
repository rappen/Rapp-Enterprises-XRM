using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace Common
{
    public abstract class RappPluginBase : IPlugin
    {
        public abstract string ExpectedEntity { get; }
        public abstract string[] ExpectedMessages { get; }

        public void Execute(IServiceProvider serviceProvider)
        {
            var rc = new RappContext(serviceProvider);
            if (!ExpectedMessages.Contains(rc.Context.MessageName))
            {
                rc.Trace($"Wrong message: {rc.Context.MessageName}");
                return;
            }
            if (ExpectedEntity != rc.Context.PrimaryEntityName)
            {
                rc.Trace($"Wrong entity: {rc.Context.PrimaryEntityName}");
                return;
            }

            Execute(rc);
        }

        public abstract void Execute(RappContext rc);
    }
}