using BankingSystemApplication.Model;
using BankingSystemApplication.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystemApplication.Service
{
    public class InterestService
    {
        private readonly BankRepository _repository;

        public InterestService(BankRepository repository)
        {
            _repository = repository;
        }

        public void AddInterestRule(string input)
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                Console.WriteLine("Invalid input format.");
                return;
            }

            DateTime date;
            if (!DateTime.TryParseExact(parts[0], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out date))
            {
                Console.WriteLine("Invalid date format.");
                return;
            }

            decimal rate;
            string ruleId = parts[1];

            if (!decimal.TryParse(parts[2],  out rate))
            {
                Console.WriteLine("Invalid interest rate format.");
                return;
            }
            if (decimal.Round(rate, 2) != rate)
            {
                Console.WriteLine("Invalid amount format. Maximum two decimal places allowed.");
                return;
            }

            if (rate <= 0 || rate >= 100)
            {
                Console.WriteLine("Interest rate must be between 0 and 100.");
                return;
            }

            _repository.InterestRules.RemoveAll(r => r.Date == date);
            _repository.InterestRules.Add(new InterestRate { Date = date, RuleId = ruleId, Rate = rate });
            _repository.InterestRules = _repository.InterestRules.OrderBy(r => r.Date).ToList();
            Console.WriteLine("Interest rules:");
            Console.WriteLine("| Date     | RuleId | Rate (%) |");

            foreach (var rule in _repository.InterestRules)
            {
                Console.WriteLine($"| {rule.Date:yyyyMMdd} | {rule.RuleId} | {rule.Rate,8:F2} |");
            }
        }
    }

}
