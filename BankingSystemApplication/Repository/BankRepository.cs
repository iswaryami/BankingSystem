using BankingSystemApplication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystemApplication.Repository
{
    public class BankRepository
    {
        public Dictionary<string, List<BankTransaction>> Transactions { get; private set; } = new();
        public List<InterestRate> InterestRules { get;  set; } = new List<InterestRate>();
    }

}
