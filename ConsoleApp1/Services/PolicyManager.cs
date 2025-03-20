using System.Collections.Generic;
using CsvValidator.Models;

namespace CsvValidator.Services
{
    public class PolicyManager
    {
        private Dictionary<string, Policy> Policies;

        public PolicyManager()
        {
            Policies = new Dictionary<string, Policy>();
        }

        public void AddPolicy(Policy policy)
        {
            Policies[policy.FileName] = policy;
        }

        public Policy? GetPolicy(string fileName)
        {
            if (Policies.TryGetValue(fileName, out Policy? policy))
            {
                return policy;
            }
            return null;
        }
    }
}