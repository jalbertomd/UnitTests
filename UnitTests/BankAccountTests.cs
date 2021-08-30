using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    [TestFixture]
    public class BankAccountTests
    {
        private BankAccount ba;

        [Test]
        public void MyMethod()
        {
            Assert.Fail("Faio");
        }

        [SetUp]
        public void SetUp()
        {
            // Arrange
            ba = new BankAccount(100);
        }

        [Test]
        public void BankAccountShouldIncreaseOnPositiveDeposit()
        {
            // AAA
            // Arrange
            //var ba = new BankAccount(100);

            // Act
            ba.Deposit(100);

            // Assert
            Assert.That(ba.Balance, Is.EqualTo(200));
        }

        [Test]
        public void BankAccountShouldThrowOnNegativeAmount()
        {
            var ex = Assert.Throws<ArgumentException>(
                () => ba.Deposit(-100)
            );

            StringAssert.StartsWith("La cantidad no puede ser negativa", ex.Message);
        }

        [Test]
        public void MyMethod2()
        {
            ba.Withdraw(100);

            Assert.Multiple(() =>
            {
                Assert.That(ba.Balance, Is.EqualTo(0));
                Assert.That(ba.Balance, Is.LessThan(0));
            });
        }
    }

    [TestFixture]
    public class Warnings
    {
        [Test]
        public void ShowWarning()
        {
            Warn.If(2 + 2 != 5);
            Warn.If(2 + 2, Is.Not.EqualTo(5));
            Warn.If(() => 2 + 2, Is.Not.EqualTo(5).After(2000));

            Warn.Unless(2 + 2 == 5);
            Warn.Unless(2 + 2, Is.EqualTo(5));
            Warn.Unless(() => 2 + 2, Is.EqualTo(5).After(3000));

            Assert.Warn("Te lo advierto!");
        }
    }
}
