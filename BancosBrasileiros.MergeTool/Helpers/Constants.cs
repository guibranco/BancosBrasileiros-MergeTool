﻿// ***********************************************************************
// Assembly         : BancosBrasileiros.MergeTool
// Author           : Guilherme Branco Stracini
// Created          : 05-31-2022
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 19-06-2024
// ***********************************************************************
// <copyright file="Constants.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) . All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

#pragma warning disable S1075
namespace BancosBrasileiros.MergeTool.Helpers;

/// <summary>
/// Class Constants.
/// </summary>
internal static class Constants
{
    /// <summary>
    /// The change log URL.
    /// </summary>
    public const string ChangeLogUrl =
        "https://raw.githubusercontent.com/guibranco/BancosBrasileiros/main/CHANGELOG.md";

    /// <summary>
    /// The base URL.
    /// </summary>
    public const string BaseUrl =
        "https://raw.githubusercontent.com/guibranco/BancosBrasileiros/main/data/bancos.json";

    /// <summary>
    /// The STR URL.
    /// </summary>
    public const string StrUrl =
        "https://www.bcb.gov.br/content/estabilidadefinanceira/str1/ParticipantesSTR.csv";

    /// <summary>
    /// The SPI/PIX URL.
    /// </summary>
    public const string SpiUrl =
        "https://www.bcb.gov.br/content/estabilidadefinanceira/spi/participantes-spi-{0:yyyyMMdd}.csv";

    /// <summary>
    /// The SLC URL.
    /// </summary>
    public const string SlcUrl =
        "https://www2.nuclea.com.br/Monitoramento/Participantes%20Homologados.pdf";

    /// <summary>
    /// The SILOC URL.
    /// </summary>
    public const string SilocUrl = "https://www2.nuclea.com.br/Monitoramento/SILOC.pdf";

    /// <summary>
    /// The SITRAF URL.
    /// </summary>
    public const string SitrafUrl =
        "https://www2.nuclea.com.br/Monitoramento/Rela%C3%A7%C3%A3o%20de%20Clientes%20SITRAF.pdf";

    /// <summary>
    /// The CTC URL.
    /// </summary>
    public const string CtcUrl =
        "https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Clientes%20CTC.pdf";

    /// <summary>
    /// The PCPS URL.
    /// </summary>
    public const string PcpsUrl =
        "https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20PCPS.pdf";

    /// <summary>
    /// The CQL URL.
    /// </summary>
    public const string CqlUrl =
        "https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20CQL.pdf";

    /// <summary>
    /// The DetectaFlow URL.
    /// </summary>
    public const string DetectaFlowUrl =
        "https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Participantes%20-%20Detecta%20Flow.pdf";

    /// <summary>
    /// The PCR URL.
    /// </summary>
    public const string PcrUrl =
        "https://www2.nuclea.com.br/SAP/Rela%C3%A7%C3%A3o%20de%20Clientes%20PCR.pdf";
}
