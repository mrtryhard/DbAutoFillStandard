using DbAutoFillStandard.Types;
using System.Collections.Generic;

namespace DbAutoFillStandardUnitTest.Dataset
{
    [DbAutoFill(AllowMissing = false)]
    class BasicObject
    {
        public string StringProperty { get; set; }
        public int IntField;

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;

            BasicObject other = obj as BasicObject;

            return 
                string.Equals(StringProperty, other.StringProperty) &&
                IntField == other.IntField;
        }

        public override int GetHashCode()
        {
            var hashCode = 14553651;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(StringProperty);
            hashCode = hashCode * -1521134295 + IntField.GetHashCode();
            return hashCode;
        }
    }
}
