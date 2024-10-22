// ***********************************************************************
// Assembly         : BancosBrasileiros.MergeTool
// Author           : Guilherme Branco Stracini
// Created          : 05-19-2020
//
    public void LoadBcbTaxes()
    {
        // Loop through all banks' ISPB codes
        foreach (var bank in banks)
        {
            var personalUrl = string.Format(Constants.BcbTaxesPersonalUrl, bank.Ispb);
            var corporateUrl = string.Format(Constants.BcbTaxesCorporateUrl, bank.Ispb);

            var personalDoc = new HtmlWeb().Load(personalUrl);
            var corporateDoc = new HtmlWeb().Load(corporateUrl);

            // Parse HTML to extract taxes and levies information
            // Add parsed data to bank.PersonalTaxes and bank.CorporateTaxes
        }
    }

    public void LoadSfaOpenFinance()
    {
        // Implement SFA Open Finance data loading logic here
    }
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 06-01-2022
// ***********************************************************************
// <copyright file="Reader.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace BancosBrasileiros.MergeTool.Helpers;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using CrispyWaffle.Serialization;
using Dto;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

/// <summary>
/// Class Reader.
/// </summary>
internal class Reader
{
    /// <summary>
    /// The counting SLC
    /// </summary>
    private int _countingSlc;

    /// <summary>
    /// The counting SITRAF
    /// </summary>
    private int _countingSitraf;

    /// <summary>
    /// The counting CTC
    /// </summary>
    private int _countingCtc;

    /// <summary>
    /// The counting PCPS
    /// </summary>
    private int _countingPcps;

    /// <summary>
    /// The counting CQL
    /// </summary>
    private int _countingCql;

    /// <summary>
    /// The counting detecta flow
    /// </summary>
    private int _countingDetectaFlow;

    /// <summary>
    /// The counting PCR
    /// </summary>
    private int _countingPcr;

    /// <summary>
    /// Downloads the string.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>System.String.</returns>
    private static string DownloadString(string url)
    {
        using var client = new HttpClient();
        using var response = client.GetAsync(url).Result;
        using var content = response.Content;
        return content.ReadAsStringAsync().Result;
    }

    /// <summary>
    /// Downloads the bytes.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>System.Byte[].</returns>
    private static byte[] DownloadBytes(string url)
    {
        using var client = new HttpClient();
        return client.GetByteArrayAsync(url).Result;
    }

    /// <summary>
    /// Downloads and parse PDF.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <param name="system">The system.</param>
    /// <param name="callback">The callback.</param>
    /// <returns>List&lt;Bank&gt;.</returns>
    private static List<Bank> DownloadAndParsePdf(
        string url,
        Source system,
        Func<string, IEnumerable<Bank>> callback
    )
    {
        var result = new List<Bank>();
        PdfDocument reader;

        try
        {
            Logger.Log($"Downloading {system.ToString().ToUpperInvariant()}", ConsoleColor.Green);
            var data = DownloadBytes(url);
            reader = PdfDocument.Open(data);
        }
        catch (Exception e)
        {
            Logger.Log($"Error downloading {system}: {e.Message}", ConsoleColor.DarkRed);
            return result;
        }

        foreach (var page in reader.GetPages())
        {
            var currentText = ContentOrderTextExtractor.GetText(page);
            result.AddRange(callback(currentText));
        }

        return result;
    }

    /// <summary>
    /// Loads the change log.
    /// </summary>
    /// <returns>System.String.</returns>
    public static string LoadChangeLog() => DownloadString(Constants.ChangeLogUrl);

    /// <summary>
    /// Loads the base.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
    public List<Bank> LoadBase()
    {
        Logger.Log("Downloading base", ConsoleColor.Green);
        var data = DownloadString(Constants.BaseUrl);
        return SerializerFactory
            .GetCustomSerializer<List<Bank>>(SerializerFormat.Json)
            .Deserialize(data);
    }

