namespace BancosBrasileiros.MergeTool.Tests.Dto;

using System;
using System.Text.RegularExpressions;
using BancosBrasileiros.MergeTool.Dto;
using BancosBrasileiros.MergeTool.Helpers;
using FluentAssertions;
using Xunit;

public class BankTests
{
    #region CompeString

    [Fact]
    public void CompeString_ShouldFormatAsThreeDigits()
    {
        var bank = new Bank { Compe = 5 };
        bank.CompeString.Should().Be("005");
    }

    [Fact]
    public void CompeString_Setter_ShouldParseToCompe()
    {
        var bank = new Bank { CompeString = "042" };
        bank.Compe.Should().Be(42);
    }

    [Fact]
    public void CompeString_WhenCompeIsZero_ShouldFormatAs000()
    {
        var bank = new Bank { Compe = 0 };
        bank.CompeString.Should().Be("000");
    }

    [Fact]
    public void CompeString_WhenCompeIs999_ShouldFormatAs999()
    {
        var bank = new Bank { Compe = 999 };
        bank.CompeString.Should().Be("999");
    }

    [Fact]
    public void CompeString_WhenSetToInvalidString_ShouldNotChangeCompe()
    {
        var bank = new Bank { Compe = 10 };
        bank.CompeString = "abc";
        bank.Compe.Should().Be(10);
    }

    #endregion

    #region IspbString

    [Fact]
    public void IspbString_ShouldFormatAsEightDigits()
    {
        var bank = new Bank { Ispb = 360305 };
        bank.IspbString.Should().Be("00360305");
    }

    [Fact]
    public void IspbString_Setter_ShouldParseToIspb()
    {
        var bank = new Bank { IspbString = "00360305" };
        bank.Ispb.Should().Be(360305);
    }

    [Fact]
    public void IspbString_WhenIspbIsZero_ShouldFormatAs00000000()
    {
        var bank = new Bank { Ispb = 0 };
        bank.IspbString.Should().Be("00000000");
    }

    [Fact]
    public void IspbString_WhenSetToInvalidString_ShouldNotChangeIspb()
    {
        var bank = new Bank { Ispb = 100 };
        bank.IspbString = "notanumber";
        bank.Ispb.Should().Be(100);
    }

    #endregion

    #region Document

    [Fact]
    public void Document_WhenSetToNull_ShouldRemainNull()
    {
        var bank = new Bank();
        bank.Document = null;
        bank.Document.Should().BeNull();
    }

    [Fact]
    public void Document_WhenSetToWhitespace_ShouldRemainNull()
    {
        var bank = new Bank();
        bank.Document = "   ";
        bank.Document.Should().BeNull();
    }

    [Fact]
    public void Document_WhenBancoDoBrasilIspb_ShouldProduceCorrectCnpj()
    {
        var bank = new Bank { Document = "00000000" };
        bank.Document.Should().Be("00.000.000/0001-91");
    }

    [Fact]
    public void Document_WhenPreFormattedCnpj_ShouldPreserveFormat()
    {
        var bank = new Bank { Document = "00.000.000/0001-91" };
        bank.Document.Should().Be("00.000.000/0001-91");
    }

    [Fact]
    public void Document_WhenEightDigitIspb_ShouldProduceFormattedCnpj()
    {
        var bank = new Bank { Document = "00360305" };
        bank.Document.Should().NotBeNull();
        bank.Document.Should().HaveLength(18);
        bank.Document.Should().MatchRegex(@"^\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2}$");
    }

    [Fact]
    public void Document_WhenFourteenDigitCnpj_ShouldFormatCorrectly()
    {
        var bank = new Bank { Document = "00000000000191" };
        bank.Document.Should().Be("00.000.000/0001-91");
    }

    #endregion

    #region Url

    [Fact]
    public void Url_WhenSetToNull_ShouldRemainNull()
    {
        var bank = new Bank();
        bank.Url = null;
        bank.Url.Should().BeNull();
    }

    [Fact]
    public void Url_WhenSetToNA_ShouldRemainNull()
    {
        var bank = new Bank();
        bank.Url = "NA";
        bank.Url.Should().BeNull();
    }

    [Fact]
    public void Url_WhenSetToWhitespace_ShouldRemainNull()
    {
        var bank = new Bank();
        bank.Url = "   ";
        bank.Url.Should().BeNull();
    }

    [Fact]
    public void Url_WithoutProtocol_ShouldAddHttps()
    {
        var bank = new Bank { Url = "banco.com.br" };
        bank.Url.Should().Be("https://banco.com.br");
    }

    [Fact]
    public void Url_WithHttpsProtocol_ShouldNormalize()
    {
        var bank = new Bank { Url = "https://banco.com.br" };
        bank.Url.Should().Be("https://banco.com.br");
    }

