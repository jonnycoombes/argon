using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
#pragma warning disable 1574

namespace JCS.Argon.Services.Core
{
    public class ConstraintGroupManager :  BaseCoreService, IConstraintGroupManager
    {
        
        
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="dbContext"></param>
        public ConstraintGroupManager(ILogger<ConstraintGroupManager> log, SqlDbContext dbContext) : base(log, dbContext)
        {
            _log.LogDebug("Creating new instance");
        }

        /// <inheritdoc cref="IConstraintGroupManager.CreateConstraintGroupAsync"/> 
        public async Task<ConstraintGroup> CreateConstraintGroupAsync()
        {
            try
            {
                var constraintGroup = await _dbContext.AddAsync(new ConstraintGroup());
                await _dbContext.SaveChangesAsync();
                return constraintGroup.Entity;
            }
            catch (Exception ex)
            {
                throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status500InternalServerError,
                    "Failed to create new constraint group", ex);
            }
        }

        /// <inheritdoc cref="IConstraintGroupManager.CreateConstraintGroupAsync"/> 
        public async Task<ConstraintGroup> CreateConstraintGroupAsync(List<CreateOrUpdateConstraintCommand> cmds)
        {
            try
            {
                var constraintGroup = new ConstraintGroup();
                foreach(var cmd in cmds)
                {
                    constraintGroup.Constraints = new List<Constraint>();
                    var constraint = await CreateConstraintAsync(cmd);
                    constraintGroup.Constraints.Add(constraint);
                }

                var awaiter= await _dbContext.AddAsync(constraintGroup);
                await _dbContext.SaveChangesAsync();
                return awaiter.Entity;
            }
            catch (Exception ex)
            {
                throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status500InternalServerError,
                    "Failed to create new constraint group", ex);
            }
        }

        /// <inheritdoc cref="IConstraintGroupManager.CreateConstraintAsync"/>
        public async Task<Constraint> CreateConstraintAsync(CreateOrUpdateConstraintCommand cmd)
        {
            var constraint = new Constraint()
            {
               Name= cmd.Name,
               ConstraintType = cmd.ConstraintType,
               SourceProperty = cmd.SourceProperty
            };
            switch (cmd.ConstraintType)
            {
                case ConstraintType.Mandatory:
                    break;
                case ConstraintType.Mapping:
                    if (cmd.TargetProperty == null)
                    {
                        throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status400BadRequest,
                            $"No target property specified for constraint [{cmd.Name}]");
                    }
                    break;
                case ConstraintType.AllowableType:
                    if (cmd.ValueType == null)
                    {
                        throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status400BadRequest,
                            $"No value type specified for constraint [{cmd.Name}]");
                    }
                    break;
                case ConstraintType.AllowableTypeAndValues:
                    if (cmd.ValueType == null || cmd.AllowableValues == null)
                    {
                        throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status400BadRequest,
                            $"No value type or value set specified for constraint [{cmd.Name}]");
                    }
                    break;
            }

            var awaiter = (await _dbContext.AddAsync(constraint));
            await _dbContext.SaveChangesAsync();
            return awaiter.Entity;
        }
    }
}