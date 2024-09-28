using System.Collections.Generic;
using System.Linq;
using DirectDebits.Models.Entities;

namespace DirectDebits.Core.Banking.Models.AIB
{
    internal class TransactionData
    {
        public string EndToEndId { get; }
        public string InstructedAmount { get; }
        public string MandateId { get; }
        public string DateOfSignature { get; }
        public string AuthId { get; }
        public string AgentBic { get; }
        public string AgentName { get; }
        public string AgentIban { get; }
        public string SchmeName { get; } = "SEPA";

        public TransactionData(string orgName, BankAgent agent, IEnumerable<Allocation> allocations)
            : this(agent, allocations)
        {
            EndToEndId = SepaSpecification.TrunctuateEndToEndId(orgName);
        }

        public TransactionData(int transactionNum, string authId, BankAgent agent, IEnumerable<Allocation> allocations)
            : this(agent, allocations)
        {
            EndToEndId = "TRANS" + transactionNum;
            AuthId = authId;

            var debtor = agent as Debtor;
            MandateId = debtor?.MandateId;
            DateOfSignature = debtor?.MandateSignatureDate;
        }

        private TransactionData(BankAgent agent, IEnumerable<Allocation> allocations)
        {
            InstructedAmount = allocations.Sum(x => x.Amount).ToString("F2");
            AgentBic = agent.Bic;
            AgentName = agent.BankAccName;
            AgentIban = agent.Iban;
        }
    }
}
