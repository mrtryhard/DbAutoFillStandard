using DbAutoFillStandard.Types;
using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace DbAutoFillStandard.CommandHelpers
{
    public abstract class DbCommandHelper<TDbConnection>
        where TDbConnection : IDbConnection, new()
    {
        public DbCaseTransform CaseTransform { get; set; }
        private string _baseCmd { get; set; }
        private string _connString { get; set; }

        protected abstract string CreateBaseCommandString(string schemaName);

        public DbCommandHelper(string connectionString)
            : this(connectionString, null)
        {
        }

        /// <exception cref="ArgumentException">Thrown when connection string is empty or the connection failed./exception>
        /// <exception cref="FormatException">Thrown when the base command string is empty.</exception>
        public DbCommandHelper(string connectionString, string schemaName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string must not be empty.", "connectionString");

            CaseTransform = DbCaseTransform.None;
            _connString = connectionString;
            _baseCmd = CreateBaseCommandString(schemaName);

            if (string.IsNullOrWhiteSpace(_baseCmd))
                throw new FormatException(string.Format("Base command string was invalid for given object {0}.", this.GetType().AssemblyQualifiedName));

            using (IDbConnection conn = new TDbConnection())
            {
                conn.ConnectionString = _connString;

                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Connection string appears to be invalid: couldn't open connection.", ex);
                }
            }
        }

        /// <summary>
        /// Takes the caller's name and tries to find a stored procedure named identically on the database and execute it
        /// from the InputType content as parameter and returns a TResultType object filled with results.
        /// 
        /// This function is guaranteed to not throw, and to always wrap errors in the DbResponse object.
        /// </summary>
        /// <typeparam name="TResultType">Returning object type. That object will be filled with returning data from database.</typeparam>
        /// <param name="inputObject">Object type that contains data to be sent to the stored procedure.</param>
        /// <param name="callerName">If not filled, will be replaced by the calling function's name.</param>
        /// <returns>DBResponse containing a list of TResultType objects or the error message.</returns>
        public DbResponse<TResultType> ExecuteFunctionAsCallerName<TResultType>(object inputObject, [CallerMemberName] string callerName = "")
            where TResultType : new()
        {
            return InternalExecuteFunctionAsCallerName<TResultType>(new object[] { inputObject }, callerName);
        }

        /// <summary>
        /// Takes the caller's name and tries to find a stored procedure named identically on the database and execute it
        /// from the InputType content as parameter and returns a TResultType object filled with results.
        /// 
        /// This function is guaranteed to not throw, and to always wrap errors in the DbResponse object.
        /// </summary>
        /// <typeparam name="TResultType">Returning object type. That object will be filled with returning data from database.</typeparam>
        /// <param name="inputObjects">Object type that contains data to be sent to the stored procedure.</param>
        /// <param name="callerName">If not filled, will be replaced by the calling function's name.</param>
        /// <returns>DBResponse containing a list of TResultType objects or the error message.</returns>
        public DbResponse<TResultType> ExecuteDbProcedureNamedAsCallerName<TResultType>(IDbAnonymousValue[] inputObjects, [CallerMemberName] string callerName = "")
           where TResultType : new()
        {
            return InternalExecuteFunctionAsCallerName<TResultType>(inputObjects, callerName);
        }

        /// <summary>
        /// Takes the caller's name and tries to find a stored procedure named identically on the database and execute it without parameters.
        /// 
        /// This function is guaranteed to not throw, and to always wrap errors in the DbResponse object.
        /// </summary>
        /// <typeparam name="TResultType">Returning object type. That object will be filled with returning data from database.</typeparam>
        /// <param name="inputObject">Object type that contains data to be sent to the stored procedure.</param>
        /// <param name="callerName">If not filled, will be replaced by the caller function's name.</param>
        /// <returns>DatabaseResponse containing a list of TResultType objects or the error message.</returns>
        public DbResponse<TResultType> ExecuteFunctionAsCallerName<TResultType>([CallerMemberName] string callerName = "")
            where TResultType : new()
        {
            return ExecuteFunctionAsCallerName<TResultType>(DBNull.Value, callerName);
        }

        private DbResponse<TResultType> InternalExecuteFunctionAsCallerName<TResultType>(object[] inputObjects, [CallerMemberName] string callerName = "")
            where TResultType : new()
        {
            if (inputObjects == null || inputObjects.Length == 0)
                return new DbResponse<TResultType>(string.Format("Argument inputObject must not be null. Caller: {0}", callerName), new ArgumentNullException("inputObject"));

            string procedureName = TransformCallername(callerName);
            DbResponse<TResultType> response = new DbResponse<TResultType>();

            using (TDbConnection conn = new TDbConnection())
            {
                conn.ConnectionString = _connString;

                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    return new DbResponse<TResultType>(string.Format("Couldn't open connection to database. Caller: {0}", callerName), ex);
                }

                using (IDbCommand command = conn.CreateCommand())
                {
                    command.CommandText = procedureName;
                    command.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        AddParametersWithValueToCommand(command, inputObjects);
                        ReadResultsToDbResponse(command, response);
                    }
                    catch (Exception ex)
                    {
                        return new DbResponse<TResultType>(string.Format("An error occured while retrieving data for caller {0}. Database command: '{1}'. Error: {2}", callerName, procedureName, ex.Message), ex);
                    }
                }
            }

            return response;
        }

        private string TransformCallername(string callerName)
        {
            string procedureName = string.Format(_baseCmd, callerName);

            switch (CaseTransform)
            {
                case DbCaseTransform.SmallCase:
                    procedureName = procedureName.ToLower();
                    break;

                case DbCaseTransform.UpperCase:
                    procedureName = procedureName.ToUpper();
                    break;

                case DbCaseTransform.None:
                default:
                    break;
            }

            return procedureName;
        }

        private void AddParametersWithValueToCommand(IDbCommand command, params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return;

            foreach (object parameter in parameters)
            {
                if (parameter != null && false == (parameter is DBNull))
                {
                    IDbAnonymousValue anonymousParam = parameter as IDbAnonymousValue;

                    if (anonymousParam != null)
                        DbAutoFillHelper.AddParameterWithValue(command, anonymousParam.Alias, anonymousParam.GetValue(), null);
                    else
                        DbAutoFillHelper.FillDbParametersFromObject(command, parameter);
                }
            }
        }

        private void ReadResultsToDbResponse<TObjects>(IDbCommand command, DbResponse<TObjects> response)
            where TObjects : new()
        {
            using (IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    TObjects obj = new TObjects();
                    DbAutoFillHelper.FillObjectFromDataReader(reader, obj);
                    response.ResultSet.Add(obj);
                }
            }
        }
    }
}
