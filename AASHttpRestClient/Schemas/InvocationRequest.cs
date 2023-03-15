using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class InvocationRequest
    {
        public string requestId;
        public List<OperationVariable> inputArguments;
        public List<OperationVariable> inoutputArguments;
        public int timeout;
    }
}