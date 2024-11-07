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
    /// <summary>
    /// This plugin handles stock movements by updating the current stock level of a product.
    /// </summary>
    public class StockMovementPlugin : BasePlugin
    {
        /// <summary>
        /// Executes the plugin logic to handle stock movements.
        /// </summary>
        protected override void ExecutePluginLogic()
        {
            // Log a message to indicate the start of the plugin execution
            _Tracing.Trace("Starting StockMovements plugin...");

            // Get the Movement Type and Quantity fields from the target entity
            var movementTypeOption = TargetEntity.GetAttributeValue<OptionSetValue>("initiumc_movementtype");
            if (movementTypeOption == null)
            {
                // Throw an exception if the Movement Type is missing
                throw new InvalidPluginExecutionException("Movement Type is missing.");
            }
            int movementType = movementTypeOption.Value;
            _Tracing.Trace("Movement Type retrieved: " + movementType);

            // Get the Quantity field from the target entity, defaulting to 0 if null
            int stockMovQuantity = TargetEntity.GetAttributeValue<int?>("initiumc_quantity") ?? 0;
            _Tracing.Trace("Quantity retrieved: " + stockMovQuantity);

            // Ensure the Product reference is not null
            var productRef = XrmExtensions.GetEntityReference(TargetEntity, "initiumc_relatedproduct");
            _Tracing.Trace("Product reference retrieved: " + productRef.Id);

            if (productRef == null)
            {
                // Throw an exception if the Product reference is missing
                throw new InvalidPluginExecutionException("Product reference is missing in Stock Movement.");
            }

            // Construct a fetch query to retrieve the product entity
            var query = $@"
                <fetch top='1'>
                  <entity name='initiumc_product_ss'>
                    <attribute name='initiumc_currentstock' />
                    <filter>
                      <condition attribute='initiumc_product_ssid' operator='eq' value='{productRef.Id}' />
                    </filter>
                  </entity>
                </fetch>";

            // Execute the fetch query to retrieve the product entity
            var product = XrmExtensions.Fetch(_Service, query);

            _Tracing.Trace("Product entity retrieved successfully.");

            // Attempt to retrieve the Product entity
            if (product == null)
            {
                // Throw an exception if the product entity is not found
                throw new InvalidPluginExecutionException("Failed to retrieve Product entity. Product may not exist.");
            }

            // Check for the initiumc_currentstock field, defaulting to 0 if not present
            int currentStock = product.Contains("initiumc_currentstock") && product["initiumc_currentstock"] != null
                ? product.GetAttributeValue<int>("initiumc_currentstock") : 0;
            _Tracing.Trace("Current Stock retrieved: " + currentStock);

            // Handle the stock movement based on the movement type
            if (MovementTypes.In == movementType) // "In" movement
            {
                // Update the stock level by adding the movement quantity
                int newStock = currentStock + stockMovQuantity;
                product["initiumc_currentstock"] = newStock;
                _Tracing.Trace("Updated stock for 'In' movement: " + newStock);
                _Service.Update(product);
            }
            else if (MovementTypes.Out == movementType) // "Out" movement
            {
                // Check if the current stock level is sufficient for the movement
                if (currentStock < stockMovQuantity)
                {
                    // Throw an exception if the stock level is insufficient
                    throw new InvalidPluginExecutionException("Insufficient stock available. This movement would result in a negative stock level.");
                }

                // Update the stock level by subtracting the movement quantity
                int newStock = currentStock - stockMovQuantity;
                product["initiumc_currentstock"] = newStock;
                _Tracing.Trace("Updated stock for 'Out' movement: " + newStock);
                _Service.Update(product);
            }
        }
    }
}