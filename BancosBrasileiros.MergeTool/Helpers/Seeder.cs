// ***********************************************************************
// Assembly         : BancosBrasileiros.MergeTool
// Author           : Guilherme Branco Stracini
// Created          : 19/05/2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 06-01-2022
// ***********************************************************************
// <copyright file="Seeder.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

// ReSharper disable CognitiveComplexity

namespace BancosBrasileiros.MergeTool.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CrispyWaffle.Extensions;
using Dto;

/// <summary>
/// Class Seeder.
/// </summary>
internal class Seeder
{
    /// <summary>
    /// The source
    /// </summary>
    private readonly IList<Bank> _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="Seeder" /> class.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <exception cref="System.ArgumentNullException">source</exception>
    public Seeder(IList<Bank> source) =>
        _source = source ?? throw new ArgumentNullException(nameof(source));

    /// <summary>
    /// Generates the missing document.
    /// </summary>
    /// <returns>Seeder.</returns>
    public Seeder GenerateMissingDocument()
    {
        var existing = 0;
        var missing = 0;

        Logger.Log("Generate document\r\n", ConsoleColor.DarkYellow);

        foreach (var bank in _source)
        {
            if (bank.Document is { Length: 18 })
            {
                existing++;
                continue;
            }

            bank.SetChange(Source.Document, x => x.Document, bank.IspbString);
            missing++;
        }

        Logger.Log(
            $"Generate document | Existing: {existing} | Missing: {missing}\r\n",
            ConsoleColor.DarkYellow
        );
        return this;
    }

    /// <summary>
    /// Seeds the string.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>Seeder.</returns>
    public Seeder SeedStr(IEnumerable<Bank> items)
    {
        var added = 0;
        var updated = 0;
        var nameFixed = 0;

        Logger.Log("STR\r\n", ConsoleColor.DarkYellow);

        foreach (var str in items)
        {
            var bank = _source.SingleOrDefault(b => b.Compe == str.Compe);
            if (str.Compe == 0)
            {
                Logger.Log(
                    $"STR | Ignoring bank {str.Compe} | Long name: {str.LongName} | Short name: {str.ShortName}",
                    ConsoleColor.DarkRed
                );
                continue;
            }

            if (bank == null)
            {
                Logger.Log(
                    $"STR | Adding bank {str.Compe} by STR List | Long name: {str.LongName}",
                    ConsoleColor.Green
                );

                if (str.Document is not { Length: 18 })
                {
                    str.Document = str.IspbString;
                }

                _source.Add(str);
                added++;
                continue;
            }

            var longNameSame = bank.LongName
                .RemoveDiacritics()
                .Equals(
                    str.LongName.RemoveDiacritics(),
                    StringComparison.InvariantCultureIgnoreCase
                );
            var shortNameSame = bank.ShortName
                .RemoveDiacritics()
                .Equals(
                    str.ShortName.RemoveDiacritics(),
                    StringComparison.InvariantCultureIgnoreCase
                );

            if (longNameSame && shortNameSame)
            {
                Logger.Log(
                    $"STR | Bank {str.Compe} is updated: {str.LongName}",
                    ConsoleColor.DarkGreen
                );
                updated++;
                continue;
            }

            if (!longNameSame)
            {
                Logger.Log(
                    $"STR | Bank {str.Compe} with long name different | Old: {bank.LongName} | New: {str.LongName}",
                    ConsoleColor.DarkYellow
                );
                bank.SetChange(Source.Str, x => x.LongName, str.LongName);
            }

            if (!shortNameSame)
            {
                Logger.Log(
                    $"STR | Bank {str.Compe} with short name different | Old: {bank.ShortName} | New: {str.ShortName}",
                    ConsoleColor.DarkYellow
                );
                bank.SetChange(Source.Str, x => x.ShortName, str.ShortName);
            }

            nameFixed++;
        }

        Logger.Log(
            $"\r\nSTR | Added: {added} | is updated: {updated} | Fixed: {nameFixed}\r\n",
            ConsoleColor.DarkYellow
        );
        return this;
    }

