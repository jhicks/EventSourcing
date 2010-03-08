using System;
using EventSourcing.Domain;

namespace EventSourcing.Infrastructure
{
    public class Repository : IRepository
    {
        private readonly IUnitOfWork _unitOfWork;

        public Repository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public TAggregateRoot GetById<TAggregateRoot>(Guid aggregateId) where TAggregateRoot : class, IAggregateRoot
        {
            return _unitOfWork.GetById<TAggregateRoot>(aggregateId);
        }

        public void Add<TAggregateRoot>(TAggregateRoot aggregateRoot) where TAggregateRoot : class, IAggregateRoot
        {
            _unitOfWork.Add(aggregateRoot);
        }
    }
}