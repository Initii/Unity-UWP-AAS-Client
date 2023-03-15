using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace AASHttpRestClient
{
    public class AASHttpRestClient
    {
        // ================================================================================
        // CONSTANTS
        // ================================================================================
        private const string REGEX_URL_HOST_WITH_PORT_SEARCH_PATTERN = "^http://([a-z0-9\\-._~%])+(:[0-9]+)*";
        private const string GET_REGISTRY_SHELL_DESCRIPTORS_PATH = "/registry/shell-descriptors";
        private const string AAS_SERVER_SHELLS_PREFIX = "shells/";
        private const int AAS_SERVER_SHELLS_AAS_LENGTH = 11;
        private const string REGEX_BASE_64_ENCODED_IDENTIFICATION_PATTERN = "shells\\/(.)+\\/aas";
        private const string GET_SERVER_AAS_SUBMODELS = "/aas/submodels/";
        private const string GET_SERVER_AAS_SUBMODELELEMENTS = "/submodel/submodelElements/";
        private const string GET_SERVER_AAS_VALUE = "/value";
        // END OF CONSTANTS

        // ================================================================================
        // ENUMS
        // ================================================================================
        // END OF ENUMS

        // ================================================================================
        // CLASS VARIABLES
        // ================================================================================
        public delegate void _OnResultListSubmodel(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status, string id, List<Submodel> result);
        public delegate void _OnResultListSubmodelElement(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status, string id, List<SubmodelElement> result);
        public delegate void _OnResultSubmodelElement(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status, string id, SubmodelElement result);
        public delegate void _OnResultString(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status, string id, string result);
        public BaseHttpRestClient.BaseHttpRestClient._OnInitializeCompleted OnInitializeCompleted;

        private BaseHttpRestClient.BaseHttpRestClient _baseHttpRestClient;
        private bool _initialized = false;
        private Dictionary<string, BaseHttpRestClient.BaseHttpRestClient> _baseHttpRestClientsToAasServer;
        private Dictionary<string, AasData> _aasInformation;
        //END OF CLASS VARIABLES

        // ================================================================================
        // PUBLIC METHODS
        // ================================================================================

        /// <summary>
        /// Create an AASHttpRestClient.
        /// </summary>
        /// <param name="baseUrl">Url to aas registry.</param>
        public AASHttpRestClient(string baseUrlToAasRegistry)
        {
            _baseHttpRestClient = new BaseHttpRestClient.BaseHttpRestClient(baseUrlToAasRegistry);
            _baseHttpRestClient.OnInitializeCompleted += OnInitializationCompleted;
        }

        /// <summary>
        /// Get all submodel descriptors.
        /// </summary>
        /// <param name="aasIdShort">The idShort of the aas.</param>
        /// <param name="id">Can be used to identify the call in thr callback.</param>
        /// <param name="callback">Called when there is a result from the call.</param>
        public void GetSubmodels(string aasIdShort, string id, _OnResultListSubmodel callback)
        {
            if (!_initialized) return;

            AasData aasData;
            BaseHttpRestClient.BaseHttpRestClient httpClient;
            if (_aasInformation.ContainsKey(aasIdShort) && _aasInformation.TryGetValue(aasIdShort, out aasData) && _baseHttpRestClientsToAasServer.ContainsKey(aasData.aasServerUrl) && _baseHttpRestClientsToAasServer.TryGetValue(aasData.aasServerUrl, out httpClient))
            {
                GetAasSubmodels(aasData.base64EcodedIdentification, httpClient, id, (BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status, BaseHttpRestClient.BaseHttpRestClient.RestMethod method, string id, string result) =>
                {
                    List<Submodel> resultObject = JsonConvert.DeserializeObject<List<Submodel>>(result);
                    if (callback != null && resultObject != null)
                    {
                        callback(status, id, resultObject);
                    }
                    else
                    {
                        callback(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.FAILED, id, null);
                    }
                });
            }
            else
            {
                callback(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.FAILED, id, null);
            }
        }

        /// <summary>
        /// Get all submodel elements from a submodel.
        /// </summary>
        /// <param name="aasIdShort">The idShort of the aas.</param>
        /// <param name="submodelIdShort">The idShort of the submodel.</param>
        /// <param name="id">Can be used to identify the call in thr callback.</param>
        /// <param name="callback">Called when there is a result from the call.</param>
        public void GetSubmodelElements(string aasIdShort, string submodelIdShort, string id, _OnResultListSubmodelElement callback)
        {
            if (!_initialized) return;

            AasData aasData;
            BaseHttpRestClient.BaseHttpRestClient httpClient;
            if (_aasInformation.ContainsKey(aasIdShort) && _aasInformation.TryGetValue(aasIdShort, out aasData) && _baseHttpRestClientsToAasServer.ContainsKey(aasData.aasServerUrl) && _baseHttpRestClientsToAasServer.TryGetValue(aasData.aasServerUrl, out httpClient))
            {
                GetAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElements(aasData.base64EcodedIdentification, submodelIdShort, httpClient, id, (BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status, BaseHttpRestClient.BaseHttpRestClient.RestMethod method, string id, string result) =>
                {
                    List<SubmodelElement> resultObject = JsonConvert.DeserializeObject<List<SubmodelElement>>(result);
                    if (callback != null && resultObject != null)
                    {
                        callback(status, id, resultObject);
                    }
                    else
                    {
                        callback(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.FAILED, id, null);
                    }
                });
            }
            else
            {
                callback(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.FAILED, id, null);
            }
        }

        /// <summary>
        /// Get the submodel element from a given submodel element path.
        /// </summary>
        /// <param name="aasIdShort">The idShort of the aas.</param>
        /// <param name="submodelIdShort">The idShort of the submodel.</param>
        /// <param name="submodelElementIdShortPath">The submodel element path. Multiple submodel can be appended by "/".</param>
        /// <param name="id">Can be used to identify the call in thr callback.</param>
        /// <param name="callback">Called when there is a result from the call.</param>
        public void GetSubmodelElement(string aasIdShort, string submodelIdShort, string submodelElementIdShortPath, string id, _OnResultSubmodelElement callback)
        {
            if (!_initialized) return;

            AasData aasData;
            BaseHttpRestClient.BaseHttpRestClient httpClient;
            if (_aasInformation.ContainsKey(aasIdShort) && _aasInformation.TryGetValue(aasIdShort, out aasData) && _baseHttpRestClientsToAasServer.ContainsKey(aasData.aasServerUrl) && _baseHttpRestClientsToAasServer.TryGetValue(aasData.aasServerUrl, out httpClient))
            {
                GetAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElementsViaSeIdShortPath(aasData.base64EcodedIdentification, submodelIdShort, submodelElementIdShortPath, httpClient, id, (BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status, BaseHttpRestClient.BaseHttpRestClient.RestMethod method, string id, string result) =>
                {
                    SubmodelElement resultObject = JsonConvert.DeserializeObject<SubmodelElement>(result);
                    if (callback != null && resultObject != null)
                    {
                        callback(status, id, resultObject);
                    }
                    else
                    {
                        callback(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.FAILED, id, null);
                    }
                });
            }
            else
            {
                callback(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.FAILED, id, null);
            }
        }

        /// <summary>
        /// Get the value of a submodel element.
        /// </summary>
        /// <param name="aasIdShort">The idShort of the aas.</param>
        /// <param name="submodelIdShort">The idShort of the submodel.</param>
        /// <param name="submodelElementIdShortPath">The submodel element path. Multiple submodel can be appended by "/".</param>
        /// <param name="id">Can be used to identify the call in thr callback.</param>
        /// <param name="callback">Called when there is a result from the call.</param>
        public void GetSubmodelElementValue(string aasIdShort, string submodelIdShort, string submodelElementIdShortPath, string id, _OnResultString callback)
        {
            if (!_initialized) return;

            AasData aasData;
            BaseHttpRestClient.BaseHttpRestClient httpClient;
            if (_aasInformation.ContainsKey(aasIdShort) && _aasInformation.TryGetValue(aasIdShort, out aasData) && _baseHttpRestClientsToAasServer.ContainsKey(aasData.aasServerUrl) && _baseHttpRestClientsToAasServer.TryGetValue(aasData.aasServerUrl, out httpClient))
            {
                GetAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElementsViaSeIdShortPathValue(aasData.base64EcodedIdentification, submodelIdShort, submodelElementIdShortPath, httpClient, id, (BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status, BaseHttpRestClient.BaseHttpRestClient.RestMethod method, string id, string result) =>
                {
                    if (callback != null)
                    {
                        callback(status, id, result);
                    }
                });
            }
            else
            {
                callback(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.FAILED, id, null);
            }
        }

        /// <summary>
        /// Put a value in a submodel element.
        /// </summary>
        /// <param name="aasIdShort">The idShort of the aas.</param>
        /// <param name="submodelIdShort">The idShort of the submodel.</param>
        /// <param name="submodelElementIdShortPath">The submodel element path. Multiple submodel can be appended by "/".</param>
        /// <param name="payload">A string representing a JSON object. (Don't forget to escape special chartacters).</param>
        /// <param name="id">Can be used to identify the call in thr callback.</param>
        /// <param name="callback">Called when there is a result from the call.</param>
        public void PutSubmodelElementValue(string aasIdShort, string submodelIdShort, string submodelElementIdShortPath, string payload, string id, _OnResultString callback)
        {
            if (!_initialized) return;

            AasData aasData;
            BaseHttpRestClient.BaseHttpRestClient httpClient;
            if (_aasInformation.ContainsKey(aasIdShort) && _aasInformation.TryGetValue(aasIdShort, out aasData) && _baseHttpRestClientsToAasServer.ContainsKey(aasData.aasServerUrl) && _baseHttpRestClientsToAasServer.TryGetValue(aasData.aasServerUrl, out httpClient))
            {
                PutAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElementsViaSeIdShortPathValue(aasData.base64EcodedIdentification, submodelIdShort, submodelElementIdShortPath, payload, httpClient, id, (BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status, BaseHttpRestClient.BaseHttpRestClient.RestMethod method, string id, string result) =>
                {
                    if (callback != null)
                    {
                        callback(status, id, result);
                    }
                });
            }
            else
            {
                callback(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.FAILED, id, null);
            }
        }
        // END OF PUBLIC METHODS

        // ================================================================================
        // PRIVATE METHODS
        // ================================================================================

        // initialize callback called from BaseHttpRestClient
        private void OnInitializationCompleted(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status)
        {
            if (status == BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.SUCCESS)
            {
                GetRegistryShellDescriptors(null, HandleRegistryShellDescriptors);
            }
            else
            {
                CallInitializeCompletedCallback(status);
            }
        }

        // call OnInitializeCompleted callback
        private void CallInitializeCompletedCallback(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status)
        {
            _initialized = status == BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.SUCCESS;
            if (OnInitializeCompleted != null)
            {
                OnInitializeCompleted(status);
            }
        }

        // handle the RegistryShellDescriptors ather we have a valid response from GetRegistryShellDescriptors
        private void HandleRegistryShellDescriptors(BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status, BaseHttpRestClient.BaseHttpRestClient.RestMethod method, string id, string result)
        {
            if (status == BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.SUCCESS)
            {
                _aasInformation = new Dictionary<string, AasData>();
                _baseHttpRestClientsToAasServer = new Dictionary<string, BaseHttpRestClient.BaseHttpRestClient>();
                List<AssetAdministrationShellDescriptorRegistry> registryShellDescriptors = JsonConvert.DeserializeObject<List<AssetAdministrationShellDescriptorRegistry>>(result);
                foreach (AssetAdministrationShellDescriptorRegistry registryShellDescriptor in registryShellDescriptors)
                {
                    CreateAasDataObject(registryShellDescriptor);
                }

                BaseHttpRestClientsToAasServer();
            }
            else
            {
                CallInitializeCompletedCallback(status);
            }
        }

        // creates a AasData object and ad it to the dictionary if idShort and an endpoint is avalible
        private void CreateAasDataObject(AssetAdministrationShellDescriptorRegistry registryShellDescriptor)
        {
            string aasServerUrl = null;
            string base64EcodedIdentification = null;

            if (registryShellDescriptor.endpoints != null && registryShellDescriptor.endpoints.Count > 0)
            {
                Match aasServerUrlMatch = Regex.Match(registryShellDescriptor.endpoints[0].protocolInformation.endpointAddress, REGEX_URL_HOST_WITH_PORT_SEARCH_PATTERN);
                if (aasServerUrlMatch.Success)
                {
                    aasServerUrl = aasServerUrlMatch.Value;
                }

                Match base64EcodedIdentificationMatch = Regex.Match(registryShellDescriptor.endpoints[0].protocolInformation.endpointAddress, REGEX_BASE_64_ENCODED_IDENTIFICATION_PATTERN);
                if (base64EcodedIdentificationMatch.Success)
                {
                    base64EcodedIdentification = base64EcodedIdentificationMatch.Value.Substring(AAS_SERVER_SHELLS_PREFIX.Length, base64EcodedIdentificationMatch.Value.Length - AAS_SERVER_SHELLS_AAS_LENGTH);
                }
            }

            AasData aasData = new AasData();
            aasData.aasServerUrl = aasServerUrl;
            aasData.base64EcodedIdentification = base64EcodedIdentification;
            if (aasData.aasServerUrl != null && aasData.base64EcodedIdentification != null && registryShellDescriptor.idShort != null && !registryShellDescriptor.idShort.Equals(""))
            {
                _aasInformation.Add(registryShellDescriptor.idShort, aasData);
            }
        }

        // creates BaseHttpRestClient to the aas server
        private void BaseHttpRestClientsToAasServer()
        {
            int serverCount = 0;
            int serverInitializeCompleted = 0;
            BaseHttpRestClient.BaseHttpRestClient.CompletedStatus statusForAllServer = BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.SUCCESS;

            foreach (AasData aasData in _aasInformation.Values)
            {
                if (!_baseHttpRestClientsToAasServer.ContainsKey(aasData.aasServerUrl))
                {
                    serverCount++;
                    BaseHttpRestClient.BaseHttpRestClient httpRestClient = new BaseHttpRestClient.BaseHttpRestClient(aasData.aasServerUrl);
                    httpRestClient.OnInitializeCompleted += (BaseHttpRestClient.BaseHttpRestClient.CompletedStatus status) =>
                    {
                        if (statusForAllServer == BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.SUCCESS && status != BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.SUCCESS)
                        {
                            statusForAllServer = BaseHttpRestClient.BaseHttpRestClient.CompletedStatus.FAILED;
                        }

                        if (++serverInitializeCompleted == serverCount)
                        {
                            CallInitializeCompletedCallback(statusForAllServer);
                        }
                    };
                    _baseHttpRestClientsToAasServer.Add(aasData.aasServerUrl, httpRestClient);
                }
            }
        }
        // END OF PRIVATE METHODS

        // ================================================================================
        // REGISTRY AND DISCOVERY INTERFACE - PRIVATE METHODS
        // ================================================================================
        private void GetLookupShells() { throw new NotImplementedException(); }
        private void GetLookupShellsViaAasIndentifier() { throw new NotImplementedException(); }
        private void PostLookupShellsViaAasIndentifier() { throw new NotImplementedException(); }
        private void DeleteLookupShellsViaAasIndentifier() { throw new NotImplementedException(); }
        private void PostRegistrySearch() { throw new NotImplementedException(); }
        private void GetRegistryShellDescriptors(string id, BaseHttpRestClient.BaseHttpRestClient._OnRestCallCompleted callback)
        {
            _baseHttpRestClient.Get(GET_REGISTRY_SHELL_DESCRIPTORS_PATH, id, callback);
        }
        private void PostRegistryShellDescriptors() { throw new NotImplementedException(); }
        private void DeleteRegistryShellDescriptors() { throw new NotImplementedException(); }
        private void GetRegistryShellDescriptorsViaAasIdentifier() { throw new NotImplementedException(); }
        private void PutRegistryShellDescriptorsViaAasIdentifier() { throw new NotImplementedException(); }
        private void DeleteRegistryShellDescriptorsViaAasIdentifier() { throw new NotImplementedException(); }
        private void GetRegistryShellDescriptorsViaAasIdentifierSubmodelDescriptors() { throw new NotImplementedException(); }
        private void PostRegistryShellDescriptorsViaAasIdentifierSubmodelDescriptors() { throw new NotImplementedException(); }
        private void GetRegistryShellDescriptorsViaAasIdentifierSubmodelDescriptorsViaSubmodelIdentifier() { throw new NotImplementedException(); }
        private void PutRegistryShellDescriptorsViaAasIdentifierSubmodelDescriptorsViaSubmodelIdentifier() { throw new NotImplementedException(); }
        private void DeleteRegistryShellDescriptorsViaAasIdentifierSubmodelDescriptorsViaSubmodelIdentifier() { throw new NotImplementedException(); }
        // END OF REGISTRY AND DISCOVERY INTERFACE - PRIVATE METHODS

        // ================================================================================
        // AAS SERVER INTERFACE - PRIVATE METHODS
        // ================================================================================
        private void GetAas() { throw new NotImplementedException(); }
        private void GetAasSubmodels(string aasId, BaseHttpRestClient.BaseHttpRestClient httpClient, string id, BaseHttpRestClient.BaseHttpRestClient._OnRestCallCompleted callback)
        {
            httpClient.Get(AAS_SERVER_SHELLS_PREFIX + aasId + GET_SERVER_AAS_SUBMODELS, id, callback);
        }
        private void GetAasSubmodelsViaSubmodelIdShort() { throw new NotImplementedException(); }
        private void PutAasSubmodelsViaSubmodelIdShort() { throw new NotImplementedException(); }
        private void DeleteAasSubmodelsViaSubmodelIdShort() { throw new NotImplementedException(); }
        private void GetAasSubmodelsViaSubmodelIdShortSubmodel() { throw new NotImplementedException(); }
        private void GetAasSubmodelsViaSubmodelIdShortSubmodelValues() { throw new NotImplementedException(); }
        private void GetAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElements(string aasId, string submodelIdShort, BaseHttpRestClient.BaseHttpRestClient httpClient, string id, BaseHttpRestClient.BaseHttpRestClient._OnRestCallCompleted callback)
        {
            httpClient.Get(AAS_SERVER_SHELLS_PREFIX + aasId + GET_SERVER_AAS_SUBMODELS + submodelIdShort + GET_SERVER_AAS_SUBMODELELEMENTS, id, callback);
        }
        private void GetAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElementsViaSeIdShortPath(string aasId, string submodelIdShort, string submodelElementIdShortPath, BaseHttpRestClient.BaseHttpRestClient httpClient, string id, BaseHttpRestClient.BaseHttpRestClient._OnRestCallCompleted callback)
        {
            httpClient.Get(AAS_SERVER_SHELLS_PREFIX + aasId + GET_SERVER_AAS_SUBMODELS + submodelIdShort + GET_SERVER_AAS_SUBMODELELEMENTS + submodelElementIdShortPath, id, callback);
        }
        private void PutAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElementsViaSeIdShortPath() { throw new NotImplementedException(); }
        private void DeleteAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElementsViaSeIdShortPath() { throw new NotImplementedException(); }
        private void GetAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElementsViaSeIdShortPathValue(string aasId, string submodelIdShort, string submodelElementIdShortPath, BaseHttpRestClient.BaseHttpRestClient httpClient, string id, BaseHttpRestClient.BaseHttpRestClient._OnRestCallCompleted callback)
        {
            httpClient.Get(AAS_SERVER_SHELLS_PREFIX + aasId + GET_SERVER_AAS_SUBMODELS + submodelIdShort + GET_SERVER_AAS_SUBMODELELEMENTS + submodelElementIdShortPath + GET_SERVER_AAS_VALUE, id, callback);
        }
        private void PutAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElementsViaSeIdShortPathValue(string aasId, string submodelIdShort, string submodelElementIdShortPath, string payload, BaseHttpRestClient.BaseHttpRestClient httpClient, string id, BaseHttpRestClient.BaseHttpRestClient._OnRestCallCompleted callback)
        {
            httpClient.Put(AAS_SERVER_SHELLS_PREFIX + aasId + GET_SERVER_AAS_SUBMODELS + submodelIdShort + GET_SERVER_AAS_SUBMODELELEMENTS + submodelElementIdShortPath + GET_SERVER_AAS_VALUE, id, callback, payload);
        }
        private void PostAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElementsIdShortPathToOperationInvoke() { throw new NotImplementedException(); }
        private void GetAasSubmodelsViaSubmodelIdShortSubmodelSubmodelElementsIdShortPathToOperationInvocationListViaRequestId() { throw new NotImplementedException(); }
        // END OF AAS SERVER INTERFACE - PRIVATE METHODS


        // ================================================================================
        // AAS DATA
        // ================================================================================
        
        // container to hold url to the aas server and base64 encoded indentification of the aas
        private class AasData
        {
            public string aasServerUrl;
            public string base64EcodedIdentification;
        }
        //END OF AAS DATA
    }
}
