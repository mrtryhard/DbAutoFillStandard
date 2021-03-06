﻿using DbAutoFillStandard.Types;

namespace DbAutoFillStandardUnitTest.Dataset
{
    [DbAutoFill(AllowMissing = false,
        ParameterPrefix = "p_")]
    public class ComplexObject
    {
        [DbAutoFill(ParameterSuffix = "_IN")]
        public string NameIN { get; set; }

        [DbAutoFill(FillBehavior = DbFillBehavior.ToDB)]
        public int ToDbUuid { get; set; }

        [DbAutoFill(FillBehavior = DbFillBehavior.FromDB)]
        public int FromDbId { get; set; }

        [DbAutoFill(FillBehavior = DbFillBehavior.None)]
        public int Unsettable { get; private set; }

        [DbAutoFill(Alias = "ALittleTest")]
        public bool Aliased { get; set; }

        public ComplexObject()
        {
            Aliased = false;
        }
    }
}