    /// <summary>
    /// Loads and processes data from a specified STR (Standardized Transaction Report) URL.
    /// </summary>
    /// <remarks>
    /// This method initiates a download of the STR data from a predefined URL, processes the downloaded string by splitting it into lines,
    /// and then parses each line into a list of <see cref="Bank"/> objects.
    /// The method skips the first line of the data (which is typically a header), and only includes lines that contain valid data
    /// (i.e., lines with more than one column and where the third column can be parsed as an integer).
    /// Each <see cref="Bank"/> object is populated with relevant fields extracted from the CSV formatted data,
    /// including the document identifier, ISPB string, long name, short name, and the date when the operation started,
    /// which is formatted to "yyyy-MM-dd".
    /// The resulting list of <see cref="Bank"/> objects is returned to the caller.
    /// </remarks>
    /// <returns>A list of <see cref="Bank"/> objects populated with data from the STR.</returns>
    public List<Bank> LoadStr()
    {
        Logger.Log("Downloading STR", ConsoleColor.Green);
        var data = DownloadString(Constants.StrUrl);
        var lines = data.Split("\n").Skip(1).ToArray();

        return lines
            .Select(line => Patterns.CsvPattern.Split(line))
            .Where(columns => columns.Length > 1 && int.TryParse(columns[2], out _))
            .Select(columns => new Bank
            {
                CompeString = columns[2],
                Document = columns[0].Trim(),
                IspbString = columns[0],
                LongName = columns[5].Replace("\"", "").Replace("?", "-").Trim(),
                ShortName = columns[1].Trim(),
                DateOperationStarted = DateTime
                    .ParseExact(columns[6].Trim(), "dd/MM/yyyy", CultureInfo.InvariantCulture)
                    .ToString("yyyy-MM-dd"),
                Network = columns[4],
            })
            .ToList();
    }

    /// <summary>
    /// Loads the SPI (Sistema de Pagamentos Instantâneos) data for banks.
    /// </summary>
    /// <remarks>
    /// This method retrieves the SPI data by first logging the download action. It starts with today's date and attempts to fetch the data using the <c>GetPixData</c> method.
    /// If the data is not available (i.e., it is null or whitespace), it continues to fetch data for previous days until valid data is retrieved.
    /// The retrieved data is expected to be in a specific format, which is then processed line by line.
    /// Each line is split into columns, and only those lines that contain valid bank information (with at least two columns and a valid ISPB number) are selected.
    /// The method constructs a list of <c>Bank</c> objects from the valid lines, extracting relevant information such as ISPB string, long name, short name,
    /// PIX type, and the date when PIX started. The date is parsed and adjusted to UTC format before being formatted as a string.
    /// </remarks>
    /// <returns>A list of <c>Bank</c> objects populated with SPI data.</returns>
    public List<Bank> LoadSpi()
    {
        Logger.Log("Downloading SPI", ConsoleColor.Green);
        var baseDate = DateTime.Today;

        var data = GetPixData(baseDate);

        while (string.IsNullOrWhiteSpace(data))
        {
            baseDate = baseDate.AddDays(-1);
            data = GetPixData(baseDate);
        }

        var lines = data.Split("\n").Skip(1).ToArray();

        return lines
            .Select(line => Patterns.SsvPattern.Split(line))
            .Where(columns => columns.Length > 1 && int.TryParse(columns[0], out _))
            .Select(columns => new Bank
            {
                IspbString = columns[0],
                LongName = columns[1],
                ShortName = columns[2],
                PixType = columns[4],
                DatePixStarted = DateTime
                    .Parse(
                        columns[5].Trim(),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal
                    )
                    .ToUniversalTime()
                    .AddHours(-3)
                    .ToString("yyyy-MM-dd HH:mm:ss"),
            })
            .ToList();
    }

    /// <summary>
    /// Gets the pix data.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <returns>string.</returns>
    private static string GetPixData(DateTime date)
    {
        try
        {
            return DownloadString(string.Format(Constants.SpiUrl, date));
        }
        catch (WebException)
        {
            return null;
        }
    }

    /// <summary>
    /// Loads the SLC.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
    public List<Bank> LoadSlc()
    {
        _countingSlc = 0;
        return DownloadAndParsePdf(Constants.SlcUrl, Source.Slc, ParseLinesSlc);
    }