    /// <summary>
    /// Seeds the sitraf.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>Seeder.</returns>
    public Seeder SeedSitraf(IEnumerable<Bank> items)
    {
        var added = 0;
        var updated = 0;
        var nameDifferent = 0;

        Logger.Log("SITRAF\r\n", ConsoleColor.DarkYellow);

        foreach (var sitraf in items)
        {
            var bank = _source.SingleOrDefault(b => b.Compe == sitraf.Compe);

            if (bank == null)
            {
                Logger.Log(
                    $"Adding bank by SITRAF List | {sitraf.Compe} | {sitraf.LongName}",
                    ConsoleColor.DarkGreen
                );

                if (sitraf.Document is not { Length: 18 })
                {
                    sitraf.Document = sitraf.IspbString;
                }

                sitraf.ShortName ??= Regex.Replace(
                    sitraf.LongName,
                    @"(.+?)(?:\s-\s(?:.+))?",
                    "$1",
                    RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
                    TimeSpan.FromSeconds(1)
                );

                _source.Add(sitraf);
                added++;
                continue;
            }

            if (
                !bank.LongName
                    .RemoveDiacritics()
                    .Equals(
                        sitraf.LongName.RemoveDiacritics(),
                        StringComparison.InvariantCultureIgnoreCase
                    )
            )
            {
                nameDifferent++;
                continue;
            }

            Logger.Log(
                $"SITRAF | Bank {sitraf.Compe} is updated: {sitraf.LongName}",
                ConsoleColor.DarkGreen
            );
            updated++;
        }

        Logger.Log(
            $"\r\nSITRAF | Added: {added} | is updated: {updated} | Name different: {nameDifferent}\r\n",
            ConsoleColor.DarkYellow
        );

        return this;
    }

