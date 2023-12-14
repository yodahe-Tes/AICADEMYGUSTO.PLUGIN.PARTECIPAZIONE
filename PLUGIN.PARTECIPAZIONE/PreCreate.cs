using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.ComponentModel;
using System.Runtime.Remoting.Services;

namespace PLUGIN.PARTECIPAZIONE


{
    public class PreCreate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity target = (Entity)context.InputParameters["Target"];

                // Verifica se il record è di tipo Partecipazione 
                if (target.LogicalName == "aicgusto_partecipazione")
                {
                    // Verifica se il cliente ha già una partecipazione nel mese corrente
                    if (VerificaPartecipazione(service, target))
                    {
                        //se return vero( quindi "controllo>0"), impedisci di salvare e spara messaggio
                        throw new InvalidPluginExecutionException("Il cliente ha già una partecipazione nel mese corrente. La creazione della partecipazione è negata.");
                    }
                }
            }




            bool VerificaPartecipazione(IOrganizationService service1, Entity target)
            {
                // Ottenere l'ID del cliente dalla partecipazione
                //Guid clienteId = target.GetAttributeValue<E>;

                // Ottenere la data corrente

                // Costruire una query FetchXML per verificare se il cliente ha già una partecipazione nel mese corrente
                string fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                                        "  <entity name='aicgusto_partecipazione'>" +
                                        "    <attribute name='aicgusto_partecipazioneid' />" +
                                        "    <attribute name='aicgusto_name' />" +
                                        "    <attribute name='createdon' />" +
                                        "    <order attribute='aicgusto_name' descending='false' />" +
                                        "    <filter type='and'>" +
                                        "      <condition attribute='aicgusto_contatto' operator='eq'  uitype='contact' value='" + target.GetAttributeValue<EntityReference>("aicgusto_contatto").Id + "'/>" +
                                        "    </filter>" +
                                        "    <link-entity name='aicgusto_evento' from='aicgusto_eventoid' to='aicgusto_evento' link-type='inner' alias='ab'>" +
                                        "      <filter type='and'>" +
                                        "        <condition attribute='aicgusto_data' operator='this-month' />" +
                                        "      </filter>" +
                                        "    </link-entity>" +
                                        "  </entity>" +
                                        "</fetch>";
                tracingService.Trace($"dopo  fetch {fetchXml} \n   ");

                // Faccio la retrive di ciò che ho impostato qui sopra(cliente ha N partecipazioni)
                // Nella collection controllo
                EntityCollection controllo = service.RetrieveMultiple(new FetchExpression(fetchXml));

                if (controllo != null && controllo.Entities.Count > 0)
                {
                    return true;

                }

                //se avrà "catturato", return true, se è 0, return false

                return false;
            }
        }
    }
}