    /// <summary>
    /// Parses the lines SLC.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>IEnumerable&lt;Bank&gt;.</returns>
    private IEnumerable<Bank> ParseLinesSlc(string page)
    {
        var result = new List<Bank>();
        var lines = page.Split("\n");

        var spliced = new StringBuilder();

        foreach (var line in lines)
        {
            if (!Patterns.SlcPattern.IsMatch(line))
            {
                spliced.Append($" {line}");
                continue;
            }

            Bank bank;

            if (!string.IsNullOrWhiteSpace(spliced.ToString()))
            {
                bank = ParseLineSlc(spliced.ToString().Trim());

                if (bank != null)
                {
                    result.Add(bank);
                }

                spliced.Clear();
            }

            bank = ParseLineSlc(line);

            if (bank != null)
            {
                result.Add(bank);
            }
        }

        return result;
    }

    /// <summary>
    /// Parses a line of text to extract bank information based on a specific pattern.
    /// </summary>
    /// <param name="line">The line of text to be parsed for bank information.</param>
    /// <returns>A <see cref="Bank"/> object containing the extracted information, or null if the line does not match the expected pattern.</returns>
    /// <remarks>
    /// This method uses a regular expression defined in <c>Patterns.SlcPattern</c> to validate and extract data from the input line.
    /// If the line matches the pattern, it retrieves the bank code, CNPJ, and long name from the matched groups.
    /// The method also increments a counter (_countingSlc) to track how many lines have been processed.
    /// If the current count does not match the extracted code, a log message is generated to indicate the discrepancy.
    /// The resulting <see cref="Bank"/> object is populated with the CNPJ and long name, and returned to the caller.
    /// If the input line does not match the expected pattern, the method returns null.
    /// </remarks>
    private Bank ParseLineSlc(string line)
    {
        if (!Patterns.SlcPattern.IsMatch(line))
        {
            return null;
        }

        var match = Patterns.SlcPattern.Match(line);

        var code = Convert.ToInt32(match.Groups["code"].Value.Trim());

        _countingSlc++;

        if (_countingSlc != code)
        {
            Logger.Log($"SLC | Counting: {_countingSlc++} | Code: {code}", ConsoleColor.DarkYellow);
        }

        return new()
        {
            Document = match.Groups["cnpj"].Value.Trim(),
            LongName = match.Groups["nome"].Value.Replace("\"", "").Trim(),
        };
    }

    /// <summary>
    /// Loads the SILOC.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
    public List<Bank> LoadSiloc()
    {
        return DownloadAndParsePdf(Constants.SilocUrl, Source.Siloc, ParseLinesSiloc);
    }

    /// <summary>
    /// Parses the lines siloc.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>IEnumerable&lt;Bank&gt;.</returns>
    private static IEnumerable<Bank> ParseLinesSiloc(string page)
    {
        var result = new List<Bank>();
        var lines = page.Split("\n");

        foreach (var line in lines)
        {
            var bank = ParseLineSiloc(line);

            if (bank != null)
            {
                result.Add(bank);
            }
        }

        return result;
    }

    /// <summary>
    /// Parses a line of text according to the Siloc pattern and returns a Bank object.
    /// </summary>
    /// <param name="line">The line of text to be parsed.</param>
    /// <returns>A <see cref="Bank"/> object populated with data extracted from the input line, or null if the line does not match the expected pattern.</returns>
    /// <remarks>
    /// This method uses a regular expression defined in <see cref="Patterns.SilocPattern"/> to validate and extract information from the input string <paramref name="line"/>.
    /// If the line matches the pattern, it retrieves values for various properties such as Compe, IspbString, LongName, ChargeStr, and CreditDocumentStr.
    /// The extracted values are then converted and trimmed as necessary before being assigned to the corresponding properties of the new <see cref="Bank"/> object.
    /// If the input line does not conform to the expected format, the method returns null, indicating that parsing was unsuccessful.
    /// </remarks>
    private static Bank ParseLineSiloc(string line)
    {
        if (!Patterns.SilocPattern.IsMatch(line))
        {
            return null;
        }

        var match = Patterns.SilocPattern.Match(line);

        return new()
        {
            Compe = Convert.ToInt32(match.Groups["compe"].Value.Trim()),
            IspbString = match.Groups["ispb"].Value.Trim(),
            LongName = match.Groups["nome"].Value.Replace("\"", "").Trim(),
            ChargeStr = match.Groups["cobranca"].Value.Trim(),
            CreditDocumentStr = match.Groups["doc"].Value.Trim(),
        };
    }

