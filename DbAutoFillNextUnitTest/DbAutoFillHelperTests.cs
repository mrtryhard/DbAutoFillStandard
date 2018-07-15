using DbAutoFillStandard;
using DbAutoFillStandard.Types;
using DbAutoFillNextCoreUnitTest.Dataset;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace DbAutoFillStandardUnitTest
{
    [TestCategory("DbAutoFillHelper")]
    [TestClass]
    public class DbAutoFillHelperTests
    {
        SqlConnection conn;
        DbCommand command;

        [TestInitialize]
        public void Setup()
        {
            conn = new SqlConnection();
            command = conn.CreateCommand();
        }

        [TestMethod]
        public void AddAnonymousValue()
        {
            const string expectedAlias = "Integer";
            const int expectedValue = 50;

            DbAutoFillHelper.AddParameterWithValue(command, expectedAlias, expectedValue, null);

            Assert.AreEqual(expectedAlias, command.Parameters[0].ParameterName);
            Assert.AreEqual(expectedValue, command.Parameters[0].Value);
        }

        [TestMethod]
        public void AddConcreteAnonymousValue()
        {
            DbAnonymousValue<string> expected = new DbAnonymousValue<string>("ParameterName", "AString");

            DbAutoFillHelper.AddParameterWithValue(command, expected);

            Assert.AreEqual(expected.Alias, command.Parameters[0].ParameterName);
            Assert.AreEqual(expected.GetValue(), command.Parameters[0].Value);
        }

        [TestMethod]
        public void DbParametersFromBasicObject()
        {
            const int expectedInt = 11;
            const string expectedString = "HollyMolly";
            const string expectedIntName = "IntField";
            const string expectedStringProperty = "StringProperty";

            BasicObject obj = new BasicObject
            {
                IntField = expectedInt,
                StringProperty = expectedString
            };

            DbAutoFillHelper.FillDbParametersFromObject(command, obj);

            Assert.AreEqual(obj.IntField, command.Parameters[expectedIntName].Value);
            Assert.AreEqual(obj.StringProperty, command.Parameters[expectedStringProperty].Value);
        }

        [TestMethod]
        public void DbParametersFromComplexObject()
        {
            const string expectedNameInName = "p_NameIN_IN";
            const string expectedToDbUuidName = "p_ToDbUuid";
            const string expectedNameINValue = "ComplexObject";
            const int expectedToDbUuidValue = 5;

            ComplexObject obj = new ComplexObject
            {
                NameIN = expectedNameINValue,
                ToDbUuid = expectedToDbUuidValue,
                FromDbId = 66
            };

            DbAutoFillHelper.FillDbParametersFromObject(command, obj);

            Assert.AreEqual(expectedNameInName, command.Parameters[expectedNameInName].ParameterName);
            Assert.AreEqual(expectedNameINValue, command.Parameters[expectedNameInName].Value);
            Assert.AreEqual(expectedToDbUuidName, command.Parameters[expectedToDbUuidName].ParameterName);
            Assert.AreEqual(expectedToDbUuidValue, command.Parameters[expectedToDbUuidName].Value);
            Assert.ThrowsException<IndexOutOfRangeException>(() => { string n = command.Parameters["FromDbId"].ParameterName; });
            Assert.ThrowsException<IndexOutOfRangeException>(() => { string n = command.Parameters["Unsettable"].ParameterName; });
        }

        [TestMethod]
        public void DbResultsToBasicObject()
        {
            const string expectedStringValue = "String";
            const int expectedIntValue = 76;
            const string stringPropName = "StringProperty";
            const string intPropName = "IntField";

            DataTable dt = DatasetGenerator.CreateNewBasicDataTable(
                new string[] { stringPropName, intPropName }, 
                new Type[] { typeof(string), typeof(int) });

            using (IDataReader dr = DatasetGenerator.CreateBasicDataReader(dt, expectedStringValue, expectedIntValue))
            {
                dr.Read();

                BasicObject obj = new BasicObject();
                DbAutoFillHelper.FillObjectFromDataReader(dr, obj);

                Assert.AreEqual(obj.StringProperty, expectedStringValue);
                Assert.AreEqual(obj.IntField, expectedIntValue);
            }
        }

        [TestMethod]
        public void DbResultToComplexObject()
        {
            const string nameINField = "NameIN";
            const string toDbUuidField = "ToDbUuid";
            const string fromDbIdField = "FromDbId";

            const string expectedNameINValue = "ComplexObject";
            const int unexpectedToDbUuidValue = 23;
            const int expectedFromDbUuid = 3;
            const int expectedToDbUuidValue = -19;

            DataTable dt = DatasetGenerator.CreateNewBasicDataTable(
                new string[] { nameINField, toDbUuidField, fromDbIdField },
                new Type[] { typeof(string), typeof(int), typeof(int) });

            using (IDataReader dr = DatasetGenerator.CreateBasicDataReader(dt, 
                expectedNameINValue, 
                unexpectedToDbUuidValue, 
                expectedFromDbUuid))
            {
                dr.Read();

                ComplexObject obj = new ComplexObject();
                obj.ToDbUuid = expectedToDbUuidValue;

                DbAutoFillHelper.FillObjectFromDataReader(dr, obj);

                Assert.AreEqual(expectedNameINValue, obj.NameIN);
                Assert.AreNotEqual(unexpectedToDbUuidValue, obj.ToDbUuid);
                Assert.AreEqual(expectedToDbUuidValue, obj.ToDbUuid);
                Assert.AreEqual(expectedFromDbUuid, obj.FromDbId);
            }
        }

        [TestCleanup]
        public void TestCleanp()
        {
            conn.Dispose();
            command.Dispose();
        }
    }
}
