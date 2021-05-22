// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace WebCharts.Services
{
    internal interface IKeywordsRegistry
    {
        void Register(string name, string keyword, string keywordAliases, string description, string appliesToTypes, string appliesToProperties, bool supportsFormatting, bool supportsValueIndex);
    }
}