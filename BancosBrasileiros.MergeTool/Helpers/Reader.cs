// ***********************************************************************
// Assembly         : BancosBrasileiros.MergeTool
// Author           : Guilherme Branco Stracini
// Created          : 05-19-2020
//
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
    /// Loads the string.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
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
                Network = columns[4]
            })
            .ToList();
    }

    /// <summary>
    /// Loads the spi.
    /// </summary>
    /// <returns>List&lt;Bank&gt;.</returns>
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
                    .ToString("yyyy-MM-dd HH:mm:ss")
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
    /// Parses the line SLC.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>Bank.</returns>
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
            LongName = match.Groups["nome"].Value.Replace("\"", "").Trim()
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
    /// Parses the line siloc.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>Bank.</returns>
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
            CreditDocumentStr = match.Groups["doc"].Value.Trim()
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
    /// Parses the line sitraf.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>Bank.</returns>
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
            LongName = match.Groups["nome"].Value.Replace("\"", "").Trim()
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

        foreach (var line in lines)
        {
            if (!Patterns.CtcPattern.IsMatch(line))
            {
                spliced.Append($" {line}");
                continue;
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
    /// Parses the line CTC.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>Bank.</returns>
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
            Products = products.OrderBy(p => p).ToArray()
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
    /// Parses the line PCPS.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>Bank.</returns>
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
            SalaryPortability = match.Groups["adesao"].Value.Trim().Replace("- 1 -", "")
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
    /// Parses the line CQL.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>Bank.</returns>
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
            LegalCheque = true
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
    /// Parses the line detecta flow.
    /// </summary>
    /// <param name="line">The line.</param>
    /// <returns>Bank.</returns>
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
            DetectaFlow = true
        };
    }
}
