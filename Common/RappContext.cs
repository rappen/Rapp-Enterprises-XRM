using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rappen.CDS.Canary;
using System;
using System.Linq;

namespace Common
{
    public class RappContext : ITracingService, IOrganizationService
    {
        private ITracingService tracer;
        private Lazy<IPluginExecutionContext> context;
        private IOrganizationService service;

        public IPluginExecutionContext Context => context.Value;
        public Entity Target => new Lazy<Entity>(() => getTarget()).Value;
        public Entity PreImage => new Lazy<Entity>(() => getImage(Context.PreEntityImages)).Value;
        public Entity PostImage => new Lazy<Entity>(() => getImage(Context.PostEntityImages)).Value;

        public RappContext(IServiceProvider serviceProvider)
        {
            tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = new Lazy<IPluginExecutionContext>(() => (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            service = new Lazy<IOrganizationService>(() => factory.CreateOrganizationService(Context.UserId)).Value;
            tracer.TraceContext(Context, true, true, true, true, service);
        }

        public T GetAttributeValue<T>(string attributename, T defaultvalue)
        {
            if (Target?.Contains(attributename) == true)
            {
                return (T)Target[attributename];
            }
            if (PostImage?.Contains(attributename) == true)
            {
                return (T)PostImage[attributename];
            }
            if (PreImage?.Contains(attributename) == true)
            {
                return (T)PreImage[attributename];
            }
            return defaultvalue;
        }

        #region ITracingService implementation

        public void Trace(string format, params object[] args)
        {
            var now = DateTime.Now.ToString("HH:mm:ss.fff");
            var msg = string.Format(format, args);
            tracer.Trace(now + "  " + msg);
        }

        #endregion ITracingService implementation

        #region IOrganizationService implementation

        public Guid Create(Entity entity)
        {
            Trace($"Creating {entity.LogicalName} with {entity.Attributes.Count} attributes");
            var result = service.Create(entity);
            Trace("Created!");
            return result;
        }

        public Entity Retrieve(string entityName, Guid id, ColumnSet columnSet)
        {
            Trace($"Retrieving {entityName} {id} with {columnSet.Columns.Count} attributes");
            var result = service.Retrieve(entityName, id, columnSet);
            Trace("Retrieved!");
            return result;
        }

        public Entity Retrieve(EntityReference reference, params string[] columns)
        {
            return Retrieve(reference.LogicalName, reference.Id, new ColumnSet(columns));
        }

        public void Update(Entity entity)
        {
            Trace($"Updating {entity.LogicalName} with {entity.Attributes.Count} attributes");
            service.Update(entity);
            Trace("Updated!");
        }

        public void Delete(string entityName, Guid id)
        {
            Trace($"Deletng {entityName} {id}");
            service.Delete(entityName, id);
            Trace("Deleted!");
        }

        public void Delete(EntityReference reference)
        {
            Delete(reference.LogicalName, reference.Id);
        }

        public OrganizationResponse Execute(OrganizationRequest request)
        {
            Trace($"Executing {request}");
            var result = service.Execute(request);
            Trace("Executed!");
            return result;
        }

        public void Associate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Trace($"Associating {entityName} {entityId} over {relationship.SchemaName} with {relatedEntities.Count} {string.Join(", ", relatedEntities.Select(r => r.LogicalName))}");
            service.Associate(entityName, entityId, relationship, relatedEntities);
            Trace("Associated!");
        }

        public void Disassociate(string entityName, Guid entityId, Relationship relationship, EntityReferenceCollection relatedEntities)
        {
            Trace($"Disassociating {entityName} {entityId} over {relationship.SchemaName} with {relatedEntities.Count} {string.Join(", ", relatedEntities.Select(r => r.LogicalName))}");
            service.Disassociate(entityName, entityId, relationship, relatedEntities);
            Trace("Disassociated!");
        }

        public EntityCollection RetrieveMultiple(QueryBase query)
        {
            Trace($"Retrieving with {query}");
            var result = service.RetrieveMultiple(query);
            Trace($"Retrieved {result.Entities.Count} {result.EntityName}");
            return result;
        }

        #endregion IOrganizationService implementation

        private Entity getImage(EntityImageCollection images)
        {
            return images.Count > 0 ? images.First().Value : null;
        }

        private Entity getTarget()
        {
            if (Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is Entity result)
            {
                return result;
            }
            return null;
        }
    }
}
