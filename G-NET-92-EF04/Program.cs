using G_NET_92_EF04.Models;
using G_NET_92_EF04.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace G_NET_92_EF04
{
    internal class Program
    {

        static void Main(string[] args)
        {


            // SQL Server connection
            var options = new DbContextOptionsBuilder<BankDbContext>()
                .UseSqlServer(
                    @"Server=.;Database=BankDb;Trusted_Connection=True;TrustServerCertificate=True;")
                .Options;

            // Initialize database
            try
            {
                using (var db = new BankDbContext(options))
                {
                    Console.WriteLine("Connecting to database...");
                    db.Database.Migrate();
                    Console.WriteLine("Database is ready.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database error:");
                Console.WriteLine(ex.Message);
                Console.WriteLine("\nPress any key to close...");
                Console.ReadKey();
                return;
            }

            // Main menu
            while (true)
            {
                Console.Clear();

                Console.WriteLine("================================");
                Console.WriteLine("       National Bank - Management");
                Console.WriteLine("================================");
                Console.WriteLine("1) Add a new Customer");
                Console.WriteLine("2) Open a new Account for a Customer");
                Console.WriteLine("3) Update Account Status (Active / Closed)");
                Console.WriteLine("4. Remove an Account from a Customer");
                Console.WriteLine("5. List All Customers with Accounts");
                Console.WriteLine("0. Exit");
                Console.WriteLine("--------------------------------");

                Console.Write("Enter choice: ");

                string? choice = Console.ReadLine();

                if (choice == "0")
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                using (var db = new BankDbContext(options))
                {
                    try
                    {
                        switch (choice)
                        {
                            case "1":
                                AddCustomer(db);
                                break;

                            case "2":
                                OpenAccount(db);
                                break;

                            case "3":
                                ToggleAccountStatus(db);
                                break;

                            case "4":
                                RemoveCustomerAccount(db);
                                break;

                            case "5":
                                ListCustomers(db);
                                break;

                            default:
                                Console.WriteLine("Invalid option. Please choose 0-5.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("\nAn error occurred:");
                        Console.WriteLine(ex.Message);
                    }
                }

                Console.WriteLine("\nPress any key to return to the menu...");
                Console.ReadKey();
            }
        }


        // 1. ADD CUSTOMER
        static void AddCustomer(BankDbContext db)
        {
            Console.WriteLine("\n--- Add new Customer ---");

        

            string fullName = ReadString("Full Name   : ");

            string nationalId = ReadString("National ID   : ");

            if (db.Customers.Any(c => c.NationalId == nationalId))
            {
                Console.WriteLine("This National ID already exists.");
                return;
            }

            DateTime dob = ReadDate("Date of Birth   : (yyyy-MM-dd)");

            if (dob >= DateTime.Today)
            {
                Console.WriteLine("Date of birth must be in the past.");
                return;
            }

        
            string email = ReadString("Email   : ");

            if (!IsValidEmail(email))
            {
                Console.WriteLine("Invalid email address.");
                return;
            }

            string phone = ReadString("Phone  : ");
            string address = ReadString("Address   : ");

            CustomerType customerType =
                ReadEnum<CustomerType>("Customer Type (Individual/Business): ");

            Customer customer = new Customer
            {
                NationalId = nationalId,
                FullName = fullName,
                DateOfBirth = dob,
                Email = email,
                PhoneNumber = phone,
                Address = address,
                CustomerType = customerType
            };

            db.Customers.Add(customer);
            db.SaveChanges();

            Console.WriteLine("Customer added successfully.");
            Console.WriteLine($"Customer ID: {customer.Id}");
        }


        // 2. OPEN ACCOUNT
        static void OpenAccount(BankDbContext db)
        {
            Console.WriteLine("\n--- Open New Account ---");

            string accountNumber = ReadString("Account Number   : ");

            if (db.Accounts.Any(a => a.AccountNumber == accountNumber))
            {
                Console.WriteLine("Account number already exists.");
                return;
            }

            string accountType = ReadString("Account Type (Savings/Current): ");

            string branchCode = ReadString("Branch Code: ");

            var branch = db.Branches
                .FirstOrDefault(b => b.Code == branchCode);

            if (branch == null)
            {
                Console.WriteLine("Branch not found.");
                return;
            }

            int customerId = ReadInt("Customer ID: ");

            var customer = db.Customers
                .FirstOrDefault(c => c.Id == customerId);

            if (customer == null)
            {
                Console.WriteLine("Customer not found.");
                return;
            }

          

            OwnershipType ownership =
                ReadEnum<OwnershipType>("Ownership (Primary/CoHolder): ");

            Account account = new Account
            {
                AccountNumber = accountNumber,
                AccountType = accountType,
                CurrentBalance = 0,
                OpeningDate = DateTime.Now,
                BranchCode = branchCode
            };

            CustomerAccount customerAccount = new CustomerAccount
            {
                CustomerId = customerId,
                AccountNumber = accountNumber,
                OwnershipType = ownership,
                OwnershipStartDate = DateTime.Now,
                AccountStatus = AccountStatus.Active
            };

            db.Accounts.Add(account);
            db.CustomerAccounts.Add(customerAccount);

            db.SaveChanges();

            Console.WriteLine("Account opened successfully.");
        }


        // 3. TOGGLE ACCOUNT STATUS
        static void ToggleAccountStatus(BankDbContext db)
        {
            Console.WriteLine("\n--- Update Account Status ---");

            string accountNumber = ReadString("Account Number: ");
            int customerId = ReadInt("Customer Id: ");

            var customerAccount = db.CustomerAccounts
                .FirstOrDefault(ca =>
                    ca.AccountNumber == accountNumber &&
                    ca.CustomerId == customerId);

            if (customerAccount == null)
            {
                Console.WriteLine("Customer-account relationship not found.");
                return;
            }

            customerAccount.AccountStatus =
                customerAccount.AccountStatus == AccountStatus.Active
                    ? AccountStatus.Inactive
                    : AccountStatus.Active;

            db.SaveChanges();

            Console.WriteLine($"Account Status updated to: {customerAccount.AccountStatus}");
        }


        // 4. REMOVE CUSTOMER-ACCOUNT RELATIONSHIP
        static void RemoveCustomerAccount(BankDbContext db)
        {
            Console.WriteLine("\n--- Remove Account from a Customer ---");

            string accountNumber = ReadString("Account Number: ");
            int customerId = ReadInt("Customer ID: ");

            var customerAccount = db.CustomerAccounts
                .FirstOrDefault(ca =>
                    ca.AccountNumber == accountNumber &&
                    ca.CustomerId == customerId);

            if (customerAccount == null)
            {
                Console.WriteLine("Customer-account relationship not found.");
                return;
            }

            db.CustomerAccounts.Remove(customerAccount);
            db.SaveChanges();

            Console.WriteLine("Customer-account relationship removed.");
            Console.WriteLine("The bank account itself was NOT deleted.");
        }


        // 5. LIST CUSTOMERS AND THEIR ACCOUNTS
        static void ListCustomers(BankDbContext db)
        {
            Console.WriteLine("\n--- All Customers---");

            var customers = db.Customers
                .Include(c => c.CustomerAccounts)
                .ThenInclude(ca => ca.Account)
                .ToList();

            if (!customers.Any())
            {
                Console.WriteLine("No customers found.");
                return;
            }

            foreach (var customer in customers)
            {
                Console.WriteLine("\n--------------------------------");
                Console.WriteLine($"Customer ID: {customer.Id}");
                Console.WriteLine($"Name: {customer.FullName}");
                Console.WriteLine($"National ID: {customer.NationalId}");
                Console.WriteLine($"Email: {customer.Email}");
                Console.WriteLine($"Phone: {customer.PhoneNumber}");
                Console.WriteLine($"Type: {customer.CustomerType}");

                if (!customer.CustomerAccounts.Any())
                {
                    Console.WriteLine("No accounts linked to this customer.");
                    continue;
                }

                Console.WriteLine("Accounts:");

                foreach (var ca in customer.CustomerAccounts)
                {
                    Console.WriteLine($"  Account Number: {ca.Account.AccountNumber}");
                    Console.WriteLine($"  Account Type: {ca.Account.AccountType}");
                    Console.WriteLine($"  Balance: {ca.Account.CurrentBalance}");
                    Console.WriteLine($"  Ownership: {ca.OwnershipType}");
                    Console.WriteLine($"  Status: {ca.AccountStatus}");
                    Console.WriteLine($"  Branch: {ca.Account.BranchCode}");
                    Console.WriteLine();
                }
            }
        }


        // HELPER METHODS

        static string ReadString(string message)
        {
            while (true)
            {
                Console.Write(message);
                string? value = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();

                Console.WriteLine("This field cannot be empty.");
            }
        }


        static int ReadInt(string message)
        {
            while (true)
            {
                Console.Write(message);

                if (int.TryParse(Console.ReadLine(), out int value))
                    return value;

                Console.WriteLine("Please enter a valid number.");
            }
        }


        static DateTime ReadDate(string message)
        {
            while (true)
            {
                Console.Write(message);

                if (DateTime.TryParse(Console.ReadLine(), out DateTime date))
                    return date;

                Console.WriteLine("Invalid date. Try again.");
            }
        }


        static T ReadEnum<T>(string message) where T : struct, Enum
        {
            while (true)
            {
                Console.Write(message);
                string? value = Console.ReadLine();

                if (Enum.TryParse(value, true, out T result) &&
                    Enum.IsDefined(typeof(T), result))
                {
                    return result;
                }

                Console.WriteLine($"Invalid value. Choose: {string.Join(", ", Enum.GetNames(typeof(T)))}");
            }
        }


        static bool IsValidEmail(string email)
        {
            try
            {
                var address = new System.Net.Mail.MailAddress(email);
                return address.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
