﻿// ==========================================================================
//  AggregateHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Infrastructure.CQRS.Commands
{
    public sealed class AggregateHandler : IAggregateHandler
    {
        private readonly IDomainObjectRepository domainObjectRepository;
        private readonly IDomainObjectFactory domainObjectFactory;

        public IDomainObjectRepository Repository
        {
            get { return domainObjectRepository; }
        }

        public IDomainObjectFactory Factory
        {
            get { return domainObjectFactory; }
        }

        public AggregateHandler(
            IDomainObjectFactory domainObjectFactory, 
            IDomainObjectRepository domainObjectRepository)
        {
            Guard.NotNull(domainObjectFactory, nameof(domainObjectFactory));
            Guard.NotNull(domainObjectRepository, nameof(domainObjectRepository));

            this.domainObjectFactory = domainObjectFactory;
            this.domainObjectRepository = domainObjectRepository;
        }

        public async Task CreateAsync<T>(CommandContext context, Func<T, Task> creator) where T : class, IAggregate
        {
            Guard.NotNull(creator, nameof(creator));
            Guard.NotNull(context, nameof(context));

            var aggregateCommand = GetCommand(context);
            var aggregate = (T)domainObjectFactory.CreateNew(typeof(T), aggregateCommand.AggregateId);

            await creator(aggregate);

            await SaveAsync(aggregate);

            if (!context.IsHandled)
            {
                context.Succeed(new EntityCreatedResult<Guid>(aggregate.Id, aggregate.Version));
            }
        }

        public async Task UpdateAsync<T>(CommandContext context, Func<T, Task> updater) where T : class, IAggregate
        {
            Guard.NotNull(updater, nameof(updater));
            Guard.NotNull(context, nameof(context));

            var aggregateCommand = GetCommand(context);
            var aggregate = await domainObjectRepository.GetByIdAsync<T>(aggregateCommand.AggregateId, aggregateCommand.ExpectedVersion);

            await updater(aggregate);

            await SaveAsync(aggregate);

            if (!context.IsHandled)
            {
                context.Succeed(new EntitySavedResult(aggregate.Version));
            }
        }

        private IAggregateCommand GetCommand(CommandContext context)
        {
            var command = context.Command as IAggregateCommand;

            if (command == null)
            {
                throw new ArgumentException("Context must have an aggregate command.", nameof(context));
            }

            Guard.NotEmpty(command.AggregateId, "context.Command.AggregateId");

            return command;
        }

        private async Task SaveAsync(IAggregate aggregate)
        {
            var events = aggregate.GetUncomittedEvents();

            foreach (var @event in events)
            {
                @event.SetAggregateId(aggregate.Id);
            }

            await domainObjectRepository.SaveAsync(aggregate, events, Guid.NewGuid());

            aggregate.ClearUncommittedEvents();
        }
    }
}
