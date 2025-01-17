using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceActor.Tests
{
    [TestClass]
    public partial class ServiceActorTests
    {
        public interface ITestService
        {
            int SimplePropertyGet { get; }
            int SimplePropertySet { get; }
            int SimplePropertyGetSet { get; set; }

            void SimpleMethod();

            void SimpleMethodWithArguments(int i);

            void SimpleMethodWithGenericArguments<T1, T2>(T1 t1, T2 t2);

            void SimpleMethosWithComplexArguments<T1, T2>(Action<T1> action, IDictionary<T1, T2> dict);

            Task TaskMethod();

            Task TaskMethodWithArguments(int i);

            Task TaskMethodWithGenericArguments<T1, T2>(T1 t1, T2 t2);

            Task TaskMethosWithComplexArguments<T1, T2>(Action<T1> action, IDictionary<T1, T2> dict);

            Task<int> TaskMethodWithReturnType();

            Task<int> TaskMethodWithReturnTypeAndWithArguments(int i);

            Task<int> TaskMethodWithReturnTypeAndWithGenericArguments<T1, T2>(T1 t1, T2 t2);

            Task<int> TaskMethodWithReturnTypeAndWithWithComplexArguments<T1, T2>(Action<T1> action, IDictionary<T1, T2> dict);

            Task<Action<int>> TaskMethodWithComplexReturnType();

            Task<Action<int>> TaskMethodWithComplexReturnTypeAndWithArguments(int i);

            Task<Action<T2>> TaskMethodWithComplexReturnTypeAndWithGenericArguments<T1, T2>(T1 t1, T2 t2);

            Task<Action<T2>> TaskMethodWithComplexReturnTypeAndWithWithComplexArguments<T1, T2>(Action<T1> action, IDictionary<T1, T2> dict);
        }

        private class TestService : ITestService
        {
            public int SimplePropertyGet => throw new NotImplementedException();

            public int SimplePropertySet => throw new NotImplementedException();

            public int SimplePropertyGetSet { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public void SimpleMethod()
            {
                throw new NotImplementedException();
            }

            public void SimpleMethodWithArguments(int i)
            {
                throw new NotImplementedException();
            }

            public void SimpleMethodWithArguments(ref int i, out string s)
            {
                throw new NotImplementedException();
            }

            public void SimpleMethodWithGenericArguments<T1, T2>(T1 t1, T2 t2)
            {
                throw new NotImplementedException();
            }

            public void SimpleMethosWithComplexArguments<T1, T2>(Action<T1> action, IDictionary<T1, T2> dict)
            {
                throw new NotImplementedException();
            }

            public Task TaskMethod()
            {
                throw new NotImplementedException();
            }

            public Task TaskMethodWithArguments(int i)
            {
                throw new NotImplementedException();
            }

            public Task<Action<int>> TaskMethodWithComplexReturnType()
            {
                throw new NotImplementedException();
            }

            public Task<Action<int>> TaskMethodWithComplexReturnTypeAndWithArguments(int i)
            {
                throw new NotImplementedException();
            }

            public Task<Action<T2>> TaskMethodWithComplexReturnTypeAndWithGenericArguments<T1, T2>(T1 t1, T2 t2)
            {
                throw new NotImplementedException();
            }

            public Task<Action<T2>> TaskMethodWithComplexReturnTypeAndWithWithComplexArguments<T1, T2>(Action<T1> action, IDictionary<T1, T2> dict)
            {
                throw new NotImplementedException();
            }

            public Task TaskMethodWithGenericArguments<T1, T2>(T1 t1, T2 t2)
            {
                throw new NotImplementedException();
            }

            public Task<int> TaskMethodWithReturnType()
            {
                throw new NotImplementedException();
            }

            public Task<int> TaskMethodWithReturnTypeAndWithArguments(int i)
            {
                throw new NotImplementedException();
            }

            public Task<int> TaskMethodWithReturnTypeAndWithGenericArguments<T1, T2>(T1 t1, T2 t2)
            {
                throw new NotImplementedException();
            }

            public Task<int> TaskMethodWithReturnTypeAndWithWithComplexArguments<T1, T2>(Action<T1> action, IDictionary<T1, T2> dict)
            {
                throw new NotImplementedException();
            }

            public Task TaskMethosWithComplexArguments<T1, T2>(Action<T1> action, IDictionary<T1, T2> dict)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ShouldCreateRefToServiceWithoutException()
        {
            var serviceRef = ServiceRef.Create<ITestService>(new TestService());

            Assert.IsNotNull(serviceRef);

            Assert.AreSame(serviceRef, ServiceRef.Create(serviceRef));
        }

        public interface ITestServiceWithRefOutParemeters
        {
            void MethodWithRefOutParameter(ref int i, out string s);
        }

        public class TestServiceWithRefOutParemeters : ITestServiceWithRefOutParemeters
        {
            public void MethodWithRefOutParameter(ref int i, out string s)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ShouldCreateRefThrowExceptionWhenTypeDefineMethodsWithRefOutParameters()
        {
            Assert.ThrowsException<InvalidOperationException>(() => ServiceRef.Create<ITestServiceWithRefOutParemeters>(new TestServiceWithRefOutParemeters()));
        }



        public interface ITestServiceWithEvents
        {
            event EventHandler Event;
        }

        public class TestServiceWithEvents : ITestServiceWithEvents
        {
#pragma warning disable 0067

            public event EventHandler Event;

#pragma warning restore 0067
        }

        [TestMethod]
        public void ShouldCreateRefThrowExceptionWhenWrapTypesWithEvents()
        {
            Assert.ThrowsException<InvalidOperationException>(() => ServiceRef.Create<ITestServiceWithEvents>(new TestServiceWithEvents()));
        }

        [TestMethod]
        public void ShouldCreateRefThrowExceptionWhenWrapTypesThatNotAreInterfaces()
        {
            Assert.ThrowsException<InvalidOperationException>(() => ServiceRef.Create(new TestServiceWithEvents()));
        }

        [TestMethod]
        public void ShouldCreateRefReuseAlreadyCreatedWrapper()
        {
            var testService = new TestService();
            var serviceRef1 = ServiceRef.Create<ITestService>(testService);
            var serviceRef2 = ServiceRef.Create<ITestService>(testService);

            Assert.AreSame(serviceRef1, serviceRef2);
        }

        public interface ICounter
        {
            int Count { get; }

            void Increment();
        }

        private class Counter : ICounter
        {
            public int Count { get; protected set; }

            public void Increment()
            {
                Count += 1;
            }
        }

        [TestMethod]
        public void ConcurrentAccessToServiceActorShouldJustWork()
        {
            var counter = ServiceRef.Create<ICounter>(new Counter());

            Task.WaitAll(
                Enumerable
                .Range(0, 10)
                .Select(_ =>
                    Task.Factory.StartNew(() =>
                    {
                        counter.Increment();
                    }))
                .ToArray());

            Assert.AreEqual(10, counter.Count);
        }

        [TestMethod]
        public void AccessingPropertyAfterMethodInvocationShouldWork()
        {
            var counter = ServiceRef.Create<ICounter>(new Counter());

            counter.Increment();

            Assert.AreEqual(1, counter.Count);
        }

        public interface IAdvancedCounter : ICounter
        {
            void Increment(int countToAdd);
        }

        public interface IDecrementerCounter : IAdvancedCounter
        {
            void Decrementer();
        }

        private class AdvancedCounter : Counter, IAdvancedCounter
        {
            public void Increment(int countToAdd)
            {
                Count += 10;
            }
        }
        private class AdvancedCounterWithDecrementer : Counter, IAdvancedCounter, IDecrementerCounter
        {
            public void Decrementer()
            {
                Count -= 1;
            }

            public void Increment(int countToAdd)
            {
                Count += 10;
            }
        }

        [TestMethod]
        public void ShouldCreateRefToServiceWithBaseInterfacesWithoutException()
        {
            Assert.IsNotNull(ServiceRef.Create<IAdvancedCounter>(new AdvancedCounter()));
            Assert.IsNotNull(ServiceRef.Create<IDecrementerCounter>(new AdvancedCounterWithDecrementer()));
        }

        public interface IInterfaceWithIncrement
        {
            void Increment<T>(T counter) where T : ICounter;
        }

        public class TypeWithIncrement : IInterfaceWithIncrement
        {
            public void Increment<T>(T counter) where T : ICounter
            {

            }
        }

        [TestMethod]
        public void ShouldCreateRefToServiceWithTemplateConstraintWithoutException()
        {
            Assert.IsNotNull(ServiceRef.Create<IInterfaceWithIncrement>(new TypeWithIncrement()));
        }

        [ServiceDomain(Domain.MyCustomDomain)]
        public interface IServiceA
        {
            void UseServiceB();
        }

        [ServiceDomain(Domain.MyCustomDomain)]
        public interface IServiceB
        {
            bool ServiceUsed { get; }
            void UseService();
        }

        private enum Domain
        {
            MyCustomDomain
        }

        private class ServiceA : IServiceA
        {
            private readonly IServiceB _serviceB;

            public ServiceA(IServiceB serviceB)
            {
                _serviceB = serviceB;
            }

            public void UseServiceB()
            {
                _serviceB.UseService();
            }
        }

        private class ServiceB : IServiceB
        {
            public bool ServiceUsed { get; private set; }

            public void UseService()
            {
                ServiceUsed = true;
            }
        }

        [TestMethod]
        public void ShouldCreateRefWithSingleDomain()
        {
            var serviceB = ServiceRef.Create<IServiceB>(new ServiceB());
            var serviceA = ServiceRef.Create<IServiceA>(new ServiceA(serviceB));

            serviceA.UseServiceB();
            Assert.IsTrue(serviceB.ServiceUsed);
        }

        public interface IBlockingTestCounter
        {
            int Count { get; }

            void NotBlockingIncrement();

            [BlockCaller]
            void BlockingIncrement();
        }

        private class BlockingTestCounter : IBlockingTestCounter
        {
            public int Count { get; private set; }

            public void BlockingIncrement()
            {
                Count += 1;
            }

            public void NotBlockingIncrement()
            {
                Count += 1;
            }
        }

        [TestMethod]
        public void ServiceActorWithBlockingVoidCallShouldWorkAsExpected()
        {
            //keep a reference to the actual implementation class just to be able to 
            //read the Count property without pass thru the wrapper
            var private_counter_ref = new BlockingTestCounter();
            var counter = ServiceRef.Create<IBlockingTestCounter>(private_counter_ref);

            counter.NotBlockingIncrement();
            //as the previous call is not blocking here the count is still 0 
            Assert.AreEqual(0, private_counter_ref.Count);

            //now perform a blocking call (method marked with [BlockCaller] attribute)
            counter.BlockingIncrement();

            //here we can be sure that both previous call has been invoked to the actual class
            Assert.AreEqual(2, private_counter_ref.Count);
        }

        public interface IBlockingActualTypeTestCounter
        {
            int Count { get; }

            void NotBlockingIncrement();

            void BlockingIncrement();
        }

        private class BlockingActualTypeTestCounter : IBlockingActualTypeTestCounter
        {
            public int Count { get; private set; }

            [BlockCaller]
            public void BlockingIncrement()
            {
                Count += 1;
            }

            public void NotBlockingIncrement()
            {
                Count += 1;
            }
        }


        [TestMethod]
        public void ServiceActorWithBlockingVoidCallOnActualTypeShouldWorkAsExpected()
        {
            //keep a reference to the actual implementation class just to be able to 
            //read the Count property without pass thru the wrapper
            var private_counter_ref = new BlockingActualTypeTestCounter();
            var counter = ServiceRef.Create<IBlockingActualTypeTestCounter>(private_counter_ref);

            counter.NotBlockingIncrement();
            //as the previous call is not blocking here the count is still 0 
            Assert.AreEqual(0, private_counter_ref.Count);

            //now perform a blocking call (method marked with [BlockCaller] attribute)
            counter.BlockingIncrement();

            //here we can be sure that both previous call has been invoked to the actual class
            Assert.AreEqual(2, private_counter_ref.Count);
        }

        [TestMethod]
        public void ServiceActorWithBlockingVoidWaitingForCallCompletionShouldWorkAsDesigned()
        {
            //keep a reference to the actual implementation class just to be able to 
            //read the Count property without pass thru the wrapper
            var private_counter_ref = new BlockingActualTypeTestCounter();
            var counter = ServiceRef.Create<IBlockingActualTypeTestCounter>(private_counter_ref);

            counter.NotBlockingIncrement();

            ServiceRef.WaitForCallQueueCompletion(counter);

            //as the previous call is not blocking here the count is still 0 
            Assert.AreEqual(1, private_counter_ref.Count);
        }

        [BlockCaller]
        public interface IBlockingInterfaceAttribute
        {
            int Count { get; }

            void BlockingIncrement();
        }

        private class BlockingInterfaceAttributeTestClass : IBlockingInterfaceAttribute
        {
            public int Count { get; private set; }

            public void BlockingIncrement()
            {
                Count += 1;
            }

            public void NotBlockingIncrement()
            {
                Count += 1;
            }
        }


        [TestMethod]
        public void ServiceActorWithBlockingAttributeOnInterfaceShouldWorkAsExpected()
        {
            var private_counter_ref = new BlockingInterfaceAttributeTestClass();
            var counter = ServiceRef.Create<IBlockingInterfaceAttribute>(private_counter_ref);

            Assert.AreEqual(0, private_counter_ref.Count);

            counter.BlockingIncrement();

            Assert.AreEqual(1, private_counter_ref.Count);
        }

        public interface IAsyncCounter
        {
            [AllowConcurrentAccess]
            int Count { get; }

            Task IncrementAsync();
        }

        private class CounterAsync : IAsyncCounter
        {
            public int Count { get; protected set; }

            public async Task IncrementAsync()
            {
                int threadId = Thread.CurrentThread.ManagedThreadId;
                await Task.Delay(100);
                Count += 1;
                Assert.AreEqual(threadId, Thread.CurrentThread.ManagedThreadId);
            }
        }

        [TestMethod]
        public async Task ServiceActorShouldRestoreTheSynchronizationContext()
        {
            var asyncConter = ServiceRef.Create<IAsyncCounter>(new CounterAsync());

            await asyncConter.IncrementAsync();
        }

        public interface IServiceWithException
        {
            int PropertyThatRaiseException { get; }

            int MethodThatRaiseException();
        }

        private class ServiceWithException : IServiceWithException
        {
            public int PropertyThatRaiseException => throw new NotImplementedException();

            public int MethodThatRaiseException()
            {
                throw new IndexOutOfRangeException();
            }
        }

        [TestMethod]
        public void ServiceActorShouldRethrowExceptions()
        {
            var service = ServiceRef.Create<IServiceWithException>(new ServiceWithException());

            Assert.ThrowsException<IndexOutOfRangeException>(()=> service.MethodThatRaiseException());
            Assert.ThrowsException<NotImplementedException>(() => service.PropertyThatRaiseException);
        }

        public interface IService1
        {

        }

        public interface IService2
        {

        }

        private class MultiInterfaceService : IService1, IService2
        {

        }

        [TestMethod]
        public void ServiceActorShouldWorkWithServiceImplementingMoreInterfaces()
        {
            var multiItfService = new MultiInterfaceService();
            var service1 = ServiceRef.Create<IService1>(multiItfService);
            var service2 = ServiceRef.Create<IService2>(multiItfService);

            Assert.IsNotNull(service1);
            Assert.IsNotNull(service2);
        }

        public interface ITestActionCallService
        {
            int Order { get; }
            void Method(int order);
        }

        public class TestActionCallService : ITestActionCallService
        {
            public int Order { get; private set; }
            public void Method(int order)
            {
                Assert.AreEqual(Order + 1, order);
                Order = order;
            }
        }

        [TestMethod]
        public void TestExecutionOfExternaActionInServiceQueue()
        {
            var service = new TestActionCallService();

            Assert.ThrowsException<InvalidOperationException>(() => ServiceRef.Call(service, () => service.Method(0)));

            var wrapper = ServiceRef.Create<ITestActionCallService>(service);

            wrapper.Method(1);
            ServiceRef.Call(wrapper, () => service.Method(2));
            wrapper.Method(3);

            Assert.AreEqual(3, wrapper.Order);
        }

        [TestMethod]
        public void TestCallsMonitor()
        {
            var callTracer = new SimpleCallMonitorTracer();
            ActionQueue.BeginMonitor(callTracer);

            try
            {
                TestExecutionOfExternaActionInServiceQueue();
            }
            finally
            {
                ActionQueue.ExitMonitor(callTracer);
            }
        }

        [AllowReentrantCalls(false)]
        public interface ICircularDependencyServiceA
        {
            bool TestProperty { get; }

            bool MethodServiceA(ICircularDependencyServiceB serviceB);
        }

        public interface ICircularDependencyServiceB
        {
            bool MethodServiceB(ICircularDependencyServiceA serviceA);
        }

        private class CircularDependencyServiceA : ICircularDependencyServiceA
        {
            public bool TestProperty => true;

            public bool MethodServiceA(ICircularDependencyServiceB serviceB)
            {
                return serviceB.MethodServiceB(ServiceRef.Create<ICircularDependencyServiceA>(this));
            }
        }

        private class CircularDependencyServiceB : ICircularDependencyServiceB
        {
            public bool MethodServiceB(ICircularDependencyServiceA serviceA)
            {
                return serviceA.TestProperty;
            }
        }

        [TestMethod]
        public void TestCircularDependency()
        {
            var serviceA = ServiceRef.Create<ICircularDependencyServiceA>(new CircularDependencyServiceA());
            var serviceB = ServiceRef.Create<ICircularDependencyServiceB>(new CircularDependencyServiceB());

            Assert.ThrowsException<InvalidOperationException>(() => serviceA.MethodServiceA(serviceB));
        }

        public interface IAllowReentrantServiceA
        {
            bool TestProperty { get; set; }

            void MethodServiceA(IAllowReentrantServiceB serviceB);
        }

        public interface IAllowReentrantServiceB
        {
            void MethodServiceB(IAllowReentrantServiceA serviceA);
        }

        private class AllowReentrantServiceA : IAllowReentrantServiceA
        {
            public bool TestProperty { get; set; }

            public void MethodServiceA(IAllowReentrantServiceB serviceB)
            {
                serviceB.MethodServiceB(ServiceRef.Create<IAllowReentrantServiceA>(this));
            }
        }

        private class AllowReentrantServiceB : IAllowReentrantServiceB
        {
            public void MethodServiceB(IAllowReentrantServiceA serviceA)
            {
                serviceA.TestProperty = true;
            }
        }

        [TestMethod]
        public void TestAllowReentrantServices()
        {
            var serviceA = ServiceRef.Create<IAllowReentrantServiceA>(new AllowReentrantServiceA());
            var serviceB = ServiceRef.Create<IAllowReentrantServiceB>(new AllowReentrantServiceB());

            serviceA.MethodServiceA(serviceB);

            ServiceRef.WaitForCallQueueCompletion(serviceA);
            ServiceRef.WaitForCallQueueCompletion(serviceB);

            Assert.IsTrue(serviceA.TestProperty);
        }
    }
}