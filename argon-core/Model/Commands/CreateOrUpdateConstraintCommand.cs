#region

using System.ComponentModel.DataAnnotations;
using JCS.Argon.Model.Schema;

#endregion

namespace JCS.Argon.Model.Commands
{
    /// <summary>
    /// </summary>
    public class CreateOrUpdateConstraintCommand
    {
        /// <summary>
        ///     The name of the constraint to update or create
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;

        /// <summary>
        ///     The type of the constraint.
        ///     <remarks>
        ///         May be one of the following values:
        ///         Mandatory - places a constraint making a property mandatory
        ///         Mapping - places a mapping constraint (different based on VSP)
        ///         AllowableType - places a type constraint on a property
        ///         AllowableTypeAndValues - contains an array of allowable values
        ///     </remarks>
        /// </summary>
        [Required]
        public ConstraintType ConstraintType { get; set; }

        /// <summary>
        ///     The property that the constraint applies to
        /// </summary>
        [Required]
        public string SourceProperty { get; set; } = null!;

        /// <summary>
        ///     Only applicable for mapping constraint types.  Should contain a string value
        ///     that defines the mapping to a property/action within the underlying storage layer
        /// </summary>
        public string? TargetProperty { get; set; } = null!;

        /// <summary>
        ///     If this is an AllowableType constraint, contains one of the following:
        ///     string
        ///     number
        ///     dateTime
        ///     boolean
        ///     When specified, values given for the target property will be coerced into the suggested type.
        ///     Exceptions will be thrown if the constraint is violated (i.e. the coercion fails)
        /// </summary>
        public ConstraintValidTypes? ValueType { get; set; }

        /// <summary>
        ///     An optional list of values for a specified target property
        /// </summary>
        public string[]? AllowableValues { get; set; }
    }
}