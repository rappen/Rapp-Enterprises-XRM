using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    public static class Extensions
    {
        public static Entity GetParent(this Entity child, RappContext rc, string lookup, params string[] columns)
        {
            if (child == null || !child.Contains(lookup))
            {
                return null;
            }
            var pareref = child[lookup] as EntityReference;
            return rc.Retrieve(pareref.LogicalName, pareref.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(columns));
        }

        public static IEnumerable<Entity> GetChildren(this Entity parent, RappContext rc, string child, string lookup, params string[] columns)
        {
            return GetChildren(parent, rc, child, lookup, true, null, null, columns);
        }

        public static IEnumerable<Entity> GetChildren(this Entity parent, RappContext rc, string child, string lookup, bool activeonly, FilterExpression filter, IEnumerable<OrderExpression> orders, params string[] columns)
        {
            if (parent == null)
            {
                return default(IEnumerable<Entity>);
            }
            var qry = new QueryExpression(child);
            qry.ColumnSet = new ColumnSet(columns);
            qry.Criteria.AddCondition(lookup, ConditionOperator.Equal, parent.Id);
            if (activeonly)
            {
                qry.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            }
            if (filter != null)
            {
                qry.Criteria.AddFilter(filter);
            }
            if (orders?.Count() > 0)
            {
                orders.ToList().ForEach(o => qry.AddOrder(o.AttributeName, o.OrderType));
            }
            return rc.RetrieveMultiple(qry).Entities;
        }
    }
}
