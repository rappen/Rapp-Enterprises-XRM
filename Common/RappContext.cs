using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;

namespace Common
{
    public class RappContext : ITracingService, IOrganizationService
    {
        private Lazy<IOrganizationService> lazyservice;

        private ITracingService tracer;
        public IPluginExecutionContext context { get; private set; }
        private IOrganizationService Service => lazyservice.Value;

        public Entity Target => new Lazy<Entity>(() => context.InputParameters.TryGetValue("Target", out Entity target) ? target : null).Value;
        public Entity Pre => context.PreEntityImages.Select(i => i.Value).FirstOrDefault();
        public Entity Post => context.PostEntityImages.Select(i => i.Value).FirstOrDefault();
        public Entity Complete => getComplete();

        private Entity getComplete()
        {
            var result = new Entity(Target.LogicalName, Target.Id);
            result.Attributes.AddRange(Target.Attributes);
            if (Post != null)
            {
                result.Attributes.AddRange(Post.Attributes.Where(a => result.Attributes.Contains(a.Key)));
            }
            if (Pre != null)
            {
                result.Attributes.AddRange(Pre.Attributes.Where(a => result.Attributes.Contains(a.Key)));
            }
            return result;
        }

        public RappContext(IServiceProvider serviceProvider)
        {
            tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            lazyservice = new Lazy<IOrganizationService>(() => factory.CreateOrganizationService(context.UserId));
        }

        public void Trace(string format, params object[] args)
        {
            var msg = string.Format(format, args);
            tracer.Trace(DateTime.Now.ToString("T.fff  ") + msg);
        }

        #region IOrganizationService implementation

        public Guid Save(Entity entity)
        {
            if (entity.Id.Equals(Guid.Empty))
            {
                return Service.Create(entity);
            }
            else
            {
                Service.Update(entity);
                return entity.Id;
            }
        }

        [Obsolete("Use Save instead!")]
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

        [Obsolete("Use Save instead!")]
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