﻿// ==========================================================================
//  IAppCommand.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using PinkParrot.Infrastructure.CQRS.Commands;

namespace PinkParrot.Write
{
    public interface IAppCommand : ICommand
    {
        Guid AppId { get; set; }
    }
}