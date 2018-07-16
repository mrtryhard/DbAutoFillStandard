using System.Data;

namespace DbAutoFillStandard.Types
{
    public interface IDbCustomType
    {
        void Deserialize(string serialized);
        string SetParameterValue(IDbDataParameter parameter);
    }
}
