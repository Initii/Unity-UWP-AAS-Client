using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class ISubmodelElement
    {
        public string idShort;
        public Reference semanticId;
        public List<IConstraint> constraints;
        public List<LangString> description;
        public string category;
        public string kind;
        public ModelType modelType;
        public List<IEmbeddedDataSpecification> embeddedDataSpecifications;
    }
}
