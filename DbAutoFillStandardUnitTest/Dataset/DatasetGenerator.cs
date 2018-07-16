using System;
using System.Data;

namespace DbAutoFillStandardUnitTest.Dataset
{
    internal static class DatasetGenerator
    {
        internal static IDataReader CreateBasicDataReader(DataTable table, params object[] values)
        {
            if (values.Length != table.Columns.Count)
                throw new ArgumentOutOfRangeException();

            table.Rows.Add(values);
            table.AcceptChanges();

            return table.CreateDataReader();
        }

        internal static DataTable CreateNewBasicDataTable(string[] columns, Type[] columnTypes)
        {
            if (columns.Length != columnTypes.Length)
                throw new ArgumentOutOfRangeException();

            DataTable table = new DataTable();

            for (int i = 0; i < columns.Length; ++i)
            {
                table.Columns.Add(columns[i], columnTypes[i]);
            }

            table.AcceptChanges();

            return table;
        }
    }
}
