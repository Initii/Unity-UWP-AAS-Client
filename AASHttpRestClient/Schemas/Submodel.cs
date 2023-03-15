using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class Submodel
    {
        public string idShort;
        public string kind;
        public Reference semanticId;
        public Dictionary<string, ISubmodelElement> submodelElements;
        public ModelType modelType;
        public List<IEmbeddedDataSpecification> embeddedDataSpecifications;
        public List<IConstraint> qualifiers;
        public Identifier identification;
        public AdministrativeInformation administration;
        public string category;
        public List<LangString> description;
    }
}