using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class SubmodelDescriptorServer
    {
        public string idShort;
        public Identifier identification;
        public AdministrativeInformation administration;
        public List<LangString> description;
        public Reference semanticId;
        public IEndpoint endpoints;
        public string category;
        public ModelType modelType;
    }
}