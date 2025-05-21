using Microsoft.Xrm.Sdk;
using Rappen.Dataverse.Canary;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonStuff
{
    public abstract class MyPluginBase : IPlugin
    {
        public abstract string ExpectedEntity { get; }
        public abstract string[] ExpectedMessage { get; }
        public MyContainer MyContainer { get; private set; }

        public void Execute(IServiceProvider serviceProvider)
        {
            serviceProvider.TraceContext();
            MyContainer = new MyContainer(serviceProvider);
            if (!Valid())
            {
                return;
            }

            Execute();
        }

        private bool Valid()
        {
            if (!string.IsNullOrEmpty(ExpectedEntity) && MyContainer.context.PrimaryEntityName != ExpectedEntity)
            {
                MyContainer.Trace($"Wrong entity: {MyContainer.context.PrimaryEntityName} != {ExpectedEntity}");
                return false;
            }
            if (ExpectedMessage != null && !Array.Exists(ExpectedMessage, m => m == MyContainer.context.MessageName))
            {
                MyContainer.Trace($"Wrong message: {MyContainer.context.MessageName} != {string.Join(", ", ExpectedMessage)}");
                return false;
            }
            return true;
        }

        public abstract void Execute();
    }
}