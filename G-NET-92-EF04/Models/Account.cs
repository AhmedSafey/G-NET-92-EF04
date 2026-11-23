using System;
using System.Collections.Generic;
using System.Text;

namespace G_NET_92_EF04.Models
{
    internal class Account
    {
        public string AccountNumber { get; set; } = null!;
        public decimal CurrentBalance { get; set; }
        public string AccountType { get; set; } = null!;
        public DateTime OpeningDate { get; set; }

        public string BranchCode { get; set; } = null!;
        public Branch Branch { get; set; } = null!;

        public ICollection<CustomerAccount> CustomerAccounts { get; set; }
            = new List<CustomerAccount>();

        public ICollection<Transaction> Transactions { get; set; }
            = new List<Transaction>();
    }
}
