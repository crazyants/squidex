// ==========================================================================
//  IEventConsumer.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
{
    public interface IEventConsumer
    {
        Task ClearAsync();

        Task On(Envelope<IEvent> @event);
    }
}