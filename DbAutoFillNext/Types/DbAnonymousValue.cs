using System;

namespace DbAutoFillStandard.Types
{
    public class DbAnonymousValue<T> : IDbAnonymousValue
    {
        internal DbAnonymousValue()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="alias">Non-empty alias name (used as parameter name or column name in result set).</param>
        /// <param name="value"></param>
        public DbAnonymousValue(string alias, T value)
        {
            if (string.IsNullOrWhiteSpace(alias))
                throw new ArgumentNullException(alias);

            Alias = alias;
            _value = value;
        }

        public override object GetValue()
        {
            return GetTypedValue();
        }

        /// <summary>
        /// </summary>
        /// <param name="value">Value must be of T type.</param>
        /// <exception cref="ArgumentException"></exception>
        public override void SetValue(object value)
        {
            if (value.GetType() != typeof(T))
                throw new ArgumentException("Value is not of the right type. Type given:" + value.GetType().Name + " instead of " + typeof(T).Name);

            _value = value;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">If conversion fails.</exception>
        public T GetTypedValue()
        {
            return (T)_value;
        }
    }
}