    /// <summary>
    /// Loads the sitraf.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
    public List<Bank> LoadSitraf()
    {
        _countingSitraf = 0;
        return DownloadAndParsePdf(Constants.SitrafUrl, Source.Sitraf, ParseLinesSitraf);
    }

    /// <summary>
    /// Parses the lines sitraf.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>IEnumerable&lt;Bank&gt;.</returns>
    private IEnumerable<Bank> ParseLinesSitraf(string page)
    {
        var result = new List<Bank>();
        var lines = page.Split("\n");

        var spliced = new StringBuilder();

        foreach (var line in lines)
        {
            if (!Patterns.SitrafPattern.IsMatch(line))
            {
                spliced.Append($" {line}");
                continue;
            }

            Bank bank;

            if (!string.IsNullOrWhiteSpace(spliced.ToString()))
            {
                bank = ParseLineSitraf(spliced.ToString().Trim());

                if (bank != null)
                {
                    result.Add(bank);
                }

                spliced.Clear();
            }

            bank = ParseLineSitraf(line);

            if (bank != null)
            {
                result.Add(bank);
            }
        }

        return result;
    }

    /// <summary>
    /// Parses a line of SITRAF data and returns a corresponding Bank object.
    /// </summary>
    /// <param name="line">The line of SITRAF data to be parsed.</param>
    /// <returns>A <see cref="Bank"/> object populated with data from the parsed line, or null if the line does not match the expected pattern.</returns>
    /// <remarks>
    /// This method uses a regular expression defined in <see cref="Patterns.SitrafPattern"/> to validate and extract information from the input line.
    /// If the line does not match the pattern, the method returns null.
    /// If the line is valid, it extracts the code, compe, ispb, and long name from the matched groups.
    /// The method also keeps track of how many SITRAF lines have been processed, logging a warning if the current count does not match the extracted code.
    /// The resulting <see cref="Bank"/> object contains the extracted values, with the long name cleaned of any quotation marks.
    /// </remarks>
    private Bank ParseLineSitraf(string line)
    {
        if (!Patterns.SitrafPattern.IsMatch(line))
        {
            return null;
        }

        var match = Patterns.SitrafPattern.Match(line);

        var code = Convert.ToInt32(match.Groups["code"].Value.Trim());

        _countingSitraf++;

        if (_countingSitraf != code)
        {
            Logger.Log(
                $"SITRAF | Counting: {_countingSitraf++} | Code: {code}",
                ConsoleColor.DarkYellow
            );
        }

        return new()
        {
            Compe = Convert.ToInt32(match.Groups["compe"].Value.Trim()),
            IspbString = match.Groups["ispb"].Value.Trim(),
            LongName = match.Groups["nome"].Value.Replace("\"", "").Trim(),
        };
    }

    /// <summary>
    /// Loads the CTC.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
    public List<Bank> LoadCtc()
    {
        _countingCtc = 0;
        return DownloadAndParsePdf(Constants.CtcUrl, Source.Ctc, ParsePageCtc);
    }

    /// <summary>
    /// Parses the lines CTC.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>IEnumerable&lt;Bank&gt;.</returns>
    private IEnumerable<Bank> ParsePageCtc(string page)
    {
        var result = new List<Bank>();
        var lines = page.Split("\n");

        var spliced = new StringBuilder();
        var splicedCount = 0;

        foreach (var line in lines)
        {
            if (!Patterns.CtcPattern.IsMatch(line))
            {
                spliced.Append($" {line}");
                splicedCount++;
                if (splicedCount < 2)
                {
                    continue;
                }
            }

            Bank bank;

            if (!string.IsNullOrWhiteSpace(spliced.ToString()))
            {
                bank = ParseLineCtc(spliced.ToString().Trim());

                if (bank != null)
                {
                    result.Add(bank);
                }

                spliced.Clear();
                splicedCount = 0;
            }

            bank = ParseLineCtc(line);

            if (bank != null)
            {
                result.Add(bank);
            }
        }

        return result;
    }