    [Fact]
    public void Url_WithUppercaseHttps_ShouldLowercase()
    {
        var bank = new Bank { Url = "HTTPS://BANCO.COM.BR" };
        bank.Url.Should().Be("https://banco.com.br");
    }

    #endregion

    #region Charge

    [Fact]
    public void Charge_WhenChargeStrIsNull_ShouldReturnNull()
    {
        var bank = new Bank();
        bank.Charge.Should().BeNull();
    }

    [Fact]
    public void Charge_WhenChargeStrIsSim_ShouldReturnTrue()
    {
        var bank = new Bank { ChargeStr = "sim" };
        bank.Charge.Should().BeTrue();
    }

    [Fact]
    public void Charge_WhenChargeStrIsNao_ShouldReturnFalse()
    {
        var bank = new Bank { ChargeStr = "não" };
        bank.Charge.Should().BeFalse();
    }

    [Fact]
    public void Charge_WhenSetToTrue_ShouldSetChargeStrToSim()
    {
        var bank = new Bank { Charge = true };
        bank.ChargeStr.Should().Be("sim");
    }

    [Fact]
    public void Charge_WhenSetToFalse_ShouldSetChargeStrToNao()
    {
        var bank = new Bank { Charge = false };
        bank.ChargeStr.Should().Be("não");
    }

    [Fact]
    public void Charge_WhenSetToNull_ShouldNotChangeChargeStr()
    {
        var bank = new Bank { ChargeStr = "sim" };
        bank.Charge = null;
        bank.ChargeStr.Should().Be("sim");
    }

    #endregion

    #region CreditDocument

    [Fact]
    public void CreditDocument_WhenCreditDocumentStrIsNull_ShouldReturnNull()
    {
        var bank = new Bank();
        bank.CreditDocument.Should().BeNull();
    }

    [Fact]
    public void CreditDocument_WhenCreditDocumentStrIsSim_ShouldReturnTrue()
    {
        var bank = new Bank { CreditDocumentStr = "sim" };
        bank.CreditDocument.Should().BeTrue();
    }

    [Fact]
    public void CreditDocument_WhenSetToFalse_ShouldSetCreditDocumentStrToNao()
    {
        var bank = new Bank { CreditDocument = false };
        bank.CreditDocumentStr.Should().Be("não");
    }

    #endregion

    #region Pcr / Pcrp

    [Fact]
    public void Pcr_WhenPcrStrIsSim_ShouldReturnTrue()
    {
        var bank = new Bank { PcrStr = "sim" };
        bank.Pcr.Should().BeTrue();
    }

    [Fact]
    public void Pcr_WhenSetToTrue_ShouldSetPcrStrToSim()
    {
        var bank = new Bank { Pcr = true };
        bank.PcrStr.Should().Be("sim");
    }

    [Fact]
    public void Pcrp_WhenPcrpStrIsNao_ShouldReturnFalse()
    {
        var bank = new Bank { PcrpStr = "não" };
        bank.Pcrp.Should().BeFalse();
    }

    [Fact]
    public void Pcrp_WhenSetToFalse_ShouldSetPcrpStrToNao()
    {
        var bank = new Bank { Pcrp = false };
        bank.PcrpStr.Should().Be("não");
    }

    #endregion

    #region SetChange / AcceptChanges / RejectChanges

    [Fact]
    public void SetChange_ShouldRecordChange()
    {
        var bank = new Bank { LongName = "Old Name" };

        bank.SetChange(Source.Str, x => x.LongName, "New Name");

        var changes = bank.GetChanges();
        changes.Keys.Should().Contain("LongName");
        changes["LongName"].OldValue.Should().Be("Old Name");
        changes["LongName"].NewValue.Should().Be("New Name");
        changes["LongName"].Source.Should().Be(Source.Str);
    }

    [Fact]
    public void SetChange_ShouldUpdateTheProperty()
    {
        var bank = new Bank { LongName = "Old Name" };

        bank.SetChange(Source.Str, x => x.LongName, "New Name");

        bank.LongName.Should().Be("New Name");
    }

