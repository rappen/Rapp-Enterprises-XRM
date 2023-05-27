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
            if (entity == null)
            {
                return null;
            }
            if (entity.GetAttributeValue<EntityReference>(lookup) is EntityReference parent)
            {
                return rc.Retrieve(parent, columns);
            }
            return null;
        }

        public static EntityCollection GetChildren(this Entity parent, RappContext rc, string children, string lookup, params string[] columns)
        {
            if (parent == null)
            {
                return null;
            }
            var qry = new QueryExpression(children);
            qry.ColumnSet.AddColumns(columns);
            qry.Criteria.AddCondition(lookup, ConditionOperator.Equal, parent.Id);
            return rc.RetrieveMultiple(qry);
        }

        public static Entity Clone(this Entity entity, bool withattributes = false)
        {
            if (entity == null)
            {
                return null;
            }
            var result = new Entity(entity.LogicalName, entity.Id);
            if (withattributes)
            {
                result.Attributes.AddRange(entity.Attributes);
            }
            return result;
        }
    }
}