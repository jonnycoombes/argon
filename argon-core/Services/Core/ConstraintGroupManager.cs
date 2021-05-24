#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JCS.Argon.Model.Commands;
using JCS.Argon.Model.Configuration;
using JCS.Argon.Model.Schema;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Serilog;
using static JCS.Neon.Glow.Helpers.General.LogHelpers;

#endregion

#pragma warning disable 1574

namespace JCS.Argon.Services.Core
{
    /// <summary>
    ///     Class for managing most of the interactions relating to <see cref="ConstraintGroup" /> and <see cref="Constraint" /> schema entities
    /// </summary>
    public class ConstraintGroupManager : BaseCoreService, IConstraintGroupManager
    {
        /// <summary>
        ///     Static logger
        /// </summary>
        private static readonly ILogger _log = Log.ForContext<ConstraintGroupManager>();

        /// <summary>
        /// </summary>
        /// <param name="options"></param>
        /// <param name="serviceProvider"></param>
        public ConstraintGroupManager(IOptionsMonitor<ApiOptions> options, IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
            LogMethodCall(_log);
        }

        /// <inheritdoc></inheritdoc>
        public async Task<ConstraintGroup> CreateConstraintGroupAsync()
        {
            LogMethodCall(_log);
            try
            {
                var op = await DbContext.AddAsync(new ConstraintGroup());
                await CheckedContextSave();
                return op.Entity;
            }
            catch (Exception ex)
            {
                throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status500InternalServerError,
                    "Failed to create new constraint group", ex);
            }
        }