    /// <summary>
    /// Seeds the SLC.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>Seeder.</returns>
    public Seeder SeedSlc(IEnumerable<Bank> items)
    {
        var found = 0;
        var notFound = 0;

        Logger.Log("SLC\r\n", ConsoleColor.DarkYellow);

        foreach (var slc in items)
        {
            var bank = _source.SingleOrDefault(
                b => b.Document != null && b.Document.Equals(slc.Document)
            );

            if (bank == null)
            {
                bank = _source.SingleOrDefault(
                    b =>
                        b.LongName
                            .RemoveDiacritics()
                            .Equals(
                                slc.LongName.RemoveDiacritics(),
                                StringComparison.InvariantCultureIgnoreCase
                            )
                        || (
                            b.ShortName != null
                            && b.ShortName
                                .RemoveDiacritics()
                                .Equals(
                                    slc.LongName.RemoveDiacritics(),
                                    StringComparison.InvariantCultureIgnoreCase
                                )
                        )
                );
            }

            if (bank == null)
            {
                var ispb = int.Parse(slc.Document.RemoveNonNumeric()[..8]);

                if (
                    ispb == 0
                    && !slc.LongName.Equals(
                        "Banco do Brasil",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                {
                    Logger.Log(
                        $"SLC | ISPB null-ed: {slc.LongName} | {slc.Document.Trim()}",
                        ConsoleColor.DarkRed
                    );
                    continue;
                }

                bank = _source.SingleOrDefault(
                    b =>
                        b.Ispb.Equals(ispb)
                        && b.LongName.Contains(
                            slc.LongName,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                );
            }

            if (bank == null)
            {
                Logger.Log(
                    $"SLC | Bank not found: {slc.LongName} | {slc.Document.Trim()}",
                    ConsoleColor.DarkRed
                );

                notFound++;
                continue;
            }

            if (
                (string.IsNullOrWhiteSpace(bank.Document) || bank.Document.Length != 18)
                && !string.IsNullOrWhiteSpace(slc.Document)
            )
            {
                bank.SetChange(Source.Slc, x => x.Document, slc.Document);
            }
            else if (string.IsNullOrWhiteSpace(bank.Document) || bank.Document.Length != 18)
            {
                Logger.Log(
                    $"SLC | Invalid document {slc.Compe} | {bank.Document} | {slc.Document}",
                    ConsoleColor.DarkRed
                );
            }

            if (string.IsNullOrWhiteSpace(bank.ShortName))
            {
                bank.SetChange(Source.Slc, x => x.ShortName, slc.LongName);
            }

            found++;
        }

        Logger.Log(
            $"\r\nSLC | Found: {found} | Not found: {notFound}\r\n",
            ConsoleColor.DarkYellow
        );
        return this;
    }

    /// <summary>
    /// Seeds the SPI.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>Seeder.</returns>
    public Seeder SeedSpi(IEnumerable<Bank> items)
    {
        var found = 0;
        var upToDate = 0;
        var notFound = 0;

        Logger.Log("SPI\r\n", ConsoleColor.DarkYellow);

        foreach (var spi in items)
        {
            var bank = _source.SingleOrDefault(
                b =>
                    b.LongName
                        .RemoveDiacritics()
                        .Equals(
                            spi.LongName.RemoveDiacritics(),
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    || (
                        b.ShortName != null
                        && b.ShortName
                            .RemoveDiacritics()
                            .Equals(
                                spi.LongName.RemoveDiacritics(),
                                StringComparison.InvariantCultureIgnoreCase
                            )
                    )
            );

            bank ??= _source.SingleOrDefault(b => b.Ispb.Equals(spi.Ispb));

            if (bank == null)
            {
                Logger.Log($"SPI | PSP not found: {spi.LongName}", ConsoleColor.DarkRed);

                notFound++;
                continue;
            }

            if (
                bank.PixType != null
                && bank.PixType.Equals(spi.PixType)
                && bank.DatePixStarted != null
                && bank.DatePixStarted.Equals(spi.DatePixStarted)
            )
            {
                Logger.Log($"SPI | PSP is updated: {spi.LongName}", ConsoleColor.DarkGreen);

                upToDate++;
                continue;
            }

            bank.SetChange(Source.Spi, x => x.PixType, spi.PixType);
            bank.SetChange(Source.Spi, x => x.DatePixStarted, spi.DatePixStarted);
            found++;
        }

        Logger.Log(
            $"\r\nSPI | Found: {found} | Not found: {notFound} | Up to Date: {upToDate}\r\n",
            ConsoleColor.DarkYellow
        );

        return this;
    }

    /// <summary>
    /// Seeds the CTC.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>Seeder.</returns>
    public Seeder SeedCtc(IEnumerable<Bank> items)
    {
        var found = 0;
        var upToDate = 0;
        var notFound = 0;

        Logger.Log("CTC\r\n", ConsoleColor.DarkYellow);

        foreach (var ctc in items)
        {
            var bank = _source.SingleOrDefault(
                b => b.Document != null && b.Document.Equals(ctc.Document)
            );

            if (bank == null)
            {
                bank = _source.SingleOrDefault(
                    b =>
                        b.LongName
                            .RemoveDiacritics()
                            .Equals(
                                ctc.LongName.RemoveDiacritics(),
                                StringComparison.InvariantCultureIgnoreCase
                            )
                        || (
                            b.ShortName != null
                            && b.ShortName
                                .RemoveDiacritics()
                                .Equals(
                                    ctc.LongName.RemoveDiacritics(),
                                    StringComparison.InvariantCultureIgnoreCase
                                )
                        )
                );
            }

            if (bank == null)
            {
                var ispb = int.Parse(ctc.Document.RemoveNonNumeric()[..8]);

                if (
                    ispb == 0
                    && !ctc.LongName.Equals(
                        "Banco do Brasil",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                {
                    Logger.Log(
                        $"CTC | ISPB nulled: {ctc.LongName} | {ctc.Document.Trim()}",
                        ConsoleColor.DarkRed
                    );
                    continue;
                }

                bank = _source.SingleOrDefault(
                    b =>
                        b.Ispb.Equals(ispb)
                        && b.LongName.Contains(
                            ctc.LongName,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                );
            }

            if (bank == null)
            {
                Logger.Log(
                    $"CTC | Bank {ctc.Compe} not found: {ctc.LongName} | {ctc.Document.Trim()}",
                    ConsoleColor.DarkRed
                );
                notFound++;
                continue;
            }

            if (
                (string.IsNullOrWhiteSpace(bank.Document) || bank.Document.Length != 18)
                && !string.IsNullOrWhiteSpace(ctc.Document)
            )
            {
                bank.SetChange(Source.Ctc, x => x.Document, ctc.Document);
            }
            else if (string.IsNullOrWhiteSpace(bank.Document) || bank.Document.Length != 18)
            {
                Logger.Log(
                    $"CTC | Invalid document {ctc.Compe} | {bank.Document} | {ctc.Document}",
                    ConsoleColor.DarkRed
                );
            }

            if (bank.Products != null && !bank.Products.Except(ctc.Products).Any())
            {
                Logger.Log(
                    $"CTC |Bank {ctc.Compe} Products is updated: {ctc.LongName}",
                    ConsoleColor.DarkGreen
                );
                upToDate++;
                continue;
            }

            bank.SetChange(Source.Ctc, x => x.Products, ctc.Products);
            found++;
        }

        Logger.Log(
            $"\r\nCTC | Found: {found} | Not found: {notFound} | Up to date: {upToDate}\r\n",
            ConsoleColor.DarkYellow
        );
        return this;
    }

    /// <summary>
    /// Seeds the siloc.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>Seeder.</returns>
    public Seeder SeedSiloc(IEnumerable<Bank> items)
    {
        var found = 0;
        var upToDate = 0;
        var notFound = 0;

        Logger.Log("SILOC\r\n", ConsoleColor.DarkYellow);

        foreach (var siloc in items)
        {
            var bank = _source.SingleOrDefault(
                b => b.IspbString != null && b.IspbString.Equals(siloc.IspbString)
            );

            bank ??= _source.SingleOrDefault(
                b =>
                    b.LongName
                        .RemoveDiacritics()
                        .Equals(
                            siloc.LongName.RemoveDiacritics(),
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    || (
                        b.ShortName != null
                        && b.ShortName
                            .RemoveDiacritics()
                            .Equals(
                                siloc.LongName.RemoveDiacritics(),
                                StringComparison.InvariantCultureIgnoreCase
                            )
                    )
            );

            if (bank == null)
            {
                Logger.Log(
                    $"SILOC | Bank not found: {siloc.LongName} | {siloc.Document.Trim()}",
                    ConsoleColor.DarkRed
                );
                notFound++;
                continue;
            }

            if (
                (string.IsNullOrWhiteSpace(bank.Document) || bank.Document.Length != 18)
                && !string.IsNullOrWhiteSpace(siloc.Document)
            )
            {
                bank.SetChange(Source.Siloc, x => x.Document, siloc.Document);
            }
            else if (string.IsNullOrWhiteSpace(bank.Document) || bank.Document.Length != 18)
            {
                Logger.Log(
                    $"SILOC | Invalid document {siloc.Compe} | {bank.Document} | {siloc.Document}",
                    ConsoleColor.DarkRed
                );
            }

            if (
                bank.Charge != null
                && bank.Charge.Equals(siloc.Charge)
                && bank.CreditDocument != null
                && bank.CreditDocument.Equals(siloc.CreditDocument)
            )
            {
                Logger.Log(
                    $"SILOC | Bank {siloc.Compe} COB/DOC is updated: {siloc.LongName}",
                    ConsoleColor.DarkGreen
                );
                upToDate++;
                continue;
            }

            bank.SetChange(Source.Siloc, x => x.Charge, siloc.Charge);
            bank.SetChange(Source.Siloc, x => x.CreditDocument, siloc.CreditDocument);
            found++;
        }

        Logger.Log(
            $"\r\nSILOC | Found: {found} | Not found: {notFound} | Up to date: {upToDate}\r\n",
            ConsoleColor.DarkYellow
        );
        return this;
    }

    /// <summary>
    /// Seeds the PCPS.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>Seeder.</returns>
    public Seeder SeedPcps(IEnumerable<Bank> items)
    {
        var found = 0;
        var upToDate = 0;
        var notFound = 0;

        Logger.Log("PCPS\r\n", ConsoleColor.DarkYellow);

        foreach (var pcps in items)
        {
            var bank = _source.SingleOrDefault(
                b => b.Document != null && b.Document.Equals(pcps.Document)
            );

            bank ??= _source.SingleOrDefault(
                b =>
                    b.LongName
                        .RemoveDiacritics()
                        .Equals(
                            pcps.LongName.RemoveDiacritics(),
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    || (
                        b.ShortName != null
                        && b.ShortName
                            .RemoveDiacritics()
                            .Equals(
                                pcps.LongName.RemoveDiacritics(),
                                StringComparison.InvariantCultureIgnoreCase
                            )
                    )
            );

            if (bank == null)
            {
                var ispb = int.Parse(pcps.Document.RemoveNonNumeric()[..8]);

                if (
                    ispb == 0
                    && !pcps.LongName.Equals(
                        "Banco do Brasil",
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                {
                    Logger.Log(
                        $"PCPS | ISPB null: {pcps.LongName} | {pcps.Document.Trim()}",
                        ConsoleColor.DarkRed
                    );
                    continue;
                }

                bank = _source.SingleOrDefault(
                    b =>
                        b.Ispb.Equals(ispb)
                        && b.LongName.Contains(
                            pcps.LongName,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                );
            }

            if (bank == null)
            {
                Logger.Log(
                    $"PCPS | Bank not found: {pcps.LongName} | {pcps.Document.Trim()}",
                    ConsoleColor.DarkRed
                );
                notFound++;
                continue;
            }

            if (
                (string.IsNullOrWhiteSpace(bank.Document) || bank.Document.Length != 18)
                && !string.IsNullOrWhiteSpace(pcps.Document)
            )
            {
                bank.SetChange(Source.Pcps, x => x.Document, pcps.Document);
            }
            else if (string.IsNullOrWhiteSpace(bank.Document) || bank.Document.Length != 18)
            {
                Logger.Log(
                    $"SILOC | Invalid document {pcps.Compe} | {bank.Document} | {pcps.Document}",
                    ConsoleColor.DarkRed
                );
            }

            if (
                bank.SalaryPortability != null
                && bank.SalaryPortability.Equals(pcps.SalaryPortability)
            )
            {
                Logger.Log(
                    $"PCPS | Bank {pcps.Compe} salary portability is updated: {pcps.LongName}",
                    ConsoleColor.DarkGreen
                );
                upToDate++;
                continue;
            }

            bank.SetChange(Source.Pcps, x => x.SalaryPortability, pcps.SalaryPortability);
            found++;
        }

        Logger.Log(
            $"\r\nPCPS | Found: {found} | Not found: {notFound} | Up to date: {upToDate}\r\n",
            ConsoleColor.DarkYellow
        );

        return this;
    }

    /// <summary>
    /// Seeds the CQL.
    /// </summary>
    /// <param name="items">The items.</param>
    /// <returns>Seeder.</returns>
    public Seeder SeedCql(IEnumerable<Bank> items)
    {
        var found = 0;
        var upToDate = 0;
        var notFound = 0;

        Logger.Log("CQL\r\n", ConsoleColor.DarkYellow);

        foreach (var cql in items)
        {
            var bank = _source.SingleOrDefault(b => b.Ispb.Equals(cql.Ispb));

            if (bank == null)
            {
                Logger.Log(
                    $"CQL | Bank not found: {cql.LongName} | {cql.Document.Trim()}",
                    ConsoleColor.DarkRed
                );
                notFound++;
                continue;
            }

            if (bank.LegalCheque)
            {
                Logger.Log(
                    $"CQL | Banl  {cql.Compe} Legal Cheque is updated: {cql.LongName}",
                    ConsoleColor.DarkGreen
                );
                upToDate++;
                continue;
            }

            bank.SetChange(Source.Cql, x => x.LegalCheque, true);
            found++;
        }

        Logger.Log(
            $"\r\nCQL | Found: {found} | Not found: {notFound} | Up to date: {upToDate}\r\n",
            ConsoleColor.DarkYellow
        );

        return this;
    }

    /// <summary>
    /// Seeds the detecta flow.
    /// </summary>
    /// <param name="items">The items.</param>
    public void SeedDetectaFlow(IEnumerable<Bank> items)
    {
        var found = 0;
        var upToDate = 0;
        var notFound = 0;

        Logger.Log("DetectaFlow\r\n", ConsoleColor.DarkYellow);

        foreach (var detectaFlow in items)
        {
            var bank = _source.SingleOrDefault(b => b.Ispb.Equals(detectaFlow.Ispb));

            if (bank == null)
            {
                Logger.Log(
                    $"Detecta Flow | Bank not found: {detectaFlow.LongName} | {detectaFlow.Document.Trim()}",
                    ConsoleColor.DarkRed
                );
                notFound++;
                continue;
            }

            if (bank.DetectaFlow)
            {
                Logger.Log(
                    $"Detecta Flow | Bank {detectaFlow.Compe} Detecta Flow is updated: {detectaFlow.LongName}",
                    ConsoleColor.DarkGreen
                );
                upToDate++;
                continue;
            }

            bank.SetChange(Source.DetectaFlow, x => x.DetectaFlow, true);
            found++;
        }

        Logger.Log(
            $"\r\nDetecta Flow | Found: {found} | Not found: {notFound} | Up to date: {upToDate}\r\n",
            ConsoleColor.DarkYellow
        );
    }
}
