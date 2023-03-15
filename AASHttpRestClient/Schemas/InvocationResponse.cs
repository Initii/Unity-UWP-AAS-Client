using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class InvocationResponse
    {
        public string requestId;
        public List<OperationVariable> inoutputArguments;
        public List<OperationVariable> outputArguments;
        public OperationResult operationResult;
        public string executionState;
    }
}