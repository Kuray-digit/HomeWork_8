using System;
using System.Collections;
using System.Collections.Generic;

namespace Program
{
    enum BankCards
    {
        current,
        saving
    }

    class BankTransaction
    {
        private readonly DateTime time;
        private readonly decimal amount;

        public DateTime Time => time;
        public decimal Amount => amount;

        public BankTransaction(decimal amount)
        {
            this.time = DateTime.Now;
            this.amount = amount;
        }
    }

    class BankAccount
    {
        private static int lastAccountNumber = 0;

        private string accountNumber;
        private decimal balance;
        private BankCards accountType;

        private Queue transactions = new Queue();

        public string AccountNumber => accountNumber;
        public decimal Balance => balance;

        public BankCards AccountType
        {
            get => accountType;
            set => accountType = value;
        }

        private static string GenerateAccountNumber()
        {
            lastAccountNumber++;
            return lastAccountNumber.ToString();
        }

        public BankAccount()
        {
            accountNumber = GenerateAccountNumber();
            balance = 0;
            accountType = BankCards.current;
        }

        public BankAccount(decimal initialBalance)
        {
            accountNumber = GenerateAccountNumber();
            balance = initialBalance;
            accountType = BankCards.current;
        }

        public BankAccount(BankCards type)
        {
            accountNumber = GenerateAccountNumber();
            balance = 0;
            accountType = type;
        }

