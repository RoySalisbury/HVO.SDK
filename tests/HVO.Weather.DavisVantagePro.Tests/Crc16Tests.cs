using System;
using HVO.Core.Security.Cryptography;

namespace HVO.Weather.DavisVantagePro.Tests;

[TestClass]
public class Crc16Tests
{
    [TestMethod]
    public void ComputeHash_EmptyArray_ReturnsZeroSeed()
    {
        using var crc = new Crc16();
        var result = crc.ComputeHash(Array.Empty<byte>());

        // CRC-16 CCITT with seed=0 and empty input returns 0
        Assert.AreEqual(2, result.Length);
        ushort value = (ushort)((result[0] << 8) | result[1]);
        Assert.AreEqual((ushort)0, value, "CRC of empty input with seed 0 should be 0");
    }

    [TestMethod]
    public void ComputeHash_KnownInput_ReturnsExpectedCrc()
    {
        using var crc = new Crc16();
        // "123456789" is a standard test vector for CRC algorithms
        byte[] input = System.Text.Encoding.ASCII.GetBytes("123456789");
        var result = crc.ComputeHash(input);

        ushort value = (ushort)((result[0] << 8) | result[1]);
        // CRC-16 CCITT (XModem, poly 0x1021, init 0) of "123456789" = 0x31C3
        Assert.AreEqual((ushort)0x31C3, value, $"Expected 0x31C3, got 0x{value:X4}");
    }

    [TestMethod]
    public void ComputeHash_SingleByte_ReturnsNonZero()
    {
        using var crc = new Crc16();
        byte[] input = { 0x41 }; // 'A'
        var result = crc.ComputeHash(input);

        ushort value = (ushort)((result[0] << 8) | result[1]);
        Assert.AreNotEqual((ushort)0, value, "CRC of non-empty input should be non-zero");
    }

    [TestMethod]
    public void ComputeHash_Offset_ComputesSubsetOnly()
    {
        using var crc = new Crc16();
        byte[] input = { 0x00, 0x41, 0x42, 0x00 }; // padding + "AB" + padding
        var resultSubset = crc.ComputeHash(input, 1, 2);

        using var crc2 = new Crc16();
        byte[] ab = { 0x41, 0x42 };
        var resultDirect = crc2.ComputeHash(ab);

        Assert.AreEqual((ushort)((resultDirect[0] << 8) | resultDirect[1]), (ushort)((resultSubset[0] << 8) | resultSubset[1]),
            "Computing CRC on a subset should match direct computation on the same bytes");
    }

    [TestMethod]
    public void HashSize_Returns16()
    {
        using var crc = new Crc16();
        Assert.AreEqual(16, crc.HashSize);
    }
}
