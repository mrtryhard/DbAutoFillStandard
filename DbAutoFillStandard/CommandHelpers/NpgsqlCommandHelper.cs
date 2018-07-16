using Npgsql;
using System.Text;

namespace DbAutoFillStandard.CommandHelpers
{
    public sealed class NpgsqlCommandHelper : DbCommandHelper<NpgsqlConnection>
    {
        public NpgsqlCommandHelper(string connectionString)
            : base(connectionString, null)
        {
        }

        public NpgsqlCommandHelper(string connectionString, string schemaName)
            : base(connectionString, schemaName)
        {
        }

        protected override string CreateBaseCommandString(string schemaName)
        {
            StringBuilder cmd = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(schemaName))
                cmd.AppendFormat("\"{0}\".", schemaName);

            cmd.Append("\"{0}\"");
            return cmd.ToString();
        }
    }
}
