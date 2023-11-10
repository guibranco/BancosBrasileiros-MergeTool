// ***********************************************************************
// Assembly         : BancosBrasileiros.MergeTool
// Author           : Guilherme Branco Stracini
// Created          : 19/05/2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 06-01-2022
// ***********************************************************************
// <copyright file="Worker.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace BancosBrasileiros.MergeTool;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrispyWaffle.Extensions;
using Dto;
using Helpers;

/// <summary>
/// Class Worker.
/// </summary>
internal class Worker
{
    /// <summary>
    /// Works this instance.
    /// </summary>
    public void Work()
    {
        Logger.Log("Reading data files", ConsoleColor.White);

        var reader = new Reader();

        var source = reader.LoadBase();
        var original = source.DeepClone();

        AcquireData(reader, source);

        if (ProcessData(original, ref source, out var except))
        {
            return;
        }

        ProcessChanges(source, except, original);
    }

    /// <summary>
    /// Acquires the data.
    /// </summary>
    /// <param name="reader">The reader.</param>
    /// <param name="source">The source.</param>
    private static void AcquireData(Reader reader, List<Bank> source)
    {
        var logBuilder = new StringBuilder("\r\n");

        logBuilder.AppendFormat("Source: {0} | ", source.Count);

        var cql = reader.LoadCql();
        logBuilder.AppendFormat("CQL: {0} | ", cql.Count);

        var ctc = reader.LoadCtc();
        logBuilder.AppendFormat("CTC: {0} | ", ctc.Count);

        var detectaFlow = reader.LoadDetectaFlow();
        logBuilder.AppendFormat("Detecta Flow: {0} | ", detectaFlow.Count);

        var siloc = reader.LoadSiloc();
        logBuilder.AppendFormat("SILOC: {0} | ", siloc.Count);

        var sitraf = reader.LoadSitraf();
        logBuilder.AppendFormat("SITRAF: {0} | ", sitraf.Count);

        var slc = reader.LoadSlc();
        logBuilder.AppendFormat("SLC: {0} | ", slc.Count);

        var spi = reader.LoadSpi();
        logBuilder.AppendFormat("SPI: {0} | ", spi.Count);

        var str = reader.LoadStr();
        logBuilder.AppendFormat("STR: {0} | ", str.Count);

        var pcps = reader.LoadPcps();
        logBuilder.AppendFormat("PCPS: {0} | ", pcps.Count);

        logBuilder.AppendLine();

        Logger.Log(logBuilder.ToString(), ConsoleColor.DarkGreen);

        new Seeder(source)
            .GenerateMissingDocument()
            .SeedStr(str)
            .SeedSitraf(sitraf)
            .SeedSlc(slc)
            .SeedSpi(spi)
            .SeedCtc(ctc)
            .SeedSiloc(siloc)
            .SeedPcps(pcps)
            .SeedCql(cql)
            .SeedDetectaFlow(detectaFlow);
    }

    /// <summary>
    /// Processes the data.
    /// </summary>
    /// <param name="original">The original.</param>
    /// <param name="source">The source.</param>
    /// <param name="except">The except.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    private static bool ProcessData(
        List<Bank> original,
        ref List<Bank> source,
        out List<Bank> except
    )
    {
        foreach (var bank in source)
        {
            bank.DateRegistered ??= DateTimeOffset.UtcNow;
            bank.DateUpdated ??= DateTimeOffset.UtcNow;
        }

        var types = source.GroupBy(b => b.Type);

        Logger.Log($"Type: All | Total: {source.Count}", ConsoleColor.Yellow);

        foreach (var type in types.OrderBy(g => g.Key))
        {
            Logger.Log(
                $"Type: {(string.IsNullOrWhiteSpace(type.Key) ? "-" : type.Key)} | Total: {type.Count()}",
                ConsoleColor.DarkYellow
            );
        }

        source = source.Where(b => b.Ispb != 0 || b.Compe == 1).ToList();

        except = source.Except(original).ToList();

        if (except.Any())
        {
            return false;
        }

        Logger.Log("No new data or updated information", ConsoleColor.DarkMagenta);
        Environment.Exit(187);
        return true;
    }

    /// <summary>
    /// Processes the changes.
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="except">The except.</param>
    /// <param name="original">The original.</param>
    private static void ProcessChanges(List<Bank> source, List<Bank> except, List<Bank> original)
    {
        var added = new List<Bank>();
        var updated = new List<Bank>();

        foreach (var exc in except)
        {
            var isUpdated = original.Exists(b => b.Ispb == exc.Ispb);

            if (isUpdated)
            {
                updated.Add(exc);
            }
            else
            {
                added.Add(exc);
            }
        }

        var changeLog = new StringBuilder();
        ProcessChangesAdded(changeLog, added);
        ProcessChangesUpdated(changeLog, updated);

        Logger.Log("\r\nSaving result files", ConsoleColor.White);

        var changeLogData = changeLog.ToString();

        Writer.WriteReleaseNotes(changeLogData);
        Writer.WriteChangeLog(changeLogData);
        Writer.SaveBanks(source);

        Logger.Log($"Merge done. Banks: {source.Count}", ConsoleColor.White);
    }

    /// <summary>
    /// Processes the changes added.
    /// </summary>
    /// <param name="changeLog">The change log.</param>
    /// <param name="added">The added.</param>
    private static void ProcessChangesAdded(StringBuilder changeLog, List<Bank> added)
    {
        if (added.Count == 0)
        {
            return;
        }

        changeLog.AppendLine(
            $"- Added {added.Count} bank{(added.Count == 1 ? string.Empty : "s")}"
        );

        Logger.Log($"\r\nAdded items: {added.Count}\r\n\r\n", ConsoleColor.White);

        var color = ConsoleColor.DarkGreen;

        foreach (var item in added)
        {
            changeLog.AppendLine($"  - {item.Compe} - {item.ShortName} - {item.Document}");
            color = color == ConsoleColor.DarkGreen ? ConsoleColor.Cyan : ConsoleColor.DarkGreen;
            Logger.Log($"Added: {item}\r\n", color);
        }
    }

    /// <summary>
    /// Processes the changes updated.
    /// </summary>
    /// <param name="changeLog">The change log.</param>
    /// <param name="updated">The updated.</param>
    private static void ProcessChangesUpdated(StringBuilder changeLog, List<Bank> updated)
    {
        if (updated.Count == 0)
        {
            return;
        }

        changeLog.AppendLine(
            $"- Updated {updated.Count} bank{(updated.Count == 1 ? string.Empty : "s")}"
        );

        Logger.Log($"\r\nUpdated items: {updated.Count}\r\n\r\n", ConsoleColor.White);

        var color = ConsoleColor.DarkBlue;

        foreach (var item in updated)
        {
            changeLog.AppendLine($"  - {item.Compe} - {item.ShortName} - {item.Document}");
            if (item.HasChanges)
            {
                foreach (var change in item.GetChanges())
                {
                    changeLog.AppendLine(
                        $"    - **{change.Key}** ({change.Value.Source.GetHumanReadableValue()}): {change.Value.OldValue} **->** {change.Value.NewValue}"
                    );
                }
            }

            color = color == ConsoleColor.DarkBlue ? ConsoleColor.Blue : ConsoleColor.DarkBlue;
            Logger.Log($"Updated: {item}\r\n", color);
        }
    }
}
