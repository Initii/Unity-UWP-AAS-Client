using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class ConceptDictionary
    {
        public string idShort;
        public string category;
        public List<LangString> description;
        public Dictionary<string, string> metaData;
        public List<ConceptDescriptionReference> conceptDescriptions;
        public ModelType modelType;
    }
}