﻿// ***********************************************************************
// Assembly         : BancosBrasileiros.MergeTool
// Author           : Guilherme Branco Stracini
// Created          : 05-31-2022
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 14-05-2024
// ***********************************************************************
// <copyright file="Patterns.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace BancosBrasileiros.MergeTool.Helpers;

using System;
using System.Text.RegularExpressions;

/// <summary>
/// Class Patterns.
/// </summary>
internal static class Patterns
{
    /// <summary>
    /// The comma separated values pattern
    /// </summary>
    public static readonly Regex CsvPattern = new(
        ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5)
    );

    /// <summary>
    /// The semicolon separated values pattern
    /// </summary>
    public static readonly Regex SsvPattern = new(
        ";(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5)
    );

    /// <summary>
    /// The SLC pattern
    /// </summary>
    public static readonly Regex SlcPattern = new(
        @"^(?<code>\d{1,3})\s(?<cnpj>\d{1,2}\.\d{3}\.\d{3}(?:.|\/)\d{4}([-|·|\.|\s]{1,2})\d{2})\s(?<nome>.+?)(?:[\s|X]){2,7}(Confidencial)?$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5)
    );

    /// <summary>
    /// The SILOC pattern
    /// </summary>
    public static readonly Regex SilocPattern = new(
        @"^(?<code>\d{1,3})\s(?<compe>\d{3})\s(?<ispb>\d{8})\s(?<cobranca>sim|não)\s(?<doc>sim|não)\s(?<nome>.+?)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5)
    );

    /// <summary>
    /// The SITRAF pattern
    /// </summary>
    public static readonly Regex SitrafPattern = new(
        @"^(?<code>\d{1,3})\s(?<compe>\d{3})\s(?<ispb>\d{8})\s(?<nome>.+?)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5)
    );

    /// <summary>
    /// The ctc pattern
    /// </summary>
    public static readonly Regex CtcPattern = new(
        @"^\s?(?<code>\d{1,3})\s(?<nome>.+?)\s(?<cnpj>\d{1,2}\.\d{3}\.\d{3}(?:.|\/)\d{4}([-|·|\.|\s]{1,2})\d{2})\s+(?<ispb>\d{8})\s(?<produtos>.+?)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5)
    );

    /// <summary>
    /// The PCPS pattern
    /// </summary>
    public static readonly Regex PcpsPattern = new(
        @"^(?<code>\d{1,3})\s(?<nome>.+?)\s(?<cnpj>\d{1,2}\.\d{3}\.\d{3}(?:.|\/)\d{4}([-|·|\.|\s]{1,3})\d{2})\s+(?<ispb>\d{7,8})\s(?<adesao>.+?)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5)
    );

    /// <summary>
    /// The CQL pattern
    /// </summary>
    public static readonly Regex CqlPattern = new(
        @"^(?<code>\d{1,3})\s(?<nome>.+?)\s(?<ispb>\d{7,8})\s(?<tipo>.+?)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5)
    );

    /// <summary>
    /// The detecta flow pattern
    /// </summary>
    public static readonly Regex DetectaFlowPattern = new(
        @"^(?<code>\d{1,3})\s(?<nome>.+?)\s(?<cnpj>\d{1,2}\.\d{3}\.\d{3}(?:.|\/)\d{4}([-|·|\.|\s]{1,2})\d{2})\s+(?<ispb>\d{7,8})(.+?)$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5)
    );

    /// <summary>
    /// The PCR pattern
    /// </summary>
    public static readonly Regex PcrPattern = new(
        @"^(?<code>\d{1,3})\s(?<nome>.+?)\s(?<cnpj>\d{1,2}\.\d{3}\.\d{3}(?:.|\/)\d{4}([-|·|\.|\s]{1,2})\d{2})\s+(?<compe>\d{3})\s+(?<ispb>\d{7,8})\s(?<pcr>.{3})\s(?<pcrp>.{3})(.+)?$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled,
        TimeSpan.FromSeconds(5)
    );
}
