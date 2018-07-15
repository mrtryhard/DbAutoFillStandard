using System;
using System.Data;

namespace DbAutoFillStandard.Types
{
    [AttributeUsage(
        AttributeTargets.Class
        | AttributeTargets.Field
        | AttributeTargets.Property
    )]
    public class DbAutoFillAttribute : Attribute
    {
        /// <summary>
        /// Alias to the column's name in the DataReader or the StoredProcedure parameter.
        /// If not set (null or empty), it will use the field's name (or property's name) instead.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// When in FillMode
        /// </summary>
        public string ParameterPrefix { get; set; }

        /// <summary>
        /// When in FillMode
        /// </summary>
        public string ParameterSuffix { get; set; }

        /// <summary>
        /// Allow specifying the DbType for the current property or field (e.g. DateTime2 instead of DateTime).
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// Determines the fill behavior of the object, property or field.
        /// Default is FillBehavior.Both
        /// </summary>
        public DbFillBehavior FillBehavior { get; set; }

        /// <summary>
        /// Defines if the property or the field is allowed to be missing from the DataReader. 
        /// </summary>
        public bool AllowMissing { get; set; }

        public DbAutoFillAttribute()
        {
            AllowMissing = false;
            FillBehavior = DbFillBehavior.Both;
        }
    }
}
