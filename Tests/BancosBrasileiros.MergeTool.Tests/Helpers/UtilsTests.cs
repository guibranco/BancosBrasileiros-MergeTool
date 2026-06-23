namespace BancosBrasileiros.MergeTool.Tests.Helpers;

using BancosBrasileiros.MergeTool.Dto;
using BancosBrasileiros.MergeTool.Helpers;
using FluentAssertions;
using Xunit;

public class UtilsTests
{
    #region GetStringValue

    [Fact]
    public void GetStringValue_WhenNull_ShouldReturnNullString()
    {
        object value = null;
        value.GetStringValue().Should().Be("Null");
    }

    [Fact]
    public void GetStringValue_WhenNonEmptyString_ShouldReturnString()
    {
        object value = "hello";
        value.GetStringValue().Should().Be("hello");
    }

    [Fact]
    public void GetStringValue_WhenEmptyString_ShouldReturnEmpty()
    {
        object value = "";
        value.GetStringValue().Should().Be("Empty");
    }

    [Fact]
    public void GetStringValue_WhenWhitespaceString_ShouldReturnEmpty()
    {
        object value = "   ";
        value.GetStringValue().Should().Be("Empty");
    }

    [Fact]
    public void GetStringValue_WhenBoolTrue_ShouldReturnTrueString()
    {
        object value = true;
        value.GetStringValue().Should().Be("True");
    }

    [Fact]
    public void GetStringValue_WhenBoolFalse_ShouldReturnFalseString()
    {
        object value = false;
        value.GetStringValue().Should().Be("False");
    }

    [Fact]
    public void GetStringValue_WhenStringArray_ShouldReturnJoinedWithCommaSpace()
    {
        object value = new[] { "DOC", "TED", "PIX" };
        value.GetStringValue().Should().Be("DOC, TED, PIX");
    }

    [Fact]
    public void GetStringValue_WhenEmptyStringArray_ShouldReturnEmptyJoined()
    {
        object value = new string[0];
        value.GetStringValue().Should().Be("");
    }

    [Fact]
    public void GetStringValue_WhenInteger_ShouldReturnEmpty()
    {
        object value = 42;
        value.GetStringValue().Should().Be("Empty");
    }

    #endregion

    #region DeepClone

    [Fact]
    public void DeepClone_ShouldReturnNewInstance()
    {
        var bank = new Bank { Compe = 1, LongName = "Banco do Brasil S.A." };

        var clone = bank.DeepClone();

        clone.Should().NotBeSameAs(bank);
    }

    [Fact]
    public void DeepClone_ShouldCopyPropertyValues()
    {
        var bank = new Bank
        {
            Compe = 1,
            LongName = "Banco do Brasil S.A.",
            ShortName = "BCO DO BRASIL",
            Network = "RSFN",
            Type = "Banco Múltiplo",
        };

        var clone = bank.DeepClone();

        clone.Compe.Should().Be(1);
        clone.LongName.Should().Be("Banco do Brasil S.A.");
        clone.ShortName.Should().Be("BCO DO BRASIL");
        clone.Network.Should().Be("RSFN");
        clone.Type.Should().Be("Banco Múltiplo");
    }

    [Fact]
    public void DeepClone_WhenOriginalModified_ShouldNotAffectClone()
    {
        var bank = new Bank { Compe = 1, LongName = "Original Name" };

        var clone = bank.DeepClone();
        bank.LongName = "Modified Name";

        clone.LongName.Should().Be("Original Name");
    }

    [Fact]
    public void DeepClone_WhenCloneModified_ShouldNotAffectOriginal()
    {
        var bank = new Bank { Compe = 1, LongName = "Original Name" };

        var clone = bank.DeepClone();
        clone.LongName = "Modified Name";

        bank.LongName.Should().Be("Original Name");
    }

    [Fact]
    public void DeepClone_WithArrayProperty_ShouldCopyArray()
    {
        var bank = new Bank { Compe = 1, Products = new[] { "DOC", "TED" } };

        var clone = bank.DeepClone();

        clone.Products.Should().BeEquivalentTo(bank.Products);
        clone.Products.Should().NotBeSameAs(bank.Products);
    }

    #endregion
}
