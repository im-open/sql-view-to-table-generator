using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shouldly;
using Xunit;

namespace viewObjectBuilder.unitTest
{
    public class OrmProjectToolsTest
    {

        public OrmProjectToolsTest()
        {
        }

        [Theory,
         InlineData("char", false, "string"),
         InlineData("char", true, "string"),
         InlineData("varchar", false, "string"),
         InlineData("nvarchar", false, "string"),
         InlineData("bit", false, "bool"),
         InlineData("bit", true, "bool?"),
         InlineData("int", false, "int"),
         InlineData("int", true, "int?"),
         InlineData("tinyint", false, "int"),
         InlineData("tinyint", true, "int?"),
         InlineData("bigint", false, "long"),
         InlineData("bigint", true, "long?"),
         InlineData("datetime", false, "DateTime"),
         InlineData("datetime", true, "DateTime?"),
         InlineData("datetime2", false, "DateTime"),
         InlineData("datetime2", true, "DateTime?"),
         InlineData("date", false, "DateTime"),
         InlineData("date", true, "DateTime?"),
         InlineData("datetimeoffset", false, "DateTime"),
         InlineData("datetimeoffset", true, "DateTime?"),
         InlineData("money", false, "decimal"),
         InlineData("money", true, "decimal?"),
         InlineData("decimal", false, "decimal"),
         InlineData("decimal", true, "decimal?"),
         InlineData("unknown", false, "object"),
         InlineData("unknown", true, "object")]
        public void db_type_to_right_c_sharp_type(string dbType, bool nullable, string expectedCSharpType)
            => OrmProjectTools.sqlToCSharpType(dbType, nullable).ShouldBe(expectedCSharpType);
    }
}
