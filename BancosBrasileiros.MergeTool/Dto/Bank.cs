﻿// ***********************************************************************
// Assembly         : BancosBrasileiros.MergeTool
// Author           : Guilherme Branco Stracini
// Created          : 18/05/2020
//
// Last Modified By : Guilherme Branco Stracini
// Last Modified On : 06-01-2022
// ***********************************************************************
// <copyright file="Bank.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace BancosBrasileiros.MergeTool.Dto;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using CrispyWaffle.Extensions;
using CrispyWaffle.I18n.PtBr;
using CrispyWaffle.Validations;
using Helpers;
using Newtonsoft.Json;

/// <summary>
/// Class Bank.
/// Implements the <see cref="IEquatable{Bank}" />
/// </summary>
/// <seealso cref="IEquatable{Bank}" />
[XmlRoot("Bank")]
[Serializable]
public sealed class Bank : IEquatable<Bank>
{
    #region Track Changes

    /// <summary>
    /// The changes
    /// </summary>
    private readonly Dictionary<string, ChangeModel> _changes = new();

    /// <summary>
    /// Sets a specified property of the current object to a new value and records the change.
    /// </summary>
    /// <param name="source">The source of the change, indicating where the change originated from.</param>
    /// <param name="predicate">An expression that specifies the property to be changed.</param>
    /// <param name="newValue">The new value to set for the specified property.</param>
    /// <remarks>
    /// This method uses a lambda expression to identify the property of the current object that needs to be updated.
    /// It first compiles the expression to get the current value of the property. Then, it determines the property name
    /// from the expression body, which can either be a member expression or a unary expression containing a member expression.
    /// If the property name is invalid or cannot be determined, an exception is thrown.
    /// After successfully setting the new value, the method updates the <c>DateUpdated</c> property to the current UTC time.
    /// Additionally, it creates a <c>ChangeModel</c> instance to record the source of the change, along with the old and new values,
    /// and adds this change model to an internal collection of changes.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the property specified by the predicate is invalid or cannot be determined.</exception>
    public void SetChange(Source source, Expression<Func<Bank, object>> predicate, object newValue)
    {
        var property = string.Empty;
        var currentValue = predicate.Compile()(this);

        property = predicate.Body switch
        {
            UnaryExpression { Operand: MemberExpression member } => member.Member.Name,
            MemberExpression { Member: PropertyInfo propertyInfo } => propertyInfo.Name,
            _ => property,
        };

        if (string.IsNullOrWhiteSpace(property))
        {
            throw new InvalidOperationException("Invalid property");
        }

        GetType().GetProperty(property)?.SetValue(this, newValue);
        DateUpdated = DateTimeOffset.UtcNow;

        var changeModel = new ChangeModel
        {
            Source = source,
            OldValue = currentValue.GetStringValue(),
            NewValue = newValue.GetStringValue(),
        };

        _changes.Add(property, changeModel);
    }

    /// <summary>
    /// Accepts the changes.
    /// </summary>
    public void AcceptChanges()
    {
        _changes.Clear();
    }

    /// <summary>
    /// Rejects the changes.
    /// </summary>
    public void RejectChanges()
    {
        foreach (var change in _changes)
        {
            var property = change.Key;
            var value = change.Value.OldValue;
            GetType().GetProperty(property)?.SetValue(this, value);
        }

        _changes.Clear();
    }

    /// <summary>
    /// Gets the has changes.
    /// </summary>
    /// <value>The has changes.</value>
    [JsonIgnore]
    [XmlIgnore]
    public bool HasChanges => _changes.Any();

    /// <summary>
    /// Gets the changes.
    /// </summary>
    /// <returns>System.Collections.Generic.IReadOnlyDictionary&lt;string, BancosBrasileiros.MergeTool.Dto.ChangeModel&gt;.</returns>
    public IReadOnlyDictionary<string, ChangeModel> GetChanges() => _changes.AsReadOnly();

    #endregion

    /// <summary>
    /// Gets or sets the COMPE.
    /// </summary>
    /// <value>The COMPE.</value>
    [JsonIgnore]
    [XmlIgnore]
    public int Compe { get; set; }

