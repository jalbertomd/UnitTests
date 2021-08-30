using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public class BankAccount
    {
        public int Balance { get; private set; }

        public BankAccount(int startingBalance)
        {
            Balance = startingBalance;
        }

        public void Deposit(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("La cantidad no puede ser negativa", nameof(amount));

            Balance += amount;
        }

        public bool Withdraw(int amount)
        {
            if(Balance >= amount)
            {
                Balance -= amount;
                return true;
            }

            return false;
        }
    }

    public class DataDrivenTests
    {
        private BankAccount ba;

        [SetUp]
        public void Setup()
        {
            ba = new BankAccount(100);
        }

        [Test]
        [TestCase(50, true, 50)]
        [TestCase(100, true, 0)]
        [TestCase(1000, false, 100)]
        public void TestMultipeWithdrawalScenarios(int amountToWithdraw, bool shouldSucceed, int expectedBalance)
        {
            var result = ba.Withdraw(amountToWithdraw);
            //Warn.If(!result, "Failed");
            Assert.Multiple(()=> 
            {
                Assert.That(result, Is.EqualTo(shouldSucceed));
                Assert.That(expectedBalance, Is.EqualTo(ba.Balance));
            });

        }
    }
}
