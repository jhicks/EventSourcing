using System;
using System.Transactions;
using EventSourcing.Domain;
using EventSourcing.EventStorage;
using EventSourcing.Infrastructure;
using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using UnitTests.Domain;
using Is = NUnit.Framework.Is;
using Property = UnitTests.Domain.Property;

namespace UnitTests
{
    public abstract class InContextOfTestingUnitOfWork : BaseTestFixture<UnitOfWork>
    {
        protected IAggregateBuilder AggregateBuilderMock;
        protected IEventStore EventStoreMock;
        protected IEventHandlerFactory EventHandlerFactoryMock;

        protected override void SetupDependencies()
        {
            base.SetupDependencies();
            EventStoreMock = MockRepository.GenerateMock<IEventStore>();
            AggregateBuilderMock = MockRepository.GenerateMock<IAggregateBuilder>();
            EventHandlerFactoryMock = MockRepository.GenerateMock<IEventHandlerFactory>();
            _subjectUnderTest = new UnitOfWork(EventStoreMock, AggregateBuilderMock, EventHandlerFactoryMock);
        }
    }

    [Specification]
    public class WhenCompletingATransaction : InContextOfTestingUnitOfWork
    {
        private TransactionScope _transactionScope;
        private IEventHandler<PropertyCreatedEvent> _handlerMock;
        private IAggregateRoot _aggregateRoot;

        protected override void SetupDependencies()
        {
            _transactionScope = new TransactionScope();
            base.SetupDependencies();
            _aggregateRoot = Property.New();


            EventStoreMock.Expect(x => x.StoreEvents(_aggregateRoot.Id, new IDomainEvent[0])).Constraints(Rhino.Mocks.Constraints.Is.Equal(_aggregateRoot.Id), Rhino.Mocks.Constraints.Is.Anything());

            _handlerMock = MockRepository.GenerateMock<IEventHandler<PropertyCreatedEvent>>();
            _handlerMock.Expect(x => x.Handle(null)).Constraints(new Anything());
            EventHandlerFactoryMock.Expect(x => x.ResolveHandlers<PropertyCreatedEvent>()).Return(new[] {_handlerMock});

            _subjectUnderTest.Add(_aggregateRoot);
        }

        protected override void When()
        {
            _transactionScope.Complete();
            _transactionScope.Dispose();
        }

        [Then]
        public void ItShouldStoreTheEventsInTheEventStore()
        {
            EventStoreMock.VerifyAllExpectations();
        }

        [Then]
        public void ItShouldGetEventHandlersFromTheEventHandlerFactory()
        {
            Assert.DoesNotThrow(EventHandlerFactoryMock.VerifyAllExpectations);
        }

        [Then]
        public void ItShouldCallEventHandlersAssociatedWithEventsBeingStored()
        {
            Assert.DoesNotThrow(_handlerMock.VerifyAllExpectations);
        }

        [Then]
        public void ItShouldFlushTransientEventsFromAggregates()
        {
            Assert.That(_aggregateRoot.FlushEvents(),Is.Empty);
        }

        [Then]
        public void ItShouldNotThrowAnExcecption()
        {
            Assert.That(_caughtException, Is.Null);
        }
    }
}