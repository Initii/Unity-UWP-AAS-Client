using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class SubmodelElement
    {
        public string idShort;
        public Reference semanticId;
        public List<IConstraint> qualifiers;
        public string kind;
        public ModelType modelType;
        public List<IEmbeddedDataSpecification> embeddedDataSpecifications;
        public string category;
        public List<LangString> description;
    }
}