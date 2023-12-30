﻿using System;
using System.Collections.Generic;
using System.Numerics;
using DuckDB.NET.Data;
using FluentAssertions;
using Xunit;

namespace DuckDB.NET.Test;

public class DuckDBDataReaderTestAllTypes : DuckDBTestBase
{
    private readonly DuckDBDataReader reader;

    public DuckDBDataReaderTestAllTypes(DuckDBDatabaseFixture db) : base(db)
    {
        Command.CommandText = "Select * from test_all_types(use_large_enum = true)";
        reader = Command.ExecuteReader();
        reader.Read();
    }

    private void VerifyDataStruct<T>(string columnName, int columnIndex, IReadOnlyList<T> data) where T : struct
    {
        reader.GetOrdinal(columnName).Should().Be(columnIndex);

        reader.GetValue(columnIndex).Should().Be(data[0]);
        reader.GetFieldValue<T>(columnIndex).Should().Be(data[0]);

        reader.Read();

        reader.GetValue(columnIndex).Should().Be(data[1]);
        reader.GetFieldValue<T>(columnIndex).Should().Be(data[1]);

        reader.Read();

        reader.IsDBNull(columnIndex).Should().Be(true);
        reader.GetFieldValue<T?>(columnIndex).Should().Be(null);
    }

    private void VerifyDataClass<T>(string columnName, int columnIndex, IReadOnlyList<T> data) where T : class
    {
        reader.GetOrdinal(columnName).Should().Be(columnIndex);

        reader.GetValue(columnIndex).Should().Be(data[0]);
        reader.GetFieldValue<T>(columnIndex).Should().Be(data[0]);

        reader.Read();

        reader.GetValue(columnIndex).Should().Be(data[1]);
        reader.GetFieldValue<T>(columnIndex).Should().Be(data[1]);

        reader.Read();

        reader.IsDBNull(columnIndex).Should().Be(true);
    }

    [Fact]
    public void ReadBool()
    {
        VerifyDataStruct<bool>("bool", 0, new List<bool> { false, true });
    }

    [Fact]
    public void ReadTinyInt()
    {
        VerifyDataStruct<sbyte>("tinyint", 1, new List<sbyte> { sbyte.MinValue, sbyte.MaxValue });
    }

    [Fact]
    public void ReadSmallInt()
    {
        VerifyDataStruct<short>("smallint", 2, new List<short> { short.MinValue, short.MaxValue });
    }

    [Fact]
    public void ReadInt()
    {
        VerifyDataStruct<int>("int", 3, new List<int> { int.MinValue, int.MaxValue });
    }

    [Fact]
    public void ReadBigInt()
    {
        VerifyDataStruct<long>("bigint", 4, new List<long> { long.MinValue, long.MaxValue });
    }

    [Fact]
    public void ReadHugeInt()
    {
        VerifyDataStruct<BigInteger>("hugeint", 5, new List<BigInteger>
        {
            BigInteger.Parse("-170141183460469231731687303715884105727"),
            BigInteger.Parse("170141183460469231731687303715884105727")
        });
    }

    [Fact]
    public void ReadUTinyInt()
    {
        VerifyDataStruct<byte>("utinyint", 6, new List<byte> { 0, byte.MaxValue });
    }

    [Fact]
    public void ReadUSmallInt()
    {
        VerifyDataStruct<ushort>("usmallint", 7, new List<ushort> { 0, ushort.MaxValue });
    }

    [Fact]
    public void ReadUInt()
    {
        VerifyDataStruct<uint>("uint", 8, new List<uint> { 0, uint.MaxValue });
    }

    [Fact]
    public void ReadUBigInt()
    {
        VerifyDataStruct<ulong>("ubigint", 9, new List<ulong> { 0, ulong.MaxValue });
    }

    [Fact]
    public void ReadDate()
    {
        VerifyDataStruct<DuckDBDateOnly>("date", 10, new List<DuckDBDateOnly>
        {
            new DuckDBDateOnly(-5877641, 6, 25),
            new DuckDBDateOnly(5881580, 7, 10)
        });
    }

    [Fact]
    public void ReadTime()
    {
        VerifyDataStruct<DuckDBTimeOnly>("time", 11, new List<DuckDBTimeOnly>
        {
            new DuckDBTimeOnly(0,0,0),
            new DuckDBTimeOnly(23, 59, 59,999999)
        });
    }

    [Fact(Skip = "These dates can't be expressed by DateTime")]
    public void ReadTimeStamp()
    {
        VerifyDataStruct<DuckDBTimestamp>("timestamp", 12, new List<DuckDBTimestamp>
        {
            new DuckDBTimestamp(new DuckDBDateOnly(-290308, 12, 22), new DuckDBTimeOnly(0,0,0)),
            new DuckDBTimestamp(new DuckDBDateOnly(294247, 1, 10), new DuckDBTimeOnly(4,0,54,775806))
        });
    }

    [Fact]
    public void ReadFloat()
    {
        VerifyDataStruct<float>("float", 18, new List<float> { float.MinValue, float.MaxValue });
    }

    [Fact]
    public void ReadDouble()
    {
        VerifyDataStruct<double>("double", 19, new List<double> { double.MinValue, double.MaxValue });
    }

    [Fact]
    public void ReadDecimal1()
    {
        VerifyDataStruct<decimal>("dec_4_1", 20, new List<decimal> { -999.9m, 999.9m });
    }

    [Fact]
    public void ReadDecimal2()
    {
        VerifyDataStruct<decimal>("dec_9_4", 21, new List<decimal> { -99999.9999m, 99999.9999m });
    }

    [Fact]
    public void ReadDecimal3()
    {
        VerifyDataStruct<decimal>("dec_18_6", 22, new List<decimal> { -999999999999.999999m, 999999999999.999999m });
    }

    [Fact]
    public void ReadDecimal4()
    {
        VerifyDataStruct<decimal>("dec38_10", 23, new List<decimal> { -9999999999999999999999999999.9999999999m, 9999999999999999999999999999.9999999999m });
    }

    [Fact]
    public void ReadGuid()
    {
        VerifyDataStruct<Guid>("uuid", 24, new List<Guid> { Guid.Parse("00000000-0000-0000-0000-000000000001"), Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff") });
    }

    [Fact]
    public void ReadString()
    {
        VerifyDataClass<string>("varchar", 26, new List<string> { "🦆🦆🦆🦆🦆🦆", "goo\0se" });
    }

    [Fact]
    public void ReadBit()
    {
        VerifyDataClass<string>("bit", 28, new List<string> { "0010001001011100010101011010111", "10101" });
    }

    [Fact]
    public void ReadSmallEnum()
    {
        VerifyDataClass<string>("small_enum", 29, new List<string> { "DUCK_DUCK_ENUM", "GOOSE"});
    }

    [Fact]
    public void ReadMediumEnum()
    {
        VerifyDataClass<string>("medium_enum", 30, new List<string> { "enum_0", "enum_299" });
    }

    [Fact]
    public void ReadLargeEnum()
    {
        VerifyDataClass<string>("large_enum", 31, new List<string> { "enum_0", "enum_69999" });
    }
}