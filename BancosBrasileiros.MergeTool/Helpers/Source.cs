// ***********************************************************************
// Assembly         : BancosBrasileiros.MergeTool
// Author           : Guilherme Branco Stracini
    BcbTaxes,
    SfaOpenFinance
// Created          : 06-01-2022
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 06-01-2022
// ***********************************************************************
// <copyright file="Source.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace BancosBrasileiros.MergeTool.Helpers;

using CrispyWaffle.Attributes;

/// <summary>
/// Enum Source.
/// </summary>
public enum Source
{
    /// <summary>
    /// The base.
    /// </summary>
    [HumanReadable("Base")]
    Base,

    /// <summary>
    /// The change log.
    /// </summary>
    [HumanReadable("ChangeLog")]
    ChangeLog,

    /// <summary>
    /// The CQL.
    /// </summary>
    [HumanReadable("CQL")]
    Cql,

    /// <summary>
    /// The CTC.
    /// </summary>
    [HumanReadable("CTC")]
    Ctc,

    /// <summary>
    /// The detecta flow.
    /// </summary>
    [HumanReadable("DetectaFlow")]
    DetectaFlow,

    /// <summary>
    /// The document.
    /// </summary>
    [HumanReadable("Document")]
    Document,

    /// <summary>
    /// The PCPS.
    /// </summary>
    [HumanReadable("PCPS")]
    Pcps,

    /// <summary>
    /// The SILOC.
    /// </summary>
    [HumanReadable("SILOC")]
    Siloc,

    /// <summary>
    /// The SITRAF.
    /// </summary>
    [HumanReadable("SITRAF")]
    Sitraf,

    /// <summary>
    /// The SLC.
    /// </summary>
    [HumanReadable("SLC")]
    Slc,

    /// <summary>
    /// The SPI..
    /// </summary>
    [HumanReadable("SPI")]
    Spi,

    /// <summary>
    /// The STR.
    /// </summary>
    [HumanReadable("STR")]
    Str,

    /// <summary>
    /// The PCR.
    /// </summary>
    [HumanReadable("PCR")]
    Pcr,
}
