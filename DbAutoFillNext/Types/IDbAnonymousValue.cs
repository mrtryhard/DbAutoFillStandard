using System;

namespace DbAutoFillStandard.Types
{
    public abstract class IDbAnonymousValue
    {
        /// <summary>
        /// Alias to which the AnonymousValue is bound to (column in result set or parameter name).
        /// </summary>
        public string Alias { get; set; }
        protected object _value;

        public abstract object GetValue();
        public abstract void SetValue(object value);

        internal Type GetValueType()
        {
            if (_value == null)
                return typeof(DBNull);

            return _value.GetType();
        }
    }
}
