using System.Data;

namespace DbAutoFillStandard.Internal
{
    internal static class DataReaderUtils
    {
        internal static string[] GetFieldsFromDataReader(IDataReader reader)
        {
            string[] lstFields = new string[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; ++i)
                lstFields[i] = reader.GetName(i);

            return lstFields;
        }
    }
}
