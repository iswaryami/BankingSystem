using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystemApplication.Model
{
    public class InterestRate
    {
        public DateTime Date { get; set; }
        public string RuleId { get; set; }
        public decimal Rate { get; set; } 
    }

}
