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
                await DbContext.SaveChangesAsync();
                return op.Entity;
            }
            catch (Exception ex)
            {
                throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status500InternalServerError,
                    "Failed to create new constraint group", ex);
            }
        }

        /// <inheritdoc cref="IConstraintGroupManager.CreateConstraintGroupAsync()"></inheritdoc>
        public async Task<ConstraintGroup> CreateConstraintGroupAsync(List<CreateOrUpdateConstraintCommand> cmds)
        {
            LogMethodCall(_log);
            try
            {
                var constraintGroup = new ConstraintGroup();
                foreach (var cmd in cmds)
                {
                    constraintGroup.Constraints = new List<Constraint>();
                    var constraint = await CreateConstraintAsync(cmd);
                    constraintGroup.Constraints.Add(constraint);
                }

                var op = await DbContext.AddAsync(constraintGroup);
                await DbContext.SaveChangesAsync();
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
                    SourceProperty = cmd.SourceProperty
                };
                switch (cmd.ConstraintType)
                {
                    case ConstraintType.Mandatory:
                        break;
                    case ConstraintType.Mapping:
                        if (cmd.TargetProperty == null)
                            throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status400BadRequest,
                                $"No target property specified for constraint [{cmd.Name}]");
                        break;
                    case ConstraintType.AllowableType:
                        if (cmd.ValueType == null)
                            throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status400BadRequest,
                                $"No value type specified for constraint [{cmd.Name}]");
                        break;
                    case ConstraintType.AllowableTypeAndValues:
                        if (cmd.ValueType == null || cmd.AllowableValues == null)
                            throw new IConstraintGroupManager.ConstraintGroupManagerException(StatusCodes.Status400BadRequest,
                                $"No value type or value set specified for constraint [{cmd.Name}]");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var addOp = await DbContext.AddAsync(constraint);
                await DbContext.SaveChangesAsync();
                return addOp.Entity;
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
            if (constraintGroup.Constraints == null) return results;

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
                if (typeAndValueConstraint.AllowableValues == null || typeAndValueConstraint.AllowableValues.Length <= 0) return false;
                var value = property.ValueToString();
                if (value == null) return false;
                if (typeAndValueConstraint.AllowableValues.Any(v => v.Equals(value))) return true;
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