using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using PlayGroundPlugins.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Runtime.Remoting.Services;
using System.Text;
using System.Threading.Tasks;
using Wishlist_App_Plugins;

namespace PlayGroundPlugins.Plugins
{
    public class StockMovementPlugin : BasePlugin
    {
        protected override void ExecutePluginLogic()
        {
            _Tracing.Trace("Starting StockMovements plugin...");

            // Get the Movement Type and Quantity fields, with null checks
            var movementTypeOption = TargetEntity.GetAttributeValue<OptionSetValue>("initiumc_movementtype");
            if (movementTypeOption == null)
            {
                throw new InvalidPluginExecutionException("Movement Type is missing.");
            }
            int movementType = movementTypeOption.Value;
            _Tracing.Trace("Movement Type retrieved: " + movementType);

            int stockMovQuantity = TargetEntity.GetAttributeValue<int?>("initiumc_quantity") ?? 0;
            _Tracing.Trace("Quantity retrieved: " + stockMovQuantity);

            // Ensure the Product reference is not null
            //var productRef = TargetEntity.GetAttributeValue<EntityReference>("initiumc_relatedproduct");





            var productRef = XrmExtensions.GetEntityReference(TargetEntity, "initiumc_relatedproduct");
            _Tracing.Trace("Product reference retrieved: " + productRef.Id);

            if (productRef == null)
            {
                throw new InvalidPluginExecutionException("Product reference is missing in Stock Movement.");
            }

            var query = $@"
                <fetch top='1'>
                  <entity name='initiumc_product_ss'>
                    <attribute name='initiumc_currentstock' />
                    <filter>
                      <condition attribute='initiumc_product_ssid' operator='eq' value='{productRef.Id}' />
                    </filter>
                  </entity>
                </fetch>";

            var product = XrmExtensions.Fetch(_Service, query);

            _Tracing.Trace("Product entity retrieved successfully.");

            // Attempt to retrieve the Product entity
            if (product == null)
            {
                throw new InvalidPluginExecutionException("Failed to retrieve Product entity. Product may not exist.");
            }

            // Check for initiumc_currentstock field, set to 0 if not present
            int currentStock = product.Contains("initiumc_currentstock") && product["initiumc_currentstock"] != null
                ? product.GetAttributeValue<int>("initiumc_currentstock")
                : 0;
            _Tracing.Trace("Current Stock retrieved: " + currentStock);

            if (MovementTypes.In == movementType) // "In" movement
            {
                int newStock = currentStock + stockMovQuantity;
                product["initiumc_currentstock"] = newStock;
                _Tracing.Trace("Updated stock for 'In' movement: " + newStock);
                _Service.Update(product);
            }
            else if (MovementTypes.Out == movementType) // "Out" movement
            {
                if (currentStock < stockMovQuantity)
                {
                    throw new InvalidPluginExecutionException("Insufficient stock available. This movement would result in a negative stock level.");
                }

                int newStock = currentStock - stockMovQuantity;
                product["initiumc_currentstock"] = newStock;
                _Tracing.Trace("Updated stock for 'Out' movement: " + newStock);
                _Service.Update(product);
            }
        }
    }
}
