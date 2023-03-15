using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class AssetAdministrationShellDescriptorServer
    {
        public string idShort;
        public Identifier identification;
        public AdministrativeInformation administration;
        public List<LangString> description;
        public List<IEndpoint> endpoints;
        public string category;
        public ModelType modelType;
        public Asset asset;
        public List<SubmodelDescriptorServer> submodels;
    }
}
