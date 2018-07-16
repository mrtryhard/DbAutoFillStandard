using DbAutoFillStandard.Types;

namespace DbAutoFillStandardUnitTest.Dataset
{
    [DbAutoFill]
    public class SampleObject
    {
        [DbAutoFill(AllowMissing = true)]
        public int MissingField { get; set; }

        public int Mandatory { get; set; }
    }
}
