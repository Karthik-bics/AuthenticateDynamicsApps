using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.Crm.Sdk.Messages;
//using Microsoft.Xrm.Tooling.Connector;
using Microsoft.Xrm.Sdk.Query;
using System.Workflow.Runtime.Tracking;
using System.Web.Services.Description;
using System.Net.PeerToPeer;
//Microsoft.CrmSdk.XrmTooling.CoreAssembly
namespace AuthenticateDynamicsApps
{
    public class autheticateApp : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            ITracingService tracingService =
                (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService _service = factory.CreateOrganizationService(context.UserId);


            tracingService.Trace("autheticateApp started...");
            string userName = (string)context.InputParameters["portalUserName"];
            tracingService.Trace("UserName:" + userName);
            string password = (string)context.InputParameters["portalPassword"];
            tracingService.Trace("Password:" + password);
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
            {
                //var checkAuthentication = CheckAutherization(context,userName, password);
                var userId = GetUserIdByName(_service, userName);
                tracingService.Trace("userId:" + userId);
                if (userId == Guid.Empty)
                {

                    tracingService.Trace("checkAuthentication failed due to some error");
                    context.OutputParameters["response"] = "User is not availble in CRM application";
                    context.OutputParameters["Authenticated"] = false;
                }
                else
                {
                    IOrganizationService userService = factory.CreateOrganizationService(userId);
                    //context.UserId = userId;
                    Entity createContact = new Entity("new_test");
                    createContact["new_name"] = "Test from custom API";
                    userService.Create(createContact);

                    tracingService.Trace("checkAuthentication verified");
                    context.OutputParameters["response"] = "User is availble in CRM application";
                    context.OutputParameters["Authenticated"] = true;
                }
            }
            else
            {
                tracingService.Trace("checkAuthentication failed due to some error");
                context.OutputParameters["response"] = "UserName and Password is mandatory";
                context.OutputParameters["Authenticated"] = false;
            }
        }
        //public WhoAmIResponse CheckAutherization(string UName, string Pwd)
        //{
        //    WhoAmIResponse response = null;
        //    return response;
        //}
        //public WhoAmIResponse CheckAutherization(string UName, string Pwd)
        //{

        //    string Url = "";
        //    WhoAmIResponse response = null;
        //    string sEnvironment = Url;
        //    string sUserKey = "";
        //    string sUserPassword = "";
        //    //Step 2- Creating A Connection String.
        //    string conn = $@" Url = {sEnvironment};AuthType = OAuth;UserName = {sUserKey}; Password = {sUserPassword};AppId = 019319f4-9a25-45f9-b12a-bef23cd3e617;RedirectUri = app://58145B91-0C36-4500-8554-080854F2AC97;LoginPrompt=Auto; RequireNewInstance = True";
        //    //Step 3 - Obtaining CRM Service.
        //    using (var service = new CrmServiceClient(conn))
        //    {
        //        if (service != null)
        //        {
        //            WhoAmIRequest req = new WhoAmIRequest();
        //            response = (WhoAmIResponse)service.Execute(req);
        //        }
        //    }
        //    return response;
        //}

        public Guid GetUserIdByName(IOrganizationService _service, string userName)
        {
            Guid userId = Guid.Empty;
            var fetchData = new
            {
                domainname = userName
            };
            var fetchXml = $@"<?xml version=""1.0"" encoding=""utf-16""?>
<fetch version=""1.0"" output-format=""xml-platform"" mapping=""logical"" distinct=""false"">
  <entity name=""systemuser"">
    <attribute name=""fullname"" />
    <attribute name=""businessunitid"" />
    <attribute name=""title"" />
    <attribute name=""address1_telephone1"" />
    <attribute name=""positionid"" />
    <attribute name=""systemuserid"" />
    <attribute name=""domainname"" />
    <order attribute=""fullname"" descending=""false"" />
    <filter type=""and"">
      <condition attribute=""domainname"" operator=""eq"" value=""{fetchData.domainname/*alans@CRM539616.OnMicrosoft.com*/}"" />
    </filter>
  </entity>
</fetch>";


            var fe = new FetchExpression(fetchXml);
            var userRecord = _service.RetrieveMultiple(fe).Entities.FirstOrDefault();
            if (userRecord != null)
            {
                userId = userRecord.Id;
            }

            return userId;
        }
    }
}
