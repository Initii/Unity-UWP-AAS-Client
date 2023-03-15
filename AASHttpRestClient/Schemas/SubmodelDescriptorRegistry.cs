using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class SubmodelDescriptorRegistry
    {
        public AdministrativeInformation administration;
        public List<LangString> description;
        public List<Endpoint> endpoints;
        public string idShort;
        public string identification;
        public Reference semanticId;
    }
}