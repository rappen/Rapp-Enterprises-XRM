using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using Rappen.Dataverse.Canary;
using System;
using System.Diagnostics;
using System.Linq;

namespace Common
{
    public abstract class JRPlugin : IPlugin, ITracingService, IOrganizationService
    {
        private ITracingService tracer;
        private IOrganizationService Service;

        public IPluginExecutionContext context { get; private set; }
        public Entity target { get; private set; }
        public Entity preImage { get; private set; }
        public Entity postImage { get; private set; }
        public Entity complete => postImage.Merge(target).Merge(preImage);

        public abstract string TriggerEntity { get; }
        public abstract string[] TriggerMessages { get; }
        public virtual int ExecutionOrder { get; }

        public void Execute(IServiceProvider serviceProvider)
        {
            serviceProvider.TraceContext();

            tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            Service = factory.CreateOrganizationService(context.UserId);

            if (context.PrimaryEntityName != TriggerEntity)
            {
                tracer.Trace($"Wrong entity: {context.PrimaryEntityName}");
                return;
            }
            if (Array.IndexOf(TriggerMessages, context.MessageName) < 0)
            {
                tracer.Trace($"Wrong message: {context.MessageName}");
                return;
            }
            if (ExecutionOrder != 0 && context.Stage != ExecutionOrder)
            {
                tracer.Trace($"Wrong stage: {context.Stage}");
                return;
            }
            target = context.InputParameterOrDefault<Entity>("Target");
            preImage = context.PreEntityImages.FirstOrDefault().Value;
            postImage = context.PostEntityImages.FirstOrDefault().Value;

            if (context.MessageName != "Delete" && target == null)
            {
                tracer.Trace("Target is null.");
                return;
            }

            try
            {
                Execute();
            }
            catch (Exception ex)
            {
                tracer.Trace("Exception: {0}", ex.ToString());
                throw new InvalidPluginExecutionException(ex.Message);
            }
        }

        public abstract void Execute();

        #region Trace

        public void Trace(string format, params object[] args) => tracer.Trace(format, args);

        #endregion Trace

        #region IOrganizationService implementation

        public Guid Create(Entity entity)
        {
            Trace($"Creating {entity.LogicalName} with {entity.Attributes.Count} attributes");
            var result = Service.Create(entity);
            Trace("Created!");
            return result;
        }
        public void Update(Entity entity)
        {
            Trace($"Updating {entity.LogicalName} with {entity.Attributes.Count} attributes");
            Service.Update(entity);
            Trace("Updated!");
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            Trace($"Retrieving {entityName} {id} with {columnSet.Columns.Count} attributes");
            var result = Service.Retrieve(entityName, id, columnSet);
            Trace("Retrieved!");
            return result;
        }

        public Entity Retrieve(EntityReference reference, params string[] columns)
        {
            return Retrieve(reference.LogicalName, reference.Id, new ColumnSet(columns));
        }

        public void Delete(string entityName, Guid id)
        {
            Trace($"Deletng {entityName} {id}");
            Service.Delete(entityName, id);
            Trace("Deleted!");
        }

        public void Delete(EntityReference reference)
        {
            Delete(reference.LogicalName, reference.Id);
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            Trace($"Executing {request}");
            var result = Service.Execute(request);
            Trace("Executed!");
            return result;
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Trace($"Associating {entityName} {entityId} over {relationship.SchemaName} with {relatedEntities.Count} {string.Join(", ", relatedEntities.Select(r => r.LogicalName))}");
            Service.Associate(entityName, entityId, relationship, relatedEntities);
            Trace("Associated!");
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Trace($"Disassociating {entityName} {entityId} over {relationship.SchemaName} with {relatedEntities.Count} {string.Join(", ", relatedEntities.Select(r => r.LogicalName))}");
            Service.Disassociate(entityName, entityId, relationship, relatedEntities);
            Trace("Disassociated!");
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            Trace($"Retrieving with {query}");
            var result = Service.RetrieveMultiple(query);
            Trace($"Retrieved {result.Entities.Count} {result.EntityName}");
            return result;
        }

        #endregion IOrganizationService implementation
    }
}