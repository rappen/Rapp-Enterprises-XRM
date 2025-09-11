using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Common
{
    public static class JRExt
    {
        public static Entity GetParent(this Entity source, JRPlugin jr, string lookup, params string[] columns)
        {
            if (source == null || jr == null || !source.Contains(lookup))
            {
                jr.Trace("GetParent: source is null or does not contain the lookup.");
                return null;
            }
            var reference = source[lookup] as EntityReference;
            if (reference == null || reference.Id == Guid.Empty)
            {
                jr.Trace("GetParent: reference is null or has empty Guid.");
                return null;
            }
            var columnSet = new ColumnSet(columns);
            return jr.Retrieve(reference.LogicalName, reference.Id, columnSet);
        }

        public static EntityCollection GetChildren(this Entity source, JRPlugin jr, string entityName, string lookup, bool onlyactive, params string[] columns)
        {
            if (source == null || jr == null || source.Id == Guid.Empty)
            {
                jr.Trace("GetChildren: source is null or has empty Guid.");
                return null;
            }
            var query = new QueryExpression(entityName);
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition(lookup, ConditionOperator.Equal, source.Id);
            if (onlyactive)
            {
                jr.Trace("GetChildren: filtering only active records.");
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            }
            return jr.RetrieveMultiple(query);
        }

        public static Entity Vanilla(this Entity source) => new Entity(source.LogicalName, source.Id);

        public static Entity Merge(this Entity e1, Entity e2)
        {
            if (e1 == null)
            {
                return e2;
            }
            if (e2 == null)
            {
                return e1;
            }
            var merged = e1.Vanilla();
            foreach (var attr in e1.Attributes)
            {
                merged[attr.Key] = attr.Value;
            }
            foreach (var attr in e2.Attributes)
            {
                if (!merged.Contains(attr.Key))
                {
                    merged[attr.Key] = attr.Value;
                }
            }
            return merged;
        }
    }
}