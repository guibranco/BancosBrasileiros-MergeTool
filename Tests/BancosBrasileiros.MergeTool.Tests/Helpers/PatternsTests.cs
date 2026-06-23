namespace BancosBrasileiros.MergeTool.Tests.Helpers;

using BancosBrasileiros.MergeTool.Helpers;
using FluentAssertions;
using Xunit;

public class PatternsTests
{
    #region CsvPattern

    [Fact]
    public void CsvPattern_ShouldSplitOnComma()
    {
        var input = "a,b,c";
        var parts = Patterns.CsvPattern.Split(input);
        parts.Should().BeEquivalentTo(["a", "b", "c"]);
    }

    [Fact]
    public void CsvPattern_ShouldNotSplitCommaInsideQuotes()
    {
        var input = "\"a,b\",c,d";
        var parts = Patterns.CsvPattern.Split(input);
        parts.Should().HaveCount(3);
        parts[0].Should().Be("\"a,b\"");
        parts[1].Should().Be("c");
        parts[2].Should().Be("d");
    }

    [Fact]
    public void CsvPattern_ShouldHandleSingleValue()
    {
        var input = "onlyone";
        var parts = Patterns.CsvPattern.Split(input);
        parts.Should().ContainSingle().Which.Should().Be("onlyone");
    }

    #endregion

    #region SsvPattern

    [Fact]
    public void SsvPattern_ShouldSplitOnSemicolon()
    {
        var input = "a;b;c";
        var parts = Patterns.SsvPattern.Split(input);
        parts.Should().BeEquivalentTo(["a", "b", "c"]);
    }

    [Fact]
    public void SsvPattern_ShouldNotSplitSemicolonInsideQuotes()
    {
        var input = "\"a;b\";c;d";
        var parts = Patterns.SsvPattern.Split(input);
        parts.Should().HaveCount(3);
        parts[0].Should().Be("\"a;b\"");
    }

    #endregion

    #region SilocPattern

    [Theory]
    [InlineData(
        "1 001 00000000 sim não Banco do Brasil S.A.",
        "1",
        "001",
        "00000000",
        "sim",
        "não",
        "Banco do Brasil S.A."
    )]
    [InlineData(
        "10 341 60746948 não sim Banco Itaú S.A.",
        "10",
        "341",
        "60746948",
        "não",
        "sim",
        "Banco Itaú S.A."
    )]
    public void SilocPattern_ShouldMatchValidLine(
        string line,
        string code,
        string compe,
        string ispb,
        string cobranca,
        string doc,
        string nome
    )
    {
        var match = Patterns.SilocPattern.Match(line);
        match.Success.Should().BeTrue();
        match.Groups["code"].Value.Should().Be(code);
        match.Groups["compe"].Value.Should().Be(compe);
        match.Groups["ispb"].Value.Should().Be(ispb);
        match.Groups["cobranca"].Value.Should().Be(cobranca);
        match.Groups["doc"].Value.Should().Be(doc);
        match.Groups["nome"].Value.Should().Be(nome);
    }

    [Fact]
    public void SilocPattern_ShouldNotMatchInvalidLine()
    {
        var match = Patterns.SilocPattern.Match("not a valid line");
        match.Success.Should().BeFalse();
    }

    #endregion

    #region SitrafPattern

    [Theory]
    [InlineData(
        "1 001 00000000 Banco do Brasil S.A.",
        "1",
        "001",
        "00000000",
        "Banco do Brasil S.A."
    )]
    [InlineData("25 341 60746948 Banco Itaú S.A.", "25", "341", "60746948", "Banco Itaú S.A.")]
    public void SitrafPattern_ShouldMatchValidLine(
        string line,
        string code,
        string compe,
        string ispb,
        string nome
    )
    {
        var match = Patterns.SitrafPattern.Match(line);
        match.Success.Should().BeTrue();
        match.Groups["code"].Value.Should().Be(code);
        match.Groups["compe"].Value.Should().Be(compe);
        match.Groups["ispb"].Value.Should().Be(ispb);
        match.Groups["nome"].Value.Should().Be(nome);
    }

    [Fact]
    public void SitrafPattern_ShouldNotMatchLineWithoutIspb()
    {
        var match = Patterns.SitrafPattern.Match("1 001 Banco do Brasil");
        match.Success.Should().BeFalse();
    }

    #endregion

    #region CtcPattern

    [Fact]
    public void CtcPattern_ShouldMatchValidLine()
    {
        var line = "1 Banco do Brasil S.A. 00.000.000/0001-91 00000000 DOC TED";
        var match = Patterns.CtcPattern.Match(line);
        match.Success.Should().BeTrue();
        match.Groups["code"].Value.Should().Be("1");
        match.Groups["nome"].Value.Should().Be("Banco do Brasil S.A.");
        match.Groups["ispb"].Value.Should().Be("00000000");
        match.Groups["produtos"].Value.Should().Be("DOC TED");
    }

