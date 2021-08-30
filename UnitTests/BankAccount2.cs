using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImpromptuInterface;

namespace UnitTests
{
    public interface ILog
    {
        bool Write(string message);
    }

    public class ConsoleLog : ILog
    {
        public bool Write(string message)
        {
            Console.WriteLine(message);
            return true;
        }
    }

    public class BankAccount2
    {
        public int Balance { get; set; }
        private readonly ILog log;

        public BankAccount2(ILog log)
        {
            this.log = log;
        }

        public void Deposit(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("La cantidad no puede ser negativa", nameof(amount));

            if(log.Write($"Depositing {amount:C}"))
                Balance += amount;
        }
    }

    public class NullLog : ILog
    {
        public bool Write(string message)
        {
            return true;
        }
    }

    public class NullLogWithResult : ILog
    {
        private bool expectedResult;

        public NullLogWithResult(bool expectedResult)
        {
            this.expectedResult = expectedResult;
        }

        public bool Write(string message)
        {
            return expectedResult;
        }
    }

    public class LogMock : ILog
    {
        private bool expectedResult;
        public Dictionary<string, int> MethodCallCount;

        public LogMock(bool expectedResult)
        {
            this.expectedResult = expectedResult;
            MethodCallCount = new Dictionary<string, int>();
        }

        private void AddOrIncrement(string methodName)
        {
            if (MethodCallCount.ContainsKey(methodName))
                MethodCallCount[methodName]++;
            else
                MethodCallCount.Add(methodName, 1);
        }

        public bool Write(string message)
        {
            AddOrIncrement(nameof(Write));
            return expectedResult;
        }
    }

    public class Null<T>: DynamicObject where T: class
    {
        public static T Instance
        {
            get
            {
                return new Null<T>().ActLike<T>();
            }
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            //return base.TryInvokeMember(binder, args, out result);
            result = Activator.CreateInstance(typeof(T).GetMethod(binder.Name).ReturnType);
            return true;
        }
    }

    [TestFixture]
    public class BankAccountTests2
    {
        private BankAccount2 ba;

        [Test]
        public void DepositIntegrationTest()
        {
            ba = new BankAccount2(new ConsoleLog()) { Balance = 100 };
            ba.Deposit(100);
            Assert.That(ba.Balance, Is.EqualTo(200));
        }

        [Test]
        public void DepositIntegrationTestWithFake()
        {
            var log = new NullLog();
            ba = new BankAccount2(log) { Balance = 100 };
            ba.Deposit(100);
            Assert.That(ba.Balance, Is.EqualTo(200));
        }

        [Test]
        public void DepositIntegrationTestWithStub()
        {
            var log = new NullLogWithResult(true);
            ba = new BankAccount2(log) { Balance = 100 };
            ba.Deposit(100);
            Assert.That(ba.Balance, Is.EqualTo(200));
        }

        [Test]
        public void DepositIntegrationTestWithDynamicFake()
        {
            var log = Null<ILog>.Instance;
            ba = new BankAccount2(log) { Balance = 100 };
            ba.Deposit(100);
            Assert.That(ba.Balance, Is.EqualTo(200));
        }

        [Test]
        public void DepositTestWithMock()
        {
            var log = new LogMock(true);
            ba = new BankAccount2(log) { Balance = 100 };
            ba.Deposit(100);
            Assert.Multiple(() =>
            {
                Assert.That(ba.Balance, Is.EqualTo(200));
                Assert.That(
                    log.MethodCallCount[nameof(LogMock.Write)], Is.EqualTo(1)
                    );
            });
        }
    }
}