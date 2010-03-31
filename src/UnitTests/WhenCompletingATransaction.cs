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
    [Specification]
    public class WhenCompletingATransaction : BaseTestFixture<UnitOfWork>
    {
        private IAggregateBuilder _aggregateBuilderMock;
        private IEventStore _eventStoreMock;
        private IEventHandlerFactory _eventHandlerFactoryMock;

        private TransactionScope _transactionScope;
        private IEventHandler<PropertyCreatedEvent> _handlerMock;
        private IAggregateRoot _aggregateRoot;

        protected override void SetupDependencies()
        {
            _transactionScope = new TransactionScope();
            base.SetupDependencies();
            _aggregateRoot = Property.New();

            _eventStoreMock = MockRepository.GenerateMock<IEventStore>();
            _aggregateBuilderMock = MockRepository.GenerateMock<IAggregateBuilder>();
            _eventHandlerFactoryMock = MockRepository.GenerateMock<IEventHandlerFactory>();
            _subjectUnderTest = new UnitOfWork(_eventStoreMock, _aggregateBuilderMock, _eventHandlerFactoryMock);

            _eventStoreMock.Expect(x => x.StoreEvents(_aggregateRoot.Id, new IDomainEvent[0])).Constraints(Rhino.Mocks.Constraints.Is.Equal(_aggregateRoot.Id), Rhino.Mocks.Constraints.Is.Anything());

            _handlerMock = MockRepository.GenerateMock<IEventHandler<PropertyCreatedEvent>>();
            _handlerMock.Expect(x => x.Handle(null)).Constraints(new Anything());
            _eventHandlerFactoryMock.Expect(x => x.ResolveHandlers<PropertyCreatedEvent>()).Return(new[] {_handlerMock});

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
            _eventStoreMock.VerifyAllExpectations();
        }

        [Then]
        public void ItShouldGetEventHandlersFromTheEventHandlerFactory()
        {
            Assert.DoesNotThrow(_eventHandlerFactoryMock.VerifyAllExpectations);
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