    [Fact]
    public void SetChange_ShouldUpdateDateUpdated()
    {
        var bank = new Bank { LongName = "Name" };
        var before = DateTimeOffset.UtcNow;

        bank.SetChange(Source.Str, x => x.LongName, "New Name");

        bank.DateUpdated.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void SetChange_WhenPropertyAlreadyChanged_ShouldOverwrite()
    {
        var bank = new Bank { LongName = "Original" };
        bank.SetChange(Source.Str, x => x.LongName, "First Change");
        bank.SetChange(Source.Sitraf, x => x.LongName, "Second Change");

        var changes = bank.GetChanges();
        changes["LongName"].NewValue.Should().Be("Second Change");
        changes["LongName"].Source.Should().Be(Source.Sitraf);
    }

    [Fact]
    public void SetChange_ForValueTypeProperty_ShouldRecordChange()
    {
        var bank = new Bank { Compe = 1 };

        bank.SetChange(Source.Str, x => x.Compe, 42);

        bank.Compe.Should().Be(42);
        bank.GetChanges().Keys.Should().Contain("Compe");
    }

    [Fact]
    public void AcceptChanges_ShouldClearChanges()
    {
        var bank = new Bank { LongName = "Name" };
        bank.SetChange(Source.Str, x => x.LongName, "New Name");

        bank.AcceptChanges();

        bank.HasChanges.Should().BeFalse();
        bank.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RejectChanges_ShouldRevertStringProperties()
    {
        var bank = new Bank { ShortName = "OldShort" };
        bank.SetChange(Source.Str, x => x.ShortName, "NewShort");

        bank.RejectChanges();

        bank.ShortName.Should().Be("OldShort");
        bank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void HasChanges_WhenNoChanges_ShouldBeFalse()
    {
        var bank = new Bank();
        bank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void HasChanges_AfterSetChange_ShouldBeTrue()
    {
        var bank = new Bank { LongName = "Name" };
        bank.SetChange(Source.Str, x => x.LongName, "New Name");
        bank.HasChanges.Should().BeTrue();
    }

    [Fact]
    public void HasChanges_AfterAcceptChanges_ShouldBeFalse()
    {
        var bank = new Bank { LongName = "Name" };
        bank.SetChange(Source.Str, x => x.LongName, "New Name");
        bank.AcceptChanges();
        bank.HasChanges.Should().BeFalse();
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_SameInstance_ShouldBeTrue()
    {
        var bank = new Bank { Compe = 1, LongName = "Banco do Brasil" };
        bank.Equals(bank).Should().BeTrue();
    }

    [Fact]
    public void Equals_Null_ShouldBeFalse()
    {
        var bank = new Bank { Compe = 1 };
        bank.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void Equals_SameValues_ShouldBeTrue()
    {
        var bank1 = new Bank { Compe = 1, Ispb = 0, LongName = "Banco do Brasil S.A.", ShortName = "BCO DO BRASIL" };
        var bank2 = new Bank { Compe = 1, Ispb = 0, LongName = "Banco do Brasil S.A.", ShortName = "BCO DO BRASIL" };
        bank1.Equals(bank2).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentCompe_ShouldBeFalse()
    {
        var bank1 = new Bank { Compe = 1, LongName = "Bank A" };
        var bank2 = new Bank { Compe = 2, LongName = "Bank A" };
        bank1.Equals(bank2).Should().BeFalse();
    }

    [Fact]
    public void Equals_DifferentLongName_ShouldBeFalse()
    {
        var bank1 = new Bank { Compe = 1, LongName = "Bank A" };
        var bank2 = new Bank { Compe = 1, LongName = "Bank B" };
        bank1.Equals(bank2).Should().BeFalse();
    }

    [Fact]
    public void EqualityOperator_SameValues_ShouldBeTrue()
    {
        var bank1 = new Bank { Compe = 5, ShortName = "Test" };
        var bank2 = new Bank { Compe = 5, ShortName = "Test" };
        (bank1 == bank2).Should().BeTrue();
    }

    [Fact]
    public void InequalityOperator_DifferentValues_ShouldBeTrue()
    {
        var bank1 = new Bank { Compe = 1 };
        var bank2 = new Bank { Compe = 2 };
        (bank1 != bank2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_SameValues_ShouldBeEqual()
    {
        var bank1 = new Bank { Compe = 1, LongName = "Banco" };
        var bank2 = new Bank { Compe = 1, LongName = "Banco" };
        bank1.GetHashCode().Should().Be(bank2.GetHashCode());
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_ShouldIncludeCompe()
    {
        var bank = new Bank { Compe = 5 };
        bank.ToString().Should().Contain("COMPE: 005");
    }

    [Fact]
    public void ToString_WithLongName_ShouldIncludeLongName()
    {
        var bank = new Bank { Compe = 1, LongName = "Banco do Brasil S.A." };
        bank.ToString().Should().Contain("Long name: Banco do Brasil S.A.");
    }

    [Fact]
    public void ToString_WithProducts_ShouldIncludeProducts()
    {
        var bank = new Bank { Compe = 1, Products = new[] { "DOC", "TED" } };
        bank.ToString().Should().Contain("Products: DOC,TED");
    }

    [Fact]
    public void ToString_WithIspb_ShouldIncludeIspb()
    {
        var bank = new Bank { Compe = 1, Ispb = 12345678 };
        bank.ToString().Should().Contain("ISPB: 12345678");
    }

    [Fact]
    public void ToString_WhenIspbIsZeroAndCompeIsOne_ShouldIncludeIspb()
    {
        var bank = new Bank { Compe = 1, Ispb = 0 };
        bank.ToString().Should().Contain("ISPB: 00000000");
    }

    #endregion
}
