namespace Urdep.Extensions.Data;

////public static class SqlStuff
////{
////    public static string FromSqlType(string sqlTypeString)
////    {
////        if (!Enum.TryParse(sqlTypeString, out SQLType typeCode))
////        {
////            throw new Exception("sql type not found");
////        }
////        switch (typeCode)
////        {
////            case SQLType.varbinary:
////            case SQLType.binary:
////            case SQLType.filestream:
////            case SQLType.image:
////            case SQLType.rowversion:
////            case SQLType.timestamp: //?
////                return "byte[]";
////            case SQLType.tinyint:
////                return "byte";
////            case SQLType.varchar:
////            case SQLType.nvarchar:
////            case SQLType.nchar:
////            case SQLType.text:
////            case SQLType.ntext:
////            case SQLType.xml:
////                return "string";
////            case SQLType.@char:
////                return "char";
////            case SQLType.bigint:
////                return "long";
////            case SQLType.bit:
////                return "bool";
////            case SQLType.smalldatetime:
////            case SQLType.datetime:
////            case SQLType.date:
////            case SQLType.datetime2:
////                return "DateTime";
////            case SQLType.datetimeoffset:
////                return "DateTimeOffset";
////            case SQLType.@decimal:
////            case SQLType.money:
////            case SQLType.numeric:
////            case SQLType.smallmoney:
////                return "decimal";
////            case SQLType.@float:
////                return "double";
////            case SQLType.@int:
////                return "int";
////            case SQLType.real:
////                return "Single";
////            case SQLType.smallint:
////                return "short";
////            case SQLType.uniqueidentifier:
////                return "Guid";
////            case SQLType.sql_variant:
////                return "object";
////            case SQLType.time:
////                return "TimeSpan";
////            default:
////                throw new Exception("none equal type");
////        }
////    }

////    public enum SQLType
////    {
////        varbinary, //(1)
////        binary, //(1)
////        image,
////        varchar,
////        @char,
////        nvarchar, //(1)
////        nchar, //(1)
////        text,
////        ntext,
////        uniqueidentifier,
////        rowversion,
////        bit,
////        tinyint,
////        smallint,
////        @int,
////        bigint,
////        smallmoney,
////        money,
////        numeric,
////        @decimal,
////        real,
////        @float,
////        smalldatetime,
////        datetime,
////        sql_variant,
////        table,
////        cursor,
////        timestamp,
////        xml,
////        date,
////        datetime2,
////        datetimeoffset,
////        filestream,
////        time,
////    }
////}
