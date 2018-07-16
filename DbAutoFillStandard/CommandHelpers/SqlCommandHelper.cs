using System.Data.SqlClient;
using System.Text;

namespace DbAutoFillStandard.CommandHelpers
{
    public sealed class SqlCommandHelper : DbCommandHelper<SqlConnection>
    {
        public SqlCommandHelper(string connectionString)
            : base(connectionString, null)
        {
        }

        public SqlCommandHelper(string connectionString, string schemaName)
            : base(connectionString, schemaName)
        {
        }

        protected override string CreateBaseCommandString(string schemaName)
        {
            StringBuilder cmd = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(schemaName))
                cmd.AppendFormat("[{0}].", schemaName);

            cmd.Append("[{0}]");
            return cmd.ToString();
        }
    }
}
