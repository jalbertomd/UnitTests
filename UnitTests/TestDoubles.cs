using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    public interface ILogMock
    {
        bool Write(string message);
    }

    public class BankAccountMock
    {
        public int Balance { get; set; }
        private readonly ILogMock log;

        public BankAccountMock(ILogMock log)
        {
            this.log = log;
        }

        public void Deposit(int amount)
        {
            if (amount < 0)
                throw new ArgumentException("La cantidad no puede ser negativa", nameof(amount));

            //if (log.Write($"Depositing {amount:C}"))
            Balance += amount;
        }
    }

    [TestFixture]
    public class BankAccountTestsMock
    {
        private BankAccountMock ba;

        [Test]
        public void DepositIntegrationTestWithFake()
        {
            var log = new Mock<ILogMock>();
            ba = new BankAccountMock(log.Object) { Balance = 100 };
            ba.Deposit(100);
            Assert.That(ba.Balance, Is.EqualTo(200));
        }
    }

    public class Bar
    {

    }

    public interface IBaz
    {
        string Name { get; }
    }

    public interface IFoo
    {
        bool DoSomething(string value);
        string ProcessString(string value);
        bool TryParse(string value, out string outputValue);
        bool Submit(ref Bar bar);
        int GetCount();
        bool Add(int amount);

        string Name { get; set; }
        IBaz SomeBaz {get;}
        int SomeOtherProperty { get; set; }
    }

    public delegate void AlienAbductionEventHandler(int galaxy, bool returned);

    public interface IAnimal
    {
        event EventHandler FallsIll;
        void Stumble();

        event AlienAbductionEventHandler AbductedByAliens;
    }

    public class Doctor
    {
        public int TimesCured;
        public int AbductionsObserved;

        public Doctor(IAnimal animal)
        {
            //animal.FallsIll += Animal_FallsIll;
            animal.FallsIll += (sender, args) =>
            {
                Console.WriteLine("I will cure you!");
                TimesCured++;
            };

            animal.AbductedByAliens += (galaxy, returned) => ++AbductionsObserved;
        }

        private void Animal_FallsIll(object sender, EventArgs e)
        {
            Console.WriteLine("I will cure you!");
        }
    }

    public class Consumer
    {
        private IFoo foo;
        public Consumer(IFoo foo)
        {
            this.foo = foo;
        }

        public void Hello()
        {
            foo.DoSomething("ping");
            var name = foo.Name;
            foo.SomeOtherProperty = 123;
        }
    }

    public abstract class Person
    {
        protected int SSN { get; set; }
        protected abstract void Execute(string cmd);
    }

    [TestFixture]
    public class MethodSamples
    {
        [Test]
        public void OrdinaryMethodCall()
        {
            var mock = new Mock<IFoo>();
            mock.Setup(foo => foo.DoSomething("ping")).Returns(true);
            mock.Setup(foo => foo.DoSomething(It.IsIn("pong", "foo"))).Returns(false);

            Assert.Multiple(() => 
            {
                Assert.IsTrue(mock.Object.DoSomething("ping"));
                Assert.IsFalse(mock.Object.DoSomething("pong"));
            });
        }

        [Test]
        public void ArgumentDependentMatching()
        {
            var mock = new Mock<IFoo>();
            mock.Setup(foo => foo.DoSomething(It.IsAny<string>())).Returns(true);
            mock.Setup(foo => foo.Add(It.Is<int>(x => x % 2 == 0))).Returns(true);
            mock.Setup(foo => foo.Add(It.IsInRange<int>(1,10, Moq.Range.Inclusive))).Returns(true);
            mock.Setup(foo => foo.DoSomething(It.IsRegex("[a-z]+"))).Returns(true);

            Assert.Multiple(() =>
            {
                Assert.That(mock.Object.DoSomething("ping"), Is.EqualTo(true));
                Assert.IsTrue(mock.Object.Add(10));
            });
        }

        [Test]
        public void OutAndRefArguments()
        {
            var mock = new Mock<IFoo>();

            var requiredOutput = "ok";
            string result;

            mock.Setup(foo => foo.TryParse("ping", out requiredOutput)).Returns(true);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(mock.Object.TryParse("ping", out result));
                Assert.That(result, Is.EqualTo(requiredOutput));
            });

            var bar = new Bar();
            mock.Setup(foo => foo.Submit(ref bar)).Returns(true);
            Assert.That(mock.Object.Submit(ref bar), Is.EqualTo(true));
            var otherBar = new Bar();
            Assert.IsFalse(mock.Object.Submit(ref otherBar));
        }

        [Test]
        public void MyTest()
        {
            var mock = new Mock<IFoo>();

            mock.Setup(foo => foo.ProcessString(It.IsAny<string>())).Returns((string s) => s.ToLowerInvariant());

            var calls = 0;

            mock.Setup(foo => foo.GetCount()).Returns(() => calls).Callback(() => ++calls);
            mock.Object.GetCount();
            mock.Object.GetCount();

            mock.Setup(foo => foo.DoSomething("kills")).Throws<InvalidOperationException>();

            mock.Setup(foo => foo.DoSomething("null")).Throws(new ArgumentException("cmd"));

            Assert.Multiple(() =>
            {
                Assert.That(mock.Object.ProcessString("ABC"), Is.EqualTo("abc"));
                Assert.That(mock.Object.GetCount(), Is.EqualTo(2));
                Assert.Throws<InvalidOperationException>(() => mock.Object.DoSomething("kills"));
                Assert.Throws<ArgumentException>(() => 
                {
                    mock.Object.DoSomething("null");
                }, "cmd");
            });
        }

        [Test]
        public void TestProperty()
        {
            var mock = new Mock<IFoo>();

            mock.Setup(foo => foo.Name).Returns("bar");

            mock.Object.Name = "will not be assigned";

            Assert.That(mock.Object.Name, Is.EqualTo("bar"));

            mock.Setup(foo => foo.SomeBaz.Name).Returns("hello");
            Assert.That(mock.Object.SomeBaz.Name, Is.EqualTo("hello"));

            bool setterCalled = false;
            mock.SetupSet(foo => 
            {
                foo.Name = It.IsAny<string>();
            }).Callback<string>(value => 
            {
                setterCalled = true;
            });

            mock.Object.Name = "def";
            mock.VerifySet(foo =>
            {
                foo.Name = "def";
            }, Times.AtLeastOnce);
            
        }

        [Test]
        public void TestProperty2()
        {
            var mock = new Mock<IFoo>();

            //mock.SetupProperty(foo => foo.Name);
            mock.SetupAllProperties();
            IFoo foo = mock.Object;
            foo.Name = "abc";
            Assert.That(mock.Object.Name, Is.EqualTo("abc"));

            foo.SomeOtherProperty = 123;
            Assert.That(mock.Object.SomeOtherProperty, Is.EqualTo(123));

        }

        [Test]
        public void TestEvents()
        {
            var mock = new Mock<IAnimal>();
            var doctor = new Doctor(mock.Object);
            mock.Raise(
                a => a.FallsIll += null,
                new EventArgs()
                );

            Assert.That(doctor.TimesCured, Is.EqualTo(1));

            mock.Setup(a => a.Stumble()).Raises(a => a.FallsIll += null,
                new EventArgs());

            mock.Object.Stumble();

            Assert.That(doctor.TimesCured, Is.EqualTo(2));

            mock.Raise(
               a => a.AbductedByAliens += null,
               15, true);

            Assert.That(doctor.AbductionsObserved, Is.EqualTo(1));
        }

        [Test]
        public void TestCallbacks()
        {
            var mock = new Mock<IFoo>();
            int x = 0;

            mock.Setup(foo => foo.DoSomething("ping")).Returns(true)
                .Callback(() => x++);
            mock.Object.DoSomething("ping");
            Assert.That(x, Is.EqualTo(1));

            mock.Setup(foo => foo.DoSomething(It.IsAny<string>()))
                .Returns(true)
                .Callback((string s) => x += s.Length);

            mock.Setup(foo => foo.DoSomething(It.IsAny<string>()))
                .Returns(true)
                .Callback<string>(s => x += s.Length);

            mock.Setup(foo => foo.DoSomething("pong"))
                .Callback(() => Console.WriteLine("before pong"))
                .Returns(false)
                .Callback(() => Console.WriteLine("after pong"));

            mock.Object.DoSomething("pong");
        }
        
        [Test]
        public void TestVerification()
        {
            var mock = new Mock<IFoo>();
            var consumer = new Consumer(mock.Object);

            consumer.Hello();

            mock.Verify(foo => foo.DoSomething("ping"), Times.AtLeastOnce);
            mock.Verify(foo => foo.DoSomething("pong"), Times.Never);
            mock.VerifyGet(foo => foo.Name);
            mock.VerifySet(foo => foo.SomeOtherProperty = It.IsInRange(100, 200, Moq.Range.Inclusive));
        }

        [Test]
        public void TestBehavior()
        {
            //var mock = new Mock<IFoo>(MockBehavior.Strict);
            //mock.Setup(f => f.DoSomething("abc")).Returns(true);
            //mock.Object.DoSomething("abc");

            var mock = new Mock<IFoo>
            {
                DefaultValue = DefaultValue.Mock
            };

            var baz = mock.Object.SomeBaz;
            var bazMock = Mock.Get(baz);
            bazMock.SetupGet(f => f.Name).Returns("abc");

            var mockRepository = new MockRepository(MockBehavior.Strict)
            {
                DefaultValue = DefaultValue.Mock
            };

            var fooMock = mockRepository.Create<IFoo>();
            var otherMock = mockRepository.Create<IBaz>(MockBehavior.Loose);
            mockRepository.Verify();
        }

        [Test]
        public void TestProtectedMembers()
        {
            var mock = new Mock<Person>();
            mock.Protected().SetupGet<int>("SSN").Returns(15);
            mock.Protected().Setup<string>("Execute", ItExpr.IsAny<string>());
        }
    }
}
