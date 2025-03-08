using BankingSystemApplication.Service;
using BankingSystemApplication.Repository;
using BankingSystemApplication.Model;
using Moq;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BankingSystemApplication.Tests
{
    public class InterestServiceTests : IDisposable
    {
        private readonly Mock<BankRepository> _mockRepo;
        private readonly InterestService _interestService;
        private readonly StringWriter _stringWriter;

        public InterestServiceTests()
        {
            _stringWriter = new StringWriter();
            Console.SetOut(_stringWriter);
            _mockRepo = new Mock<BankRepository>();
            _interestService = new InterestService(_mockRepo.Object);
        }

        [Fact]
        public void AddInterestRule_InvalidFormat_ShouldPrintError()
        {
            var ex = Record.Exception(() => _interestService.AddInterestRule("20240301 R1"));
            Assert.Null(ex);
        }

        [Fact]
        public void AddInterestRule_InvalidDate_ShouldPrintError()
        {
            var ex = Record.Exception(() => _interestService.AddInterestRule("2024-03-01 R1 5.00"));
            Assert.Null(ex);
        }

        [Fact]
        public void AddInterestRule_DifferentDates_ShouldStoreSeparately()
        {
            var _repository = new BankRepository();
            var service = new InterestService(_repository);
            service.AddInterestRule("20240301 R1 5.00");
            service.AddInterestRule("20240305 R2 4.00");
            Assert.Equal(2, _repository.InterestRules.Count);
        }

        [Fact]
        public void AddInterestRule_SameRuleId_DifferentDate_ShouldStoreSeparately()
        {
            var _repository = new BankRepository();
            var _interestService = new InterestService(_repository);
            _interestService.AddInterestRule("20240301 R1 5.00");
            _interestService.AddInterestRule("20240305 R1 4.00");
            Assert.Equal(2, _repository.InterestRules.Count);
        }

        [Fact]
        public void AddInterestRule_ShouldBeSortedByDate()
        {
            var _repository = new BankRepository();
            var _interestService = new InterestService(_repository);
            _interestService.AddInterestRule("20240305 R2 4.00");
            _interestService.AddInterestRule("20240301 R1 5.00");
            Assert.Equal("R1", _repository.InterestRules[0].RuleId);
            Assert.Equal("R2", _repository.InterestRules[1].RuleId);
        }

        [Fact]
        public void AddInterestRule_InputWithLeadingOrTrailingSpaces_ShouldBeProcessedCorrectly()
        {
            var _repository = new BankRepository();
            var _interestService = new InterestService(_repository);
            _interestService.AddInterestRule(" 20240301 R1 5.00 "); 
            Assert.Single(_repository.InterestRules);
            Assert.Equal("R1", _repository.InterestRules[0].RuleId);
            Assert.Equal(5.00m, _repository.InterestRules[0].Rate);
        }

        [Fact]
        public void AddInterestRule_ValidRule_ShouldSucceed()
        {
            var repository = new BankRepository();
            var service = new InterestService(repository);
            string input = "20240307 RULE01 2.50";
            service.AddInterestRule(input);
            Assert.Single(repository.InterestRules);
            Assert.Equal(2.50m, repository.InterestRules[0].Rate);
        }

        [Fact]
        public void AddInterestRule_InvalidRate_ShouldFail()
        {
            var repository = new BankRepository();
            var service = new InterestService(repository);
            string input = "20240307 RULE01 150.00"; 

            using (var sw = new StringWriter())
            {
                Console.SetOut(sw);
                service.AddInterestRule(input);
                var output = sw.ToString();
                Assert.Contains("Interest rate must be between 0 and 100.", output);
            }
        }

        [Fact]
        public void AddInterestRule_ValidRule_ShouldBeAdded()
        {
            var _repository = new BankRepository();
            var _interestService = new InterestService(_repository);
            _interestService.AddInterestRule("20240301 R1 5.00");
            Assert.Single(_repository.InterestRules);
            Assert.Equal("R1", _repository.InterestRules[0].RuleId);
            Assert.Equal(5.00m, _repository.InterestRules[0].Rate);
        }

        public void AddInterestRule_LargeInterestRateValid_ShouldPass()
        {
            var _repository = new BankRepository();
            _interestService.AddInterestRule("20240301 R1 99.99");
            Assert.Single(_repository.InterestRules);
            Assert.Equal(99.99m, _repository.InterestRules[0].Rate);
        }

        [Fact]
        public void AddInterestRule_EmptyInput_ShouldPrintError()
        {
            var ex = Record.Exception(() => _interestService.AddInterestRule(""));
            Assert.Null(ex);
        }

        [Fact]
        public void AddInterestRule_SameDateRule_ShouldReplaceExisting()
        {
            var repository = new BankRepository();
            var service = new InterestService(repository);
            repository.InterestRules.Add(new InterestRate { Date = new DateTime(2023, 06, 15), RuleId = "RULE01", Rate = 1.95M });
            var output = new StringWriter();
            var originalOutput = Console.Out;
            Console.SetOut(output); 

            try
            {
                service.AddInterestRule("20230615 RULE03 2.20");
                var result = output.ToString();
                Assert.True(result.Contains("| 20230615 | RULE03 |")); 
                Assert.False(result.Contains("| 20230615 | RULE01 |"));  
            }
            finally
            {
                Console.SetOut(originalOutput);
                output.Dispose();  
            }
        }

        public void Dispose()
        {
            _stringWriter.Dispose();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        }
    }
}
