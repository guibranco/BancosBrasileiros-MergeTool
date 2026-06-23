namespace BancosBrasileiros.MergeTool.Tests.Integration;

using System;
using System.Collections.Generic;
using System.IO;
using BancosBrasileiros.MergeTool.Dto;
using BancosBrasileiros.MergeTool.Helpers;
using FluentAssertions;
using Xunit;

public class WriterIntegrationTests : IDisposable
{
    private readonly string _originalDirectory;
    private readonly string _tempDirectory;

    public WriterIntegrationTests()
    {
        _originalDirectory = Directory.GetCurrentDirectory();
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"WriterTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
        Directory.SetCurrentDirectory(_tempDirectory);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDirectory);
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
    }

    private static List<Bank> BuildSampleBanks() =>
        [
            new Bank
            {
                Compe = 1,
                Ispb = 0,
                LongName = "Banco do Brasil S.A.",
                ShortName = "BCO DO BRASIL",
                Network = "RSFN",
                Type = "Banco Múltiplo",
                LegalCheque = true,
                DetectaFlow = false,
                DateRegistered = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero),
                DateUpdated = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Document = "00.000.000/0001-91",
            },
            new Bank
            {
                Compe = 341,
                Ispb = 60746948,
                LongName = "Banco Itaú S.A.",
                ShortName = "BANCO ITAU",
                Network = "RSFN",
                Type = "Banco Múltiplo",
                LegalCheque = false,
                DetectaFlow = true,
                DateRegistered = new DateTimeOffset(2020, 2, 1, 0, 0, 0, TimeSpan.Zero),
                DateUpdated = new DateTimeOffset(2023, 2, 1, 0, 0, 0, TimeSpan.Zero),
                Document = "60.746.948/0001-12",
            },
        ];

    #region SaveBanks - Output Files

    [Fact]
    public void SaveBanks_ShouldCreateResultDirectory()
    {
        var banks = BuildSampleBanks();

        Writer.SaveBanks(banks);

        Directory.Exists("result").Should().BeTrue();
    }

    [Fact]
    public void SaveBanks_ShouldCreateJsonFile()
    {
        var banks = BuildSampleBanks();

        Writer.SaveBanks(banks);

        File.Exists(Path.Combine("result", "bancos.json")).Should().BeTrue();
    }

    [Fact]
    public void SaveBanks_ShouldCreateCsvFile()
    {
        var banks = BuildSampleBanks();

        Writer.SaveBanks(banks);

        File.Exists(Path.Combine("result", "bancos.csv")).Should().BeTrue();
    }

    [Fact]
    public void SaveBanks_ShouldCreateXmlFile()
    {
        var banks = BuildSampleBanks();

        Writer.SaveBanks(banks);

        File.Exists(Path.Combine("result", "bancos.xml")).Should().BeTrue();
    }

    [Fact]
    public void SaveBanks_ShouldCreateSqlFile()
    {
        var banks = BuildSampleBanks();

        Writer.SaveBanks(banks);

        File.Exists(Path.Combine("result", "bancos.sql")).Should().BeTrue();
    }

    [Fact]
    public void SaveBanks_ShouldCreateMarkdownFile()
    {
        var banks = BuildSampleBanks();

        Writer.SaveBanks(banks);

        File.Exists(Path.Combine("result", "bancos.md")).Should().BeTrue();
    }

    #endregion

    #region CSV Content

    [Fact]
    public void SaveBanks_CsvShouldHaveHeaderRow()
    {
        var banks = BuildSampleBanks();
        Writer.SaveBanks(banks);

        var lines = File.ReadAllLines(Path.Combine("result", "bancos.csv"));
        lines.Should().NotBeEmpty();
        lines[0].Should().Contain("COMPE");
        lines[0].Should().Contain("ISPB");
        lines[0].Should().Contain("LongName");
    }

    [Fact]
    public void SaveBanks_CsvShouldHaveCorrectRowCount()
    {
        var banks = BuildSampleBanks();
        Writer.SaveBanks(banks);

        var lines = File.ReadAllLines(Path.Combine("result", "bancos.csv"));
        lines.Should().HaveCount(3); // header + 2 banks
    }

    [Fact]
    public void SaveBanks_CsvShouldContainBankData()
    {
        var banks = BuildSampleBanks();
        Writer.SaveBanks(banks);

        var content = File.ReadAllText(Path.Combine("result", "bancos.csv"));
        content.Should().Contain("001");
        content.Should().Contain("Banco do Brasil S.A.");
        content.Should().Contain("BCO DO BRASIL");
    }

    [Fact]
    public void SaveBanks_CsvShouldOrderByCompe()
    {
        var banks = new List<Bank>
        {
            new()
            {
                Compe = 341,
                Ispb = 60746948,
                LongName = "Banco Itaú",
                ShortName = "ITAU",
                DateRegistered = DateTimeOffset.UtcNow,
                DateUpdated = DateTimeOffset.UtcNow,
                Document = "60.746.948/0001-12",
            },
            new()
            {
                Compe = 1,
                Ispb = 0,
                LongName = "Banco do Brasil",
                ShortName = "BB",
                DateRegistered = DateTimeOffset.UtcNow,
                DateUpdated = DateTimeOffset.UtcNow,
                Document = "00.000.000/0001-91",
            },
        };

        Writer.SaveBanks(banks);

        var lines = File.ReadAllLines(Path.Combine("result", "bancos.csv"));
        lines[1].Should().StartWith("001");
        lines[2].Should().StartWith("341");
    }

    #endregion

    #region SQL Content

    [Fact]
    public void SaveBanks_SqlShouldContainInsertStatements()
    {
        var banks = BuildSampleBanks();
        Writer.SaveBanks(banks);

        var content = File.ReadAllText(Path.Combine("result", "bancos.sql"));
        content.Should().Contain("INSERT INTO Banks");
        content.Should().Contain("VALUES(");
    }

    [Fact]
    public void SaveBanks_SqlShouldHaveCorrectRowCount()
    {
        var banks = BuildSampleBanks();
        Writer.SaveBanks(banks);

        var lines = File.ReadAllLines(Path.Combine("result", "bancos.sql"));
        lines.Should().HaveCount(2);
    }

    [Fact]
    public void SaveBanks_SqlShouldHandleSingleQuoteInName()
    {
        var banks = new List<Bank>
        {
            new()
            {
                Compe = 1,
                Ispb = 0,
                LongName = "Banco d'Ouro",
                ShortName = "BCO",
                DateRegistered = DateTimeOffset.UtcNow,
                DateUpdated = DateTimeOffset.UtcNow,
                Document = "00.000.000/0001-91",
            },
        };

        Writer.SaveBanks(banks);

        var content = File.ReadAllText(Path.Combine("result", "bancos.sql"));
        content.Should().Contain("Banco d''Ouro");
    }

    #endregion

    #region Markdown Content

    [Fact]
    public void SaveBanks_MarkdownShouldHaveTitle()
    {
        var banks = BuildSampleBanks();
        Writer.SaveBanks(banks);

        var lines = File.ReadAllLines(Path.Combine("result", "bancos.md"));
        lines[0].Should().Be("# Bancos Brasileiros");
    }

    [Fact]
    public void SaveBanks_MarkdownShouldHaveColumnHeaders()
    {
        var banks = BuildSampleBanks();
        Writer.SaveBanks(banks);

        var lines = File.ReadAllLines(Path.Combine("result", "bancos.md"));
        lines[2].Should().Contain("COMPE");
        lines[2].Should().Contain("ISPB");
    }

    [Fact]
    public void SaveBanks_MarkdownShouldHaveSeparatorRow()
    {
        var banks = BuildSampleBanks();
        Writer.SaveBanks(banks);

        var lines = File.ReadAllLines(Path.Combine("result", "bancos.md"));
        lines[3].Should().Contain("---");
    }

    #endregion

    #region XML Content

    [Fact]
    public void SaveBanks_XmlShouldBeValidXml()
    {
        var banks = BuildSampleBanks();
        Writer.SaveBanks(banks);

        var content = File.ReadAllText(Path.Combine("result", "bancos.xml"));
        content.Should().StartWith("<?xml");
        content.Should().Contain("<banks");
        content.Should().Contain("<bank");
    }

    [Fact]
    public void SaveBanks_XmlShouldContainBankData()
    {
        var banks = BuildSampleBanks();
        Writer.SaveBanks(banks);

        var content = File.ReadAllText(Path.Combine("result", "bancos.xml"));
        content.Should().Contain("Banco do Brasil S.A.");
    }

    #endregion

    #region WriteReleaseNotes

    [Fact]
    public void WriteReleaseNotes_ShouldCreateFile()
    {
        const string notes = "## Added\n- New bank added";

        Writer.WriteReleaseNotes(notes);

        File.Exists(Path.Combine("result", "release-notes.md")).Should().BeTrue();
    }

    [Fact]
    public void WriteReleaseNotes_ShouldWriteContent()
    {
        const string notes = "## Added\n- New bank added";

        Writer.WriteReleaseNotes(notes);

        var content = File.ReadAllText(Path.Combine("result", "release-notes.md"));
        content.Should().Be(notes);
    }

    #endregion
}
