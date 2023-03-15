using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class AssetAdministrationShellDescriptorRegistry
    {
        public AdministrativeInformation administration;
        public List<LangString> description;
        public List<Endpoint> endpoints;
        public Reference globalAssetId;
        public string idShort;
        public string identification;
        public List<IdentifierKeyValuePair> specificAssetIds;
        public List<SubmodelDescriptorRegistry> submodelDescriptors;
    }
}