using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class Asset
    {
        public string idShort;
        public string kind;
        public SubmodelReference assetIdentificationModel;
        public SubmodelReference billOfMaterial;
        public List<IEmbeddedDataSpecification> embeddedDataSpecifications;
        public ModelType modelType;
        public Identifier identification;
        public AdministrativeInformation administration;
        public string category;
        public List<LangString> description;
    }
}