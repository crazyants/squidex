﻿// ==========================================================================
//  UsersController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.Users.Models;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;
using Squidex.Read.Users.Repositories;

namespace Squidex.Controllers.Api.Users
{
    /// <summary>
    /// Readonly API to retrieve information about squidex users.
    /// </summary>
    [Authorize]
    [ApiExceptionFilter]
    [SwaggerTag("Users")]
    public class UsersController : Controller
    {
        private readonly IUserRepository userRepository;

        public UsersController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        /// <summary>
        /// Get users by query.
        /// </summary>
        /// <param name="query">The query to search the user by email address. Case invariant.</param>
        /// <remarks>
        /// Search the user by query that contains the email address or the part of the email address.
        /// </remarks>
        /// <returns>
        /// 200 => Users returned.
        /// </returns>
        [HttpGet]
        [Route("users")]
        [ProducesResponseType(typeof(UserDto[]), 200)]
        public async Task<IActionResult> GetUsers(string query)
        {
            var entities = await userRepository.QueryByEmailAsync(query ?? string.Empty);

            var response = entities.Select(x => SimpleMapper.Map(x, new UserDto())).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Get user by id.
        /// </summary>
        /// <param name="id">The id of the user (GUID).</param>
        /// <returns>
        /// 200 => User found.
        /// 404 => User not found.
        /// </returns>
        [HttpGet]
        [Route("users/{id}/")]
        [ProducesResponseType(typeof(UserDto), 200)]
        public async Task<IActionResult> GetUser(string id)
        {
            var entity = await userRepository.FindUserByIdAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            var response = SimpleMapper.Map(entity, new UserDto());

            return Ok(response);
        } 
    }
}
