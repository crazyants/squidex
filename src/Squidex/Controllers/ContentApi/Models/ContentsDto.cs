﻿// ==========================================================================
//  ContentsDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Controllers.ContentApi.Models
{
    public class ContentsDto
    {
        /// <summary>
        /// The total number of content items.
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// The content items.
        /// </summary>
        public ContentDto[] Items { get; set; }
    }
}
