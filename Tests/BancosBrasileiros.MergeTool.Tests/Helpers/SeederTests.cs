namespace BancosBrasileiros.MergeTool.Tests.Helpers;

using System;
using System.Collections.Generic;
using BancosBrasileiros.MergeTool.Dto;
using BancosBrasileiros.MergeTool.Helpers;
using FluentAssertions;
using Xunit;

public class SeederTests
{
    private static Bank MakeBank(int compe, int ispb, string longName, string shortName = null, string document = null) =>
        new()
        {
            Compe = compe,
            Ispb = ispb,
            LongName = longName,
            ShortName = shortName ?? longName,
            Document = document,
            DateRegistered = DateTimeOffset.UtcNow,
        };

    #region Constructor

    [Fact]
    public void Constructor_WhenSourceIsNull_ShouldThrowArgumentNullException()
    {
        var act = () => new Seeder(null);
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("source");
    }

    #endregion

    #region GenerateMissingDocument

    [Fact]
    public void GenerateMissingDocument_WhenDocumentIsMissing_ShouldGenerateIt()
    {
        var bank = new Bank { Compe = 1, Ispb = 0, LongName = "Banco do Brasil" };
        var source = new List<Bank> { bank };
        var seeder = new Seeder(source);

        seeder.GenerateMissingDocument();

        bank.HasChanges.Should().BeTrue();
        bank.GetChanges().Keys.Should().Contain("Document");
    }

    [Fact]
    public void GenerateMissingDocument_WhenDocumentAlreadyPresent_ShouldNotChange()
    {
        var bank = MakeBank(1, 0, "Banco do Brasil");
        bank.Document = "00.000.000/0001-91";
        var source = new List<Bank> { bank };
        var seeder = new Seeder(source);

        seeder.GenerateMissingDocument();

        bank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void GenerateMissingDocument_ShouldReturnSelf()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        seeder.GenerateMissingDocument().Should().BeSameAs(seeder);
    }

    #endregion

    #region SeedStr

    [Fact]
    public void SeedStr_WhenBankNotInSource_ShouldAddIt()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        var newBank = MakeBank(999, 12345678, "New Bank S.A.", "NEW BANK");
        newBank.Document = "12.345.678/0001-00";

        seeder.SeedStr(new[] { newBank });

