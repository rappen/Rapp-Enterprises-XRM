using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;

namespace Common
{
    public static class RappExtensions
    {
        public static Entity GetParent(this Entity child, RappContext rc, string lookup, params string[] columns)
        {
            if (child == null)
            {
                rc.Trace("Tried to get parent for null child");
                return null;
            }
            if (!child.Contains(lookup) || !(child[lookup] is EntityReference parentref))
            {
                rc.Trace($"Child {child.LogicalName} {child.Id} does not have a reference to {lookup}");
                return null;
            }
            return rc.Retrieve(parentref, columns);
        }

        public static IEnumerable<Entity> GetChildren(this Entity parent, RappContext rc, string child, string lookup, params string[] columns)
        {
            if (parent == null)
            {
                rc.Trace("Tried to get children for null parent");
                return null;
            }
            var qex = new QueryExpression(child);
            qex.ColumnSet = new ColumnSet(columns);
            qex.Criteria.AddCondition(lookup, ConditionOperator.Equal, parent.Id);
            return rc.RetrieveMultiple(qex).Entities;
        }
    }
}
