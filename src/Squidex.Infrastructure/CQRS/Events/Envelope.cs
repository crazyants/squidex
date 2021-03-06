﻿// ==========================================================================
//  Envelope.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class Envelope<TPayload> where TPayload : class
    {
        private readonly EnvelopeHeaders headers;
        private readonly TPayload payload;

        public EnvelopeHeaders Headers
        {
            get { return headers; }
        }

        public TPayload Payload
        {
            get { return payload; }
        }

        public Envelope(TPayload payload)
            : this(payload, new EnvelopeHeaders())
        {
        }

        public Envelope(TPayload payload, PropertiesBag bag)
            : this(payload, new EnvelopeHeaders(bag))
        {
        }

        public Envelope(TPayload payload, EnvelopeHeaders headers)
        {
            Guard.NotNull(payload, nameof(payload));
            Guard.NotNull(headers, nameof(headers));

            this.payload = payload;
            this.headers = headers;
        }

        public static Envelope<TPayload> Create(TPayload payload)
        {
            var eventId = Guid.NewGuid();

            var envelope =
                new Envelope<TPayload>(payload)
                    .SetEventId(eventId)
                    .SetTimestamp(SystemClock.Instance.GetCurrentInstant());

            return envelope;
        }

        public Envelope<TOther> To<TOther>() where TOther : class
        {
            return new Envelope<TOther>(payload as TOther, headers.Clone());
        }
    }
}
