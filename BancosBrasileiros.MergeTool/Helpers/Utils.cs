// ***********************************************************************
// Assembly         : BancosBrasileiros.MergeTool
// Author           : GuilhermeStracini
// Created          : 08-30-2023
//
// Last Modified By : GuilhermeStracini
// Last Modified On : 08-30-2023
// ***********************************************************************
// <copyright file="Utils.cs" company="Guilherme Branco Stracini ME">
//     Copyright (c) Guilherme Branco Stracini ME. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace BancosBrasileiros.MergeTool.Helpers;

using Newtonsoft.Json;

/// <summary>
/// Class Utils.
/// </summary>
internal static class Utils
{
    /// <summary>
    /// Deep clones an object (convert it to JSON and back to the object)
    /// </summary>
    /// <typeparam name="T">The type of the item</typeparam>
    /// <param name="item">The item.</param>
    /// <returns>A deep clone of the item</returns>
    public static T DeepClone<T>(this T item)
    {
        var json = JsonConvert.SerializeObject(item);
        return JsonConvert.DeserializeObject<T>(json);
    }
}
