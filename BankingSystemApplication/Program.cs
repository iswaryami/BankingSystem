using BankingSystemApplication.Repository;
using BankingSystemApplication.Service;
using static System.Runtime.InteropServices.JavaScript.JSType;
class Program
{
    static void Main()
    {
        var repository = new BankRepository();
        var accountService = new AccountService(repository);
        var interestService = new InterestService(repository);
        Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");

        while (true)
        {
            //Console.WriteLine("Welcome to AwesomeGIC Bank! What would you like to do?");
            Console.WriteLine("[T] Input transactions");
            Console.WriteLine("[I] Define interest rules");
            Console.WriteLine("[P] Print statement");
            Console.WriteLine("[Q] Quit");
            Console.Write("> ");

            string option = Console.ReadLine()?.ToUpper();

            switch (option)
            {
                case "T":
                    Console.WriteLine("Please enter transaction details in <Date> <Account> <Type> <Amount> format:");
                    string transactionInput = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(transactionInput))
                        accountService.AddTransaction(transactionInput);
                    break;

                case "I":
                    Console.WriteLine("Please enter interest rules details in <Date> <RuleId> <Rate in %> format:");
                    string interestInput = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(interestInput))
                        interestService.AddInterestRule(interestInput);
                    break;

                case "P":
                    Console.WriteLine("Please enter account and month to generate the statement <Account> <Year><Month>:");
                    string statementInput = Console.ReadLine();
                    //if (string.IsNullOrWhiteSpace(statementInput))
                    //    return;
                    if (!string.IsNullOrWhiteSpace(statementInput))
                    {
                        DateTime date;
                        var parts = statementInput.Split(' ');
                        if (!DateTime.TryParseExact(parts[0], "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out date))
                        {
                            Console.WriteLine("Invalid date format.");
                            
                        }
                        if (parts.Length == 2)
                        {
                            string account = parts[0];
                            string yearMonth = parts[1];
                            int year = int.Parse(yearMonth.Substring(0, 4));
                            int month = int.Parse(yearMonth.Substring(4, 2));
                            accountService.PrintInterest(account, year, month);
                        }
                        else
                        {
                            Console.WriteLine("Invalid input format.");
                        }
                    }
                    break;

                case "Q":
                    Console.WriteLine("Thank you for banking with AwesomeGIC Bank.\nHave a nice day!");
                    return;

                default:
                    Console.WriteLine("Invalid option. Try again.");
                    break;
            }
            Console.WriteLine("\nIs there anything else you'd like to do?");
        }
    }
}