        source.Should().ContainSingle(b => b.Compe == 999);
    }

    [Fact]
    public void SeedStr_WhenCompeIsZero_ShouldSkip()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        var bank = MakeBank(0, 0, "Invalid Bank");

        seeder.SeedStr(new[] { bank });

        source.Should().BeEmpty();
    }

    [Fact]
    public void SeedStr_WhenNamesAreSame_ShouldNotAddChanges()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.", "BCO DO BRASIL");
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var strBank = MakeBank(1, 0, "Banco do Brasil S.A.", "BCO DO BRASIL");

        seeder.SeedStr(new[] { strBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void SeedStr_WhenLongNameDiffers_ShouldRecordChange()
    {
        var existingBank = MakeBank(1, 0, "Old Long Name", "SHORT");
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var strBank = MakeBank(1, 0, "New Long Name", "SHORT");

        seeder.SeedStr(new[] { strBank });

        existingBank.HasChanges.Should().BeTrue();
        existingBank.GetChanges().Keys.Should().Contain("LongName");
    }

    [Fact]
    public void SeedStr_WhenShortNameDiffers_ShouldRecordChange()
    {
        var existingBank = MakeBank(1, 0, "Same Long Name", "OLD SHORT");
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var strBank = MakeBank(1, 0, "Same Long Name", "NEW SHORT");

        seeder.SeedStr(new[] { strBank });

        existingBank.HasChanges.Should().BeTrue();
        existingBank.GetChanges().Keys.Should().Contain("ShortName");
    }

    [Fact]
    public void SeedStr_WhenNewBankHasNoDocument_ShouldSetDocumentFromIspb()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        var newBank = new Bank
        {
            Compe = 500,
            Ispb = 12345678,
            LongName = "Test Bank S.A.",
            ShortName = "TEST BANK",
            DateRegistered = DateTimeOffset.UtcNow,
        };

        seeder.SeedStr(new[] { newBank });

        source.Should().ContainSingle();
        source[0].Document.Should().NotBeNull();
    }

    #endregion

    #region SeedSitraf

    [Fact]
    public void SeedSitraf_WhenBankNotInSource_ShouldAddIt()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        var newBank = MakeBank(999, 12345678, "Banco Novo S.A.");

        seeder.SeedSitraf(new[] { newBank });

        source.Should().ContainSingle(b => b.Compe == 999);
    }

    [Fact]
    public void SeedSitraf_WhenNamesMatch_ShouldNotAddChanges()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var sitrafBank = MakeBank(1, 0, "Banco do Brasil S.A.");

        seeder.SeedSitraf(new[] { sitrafBank });

        existingBank.HasChanges.Should().BeFalse();
        source.Should().HaveCount(1);
    }

    [Fact]
    public void SeedSitraf_WhenNamesDiffer_ShouldNotRecordChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var sitrafBank = MakeBank(1, 0, "Banco do Brasil");

        seeder.SeedSitraf(new[] { sitrafBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void SeedSitraf_ShouldReturnSelf()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        seeder.SeedSitraf(Array.Empty<Bank>()).Should().BeSameAs(seeder);
    }

    #endregion

    #region SeedCql

    [Fact]
    public void SeedCql_WhenBankFoundAndLegalChequeFalse_ShouldSetLegalChequeTrue()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.LegalCheque = false;
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var cqlBank = new Bank { Ispb = 0, LongName = "Banco do Brasil", Document = "00.000.000/0001-91" };

        seeder.SeedCql(new[] { cqlBank });

        existingBank.HasChanges.Should().BeTrue();
        existingBank.GetChanges().Keys.Should().Contain("LegalCheque");
    }

    [Fact]
    public void SeedCql_WhenBankFoundAndLegalChequeTrue_ShouldNotAddChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.LegalCheque = true;
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var cqlBank = new Bank { Ispb = 0, LongName = "Banco do Brasil", Document = "00.000.000/0001-91" };

        seeder.SeedCql(new[] { cqlBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void SeedCql_WhenBankNotFound_ShouldNotModifySource()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var cqlBank = new Bank { Ispb = 99999999, LongName = "Unknown Bank", Document = "00.000.000/0001-00" };

        seeder.SeedCql(new[] { cqlBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void SeedCql_ShouldReturnSelf()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        seeder.SeedCql(Array.Empty<Bank>()).Should().BeSameAs(seeder);
    }

    #endregion

    #region SeedSpi

    [Fact]
    public void SeedSpi_WhenBankFoundAndPixTypeMatches_ShouldNotAddChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.PixType = "DRCT";
        existingBank.DatePixStarted = "01/01/2020";
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var spiBank = new Bank
        {
            Ispb = 0,
            LongName = "Banco do Brasil S.A.",
            PixType = "DRCT",
            DatePixStarted = "01/01/2020",
        };

        seeder.SeedSpi(new[] { spiBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void SeedSpi_WhenPixTypeDiffers_ShouldRecordChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.PixType = "INDIRECT";
        existingBank.DatePixStarted = "01/01/2020";
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var spiBank = new Bank
        {
            Ispb = 0,
            LongName = "Banco do Brasil S.A.",
            PixType = "DRCT",
            DatePixStarted = "01/01/2020",
        };

        seeder.SeedSpi(new[] { spiBank });

        existingBank.HasChanges.Should().BeTrue();
        existingBank.GetChanges().Keys.Should().Contain("PixType");
    }

    [Fact]
    public void SeedSpi_WhenBankNotFound_ShouldNotModifySource()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var spiBank = new Bank
        {
            Ispb = 99999999,
            LongName = "Unknown Bank",
            PixType = "DRCT",
            DatePixStarted = "01/01/2020",
        };

        seeder.SeedSpi(new[] { spiBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void SeedSpi_ShouldReturnSelf()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        seeder.SeedSpi(Array.Empty<Bank>()).Should().BeSameAs(seeder);
    }

    #endregion

    #region SeedSiloc

    [Fact]
    public void SeedSiloc_WhenBankFoundAndChargeAndCreditDocumentMatch_ShouldNotAddChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.Document = "00.000.000/0001-91";
        existingBank.Charge = true;
        existingBank.CreditDocument = true;
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var silocBank = new Bank
        {
            Ispb = 0,
            LongName = "Banco do Brasil S.A.",
            Document = "00.000.000/0001-91",
        };
        silocBank.Charge = true;
        silocBank.CreditDocument = true;

        seeder.SeedSiloc(new[] { silocBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void SeedSiloc_WhenChargeDiffers_ShouldRecordChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.Document = "00.000.000/0001-91";
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var silocBank = new Bank
        {
            Ispb = 0,
            LongName = "Banco do Brasil S.A.",
            Document = "00.000.000/0001-91",
        };
        silocBank.Charge = true;
        silocBank.CreditDocument = false;

        seeder.SeedSiloc(new[] { silocBank });

        existingBank.HasChanges.Should().BeTrue();
        existingBank.GetChanges().Keys.Should().Contain("Charge");
    }

    [Fact]
    public void SeedSiloc_ShouldReturnSelf()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        seeder.SeedSiloc(Array.Empty<Bank>()).Should().BeSameAs(seeder);
    }

    #endregion

    #region SeedPcps

    [Fact]
    public void SeedPcps_WhenSalaryPortabilityMatches_ShouldNotAddChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.Document = "00.000.000/0001-91";
        existingBank.SalaryPortability = "Destinatário e Originador";
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var pcpsBank = new Bank
        {
            Ispb = 0,
            LongName = "Banco do Brasil S.A.",
            Document = "00.000.000/0001-91",
            SalaryPortability = "Destinatário e Originador",
        };

        seeder.SeedPcps(new[] { pcpsBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void SeedPcps_WhenSalaryPortabilityDiffers_ShouldRecordChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.Document = "00.000.000/0001-91";
        existingBank.SalaryPortability = null;
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var pcpsBank = new Bank
        {
            Ispb = 0,
            LongName = "Banco do Brasil S.A.",
            Document = "00.000.000/0001-91",
            SalaryPortability = "Destinatário",
        };

        seeder.SeedPcps(new[] { pcpsBank });

        existingBank.HasChanges.Should().BeTrue();
        existingBank.GetChanges().Keys.Should().Contain("SalaryPortability");
    }

    [Fact]
    public void SeedPcps_ShouldReturnSelf()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        seeder.SeedPcps(Array.Empty<Bank>()).Should().BeSameAs(seeder);
    }

    #endregion

    #region SeedDetectaFlow

    [Fact]
    public void SeedDetectaFlow_WhenBankFoundAndDetectaFlowFalse_ShouldSetTrue()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.Document = "00.000.000/0001-91";
        existingBank.DetectaFlow = false;
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var dfBank = new Bank
        {
            Ispb = 0,
            LongName = "Banco do Brasil",
            Document = "00.000.000/0001-91",
        };

        seeder.SeedDetectaFlow(new[] { dfBank });

        existingBank.HasChanges.Should().BeTrue();
        existingBank.GetChanges().Keys.Should().Contain("DetectaFlow");
    }

    [Fact]
    public void SeedDetectaFlow_WhenDetectaFlowAlreadyTrue_ShouldNotAddChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.Document = "00.000.000/0001-91";
        existingBank.DetectaFlow = true;
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var dfBank = new Bank
        {
            Ispb = 0,
            LongName = "Banco do Brasil",
            Document = "00.000.000/0001-91",
        };

        seeder.SeedDetectaFlow(new[] { dfBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void SeedDetectaFlow_ShouldReturnSelf()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        seeder.SeedDetectaFlow(Array.Empty<Bank>()).Should().BeSameAs(seeder);
    }

    #endregion

    #region SeedPcr

    [Fact]
    public void SeedPcr_WhenPcrAndPcrpMatch_ShouldNotAddChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.Pcr = true;
        existingBank.Pcrp = false;
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var pcrBank = new Bank { Ispb = 0, LongName = "Banco do Brasil", Document = "00.000.000/0001-91" };
        pcrBank.Pcr = true;
        pcrBank.Pcrp = false;

        seeder.SeedPcr(new[] { pcrBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void SeedPcr_WhenPcrDiffers_ShouldRecordChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.Pcr = false;
        existingBank.Pcrp = false;
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var pcrBank = new Bank { Ispb = 0, LongName = "Banco do Brasil", Document = "00.000.000/0001-91" };
        pcrBank.Pcr = true;
        pcrBank.Pcrp = false;

        seeder.SeedPcr(new[] { pcrBank });

        existingBank.HasChanges.Should().BeTrue();
        existingBank.GetChanges().Keys.Should().Contain("Pcr");
    }

    [Fact]
    public void SeedPcr_WhenBankNotFound_ShouldNotModifySource()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var pcrBank = new Bank { Ispb = 99999999, LongName = "Unknown", Document = "00.000.000/0001-91" };
        pcrBank.Pcr = true;

        seeder.SeedPcr(new[] { pcrBank });

        existingBank.HasChanges.Should().BeFalse();
    }

    #endregion

    #region SeedSlc

    [Fact]
    public void SeedSlc_WhenBankFoundByDocumentWithMissingDocument_ShouldSetDocument()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.", "BCO BRASIL");
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var slcBank = new Bank
        {
            Compe = 1,
            LongName = "Banco do Brasil S.A.",
            Document = "00.000.000/0001-91",
        };

        seeder.SeedSlc(new[] { slcBank });

        existingBank.GetChanges().Keys.Should().Contain("Document");
    }

    [Fact]
    public void SeedSlc_ShouldReturnSelf()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        seeder.SeedSlc(Array.Empty<Bank>()).Should().BeSameAs(seeder);
    }

    #endregion

    #region SeedCtc

    [Fact]
    public void SeedCtc_WhenBankFoundAndProductsDiffer_ShouldRecordChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.Document = "00.000.000/0001-91";
        existingBank.Products = null;
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var ctcBank = new Bank
        {
            Ispb = 0,
            LongName = "Banco do Brasil S.A.",
            Document = "00.000.000/0001-91",
            Products = new[] { "DOC", "TED" },
        };

        seeder.SeedCtc(new[] { ctcBank });

        existingBank.HasChanges.Should().BeTrue();
        existingBank.GetChanges().Keys.Should().Contain("Products");
    }

    [Fact]
    public void SeedCtc_WhenProductsMatch_ShouldNotAddChange()
    {
        var existingBank = MakeBank(1, 0, "Banco do Brasil S.A.");
        existingBank.Document = "00.000.000/0001-91";
        existingBank.Products = new[] { "DOC", "TED" };
        var source = new List<Bank> { existingBank };
        var seeder = new Seeder(source);
        var ctcBank = new Bank
        {
            Ispb = 0,
            LongName = "Banco do Brasil S.A.",
            Document = "00.000.000/0001-91",
            Products = new[] { "DOC", "TED" },
        };

        seeder.SeedCtc(new[] { ctcBank });

        existingBank.GetChanges().Keys.Should().NotContain("Products");
    }

    [Fact]
    public void SeedCtc_ShouldReturnSelf()
    {
        var source = new List<Bank>();
        var seeder = new Seeder(source);
        seeder.SeedCtc(Array.Empty<Bank>()).Should().BeSameAs(seeder);
    }

    #endregion
}