    [Fact]
    public void CtcPattern_ShouldMatchLineWithLeadingSpace()
    {
        var line = " 2 Banco Bradesco S.A. 60.746.948/0001-12 60746948 DOC";
        var match = Patterns.CtcPattern.Match(line);
        match.Success.Should().BeTrue();
        match.Groups["code"].Value.Should().Be("2");
    }

    #endregion

    #region PcpsPattern

    [Fact]
    public void PcpsPattern_ShouldMatchValidLine()
    {
        var line = "1 Banco do Brasil S.A. 00.000.000/0001-91 00000000 01/01/2020";
        var match = Patterns.PcpsPattern.Match(line);
        match.Success.Should().BeTrue();
        match.Groups["code"].Value.Should().Be("1");
        match.Groups["nome"].Value.Should().Be("Banco do Brasil S.A.");
        match.Groups["ispb"].Value.Should().Be("00000000");
        match.Groups["adesao"].Value.Should().Be("01/01/2020");
    }

    [Fact]
    public void PcpsPattern_ShouldNotMatchIncompleteLine()
    {
        var match = Patterns.PcpsPattern.Match("1 Banco");
        match.Success.Should().BeFalse();
    }

    #endregion

    #region CqlPattern

    [Theory]
    [InlineData(
        "1 Banco do Brasil 00000000 Banco Comercial",
        "1",
        "Banco do Brasil",
        "00000000",
        "Banco Comercial"
    )]
    [InlineData(
        "2 Banco Itaú S.A. 60746948 Banco Comercial",
        "2",
        "Banco Itaú S.A.",
        "60746948",
        "Banco Comercial"
    )]
    public void CqlPattern_ShouldMatchValidLine(
        string line,
        string code,
        string nome,
        string ispb,
        string tipo
    )
    {
        var match = Patterns.CqlPattern.Match(line);
        match.Success.Should().BeTrue();
        match.Groups["code"].Value.Should().Be(code);
        match.Groups["nome"].Value.Should().Be(nome);
        match.Groups["ispb"].Value.Should().Be(ispb);
        match.Groups["tipo"].Value.Should().Be(tipo);
    }

    #endregion

    #region SlcPattern

    [Fact]
    public void SlcPattern_ShouldMatchValidLine()
    {
        var line = "1 00.000.000/0001-91 BANCO DO BRASIL   ";
        var match = Patterns.SlcPattern.Match(line);
        match.Success.Should().BeTrue();
        match.Groups["code"].Value.Should().Be("1");
        match.Groups["nome"].Value.Should().Be("BANCO DO BRASIL");
    }

    [Fact]
    public void SlcPattern_ShouldMatchLineWithConfidencial()
    {
        var line = "1 00.000.000/0001-91 BANCO DO BRASIL   Confidencial";
        var match = Patterns.SlcPattern.Match(line);
        match.Success.Should().BeTrue();
    }

    #endregion

    #region DetectaFlowPattern

    [Fact]
    public void DetectaFlowPattern_ShouldMatchValidLine()
    {
        var line = "1 Banco do Brasil S.A. 00.000.000/0001-91 00000000 extra info";
        var match = Patterns.DetectaFlowPattern.Match(line);
        match.Success.Should().BeTrue();
        match.Groups["code"].Value.Should().Be("1");
        match.Groups["nome"].Value.Should().Be("Banco do Brasil S.A.");
        match.Groups["ispb"].Value.Should().Be("00000000");
    }

    #endregion

    #region PcrPattern

    [Fact]
    public void PcrPattern_ShouldMatchValidLine()
    {
        var line = "1 Banco do Brasil S.A. 00.000.000/0001-91 001 00000000 sim não  ";
        var match = Patterns.PcrPattern.Match(line);
        match.Success.Should().BeTrue();
        match.Groups["code"].Value.Should().Be("1");
        match.Groups["nome"].Value.Should().Be("Banco do Brasil S.A.");
        match.Groups["compe"].Value.Should().Be("001");
        match.Groups["ispb"].Value.Should().Be("00000000");
        match.Groups["pcr"].Value.Should().Be("sim");
        match.Groups["pcrp"].Value.Should().Be("não");
    }

    [Fact]
    public void PcrPattern_ShouldNotMatchLineWithoutCompe()
    {
        var match = Patterns.PcrPattern.Match(
            "1 Banco do Brasil S.A. 00.000.000/0001-91 00000000 sim não"
        );
        match.Success.Should().BeFalse();
    }

    #endregion
}
