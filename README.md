# Unity-UWP-AAS-Client
A simple http client for sending rest calls to the asset administration shell registry and server made for Unity UWP.

Not many things supported. Only calls to the registry to get all the AAS (/registry/shell-descriptors) and calls to the server to get the Bubmodels & Submodel Elements. Also the get call and put call to the Submodel Element value is supported.

## How To Use
    [SerializeField] private string _aasRegistryUrl;
    private AASHttpRestClient _aasHttpClient;
    
    private void Start()
    {
        _aasHttpClient = new AASHttpRestClient.AASHttpRestClient(_aasRegistryUrl);
        _aasHttpClient.OnInitializeCompleted += OnInitializeCompleted;
    }
    
    private void OnInitializeCompleted(BaseHttpRestClient.CompletedStatus status)
    {
        Debug.Log(status);
    }

Now you can call:

    GetSubmodels(string aasIdShort, string id, _OnResultListSubmodel callback)
to get all submodel descriptors.


    GetSubmodelElements(string aasIdShort, string submodelIdShort, string id, _OnResultListSubmodelElement callback)
To Get all submodel elements of a submodel.


    GetSubmodelElement(string aasIdShort, string submodelIdShort, string submodelElementIdShortPath, string id, _OnResultSubmodelElement callback)
To get the submodel element from a given submodel element path.
    
    GetSubmodelElementValue(string aasIdShort, string submodelIdShort, string submodelElementIdShortPath, string id, _OnResultString callback)
To get the value of a submodel element.

    PutSubmodelElementValue(string aasIdShort, string submodelIdShort, string submodelElementIdShortPath, string payload, string id, _OnResultString callback)
To put a value in a submodel element.

## Signature on callbacks
        public delegate void _OnResultListSubmodel(BaseHttpRestClient.CompletedStatus status, string id, List<Submodel> result);
        public delegate void _OnResultListSubmodelElement(BaseHttpRestClient.CompletedStatus status, string id, List<SubmodelElement> result);
        public delegate void _OnResultSubmodelElement(BaseHttpRestClient.CompletedStatus status, string id, SubmodelElement result);
        public delegate void _OnResultString(BaseHttpRestClient.CompletedStatus status, string id, string result);