        public BankAccount(decimal initialBalance, BankCards type)
        {
            accountNumber = GenerateAccountNumber();
            balance = initialBalance;
            accountType = type;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine($"Ошибка: сумма пополнения должна быть больше нуля (введено: {amount}).");
                return;
            }
            balance += amount;
            transactions.Enqueue(new BankTransaction(amount));
            Console.WriteLine($"Счет {accountNumber} пополнен на {amount}. Текущий баланс: {balance}");
        }

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                Console.WriteLine($"Ошибка: сумма снятия должна быть больше нуля (введено: {amount}).");
                return;
            }
            if (amount > balance)
            {
                Console.WriteLine($"Ошибка: недостаточно средств. Запрошено: {amount}, доступно: {balance}.");
                return;
            }
            balance -= amount;
            transactions.Enqueue(new BankTransaction(-amount));
            Console.WriteLine($"Со счета {accountNumber} снято {amount}. Остаток: {balance}");
        }

        public void TransferTo(BankAccount targetAccount, decimal amount)
        {
            if (targetAccount == null)
            {
                Console.WriteLine("Ошибка: счет получателя не указан (null).");
                return;
            }

            if (targetAccount == this)
            {
                Console.WriteLine("Ошибка: нельзя перевести деньги на тот же самый счет.");
                return;
            }

            if (amount <= 0)
            {
                Console.WriteLine($"Ошибка: сумма перевода должна быть больше нуля (введено: {amount}).");
                return;
            }

            if (amount > balance)
            {
                Console.WriteLine($"Ошибка перевода: недостаточно средств на счете {accountNumber}. " +
                                  $"Запрошено: {amount}, доступно: {balance}.");
                return;
            }

            balance -= amount;
            targetAccount.balance += amount;

            transactions.Enqueue(new BankTransaction(-amount));
            targetAccount.transactions.Enqueue(new BankTransaction(amount));

            Console.WriteLine($"Перевод {amount} со счета {accountNumber} на счет {targetAccount.accountNumber} выполнен.");
            Console.WriteLine($"Баланс отправителя {accountNumber}: {balance}");
            Console.WriteLine($"Баланс получателя {targetAccount.accountNumber}: {targetAccount.balance}");
        }
        public void PrintTransactions()
        {
            Console.WriteLine($"--- Операции по счету {accountNumber} ---");
            if (transactions.Count == 0)
            {
                Console.WriteLine("Операций пока нет.");
                return;
            }

            foreach (BankTransaction t in transactions)
            {
                string kind = t.Amount >= 0 ? "пополнение" : "снятие";
                Console.WriteLine($"{t.Time:dd.MM.yyyy HH:mm:ss} | {kind} | {Math.Abs(t.Amount)}");
            }
        }

        public void PrintInfo()
        {
            Console.WriteLine($"Номер счета: {accountNumber}");
            Console.WriteLine($"Баланс: {balance}");
            Console.WriteLine($"Тип счета: {accountType}");
            Console.WriteLine($"Количество операций: {transactions.Count}");
        }
    }

    class HomeWork8
    {
        static List<BankAccount> accounts = new List<BankAccount>();

        static void Main()
        {
            bool running = true;

            while (running)
            {
                Console.WriteLine("\n========= БАНК =========");
                Console.WriteLine("1. Создать счет");
                Console.WriteLine("2. Показать все счета");
                Console.WriteLine("3. Пополнить счет");
                Console.WriteLine("4. Снять со счета");
                Console.WriteLine("5. Перевести между счетами");
                Console.WriteLine("6. Показать операции по счету");
                Console.WriteLine("0. Выход");
                Console.Write("Выберите действие: ");

                string choice = Console.ReadLine() ?? "";
                Console.WriteLine("\n");

                switch (choice)
                {
                    case "1": CreateAccount(); break;
                    case "2": ShowAllAccounts(); break;
                    case "3": DepositToAccount(); break;
                    case "4": WithdrawFromAccount(); break;
                    case "5": TransferBetweenAccounts(); break;
                    case "6": ShowAccountTransactions(); break;
                    case "0": running = false; break;
                    default: Console.WriteLine("Неверный выбор. Попробуйте снова."); break;
                }
            }

            Console.WriteLine("До свидания!");
        }

        static void CreateAccount()
        {
            Console.Write("Введите начальный баланс: ");
            decimal initialBalance = ReadDecimal();

            Console.WriteLine("Выберите тип счета:");
            Console.WriteLine("  1 - current");
            Console.WriteLine("  2 - saving");
            Console.Write("Ваш выбор: ");
            int typeChoice = ReadInt();

            BankCards type = (typeChoice == 2) ? BankCards.saving : BankCards.current;

            BankAccount newAccount = new BankAccount(initialBalance, type);
            accounts.Add(newAccount);

            Console.WriteLine($"Создан счет {newAccount.AccountNumber} с балансом {newAccount.Balance} (тип: {newAccount.AccountType}).");
        }

        static void ShowAllAccounts()
        {
            if (accounts.Count == 0)
            {
                Console.WriteLine("Счетов пока нет.");
                return;
            }

            Console.WriteLine("--- Все счета ---");
            for (int i = 0; i < accounts.Count; i++)
            {
                Console.Write($"\n[{i + 1}] ");
                accounts[i].PrintInfo();
            }
        }

        static void DepositToAccount()
        {
            BankAccount account = ChooseAccount("Выберите счет для пополнения");
            if (account == null) return;

            Console.Write("Введите сумму пополнения: ");
            decimal amount = ReadDecimal();

            account.Deposit(amount);
        }

        static void WithdrawFromAccount()
        {
            BankAccount account = ChooseAccount("Выберите счет для снятия");
            if (account == null) return;

            Console.Write("Введите сумму снятия: ");
            decimal amount = ReadDecimal();

            account.Withdraw(amount);
        }

        static void TransferBetweenAccounts()
        {
            if (accounts.Count < 2)
            {
                Console.WriteLine("Для перевода нужно как минимум два счета.");
                return;
            }

            BankAccount from = ChooseAccount("Выберите счет ОТПРАВИТЕЛЯ");
            if (from == null) return;

            BankAccount to = ChooseAccount("Выберите счет ПОЛУЧАТЕЛЯ");
            if (to == null) return;

            Console.Write("Введите сумму перевода: ");
            decimal amount = ReadDecimal();

            from.TransferTo(to, amount);
        }

        static BankAccount ChooseAccount(string prompt)
        {
            if (accounts.Count == 0)
            {
                Console.WriteLine("Счетов пока нет. Сначала создайте счет.");
                return null;
            }

            Console.WriteLine($"--- {prompt} ---");
            for (int i = 0; i < accounts.Count; i++)
            {
                Console.Write($"[{i + 1}] ");
                accounts[i].PrintInfo();
            }

            Console.Write("Введите номер счета из списка: ");
            int index = ReadInt();

            if (index < 1 || index > accounts.Count)
            {
                Console.WriteLine("Ошибка: счета с таким номером нет.");
                return null;
            }

            return accounts[index - 1];
        }

        static decimal ReadDecimal()
        {
            while (true)
            {
                string input = Console.ReadLine() ?? "";
                if (decimal.TryParse(input, out decimal result))
                    return result;

                Console.Write("Ошибка: введите корректное число. Попробуйте снова: ");
            }
        }

        static int ReadInt()
        {
            while (true)
            {
                string input = Console.ReadLine() ?? "";
                if (int.TryParse(input, out int result))
                    return result;

                Console.Write("Ошибка: введите корректное целое число. Попробуйте снова: ");
            }
        }
        static void ShowAccountTransactions()
        {
            BankAccount account = ChooseAccount("Выберите счет для просмотра операций");
            if (account == null) return;

            account.PrintTransactions();
        }
    }
}