﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingSystemApplication.Model
{
    public class BankTransaction
    {
        public string TransactionId { get; set; }
        public DateTime Date { get; set; }
        public string Account { get; set; }
        public char Type { get; set; } 
        public decimal Amount { get; set; }
    }

}
