using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace Common
{
    public class RappContext : ITracingService, IOrganizationService
    {
        #region Private stuff

        private Lazy<IOrganizationService> _service;

        private ITracingService tracer;

        private IOrganizationService Service => _service.Value;

        #endregion Private stuff

        #region Pubilc properties

        public IPluginExecutionContext Context { get; private set; }

        public Entity Target => Context.InputParameterOrDefault<Entity>("Target");

        public Entity Pre => Context.PreEntityImages.Select(p => p.Value).FirstOrDefault();

        public Entity Post => Context.PostEntityImages.Select(p => p.Value).FirstOrDefault();

        public Entity Complete => getComplete();

        #endregion Pubilc properties

        #region Public Constructer

        public RappContext(IServiceProvider serviceProvider)
        {
            tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            _service = new Lazy<IOrganizationService>(() => factory.CreateOrganizationService(Context.UserId));
        }

        #endregion Public Constructer

        #region ITracingService implementation

        public void Trace(string format, params object[] args)
        {
            tracer.Trace(DateTime.Now + " " + format, args);
        }

        #endregion ITracingService implementation

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

        #region Private methods

        private Entity getComplete()
        {
            var result = new Entity(Target.LogicalName, Target.Id);
            result.Attributes.AddRange(Target.Attributes);
            if (Post != null)
            {
                result.Attributes.AddRange(Post.Attributes.Where(a => !result.Attributes.Contains
                (a.Key)));
            }
            if (Pre != null)
            {
                result.Attributes.AddRange(Pre.Attributes.Where(a => !result.Attributes.Contains
                (a.Key)));
            }
            return result;
        }

        #endregion Private methods
    }
}