using System.Collections.Generic;

namespace AASHttpRestClient
{
    public class OperationResult
    {
        public bool success;
        public bool isException;
        public object entity;
        public string entityType;
        public List<Message> messages;
    }
}