using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public static class Extensions
    {
        public static Entity GetParent(this Entity entity, RappContext rc, string lookup, params string[] columns)
        {
            if (entity == null || !entity.TryGetAttributeValue(lookup, out EntityReference parentref))
            {
                return null;
            }
            return rc.Retrieve(parentref, columns);
        }

        public static IEnumerable<Entity> GetChildren(this Entity entity, RappContext rc, string chldrinname, string lookup, params string[] columns)
        {
            if (entity == null)
            {
                return null;
            }
            var query = new QueryExpression(chldrinname);
            query.ColumnSet.AddColumns(columns);
            query.Criteria.AddCondition(lookup, ConditionOperator.Equal, entity.Id);
            return rc.RetrieveMultiple(query).Entities;
        }
    }
}