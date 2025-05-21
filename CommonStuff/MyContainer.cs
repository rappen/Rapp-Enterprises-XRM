using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommonStuff
{
    public class MyContainer : ITracingService, IOrganizationService
    {
        private ITracingService tracer;
        private Lazy<IOrganizationService> service;

        public IPluginExecutionContext context { get; private set; }

        public MyContainer(IServiceProvider serviceProvider)
        {
            tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = new Lazy<IOrganizationService>(() => factory.CreateOrganizationService(context.UserId));
        }

        public void Trace(string format, params object[] args) => tracer.Trace(format, args);

        public Guid Create(Entity entity)
        {
            throw new NotImplementedException();
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            var sw = Stopwatch.StartNew();
            var account = service.Value.Retrieve(entityName, id, columnSet);
            sw.Stop();
            tracer.Trace($"Retrieved account: {account["name"]} in {sw.ElapsedMilliseconds} ms");
            return account;
        }

        public void Update(Entity entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(string entityName, Guid id)
        {
            throw new NotImplementedException();
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            throw new NotImplementedException();
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            throw new NotImplementedException();
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            throw new NotImplementedException();
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            throw new NotImplementedException();
        }
    }
}