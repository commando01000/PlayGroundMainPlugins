using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Linq;
using System;

namespace Wishlist_App_Plugins
{
    public static class XrmExtensions
    {
        public static Entity Fetch(this IOrganizationService _Service, string Query)
        {
            EntityCollection Result = _Service.RetrieveMultiple(new FetchExpression(Query));
            // return general settings
            if (Result.ContainsEntities())
                return Result.Entities.FirstOrDefault();
            return null;
        }

        public static DataCollection<Entity> FetchMultiple(this IOrganizationService _Service, string Query)
        {
            EntityCollection Result = _Service.RetrieveMultiple(new FetchExpression(Query));
            // return general settings
            if (Result.ContainsEntities())
                return Result.Entities;
            return null;
        }

        public static bool UpdateEntityStatus(this IOrganizationService _Service, EntityReference entityRef, string statusField, int statusValue)
        {
            if (entityRef != null && !string.IsNullOrEmpty(statusField))
            {
                Entity uEntity = entityRef.CloneIdentity();
                uEntity[statusField] = new OptionSetValue(statusValue);
                _Service.Update(uEntity);
                return true;
            }
            return false;
        }
        public static bool Check(this Entity Object, string PropertyName)
        {
            return Object.Contains(PropertyName) && Object[PropertyName] != null;
        }

        public static bool ContainsEntities(this EntityCollection Object)
        {
            return Object != null && Object.Entities != null && Object.Entities.Count > 0;
        }

        public static Entity CloneIdentity(this Entity Record)
        {
            return new Entity(Record.LogicalName, Record.Id);
        }

        public static Entity CloneIdentity(this EntityReference RecordReference)
        {
            return new Entity(RecordReference.LogicalName, RecordReference.Id);
        }

        public static DateTime? GetDateTime(this Entity Record, string PropertyName)
        {
            if (Record.Check(PropertyName))
                return (DateTime)Record[PropertyName];

            return null;
        }
        public static decimal GetMoneyValue(this Entity Record, string PropertyName)
        {
            if (Record.Check(PropertyName))
                return ((Money)Record[PropertyName]).Value;

            return 0m;
        }
        public static Money GetMoney(this Entity Record, string PropertyName)
        {
            if (Record.Check(PropertyName))
                return (Money)Record[PropertyName];

            return null;
        }
        public static decimal? GetDecimal(this Entity Record, string PropertyName)
        {
            if (Record.Check(PropertyName))
                return (decimal)Record[PropertyName];

            return null;
        }
        public static int? GetOptionSetValue(this Entity Record, string PropertyName)
        {
            if (Record.Check(PropertyName))
                return ((OptionSetValue)Record[PropertyName]).Value;

            return null;
        }
        public static EntityReference GetEntityReference(this Entity Record, string PropertyName)
        {
            if (Record.Check(PropertyName))
                return (EntityReference)Record[PropertyName];

            return null;
        }
        public static Guid? GetEntityReferenceId(this Entity Record, string PropertyName)
        {
            if (Record.Check(PropertyName))
                return ((EntityReference)Record[PropertyName]).Id;

            return null;
        }
        public static string GetEntityReferenceName(this Entity Record, string PropertyName)
        {
            if (Record.Check(PropertyName))
                return ((EntityReference)Record[PropertyName]).Name;

            return null;
        }
        public static T GetAliasedValue<T>(this Entity Record, string PropertyName)
        {
            if (Record.Check(PropertyName))
                return (T)((AliasedValue)Record[PropertyName]).Value;

            return default;
        }


    }
}
