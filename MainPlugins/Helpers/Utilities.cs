using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using PlayGroundPlugins.Helpers;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;

namespace Wishlist_App_Plugins
{
    public static class Utilities
    {
        internal static string ConverDateTimeToTimestamp(DateTime dateTime)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = dateTime - unixEpoch;
            long timestampInMilliseconds = (long)timeSpan.TotalMilliseconds;

            return timestampInMilliseconds.ToString();
        }

        internal static decimal FormatMoney(decimal fee)
        {
            string formattedFee = fee.ToString("0.00");
            decimal parsedFee = decimal.Parse(formattedFee);
            return parsedFee;
        }

        internal static string EscapeString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            return input.Replace("\"", "\\\"");
        }

        internal static DateTime ConvertToEgyptTimeZone(DateTime datetime)
        {
            return TimeZoneInfo.ConvertTime(datetime, TimeZoneInfo.FindSystemTimeZoneById(TimeZones.EgyptTimezone));
        }

        internal static string CalculateSHA256(string Signature)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(Signature);
                byte[] hash = sha256.ComputeHash(bytes);

                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < hash.Length; i++)
                {
                    stringBuilder.Append(hash[i].ToString("x2"));
                }
                return stringBuilder.ToString();
            }
        }

        internal static string RemovePrefix(string input, string prefix)
        {
            if (input.StartsWith(prefix))
            {
                // Remove the prefix
                return input.Substring(prefix.Length);
            }
            // If the string doesn't start with the specified prefix, return the original string
            return input;
        }

        internal static string GenerateRandomString(int length)
        {
            string charsSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(charsSet, length).Select(token => token[random.Next(token.Length)]).ToArray());
        }


        #region Auto Serial
        internal static int AutoSerial(EntityReference entityRef, string serialFieldName, IOrganizationService _service)
        {
            // get last serial
            Entity entity = _service.Retrieve(entityRef.LogicalName, entityRef.Id, new ColumnSet(serialFieldName));
            int lastSerial = entity.GetAttributeValue<int>(serialFieldName);

            // try update the new serial
            Entity uEntity = entity.CloneIdentity();
            uEntity["blser_lastwishlistserial"] = lastSerial + 1;
            uEntity.RowVersion = entity.RowVersion;
            try
            {
                // update request with custom concurrency behavior
                _service.Execute(new UpdateRequest()
                {
                    Target = uEntity,
                    ConcurrencyBehavior = ConcurrencyBehavior.IfRowVersionMatches
                });
                // return queue id
                return lastSerial + 1;
            }
            // if row version is different, repeat the operation again
            catch (FaultException<OrganizationServiceFault> ex)
            {
                switch (ex.Detail.ErrorCode)
                {
                    case -2147088254: // ConcurrencyVersionMismatch 
                    case -2147088253: // OptimisticConcurrencyNotEnabled 
                                      // re-call the function
                        return AutoSerial(entityRef, serialFieldName, _service);
                    case -2147088243: // ConcurrencyVersionNotProvided
                        throw new ArgumentNullException(ex.Detail.Message);
                    default:
                        throw ex;
                }
            }
        }
        #endregion

        #region General
        internal static void DeactivateRecord(EntityReference recordRef, IOrganizationService _service)
        {
            //StateCode = 1 and StatusCode = 2 for deactivating
            SetStateRequest setStateRequest = new SetStateRequest()
            {
                EntityMoniker = recordRef,
                State = new OptionSetValue(1),
                Status = new OptionSetValue(2)
            };
            _service.Execute(setStateRequest);
        }

        internal static void ShareRecord(IOrganizationService service, EntityReference principal, AccessRights access, EntityReference record)
        {
            PrincipalAccess principalAccess = new PrincipalAccess()
            {
                AccessMask = access,
                Principal = principal
            };
            GrantAccessRequest request = new GrantAccessRequest()
            {
                PrincipalAccess = principalAccess,
                Target = record
            };

            service.Execute(request);
        }
        #endregion
    }
}
