using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rappen.CDS.Canary;
using System;
using System.Linq;

namespace Common
{
    public class RappContext: ITracingService, IOrganizationService
    {
        private Lazy<ITracingService> tracer;
        private Lazy<IPluginExecutionContext> context;
        private Lazy<IOrganizationService> service;

        public ITracingService Tracer => tracer.Value;
        public IPluginExecutionContext Context => context.Value;
        public IOrganizationService Service => service.Value;

        public Entity Target => new Lazy<Entity>(() => getTarget()).Value;
        public Entity PreImage => new Lazy<Entity>(() => getImage(Context.PreEntityImages)).Value;
        public Entity PostImage => new Lazy<Entity>(() => getImage(Context.PostEntityImages)).Value;

        private Entity getTarget()
        {
            return Context.InputParameters.Contains("Target") && Context.InputParameters["Target"] is Entity result ? result : null;
        }

        private Entity getImage(EntityImageCollection images)
        {
            return images.Count > 0 ? images.First().Value : null;
        }

        public RappContext(IServiceProvider serviceProvider)
        {
            tracer = new Lazy<ITracingService>(() => (ITracingService)serviceProvider.GetService(typeof(ITracingService)));
            context = new Lazy<IPluginExecutionContext>(() => (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext)));
            service = new Lazy<IOrganizationService>(() => ((IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory))).CreateOrganizationService(Context.UserId));
        }

        public T GetAttributeValue<T>(string attribute, T defaultvalue)
        {
            if (Target?.Contains(attribute) == true)
            {
                return (T)Target[attribute];
            }
            if (PostImage?.Contains(attribute)==true)
            {
                return (T)PostImage[attribute];
            }
            if (PreImage?.Contains(attribute) == true)
            {
                return (T)PreImage[attribute];
            }
            return defaultvalue;
        }

        public void Trace(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            Tracer.Trace("{0}  {1}", DateTime.Now.ToString("HH:mm:ss:fff"), msg);
        }

        #region IOrganizationService implementation

        public Guid Create(Entity entity)
        {
            Trace($"Creating {entity.LogicalName} with {entity.Attributes.Count} attributes");
            var result = Service.Create(entity);
            Trace("Created!");
            return result;
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

        public void Update(Entity entity)
        {
            Trace($"Updating {entity.LogicalName} with {entity.Attributes.Count} attributes");
            Service.Update(entity);
            Trace("Updated!");
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