    /// <summary>
    /// Gets the COMPE string.
    /// </summary>
    /// <value>The COMPE string.</value>
    [JsonProperty("COMPE")]
    [XmlElement("COMPE")]
    [Display(Name = "COMPE")]
    public string CompeString
    {
        get => Compe.ToString("000");
        set
        {
            if (int.TryParse(value, out var parsed))
            {
                Compe = parsed;
            }
        }
    }

    /// <summary>
    /// Gets or sets the ispb.
    /// </summary>
    /// <value>The ispb.</value>
    [JsonIgnore]
    [XmlIgnore]
    public int Ispb { get; set; }

    /// <summary>
    /// Gets or sets the ispb string.
    /// </summary>
    /// <value>The ispb string.</value>
    [JsonProperty("ISPB")]
    [XmlElement("ISPB")]
    [Display(Name = "ISPB")]
    public string IspbString
    {
        get => Ispb.ToString("00000000");
        set
        {
            if (int.TryParse(value, out var parsed))
            {
                Ispb = parsed;
            }
        }
    }

    /// <summary>
    /// The document
    /// </summary>
    private string _document;

    /// <summary>
    /// Gets or sets the document.
    /// </summary>
    /// <value>The document.</value>
    [JsonProperty("Document")]
    [XmlElement("Document")]
    [Display(Name = "Document")]
    public string Document
    {
        get => _document;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            var document = new string(value.Where(char.IsDigit).ToArray());
            if (document.Length == 8)
            {
                if (document.Equals("00000000"))
                {
                    document = "00000000000191";
                }
                else
                {
                    document += "0001";
                    document += $"{document}00".CalculateBrazilianCorporateDocument();
                }
            }

            _document = document.FormatBrazilianDocument();
        }
    }

    /// <summary>
    /// Gets or sets the long name.
    /// </summary>
    /// <value>The long name.</value>
    [JsonProperty("LongName")]
    [XmlElement("LongName")]
    [Display(Name = "Long Name")]
    public string LongName { get; set; }

    /// <summary>
    /// Gets or sets the short name.
    /// </summary>
    /// <value>The short name.</value>
    [JsonProperty("ShortName")]
    [XmlElement("ShortName")]
    [Display(Name = "Short Name")]
    public string ShortName { get; set; }

    /// <summary>
    /// Gets or sets the network.
    /// </summary>
    /// <value>The network.</value>
    [JsonProperty("Network")]
    [XmlElement("Network")]
    [Display(Name = "Network")]
    public string Network { get; set; }

    /// <summary>
    /// Gets or sets the type.
    /// </summary>
    /// <value>The type.</value>
    [JsonProperty("Type")]
    [XmlElement("Type")]
    [Display(Name = "Type")]
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the type of the pix.
    /// </summary>
    /// <value>The type of the pix.</value>
    [JsonProperty("PixType")]
    [XmlElement("PixType")]
    [Display(Name = "PIX Type")]
    public string PixType { get; set; }

    /// <summary>
    /// Gets or sets the charge.
    /// </summary>
    /// <value>The charge.</value>
    [JsonProperty("Charge")]
    [XmlElement("Charge")]
    [Display(Name = "Charge")]
    public bool? Charge
    {
        get =>
            string.IsNullOrWhiteSpace(ChargeStr)
                ? null
                : ChargeStr.Equals("sim", StringComparison.InvariantCultureIgnoreCase);
        set
        {
            if (value == null)
            {
                return;
            }

            ChargeStr = value.Value ? "sim" : "não";
        }
    }

    /// <summary>
    /// Gets or sets the charge string.
    /// </summary>
    /// <value>The charge string.</value>
    [JsonIgnore]
    [XmlIgnore]
    public string ChargeStr { get; set; }

    /// <summary>
    /// Gets or sets the credit document.
    /// </summary>
    /// <value>The credit document.</value>
    [JsonProperty("CreditDocument")]
    [XmlElement("CreditDocument")]
    [Display(Name = "Credit Document")]
    public bool? CreditDocument
    {
        get =>
            string.IsNullOrWhiteSpace(CreditDocumentStr)
                ? null
                : CreditDocumentStr.Equals("sim", StringComparison.InvariantCultureIgnoreCase);
        set
        {
            if (value == null)
            {
                return;
            }

            CreditDocumentStr = value.Value ? "sim" : "não";
        }
    }

    /// <summary>
    /// Gets or sets the credit document string.
    /// </summary>
    /// <value>The credit document string.</value>
    [JsonIgnore]
    [XmlIgnore]
    public string CreditDocumentStr { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [legal cheque].
    /// </summary>
    /// <value><c>true</c> if [legal cheque]; otherwise, <c>false</c>.</value>
    [JsonProperty("LegalCheque")]
    [XmlElement("LegalCheque")]
    [Display(Name = "Legal Cheque")]
    public bool LegalCheque { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether [detecta flow].
    /// </summary>
    /// <value><c>true</c> if [detecta flow]; otherwise, <c>false</c>.</value>
    [JsonProperty("DetectaFlow")]
    [XmlElement("DetectaFLow")]
    [Display(Name = "Detecta Flow")]
    public bool DetectaFlow { get; set; }

    /// <summary>
    /// Gets or sets the PCR.
    /// </summary>
    /// <value>The PCR.</value>
    [JsonProperty("PCR")]
    [XmlElement("PCR")]
    [Display(Name = "PCR")]
    public bool? Pcr
    {
        get =>
            string.IsNullOrWhiteSpace(PcrStr)
                ? null
                : PcrStr.Equals("sim", StringComparison.InvariantCultureIgnoreCase);
        set
        {
            if (value == null)
            {
                return;
            }

            PcrStr = value.Value ? "sim" : "não";
        }
    }

    /// <summary>
    /// Gets or sets the PCR string.
    /// </summary>
    /// <value>The PCR string.</value>
    [JsonIgnore]
    [XmlIgnore]
    public string PcrStr { get; set; }

    /// <summary>
    /// Gets or sets the PCRP.
    /// </summary>
    /// <value>The PCRP.</value>
    [JsonProperty("PCRP")]
    [XmlElement("PCRP")]
    [Display(Name = "PCRP")]
    public bool? Pcrp
    {
        get =>
            string.IsNullOrWhiteSpace(PcrpStr)
                ? null
                : PcrpStr.Equals("sim", StringComparison.InvariantCultureIgnoreCase);
        set
        {
            if (value == null)
            {
                return;
            }

            PcrpStr = value.Value ? "sim" : "não";
        }
    }

    /// <summary>
    /// Gets or sets the PCRP string.
    /// </summary>
    /// <value>The PCRP string.</value>
    [JsonIgnore]
    [XmlIgnore]
    public string PcrpStr { get; set; }

    /// <summary>
    /// Gets or sets the salary portability.
    /// </summary>
    /// <value>The salary portability.</value>
    [JsonProperty("SalaryPortability")]
    [XmlElement("SalaryPortability")]
    [Display(Name = "Salary Portability")]
    public string SalaryPortability { get; set; }

    /// <summary>
    /// Gets or sets the products.
    /// </summary>
    /// <value>The products.</value>
    [JsonProperty("Products")]
    [XmlArray("Products")]
    [XmlArrayItem("Product")]
    public string[] Products { get; set; }

    /// <summary>
    /// The URL
    /// </summary>
    private string _url;

    /// <summary>
    /// Gets or sets the URL.
    /// </summary>
    /// <value>The URL.</value>
    [JsonProperty("Url")]
    [XmlElement("Url", IsNullable = true)]
    [Display(Name = "Url")]
    public string Url
    {
        get => _url;
        set
        {
            if (string.IsNullOrWhiteSpace(value) || value.Equals("NA"))
            {
                return;
            }

            _url = $"https://{value.ToLower().Replace("https://", "")}";
        }
    }

    /// <summary>
    /// Gets or sets the date operation started.
    /// </summary>
    /// <value>The date operation started.</value>
    [JsonProperty("DateOperationStarted")]
    [XmlElement("DateOperationStarted")]
    [Display(Name = "Date Operation Started")]
    public string DateOperationStarted { get; set; }

    /// <summary>
    /// Gets or sets the date pix started.
    /// </summary>
    /// <value>The date pix started.</value>
    [JsonProperty("DatePixStarted")]
    [XmlElement("DatePixStarted")]
    [Display(Name = "Date PIX Started")]
    public string DatePixStarted { get; set; }

    /// <summary>
    /// Gets or sets the date registered.
    /// </summary>
    /// <value>The date registered.</value>
    [JsonProperty("DateRegistered")]
    [XmlElement("DateRegistered")]
    [Display(Name = "Date Registered")]
    public DateTimeOffset? DateRegistered { get; set; }

    /// <summary>
    /// Gets or sets the date updated.
    /// </summary>
    /// <value>The date updated.</value>
    [JsonProperty("DateUpdated")]
    [XmlElement("DateUpdated")]
    [Display(Name = "Date Updated")]
    public DateTimeOffset? DateUpdated { get; set; }

    #region Equality members

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this object.</param>
    /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
    // ReSharper disable once CyclomaticComplexity
    public bool Equals(Bank other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return string.Equals(
                _document,
                other._document,
                StringComparison.InvariantCultureIgnoreCase
            )
            && string.Equals(_url, other._url, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(
                ChargeStr,
                other.ChargeStr,
                StringComparison.InvariantCultureIgnoreCase
            )
            && Compe == other.Compe
            && string.Equals(
                CreditDocumentStr,
                other.CreditDocumentStr,
                StringComparison.InvariantCultureIgnoreCase
            )
            && string.Equals(
                DateOperationStarted,
                other.DateOperationStarted,
                StringComparison.InvariantCultureIgnoreCase
            )
            && string.Equals(
                DatePixStarted,
                other.DatePixStarted,
                StringComparison.InvariantCultureIgnoreCase
            )
            && Nullable.Equals(DateRegistered, other.DateRegistered)
            && Nullable.Equals(DateUpdated, other.DateUpdated)
            && Ispb == other.Ispb
            && string.Equals(LongName, other.LongName, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(Network, other.Network, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(PixType, other.PixType, StringComparison.InvariantCultureIgnoreCase)
            && LegalCheque == other.LegalCheque
            && DetectaFlow == other.DetectaFlow
            && Pcr == other.Pcr
            && Pcrp == other.Pcrp
            //&& Equals(Products, other.Products)
            && string.Equals(
                SalaryPortability,
                other.SalaryPortability,
                StringComparison.InvariantCultureIgnoreCase
            )
            && string.Equals(
                ShortName,
                other.ShortName,
                StringComparison.InvariantCultureIgnoreCase
            )
            && string.Equals(Type, other.Type, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object  is equal to the current object; otherwise, <see langword="false" />.</returns>
    public override bool Equals(object obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return obj.GetType() == GetType() && Equals((Bank)obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
#pragma warning disable S2328
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public override int GetHashCode()
#pragma warning restore S2328
    {
        unchecked
        {
            var hashCode = new HashCode();
            hashCode.Add(_document ?? string.Empty, StringComparer.InvariantCultureIgnoreCase);
            hashCode.Add(_url ?? string.Empty, StringComparer.InvariantCultureIgnoreCase);
            hashCode.Add(ChargeStr ?? string.Empty, StringComparer.InvariantCultureIgnoreCase);
            hashCode.Add(Compe);
            hashCode.Add(
                CreditDocumentStr ?? string.Empty,
                StringComparer.InvariantCultureIgnoreCase
            );
            hashCode.Add(
                DateOperationStarted ?? string.Empty,
                StringComparer.InvariantCultureIgnoreCase
            );
            hashCode.Add(DatePixStarted ?? string.Empty, StringComparer.InvariantCultureIgnoreCase);
            hashCode.Add(DateRegistered);
            hashCode.Add(DateUpdated);
            hashCode.Add(Ispb);
            hashCode.Add(LongName ?? string.Empty, StringComparer.InvariantCultureIgnoreCase);
            hashCode.Add(Network ?? string.Empty, StringComparer.InvariantCultureIgnoreCase);
            hashCode.Add(PixType ?? string.Empty, StringComparer.InvariantCultureIgnoreCase);
            hashCode.Add(LegalCheque);
            hashCode.Add(DetectaFlow);
            hashCode.Add(Pcr);
            hashCode.Add(Pcrp);
            hashCode.Add(
                SalaryPortability ?? string.Empty,
                StringComparer.InvariantCultureIgnoreCase
            );
            hashCode.Add(ShortName ?? string.Empty, StringComparer.InvariantCultureIgnoreCase);
            hashCode.Add(Type ?? string.Empty, StringComparer.InvariantCultureIgnoreCase);

            return hashCode.ToHashCode();
        }
    }

    /// <summary>
    /// Returns a value that indicates whether the values of two <see cref="T:BancosBrasileiros.MergeTool.Dto.Bank" /> objects are equal.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
    public static bool operator ==(Bank left, Bank right) => Equals(left, right);

    /// <summary>
    /// Returns a value that indicates whether two <see cref="T:BancosBrasileiros.MergeTool.Dto.Bank" /> objects have different values.
    /// </summary>
    /// <param name="left">The first value to compare.</param>
    /// <param name="right">The second value to compare.</param>
    /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
    public static bool operator !=(Bank left, Bank right) => !Equals(left, right);

    #endregion

    #region Overrides of Object

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    // ReSharper disable once CognitiveComplexity
    public override string ToString()
    {
        var strBuilder = new StringBuilder();

        strBuilder.Append($"COMPE: {CompeString} | ");

        if (Ispb > 0 || Compe.Equals(1))
        {
            strBuilder.Append($"ISPB: {IspbString} | ");
        }

        if (!string.IsNullOrWhiteSpace(Document))
        {
            strBuilder.Append($"Document: {Document} | ");
        }

        if (!string.IsNullOrWhiteSpace(LongName))
        {
            strBuilder.Append($"Long name: {LongName} | ");
        }

        if (!string.IsNullOrWhiteSpace(ShortName))
        {
            strBuilder.Append($"Short name: {ShortName} | ");
        }

        if (!string.IsNullOrWhiteSpace(Network))
        {
            strBuilder.Append($"Network: {Network} | ");
        }

        if (!string.IsNullOrWhiteSpace(Type))
        {
            strBuilder.Append($"Type: {Type} | ");
        }

        if (!string.IsNullOrWhiteSpace(PixType))
        {
            strBuilder.Append($"PIX type: {PixType} | ");
        }

        if (Charge.HasValue)
        {
            strBuilder.Append($"Charge: {Charge} | ");
        }

        if (CreditDocument.HasValue)
        {
            strBuilder.Append($"Credit document: {CreditDocument} | ");
        }

        strBuilder.Append($"Legal cheque: {LegalCheque} | ");

        strBuilder.Append($"Detecta Flow: {DetectaFlow} | ");

        strBuilder.Append($"PCR: {Pcr} | ");

        strBuilder.Append($"PCRP: {Pcrp} | ");

        if (!string.IsNullOrWhiteSpace(SalaryPortability))
        {
            strBuilder.Append($"Salary portability: {SalaryPortability} | ");
        }

        if (Products != null)
        {
            strBuilder.Append($"Products: {string.Join(",", Products)} | ");
        }

        if (!string.IsNullOrWhiteSpace(DateOperationStarted))
        {
            strBuilder.Append($"Date operation started: {DateOperationStarted} | ");
        }

        if (!string.IsNullOrWhiteSpace(DatePixStarted))
        {
            strBuilder.Append($"Date PIX started: {DatePixStarted} | ");
        }

        strBuilder.Append($"Date registered: {DateRegistered:O} | Date updated: {DateUpdated:O}");

        return strBuilder.ToString();
    }

    #endregion
}
