using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class View
    {
        public string idShort;
        public List<Reference> containedElements;
        public Reference semanticId;
        public string category;
        public List<LangString> description;
        public ModelType modelType;
    }
}