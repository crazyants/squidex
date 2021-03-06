﻿// ==========================================================================
//  AddLanguage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Write.Apps.Commands
{
    public sealed class AddLanguage : AppAggregateCommand, IValidatable
    {
        public Language Language { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Language == null)
            {
                errors.Add(new ValidationError("Language cannot be null", nameof(Language)));
            }
        }
    }
}