        /// <inheritdoc cref="IConstraintGroupManager.CreateConstraintGroupAsync()"></inheritdoc>
        public async Task<ConstraintGroup> CreateConstraintGroupAsync(IEnumerable<CreateOrUpdateConstraintCommand> cmds)
        {
            LogMethodCall(_log);
            try
            {
                var constraintGroup = new ConstraintGroup
                {
                    Constraints = new List<Constraint>()
                };
                foreach (var cmd in cmds)
                {
                    var constraint = await CreateConstraintAsync(cmd);
                    constraintGroup.Constraints.Add(constraint);
                }

                var op = await DbContext.AddAsync(constraintGroup);
                await CheckedContextSave();
                return op.Entity;
            }
            catch (Exception ex)
            {
                throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status500InternalServerError,
                    "Failed to create new constraint group", ex);
            }
        }

        /// <inheritdoc cref="IConstraintGroupManager.CreateConstraintAsync" />
        public async Task<Constraint> CreateConstraintAsync(CreateOrUpdateConstraintCommand cmd)
        {
            LogMethodCall(_log);
            try
            {
                var constraint = new Constraint
                {
                    Name = cmd.Name,
                    ConstraintType = cmd.ConstraintType,
                    SourceProperty = cmd.SourceProperty,
                    TargetProperty = cmd.TargetProperty,
                    ValueType = cmd.ValueType,
                    AllowableValues = cmd.AllowableValues
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var op = await DbContext.AddAsync(constraint);
                await CheckedContextSave();
                return op.Entity;
            }
            catch (Exception ex)
            {
                throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status500InternalServerError,
                    "Failed to create a new constraint", ex);
            }
        }

        /// <inheritdoc cref="IConstraintGroupManager.ValidatePropertiesAgainstCosntraints" />
        public List<string> ValidatePropertiesAgainstConstraints(ConstraintGroup constraintGroup, PropertyGroup properties)
        {
            LogMethodCall(_log);
            var results = new List<string>();
            if (constraintGroup.Constraints == null)
            {
                return results;
            }

            // mandatory constraints 
            var mandatoryConstraints = constraintGroup.Constraints.Where(c => c.ConstraintType == ConstraintType.Mandatory);
            results.AddRange(from constraint in mandatoryConstraints
                where !properties.HasProperty(constraint.SourceProperty)
                select $"Constraint \"{constraint.Name}\" violated, \"{constraint.SourceProperty}\" not found within supplied properties");

            // type constraints
            var typeConstraints = constraintGroup.Constraints.Where(c => c.ConstraintType == ConstraintType.AllowableType);
            results.AddRange(from constraint in typeConstraints
                where properties.HasProperty(constraint.SourceProperty)
                let property = properties.GetPropertyByName(constraint.SourceProperty)
                where !CheckPropertyType(constraint, property!)
                select $"Type constraint \"{constraint.Name}\" violated, specified value is not of the correct type \"{property.Name}\"");

            // type and values constraints
            var typeAndValueConstraints = constraintGroup.Constraints.Where(c => c.ConstraintType == ConstraintType.AllowableTypeAndValues);
            results.AddRange(from constraint in typeAndValueConstraints
                where properties.HasProperty(constraint.SourceProperty)
                let property = properties.GetPropertyByName(constraint.SourceProperty)
                where !CheckPropertyTypeAndValue(constraint, property!)
                select
                    $"Type constraint \"{constraint.Name}\" violated, specified value is not of the correct type, or doesn't have a valid value \"{property.Name}\"");


            return results;
        }

        /// <inheritdoc cref="IConstraintGroupManager.UpdateAndMergeCollectionConstraints" />
        public async Task<Collection> UpdateAndMergeCollectionConstraints(Collection collection, List<CreateOrUpdateConstraintCommand>
            commands)
        {
            LogMethodCall(_log);
            var constraintGroup = collection.ConstraintGroup;
            if (constraintGroup == null && commands.Any())
            {
                collection.ConstraintGroup = await CreateConstraintGroupAsync(commands);
            }
            else
            {
                // a constraint group exists, but it may be empty
                constraintGroup.Constraints ??= new List<Constraint>();

                // iterate over the supplied commands and merge in the new or updated constraints
                foreach (var command in commands)
                {
                    if (collection.ConstraintGroup.Constraints!.Any(c => c.Name == command.Name))
                    {
                        var existingConstraint = (collection.ConstraintGroup.Constraints ?? null)!.First(c => c.Name == command.Name);
                        if (existingConstraint == null)
                        {
                            continue;
                        }

                        existingConstraint.AllowableValues = command.AllowableValues;
                        existingConstraint.ConstraintType = command.ConstraintType;
                        existingConstraint.SourceProperty = command.SourceProperty;
                        existingConstraint.TargetProperty = command.TargetProperty;
                        existingConstraint.ValueType = command.ValueType;
                    }
                    else
                    {
                        collection.ConstraintGroup.Constraints.Add(await ConstraintGroupManager.CreateConstraintAsync(command));
                    }
                }
            }

            return collection;
        }

        /// <summary>
        ///     Checks whether a given property has been populated with an allowable value
        /// </summary>
        /// <param name="typeAndValueConstraint">The constraint to be applied</param>
        /// <param name="property">The property to check</param>
        /// <returns>true if the property meets the constraint, false otherwise</returns>
        private static bool CheckPropertyTypeAndValue(Constraint typeAndValueConstraint, Property property)
        {
            if (CheckPropertyType(typeAndValueConstraint, property))
            {
                if (typeAndValueConstraint.AllowableValues == null || typeAndValueConstraint.AllowableValues.Length <= 0)
                {
                    return false;
                }

                var value = property.ValueToString();
                if (value == null)
                {
                    return false;
                }

                if (typeAndValueConstraint.AllowableValues.Any(v => v.Equals(value)))
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

            return false;
        }

        /// <summary>
        ///     Checks whether a specific property value is of a given type
        /// </summary>
        /// <param name="typeConstraint">The <see cref="Constraint" /> that provides the type constraint</param>
        /// <param name="property">The property to check</param>
        /// <returns>true if the property meets the type constraint, false otherwise</returns>
        private static bool CheckPropertyType(Constraint typeConstraint, Property property)
        {
            return typeConstraint.ValueType switch
            {
                ConstraintValidTypes.String => property.Type == PropertyType.String,
                ConstraintValidTypes.Number => property.Type == PropertyType.Number,
                ConstraintValidTypes.DateTime => property.Type == PropertyType.DateTime,
                ConstraintValidTypes.Boolean => property.Type == PropertyType.Boolean,
                null => false,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}