using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class AssetAdministrationShell
    {
        public string idShort;
        public Asset asset;
        public List<Submodel> submodels;
        public AssetAdministrationShellReference derivedFrom;
        public List<View> views;
        public ModelType modelType;
        public List<IEmbeddedDataSpecification> embeddedDataSpecifications;
        public List<ConceptDictionary> conceptDictionaries;
        public Identifier identification;
        public AdministrativeInformation administration;
        public string category;
        public List<LangString> description;
    }
}