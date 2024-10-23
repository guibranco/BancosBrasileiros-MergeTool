using System.Collections.Generic;
using System.IO;
using Xunit;

public class WriterTests
{
    [Fact]
    public void SaveYaml_CreatesYamlFile()
    {
        // Arrange
        var banks = new List<Bank> { new Bank { Compe = "001", Ispb = "12345678", Document = "00.000.000/0001-91", LongName = "Test Bank", ShortName = "TB" } };
        var expectedFilePath = Path.Combine(Directory.GetCurrentDirectory(), "result", "bancos.yml");

        // Act
        Writer.SaveYaml(banks);

        // Assert
        Assert.True(File.Exists(expectedFilePath));
        var content = File.ReadAllText(expectedFilePath);
        Assert.Contains("compe: '001'", content);
    }
}
