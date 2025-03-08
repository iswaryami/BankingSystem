using BankingSystemApplication.Model;
using BankingSystemApplication.Repository;
using BankingSystemApplication.Service;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystemApplication.Tests
{
    public class AccountServiceTests : IDisposable
    {
        private readonly Mock<BankRepository> _mockRepo;
        private readonly AccountService _accountService;
        private readonly StringWriter _stringWriter;

        public AccountServiceTests()
        {
            _stringWriter = new StringWriter();
            Console.SetOut(_stringWriter);
            _mockRepo = new Mock<BankRepository>();
            _accountService = new AccountService(_mockRepo.Object);
        }

        [Fact]
        public void AddTransaction_InvalidFormat_ShouldPrintError()
        {
            var ex = Record.Exception(() => _accountService.AddTransaction("20240306 AC001 D"));
            Assert.Null(ex); 
        }

        [Fact]
        public void AddTransaction_InvalidDate_ShouldPrintError()
        {
            var ex = Record.Exception(() => _accountService.AddTransaction("2024-06-03 AC001 D 100.00"));
            Assert.Null(ex);
        }

        [Fact]
        public void AddTransaction_InvalidTransactionType_ShouldPrintError()
        {
            var ex = Record.Exception(() => _accountService.AddTransaction("20240306 AC001 X 100.00"));
            Assert.Null(ex);
        }

        [Fact]
        public void AddTransaction_InvalidAmount_ShouldPrintError()
        {
            var ex = Record.Exception(() => _accountService.AddTransaction("20240306 AC001 D -50.00"));
            Assert.Null(ex);
        }

        [Fact]
        public void AddTransaction_FirstTransactionIsWithdrawal_ShouldPrintError()
        {
            var ex = Record.Exception(() => _accountService.AddTransaction("20240306 AC001 W 100.00"));
            Assert.Null(ex);
        }

        [Fact]
        public void AddTransaction_Deposit_ValidTransaction_ShouldSucceed()
        {
            var repository = new BankRepository();
            var service = new AccountService(repository);
            string input = "20240307 AC001 D 500.00"; 
            service.AddTransaction(input);
            Assert.True(repository.Transactions.ContainsKey("AC001"));
            var transactions = repository.Transactions["AC001"];
            Assert.Single(transactions);
            Assert.Equal('D', transactions[0].Type);
            Assert.Equal(500.00m, transactions[0].Amount);
        }

        [Fact]
        public void AddTransaction_ValidWithdrawal_ShouldPass()
        {
            var repository = new BankRepository();
            var service = new AccountService(repository);
            service.AddTransaction("20240306 AC001 D 100.00");
            service.AddTransaction("20240307 AC001 W 50.00");
            Assert.Equal(2, repository.Transactions["AC001"].Count);
        }

        [Fact]
        public void PrintStatement_NoTransactions_ShouldPrintError()
        {
            var ex = Record.Exception(() => _accountService.PrintStatement("AC001"));
            Assert.Null(ex);
        }

        [Fact]
        public void PrintInterest_ValidInterestCalculation_ShouldPass()
        {
            var repository = new BankRepository();
            _accountService.AddTransaction("20240301 AC001 D 1000.00");
            repository.InterestRules.Add(new InterestRate { Date = new DateTime(2024, 3, 1), RuleId = "R1", Rate = 5.0m });

            var ex = Record.Exception(() => _accountService.PrintInterest("AC001",2023,05));
            Assert.Null(ex);
        }

        [Fact]
        public void AddTransaction_Withdraw_InsufficientFunds_ShouldPrintError()
        {
            var repository = new BankRepository();
            var service = new AccountService(repository);
            string input = "20240307 AC001 W 200.00"; 

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                service.AddTransaction(input);
                var output = sw.ToString();
                Assert.Contains("First transaction cannot be a withdrawal.", output);
            }
        }

        [Fact]
        public void AddTransaction_InvalidAmount_ShouldFail()
        {
            var repository = new BankRepository();
            var service = new AccountService(repository);
            string input = "20240307 AC001 D -50.00"; 
            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);

                // Act
                service.AddTransaction(input);
                var output = sw.ToString();

                // Assert
                Assert.Contains("Amount must be greater than zero.", output);
            }
        }

        public void Dispose()
        {
            _stringWriter.Dispose();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }
}
