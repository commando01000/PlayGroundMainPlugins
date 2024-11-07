using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayGroundPlugins
{
    public abstract class BasePlugin : IPlugin
    {
        protected IServiceProvider _ServiceProvider { get; set; }
        protected ITracingService _Tracing { get; set; }
        protected IPluginExecutionContext PluginContext { get; set; }
        protected IOrganizationService _Service { get; set; }
        protected IPluginExecutionContext _Context { get; set; }
        protected Entity TargetEntity { get; set; }
        protected EntityReference TargetEntityRef { get; set; }
        protected string Relationship { get; set; }
        protected EntityReferenceCollection RelatedEntities { get; set; }

        public void Execute(IServiceProvider serviceProvider)
        {
            _Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            _ServiceProvider = serviceProvider;
            _Tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            PluginContext = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            _Service = serviceFactory.CreateOrganizationService(PluginContext.UserId);
            // associate & disassociate
            if (_Context.InputParameters.Contains("Relationship") && _Context.InputParameters["Relationship"] is Relationship)
            {
                Relationship = ((Relationship)_Context.InputParameters["Relationship"]).SchemaName;
            }
            if (_Context.InputParameters.Contains("RelatedEntities") && _Context.InputParameters["RelatedEntities"] is EntityReferenceCollection)
            {
                RelatedEntities = (EntityReferenceCollection)_Context.InputParameters["RelatedEntities"];
            }
            // update & create
            if (_Context.InputParameters.Contains("Target") && _Context.InputParameters["Target"] is Entity)
            {
                TargetEntity = (Entity)_Context.InputParameters["Target"];
                ExecutePluginLogic();
            }
            // Delete
            else if ((_Context.InputParameters.Contains("Target")) && (_Context.InputParameters["Target"] is EntityReference))
            {
                TargetEntityRef = (EntityReference)_Context.InputParameters["Target"];
                ExecutePluginLogic();
            }
        }
        protected virtual void ExecutePluginLogic()
        {
            throw new NotImplementedException();
        }
    }
}
