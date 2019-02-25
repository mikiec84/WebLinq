// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Source:
// https://github.com/aspnet/AspNetCore/blob/574be0d22c1678ed5f6db990aec78b4db587b267/src/Http/Http/src/Internal/QueryCollection.cs
//
// This is a slightly modified version from the snapshot above with the
// following changes:
//
// - Moved from namespace Microsoft.AspNetCore.WebUtilities to one belonging
//   to this project.
// - Renamed from StringValues to Strings.
// - Re-styled to use project conventions.
// - Remove unused members.

namespace WebLinq
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Collections;

    static class QueryHelpers
    {
        public static QueryCollection ParseAsParams(string queryString) =>
            new QueryCollection(ImmutableArray.CreateRange(Parse(queryString)));

        public static IEnumerable<KeyValuePair<string, string>> Parse(string queryString)
        {
            if (string.IsNullOrEmpty(queryString) || queryString == "?")
                yield break;

            var scanIndex = 0;
            if (queryString[0] == '?')
                scanIndex = 1;

            var textLength = queryString.Length;
            var equalIndex = queryString.IndexOf('=');
            if (equalIndex == -1)
                equalIndex = textLength;

            string UnescapeDataString(string s) =>
                string.IsNullOrEmpty(s) ? s : Uri.UnescapeDataString(s.Replace('+', ' '));

            while (scanIndex < textLength)
            {
                var delimiterIndex = queryString.IndexOf('&', scanIndex);
                if (delimiterIndex == -1)
                    delimiterIndex = textLength;

                if (equalIndex < delimiterIndex)
                {
                    while (scanIndex != equalIndex && char.IsWhiteSpace(queryString[scanIndex]))
                        ++scanIndex;

                    var name  = UnescapeDataString(queryString.Substring(scanIndex, equalIndex - scanIndex));
                    var value = UnescapeDataString(queryString.Substring(equalIndex + 1, delimiterIndex - equalIndex - 1));

                    yield return KeyValuePair.Create(name, value);

                    equalIndex = queryString.IndexOf('=', delimiterIndex);
                    if (equalIndex == -1)
                        equalIndex = textLength;
                }
                else
                {
                    if (delimiterIndex > scanIndex)
                    {
                        var name = UnescapeDataString(queryString.Substring(scanIndex, delimiterIndex - scanIndex));
                        yield return KeyValuePair.Create(name, (string)null);
                    }
                }

                scanIndex = delimiterIndex + 1;
            }
        }
    }
}