    /// <summary>
    /// Parses a line of text containing CTC (Código de Transação de Crédito) information and returns a Bank object.
    /// </summary>
    /// <param name="line">The line of text to be parsed, which should match the CTC pattern.</param>
    /// <returns>A <see cref="Bank"/> object populated with the parsed data, or null if the line does not match the expected pattern.</returns>
    /// <remarks>
    /// This method uses a regular expression defined in <see cref="Patterns.CtcPattern"/> to extract various components from the input line.
    /// If the line does not match the pattern, the method returns null.
    /// It increments a counter (_countingCtc) each time it successfully parses a line, and logs a message if the counter does not match the extracted code.
    /// The method also processes product information by splitting it into individual items and ensuring they are sorted in ascending order.
    /// The resulting <see cref="Bank"/> object contains fields such as Document, IspbString, LongName, and an array of Products.
    /// </remarks>
    private Bank ParseLineCtc(string line)
    {
        if (!Patterns.CtcPattern.IsMatch(line))
        {
            return null;
        }

        var match = Patterns.CtcPattern.Match(line);

        var code = Convert.ToInt32(match.Groups["code"].Value.Trim());

        _countingCtc++;

        if (_countingCtc != code)
        {
            Logger.Log($"CTC | Counting: {_countingCtc++} | Code: {code}", ConsoleColor.DarkYellow);
        }

        var products = match.Groups["produtos"].Value.Split(",").Select(p => p.Trim()).ToList();
        var last = products[^1];
        products.RemoveAt(products.Count - 1);
        var split = last.Split(" e ");
        products.AddRange(split);

        return new()
        {
            Document = match.Groups["cnpj"].Value.Trim(),
            IspbString = match.Groups["ispb"].Value.Trim(),
            LongName = match.Groups["nome"].Value.Replace("\"", "").Trim(),
            Products = products.OrderBy(p => p).ToArray(),
        };
    }

    /// <summary>
    /// Loads the PCPS.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
    public List<Bank> LoadPcps()
    {
        _countingPcps = 0;
        return DownloadAndParsePdf(Constants.PcpsUrl, Source.Pcps, ParsePagePcps);
    }

    /// <summary>
    /// Parses the page PCPS.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>IEnumerable&lt;Bank&gt;.</returns>
    private IEnumerable<Bank> ParsePagePcps(string page)
    {
        var result = new List<Bank>();
        var lines = page.Split("\n");

        var spliced = new StringBuilder();

        foreach (var line in lines)
        {
            if (!Patterns.PcpsPattern.IsMatch(line))
            {
                spliced.Append($" {line}");
                continue;
            }

            Bank bank;

            if (!string.IsNullOrWhiteSpace(spliced.ToString()))
            {
                bank = ParseLinePcps(spliced.ToString().Trim());

                if (bank != null)
                {
                    result.Add(bank);
                }

                spliced.Clear();
            }

            bank = ParseLinePcps(line);

            if (bank != null)
            {
                result.Add(bank);
            }
        }

        return result;
    }

    /// <summary>
    /// Parses a line of text to extract bank information based on a predefined pattern.
    /// </summary>
    /// <param name="line">The line of text to be parsed for bank information.</param>
    /// <returns>A <see cref="Bank"/> object containing the extracted bank details, or null if the line does not match the expected pattern.</returns>
    /// <remarks>
    /// This method uses a regular expression defined in <see cref="Patterns.PcpsPattern"/> to match and extract relevant data from the input line.
    /// If the line does not match the pattern, the method returns null, indicating that no valid bank information could be parsed.
    /// The method also increments a counter (_countingPcps) each time it is called, and logs a warning if the current count does not match the extracted code.
    /// The extracted bank details include the document identifier (CNPJ), ISPB string, long name, and salary portability status.
    /// </remarks>
    private Bank ParseLinePcps(string line)
    {
        if (!Patterns.PcpsPattern.IsMatch(line))
        {
            return null;
        }

        var match = Patterns.PcpsPattern.Match(line);

        var code = Convert.ToInt32(match.Groups["code"].Value.Trim());

        _countingPcps++;

        if (_countingPcps != code)
        {
            Logger.Log(
                $"PCPS | Counting: {_countingPcps++} | Code: {code}",
                ConsoleColor.DarkYellow
            );
        }

        return new()
        {
            Document = match.Groups["cnpj"].Value.Trim(),
            IspbString = match.Groups["ispb"].Value.Trim(),
            LongName = match.Groups["nome"].Value.Replace("\"", "").Trim(),
            SalaryPortability = match.Groups["adesao"].Value.Trim().Replace("- 1 -", ""),
        };
    }

