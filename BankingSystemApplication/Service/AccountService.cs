using BankingSystemApplication.Repository;
using BankingSystemApplication.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Data.Common;
using System.IO;

namespace BankingSystemApplication.Service
{
    public class AccountService 
    {
        private readonly BankRepository _repository;
       // private readonly StringWriter _stringWriter;

        public AccountService(BankRepository repository)
        {
            //_stringWriter = new StringWriter();
            //Console.SetOut(_stringWriter);
            _repository = repository;
        }


        public void AddTransaction(string input)
        {
            var parts = input.Split(' ');
            if (parts.Length != 4)
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

            string account = parts[1];
            char type = char.ToUpper(parts[2].Trim()[0]);

            if (type != 'D' && type != 'W')
            {
                Console.WriteLine("Invalid transaction type.");
                return;
            }

            decimal amount;
            if (!decimal.TryParse(parts[3], out  amount))
            {
                Console.WriteLine("Invalid amount format.");
                return;
            }

            if (decimal.Round(amount, 2) != amount)
            {
                Console.WriteLine("Invalid amount format. Maximum two decimal places allowed.");
                return;
            }

            if (amount <= 0)
            {
                Console.WriteLine("Amount must be greater than zero.");
                return;
            }

            if (!_repository.Transactions.ContainsKey(account))
            {
                if (type == 'W')
                {
                    Console.WriteLine("First transaction cannot be a withdrawal.");
                    return;
                }
                _repository.Transactions[account] = new List<BankTransaction>();
            }

            var transactions = _repository.Transactions[account];
            if (type == 'W' && transactions.Sum(t => t.Amount * (t.Type == 'D' ? 1 : -1)) < amount)
            {
                Console.WriteLine("Insufficient funds.");
                return;
            }

            int count = transactions.Count(t => t.Date == date) + 1;
            string txnId = $"{date:yyyyMMdd}-{count:D2}";

            transactions.Add(new BankTransaction { TransactionId = txnId, Date = date, Account = account, Type = type, Amount = amount });

            PrintStatement(account);
        }

        public void PrintStatement(string account)
        {
            if (!_repository.Transactions.ContainsKey(account))
            {
                Console.WriteLine("No transactions found.");
                return;
            }
            var sortedTransactions = _repository.Transactions[account].OrderBy(t => t.Date).ToList();
            Console.WriteLine($"Account: {account}");
            Console.WriteLine("| Date     | Txn Id      | Type | Amount |");

            foreach (var txn in _repository.Transactions[account])
            {
                Console.WriteLine($"| {txn.Date:yyyyMMdd} | {txn.TransactionId,-12} | {txn.Type,-1}    | {txn.Amount,8:F2} |");

            }
        }

        public void PrintInterest(string account, int nyear, int nmonth)
        {
            if (!_repository.Transactions.ContainsKey(account))
            {
                Console.WriteLine($"Account '{account}' not found.");
                return;
            }
            
            var accountTransactions = _repository.Transactions[account]
           .Where(t => t.Account == account && t.Date.Year == nyear && t.Date.Month == nmonth)
           .OrderBy(t => t.Date)
           .ToList();

            if (!accountTransactions.Any())
            {
                Console.WriteLine($"No transactions found for account {account} for the year {nyear} and {nmonth}.");
                return;
            }
            var latestTransaction = accountTransactions.MaxBy(t => t.Date);
            int year = latestTransaction.Date.Year;
            int month = latestTransaction.Date.Month;
            DateTime date;
            Console.WriteLine($"Generating statement for Account: {account} for {year}{month:D2}");
            Console.WriteLine("| Date     | Txn Id       | Type | Amount  | Balance   |");
            decimal balance = GetBalanceBeforeMonth(account, year, month);
            decimal interest = 0m;
            decimal eodBalance = balance;
            int daysInMonth = DateTime.DaysInMonth(year, month);
            var applicableInterestRules = _repository.InterestRules
                .Where(r => r.Date <= new DateTime(year, month, daysInMonth))
                .OrderBy(r => r.Date)
                .ToList();
            int currentDay = 1;
            foreach (var transaction in accountTransactions.Where(t => t.Date.Year == year && t.Date.Month == month))
            {
                interest += ApplyInterest(ref eodBalance, applicableInterestRules, year, month, currentDay, transaction.Date.Day - 1);

                if (transaction.Type == 'D') eodBalance += transaction.Amount;
                else if (transaction.Type == 'W') eodBalance -= transaction.Amount;

                Console.WriteLine($"| {transaction.Date:yyyyMMdd} | {transaction.TransactionId,-12} | {transaction.Type,-1}    | {transaction.Amount,8:F2} | {eodBalance,9:F2} |");

                currentDay = transaction.Date.Day;
            }

            interest += ApplyInterest(ref eodBalance, applicableInterestRules, year, month, currentDay, daysInMonth);

            if (interest > 0)
            {
                Console.WriteLine($"| {year}{month:D2}30 |              | I    | {interest,8:F2} | {eodBalance + interest,9:F2} |");
            }

       }

        private decimal GetBalanceBeforeMonth(string account, int year, int month)
        {

            var previousTransactions = _repository.Transactions[account]
                .Where(t => t.Account == account && (t.Date.Year < year || (t.Date.Year == year && t.Date.Month < month)))
                .OrderBy(t => t.Date)
                .ToList();

            if (!previousTransactions.Any())
                return 0m;

            decimal balance = 0m;
            foreach (var transaction in previousTransactions)
            {
                if (transaction.Type == 'D') balance += transaction.Amount;
                else if (transaction.Type == 'W') balance -= transaction.Amount;
            }
            return balance;
        }

        private decimal ApplyInterest(ref decimal eodBalance, List<InterestRate> interestRules, int year, int month, int startDay, int endDay)
        {
            if (startDay > endDay) return 0m;

            decimal totalInterest = 0m;
            for (int day = startDay; day <= endDay; day++)
            {
                var applicableRule = interestRules.LastOrDefault(r => r.Date <= new DateTime(year, month, day));
                if (applicableRule != null)
                {
                    decimal dailyInterest = (eodBalance * (applicableRule.Rate / 100) * 1) / 365; 
                    totalInterest += dailyInterest;
                }
            }

            return Math.Round(totalInterest, 2); 
        }

        //public void Dispose()
        //{
        //    _stringWriter.Dispose();
        //    Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        //}

    }

}
