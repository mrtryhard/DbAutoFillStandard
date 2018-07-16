using DbAutoFillStandard.Internal;
using DbAutoFillStandard.Types;
using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DbAutoFillStandard
{
    public static class DbAutoFillHelper
    {
        /// <summary>
        /// Add a parameter to the command's parameters collection.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException">Param mustn't be null.</exception>
        public static void AddParameterWithValue<TValue>(IDbCommand command, DbAnonymousValue<TValue> param)
        {
            if (param == null)
                throw new ArgumentNullException("param", "A non-null DbAnonymousValue must be provided.");

            AddParameterWithValue<TValue>(command, param.Alias, param.GetTypedValue(), null);
        }

        /// <summary>
        /// Add a parameter to the command's parameters collection.
        /// 
        /// Supports null value.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddParameterWithValue<T>(IDbCommand command, string name, T value, DbType? dbType)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Parameter name cannot be empty.", "name");

            if (command == null)
                throw new ArgumentNullException("command");

            IDbDataParameter parameter = command.CreateParameter();
            IDbCustomType customValue = value as IDbCustomType;
            parameter.ParameterName = name;

            if (customValue != null)
            {
                customValue.SetParameterValue(parameter);
            }
            else
            {
                parameter.Value = value;

                if (dbType != null && dbType.HasValue)
                    parameter.DbType = dbType.Value;

                if (value == null)
                    parameter.Value = DBNull.Value;
            }

            command.Parameters.Add(parameter);
        }

        /// <summary>
        /// Create and assign the parameters to the given DbCommand object.
        /// Parameters created from the names and values from the object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="obj"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void FillDbParametersFromObject<T>(IDbCommand command, T obj)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            if (obj == null)
                throw new ArgumentNullException("obj");

            Type modelType = obj.GetType();
            DbAutoFillAttribute modelAttribute = modelType
                .GetCustomAttributes(typeof(DbAutoFillAttribute), true)
                .FirstOrDefault() as DbAutoFillAttribute;

            if (modelAttribute != null)
            {
                if (modelAttribute.FillBehavior == DbFillBehavior.None || modelAttribute.FillBehavior == DbFillBehavior.FromDB)
                    return;
            }

            PropertyInfo[] propertyInfos = modelType.GetProperties();
            FieldInfo[] fieldInfos = modelType.GetFields();

            string paramPrefix = string.IsNullOrWhiteSpace(modelAttribute.ParameterPrefix) ? string.Empty : modelAttribute.ParameterPrefix;
            string paramSuffix = string.IsNullOrWhiteSpace(modelAttribute.ParameterSuffix) ? string.Empty : modelAttribute.ParameterSuffix;

            AddParametersFromMemberInfos(command, obj, propertyInfos, modelAttribute, paramPrefix, paramSuffix);
            AddParametersFromMemberInfos(command, obj, fieldInfos, modelAttribute, paramPrefix, paramSuffix);
        }

        /// <summary>
        /// Fills the model fromthe result set / data reader. 
        /// It uses the DbAutoFillAttribute to fill the object.
        /// 
        /// After a call to this method, the model should contain the data properly.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TypeLoadException"></exception>
        /// <exception cref="MissingFieldException"></exception>
        public static void FillObjectFromDataReader<T>(IDataReader dataReader, T obj)
            where T : new()
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (dataReader == null)
                throw new ArgumentNullException("dataReader");

            Type modelType = obj.GetType();

            if (modelType.IsSubclassOf(typeof(IDbAnonymousValue)))
            {
                SetDbAnonymousValueFromDataReader(obj as IDbAnonymousValue, dataReader);
                return;
            }

            DbAutoFillAttribute modelAttribute = Attribute.GetCustomAttribute(modelType, typeof(DbAutoFillAttribute)) as DbAutoFillAttribute;

            if (modelAttribute != null)
            {
                if (modelAttribute.FillBehavior == DbFillBehavior.None || modelAttribute.FillBehavior == DbFillBehavior.ToDB)
                    return;
            }

            PropertyInfo[] modelProperties = modelType.GetProperties();
            FieldInfo[] modelFields = modelType.GetFields();

            string[] lstDbFields = DataReaderUtils.GetFieldsFromDataReader(dataReader);

            SetMembersValuesFromDataReader(dataReader, obj, modelProperties, lstDbFields, modelAttribute);
            SetMembersValuesFromDataReader(dataReader, obj, modelFields, lstDbFields, modelAttribute);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="obj"></param>
        /// <param name="memberInfos"></param>
        /// <param name="modelAttribute"></param>
        /// <param name="modelParameterPrefix"></param>
        /// <param name="modelParameterSuffix"></param>
        private static void AddParametersFromMemberInfos<T>(IDbCommand command, T obj, MemberInfo[] memberInfos, DbAutoFillAttribute modelAttribute, string modelParameterPrefix, string modelParameterSuffix)
        {
            foreach (MemberInfo mi in memberInfos)
            {
                DbAutoFillAttribute memberAttribute = mi.GetCustomAttributes(typeof(DbAutoFillAttribute), false)
                    .FirstOrDefault() as DbAutoFillAttribute;

                if (memberAttribute == null && modelAttribute == null)
                    continue;

                AddParameterFromAttribute(command, obj, modelParameterPrefix, modelParameterSuffix, mi.Name, memberAttribute);
            }
        }

        private static void SetMembersValuesFromDataReader<T>(IDataReader reader, T obj, MemberInfo[] memberInfos, string[] lstDbFields, DbAutoFillAttribute modelAttribute)
        {
            foreach (MemberInfo mi in memberInfos)
            {
                DbAutoFillAttribute memberAttribute = mi.GetCustomAttributes(typeof(DbAutoFillAttribute), false)
                    .FirstOrDefault() as DbAutoFillAttribute;

                if (memberAttribute == null && modelAttribute == null)
                    continue;

                SetValueForObjectMember(reader, obj, mi, memberAttribute, mi.Name, lstDbFields);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="MissingFieldException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static void SetValueForObjectMember<T>(IDataReader reader, T obj, MemberInfo memberInfo, DbAutoFillAttribute attribute, string fieldName, string[] lstDbFields)
        {
            string columnName = fieldName;

            if (attribute != null)
            {
                if (attribute.FillBehavior == DbFillBehavior.None || attribute.FillBehavior == DbFillBehavior.ToDB)
                    return;

                columnName = attribute.Alias ?? fieldName;
            }

            bool hasFieldInReader = lstDbFields.Contains(columnName);

            if (!hasFieldInReader)
            {
                if (attribute != null && attribute.AllowMissing)
                    return;

                throw new MissingFieldException(string.Format("No column named '{0}' in reader for object '{1}'.", columnName, obj.GetType().FullName));
            }

            object value;

            if (reader.IsDBNull(reader.GetOrdinal(columnName)))
                value = null;

            if (memberInfo is PropertyInfo)
            {
                PropertyInfo pi = memberInfo as PropertyInfo;

                value = GetValueFromColumn(pi.PropertyType, reader[columnName]);

                pi.SetValue(obj, value, null);
            }
            else if (memberInfo is FieldInfo)
            {
                FieldInfo fi = memberInfo as FieldInfo;

                value = GetValueFromColumn(fi.FieldType, reader[columnName]);

                fi.SetValue(obj, value);
            }
            else
                throw new ArgumentException("memberInfo", "Unsupported field type");

        }

        /// <summary>
        /// </summary>
        /// <param name="typeOfField"></param>
        /// <param name="dataReaderContent"></param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="OverflowException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        private static object GetValueFromColumn(Type typeOfField, object dataReaderContent)
        {
            if (dataReaderContent == DBNull.Value)
                return null;

            object value = null;

            if (typeOfField == typeof(Guid) || typeOfField == typeof(Guid?))
            {
                value = new Guid(dataReaderContent.ToString());
            }
            else if (typeof(IDbCustomType).IsAssignableFrom(typeOfField))
            {
                value = Activator.CreateInstance(typeOfField);
                (value as IDbCustomType).Deserialize(dataReaderContent.ToString());
            }
            else if (typeOfField.IsGenericType && typeOfField.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                value = Convert.ChangeType(dataReaderContent, Nullable.GetUnderlyingType(typeOfField));
            }
            else
                value = Convert.ChangeType(dataReaderContent, typeOfField);

            return value;
        }

        private static void SetDbAnonymousValueFromDataReader(IDbAnonymousValue anonymousValue, IDataReader dr)
        {
            anonymousValue.SetValue(Convert.ChangeType(dr[0], anonymousValue.GetType().GetGenericArguments()[0]));
        }

        private static bool ParseAttributeInfosForParameter(DbAutoFillAttribute attribute, string modelParameterPrefix, string modelParameterSuffix, string fieldName, ref string parameterName, ref DbType? sqlType)
        {
            if (attribute == null)
                return true;

            if (attribute.FillBehavior == DbFillBehavior.None || attribute.FillBehavior == DbFillBehavior.FromDB)
                return false;

            sqlType = attribute.DbType;
            string propertyPrefix = string.IsNullOrWhiteSpace(modelParameterPrefix) ? string.Empty : modelParameterPrefix;
            string propertySuffix = string.IsNullOrWhiteSpace(modelParameterSuffix) ? string.Empty : modelParameterSuffix;

            if (!string.IsNullOrWhiteSpace(attribute.ParameterPrefix))
                propertyPrefix = attribute.ParameterPrefix;

            if (!string.IsNullOrWhiteSpace(attribute.ParameterSuffix))
                propertySuffix = attribute.ParameterSuffix;

            parameterName = string.Join(string.Empty, 
                propertyPrefix, 
                string.IsNullOrWhiteSpace(attribute.Alias) ? fieldName : attribute.Alias,
                propertySuffix);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        /// <param name="model"></param>
        /// <param name="modelParameterPrefix"></param>
        /// <param name="modelParameterSuffix"></param>
        /// <param name="memberName"></param>
        /// <param name="propertyAttribute"></param>
        private static void AddParameterFromAttribute<T>(IDbCommand cmd, T model, string modelParameterPrefix, string modelParameterSuffix, string memberName, DbAutoFillAttribute propertyAttribute)
        {
            string parameterName = modelParameterPrefix + memberName + modelParameterSuffix;
            DbType? sqlType = null;

            if (!ParseAttributeInfosForParameter(propertyAttribute, modelParameterPrefix, modelParameterSuffix, memberName, ref parameterName, ref sqlType))
                return;

            object parameterValue = null;
            Type modelType = model.GetType();
            {
                PropertyInfo pi = modelType.GetProperty(memberName);

                if (pi == null)
                {
                    FieldInfo fi = modelType.GetField(memberName);
                    parameterValue = fi.GetValue(model);
                }
                else
                {
                    parameterValue = pi.GetValue(model, null);
                }
            }

            AddParameterWithValue(cmd, parameterName, parameterValue, sqlType);
        }
    }
}