    /// <summary>
    /// Loads the CQL.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
    public List<Bank> LoadCql()
    {
        _countingCql = 0;
        return DownloadAndParsePdf(Constants.CqlUrl, Source.Cql, ParsePageCql);
    }

    /// <summary>
    /// Parses the page CQL.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>IEnumerable&lt;Bank&gt;.</returns>
    private IEnumerable<Bank> ParsePageCql(string page)
    {
        var lines = page.Split("\n");

        return lines.Select(ParseLineCql).Where(bank => bank != null).ToList();
    }

    /// <summary>
    /// Parses a line of CQL (Common Query Language) and returns a corresponding Bank object.
    /// </summary>
    /// <param name="line">The line of text to be parsed for CQL information.</param>
    /// <returns>A <see cref="Bank"/> object populated with data extracted from the line, or null if the line does not match the expected pattern.</returns>
    /// <remarks>
    /// This method uses a regular expression defined in <see cref="Patterns.CqlPattern"/> to validate and extract information from the input line.
    /// If the line does not match the pattern, the method returns null.
    /// If the line is valid, it extracts the 'code', 'ispb', 'nome', and 'tipo' fields from the matched groups.
    /// The method also increments a counting variable (_countingCql) and logs a message if the current count does not match the extracted code.
    /// The returned Bank object will have its properties set based on the extracted values, with 'LegalCheque' set to true by default.
    /// </remarks>
    private Bank ParseLineCql(string line)
    {
        if (!Patterns.CqlPattern.IsMatch(line))
        {
            return null;
        }

        var match = Patterns.CqlPattern.Match(line);
        var code = Convert.ToInt32(match.Groups["code"].Value.Trim());

        _countingCql++;
        if (_countingCql != code)
        {
            Logger.Log($"CQL | Counting: {_countingCql++} | Code: {code}", ConsoleColor.DarkYellow);
        }

        return new()
        {
            IspbString = match.Groups["ispb"].Value.Trim(),
            LongName = match.Groups["nome"].Value.Replace("\"", "").Trim(),
            Type = match.Groups["tipo"].Value.Trim(),
            LegalCheque = true,
        };
    }

    /// <summary>
    /// Loads the detecta flow.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
    public List<Bank> LoadDetectaFlow()
    {
        _countingDetectaFlow = 0;
        return DownloadAndParsePdf(
            Constants.DetectaFlowUrl,
            Source.DetectaFlow,
            ParsePageDetectaFlow
        );
    }

    /// <summary>
    /// Parses the page detecta flow.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>IEnumerable&lt;Bank&gt;.</returns>
    private IEnumerable<Bank> ParsePageDetectaFlow(string page)
    {
        var lines = page.Split("\n");

        return lines.Select(ParseLineDetectaFlow).Where(bank => bank != null).ToList();
    }

