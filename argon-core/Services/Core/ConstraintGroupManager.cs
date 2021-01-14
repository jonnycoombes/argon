using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JCS.Argon.Contexts;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#pragma warning disable 1574

namespace JCS.Argon.Services.Core
{
    public class ConstraintGroupManager :  BaseCoreService, IConstraintGroupManager
    {
        /// <summary>
        /// Static logger
        /// </summary>
        private static ILogger _log = Log.ForContext<ConstraintGroupManager>();
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="log"></param>
        /// <param name="dbContext"></param>
        public ConstraintGroupManager(IOptionsMonitor<ApiConfiguration> options, IServiceProvider serviceProvider)
        :base(options, serviceProvider)
        {
            LogMethodCall(_log);
        }

        /// <inheritdoc></inheritdoc> 
        public async Task<ConstraintGroup> CreateConstraintGroupAsync()
        {
            LogMethodCall(_log);
            try
            {
                var addOp = await DbContext.AddAsync(new ConstraintGroup());
                await DbContext.SaveChangesAsync();
                return addOp.Entity;
            }
            catch (Exception ex)
            {
                throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status500InternalServerError,
                    "Failed to create new constraint group", ex);
            }
        }

        /// <inheritdoc cref="IConstraintGroupManager.CreateConstraintGroupAsync"></inheritdoc> 
        public async Task<ConstraintGroup> CreateConstraintGroupAsync(List<CreateOrUpdateConstraintCommand> cmds)
        {
            LogMethodCall(_log);
            try
            {
                var constraintGroup = new ConstraintGroup();
                foreach(var cmd in cmds)
                {
                    constraintGroup.Constraints = new List<Constraint>();
                    var constraint = await CreateConstraintAsync(cmd);
                    constraintGroup.Constraints.Add(constraint);
                }

                var addOp= await DbContext.AddAsync(constraintGroup);
                await DbContext.SaveChangesAsync();
                return addOp.Entity;
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
            LogMethodCall(_log);
            try
            {
                var constraint = new Constraint()
                {
                    Name = cmd.Name,
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

                var awaiter = (await DbContext.AddAsync(constraint));
                await DbContext.SaveChangesAsync();
                return awaiter.Entity;
            }
            catch (Exception ex)
            {
                throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status500InternalServerError,
                    "Failed to create a new constraint", ex);
            }
        }

        /// <inheritdoc cref="IConstraintGroupManager.ValidatePropertiesAgainstCosntraints"/>
        public async Task<List<string>> ValidatePropertiesAgainstConstraints(ConstraintGroup constraints, PropertyGroup properties)
        {
            LogMethodCall(_log);
            throw new NotImplementedException();
        }
    }
}