    /// <summary>
    /// Parses a line of text to detect a flow pattern and returns a corresponding Bank object.
    /// </summary>
    /// <param name="line">The line of text to be parsed for the DetectaFlow pattern.</param>
    /// <returns>A <see cref="Bank"/> object populated with data extracted from the line if the pattern matches; otherwise, returns null.</returns>
    /// <remarks>
    /// This method uses a regular expression defined in <see cref="Patterns.DetectaFlowPattern"/> to determine if the input line matches the expected format.
    /// If the line matches, it extracts relevant information such as the document identifier (CNPJ), ISPB string, and long name from the matched groups.
    /// The method also maintains a count of how many times it has detected a flow, logging a message if the current count does not match the extracted code from the line.
    /// The returned <see cref="Bank"/> object indicates that a DetectaFlow was successfully parsed.
    /// </remarks>
    private Bank ParseLineDetectaFlow(string line)
    {
        if (!Patterns.DetectaFlowPattern.IsMatch(line))
        {
            return null;
        }

        var match = Patterns.DetectaFlowPattern.Match(line);
        var code = Convert.ToInt32(match.Groups["code"].Value.Trim());

        _countingDetectaFlow++;
        if (_countingDetectaFlow != code)
        {
            Logger.Log(
                $"DetectaFlow | Counting: {_countingDetectaFlow++} | Code: {code}",
                ConsoleColor.DarkYellow
            );
        }

        return new Bank
        {
            Document = match.Groups["cnpj"].Value.Trim(),
            IspbString = match.Groups["ispb"].Value.Trim(),
            LongName = match.Groups["nome"].Value.Replace("\"", "").Trim(),
            DetectaFlow = true,
        };
    }

    /// <summary>
    /// Loads the PCR.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
    public List<Bank> LoadPcr()
    {
        _countingPcr = 0;
        return DownloadAndParsePdf(Constants.PcrUrl, Source.Pcr, ParsePagePcr);
    }

    /// <summary>
    /// Parses the page PCR.
    /// </summary>
    /// <param name="page">The page.</param>
    /// <returns>IEnumerable&lt;Bank&gt;.</returns>
    private IEnumerable<Bank> ParsePagePcr(string page)
    {
        var result = new List<Bank>();
        var lines = page.Split("\n");

        var spliced = new StringBuilder();

        foreach (var line in lines)
        {
            if (!Patterns.PcrPattern.IsMatch(line))
            {
                spliced.Append($" {line}");
                continue;
            }

            Bank bank;

            if (!string.IsNullOrWhiteSpace(spliced.ToString()))
            {
                bank = ParseLinePcr(spliced.ToString().Trim());

                if (bank != null)
                {
                    result.Add(bank);
                }

                spliced.Clear();
            }

            bank = ParseLinePcr(line);

            if (bank != null)
            {
                result.Add(bank);
            }
        }

        return result;
    }

    /// <summary>
    /// Parses a line of text to extract bank information based on a predefined pattern.
    /// </summary>
    /// <param name="line">The line of text containing bank information to be parsed.</param>
    /// <returns>
    /// A <see cref="Bank"/> object containing the parsed bank information if the line matches the expected pattern; otherwise, returns null.
    /// </returns>
    /// <remarks>
    /// This method uses a regular expression defined in <see cref="Patterns.PcrPattern"/> to match the input line.
    /// If the line does not match the pattern, the method returns null. If a match is found, it extracts various groups from the match,
    /// including the bank's CNPJ, ISPB, COMPE, long name, PCR string, and PCRP string.
    /// The method also increments a counter (_countingPcr) and logs a message if the current count does not match the extracted code.
    /// The resulting <see cref="Bank"/> object is populated with the extracted values and returned.
    /// </remarks>
    public Bank ParseLinePcr(string line)
    {
        if (!Patterns.PcrPattern.IsMatch(line))
        {
            return null;
        }

        var match = Patterns.PcrPattern.Match(line);
        var code = Convert.ToInt32(match.Groups["code"].Value.Trim());

        _countingPcr++;
        if (_countingPcr != code)
        {
            Logger.Log($"PCR | Counting: {_countingPcr++} | Code: {code}", ConsoleColor.DarkYellow);
        }

        return new Bank
        {
            Document = match.Groups["cnpj"].Value.Trim(),
            IspbString = match.Groups["ispb"].Value.Trim(),
            Compe = Convert.ToInt32(match.Groups["compe"].Value.Trim()),
            LongName = match.Groups["nome"].Value.Replace("\"", "").Trim(),
            PcrStr = match.Groups["pcr"].Value.Trim(),
            PcrpStr = match.Groups["pcrp"].Value.Trim(),
        };
    }
}
