/*
 *   Copyright 2022-2025 Kate Ward <kate@dariox.club>
 *
 *   Licensed under the Apache License, Version 2.0 (the "License");
 *   you may not use this file except in compliance with the License.
 *   You may obtain a copy of the License at
 *
 *       http://www.apache.org/licenses/LICENSE-2.0
 *
 *   Unless required by applicable law or agreed to in writing, software
 *   distributed under the License is distributed on an "AS IS" BASIS,
 *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *   See the License for the specific language governing permissions and
 *   limitations under the License.
 */

using System.Collections.Generic;

namespace AOCNotify;

public static class Extensions
{
    public static IEnumerable<IEnumerable<string>> ChunkByLength(this IEnumerable<string> lines, int maxLength = 2000)
    {
        var submessage = new List<List<string>>();
        var working = new List<string>();
        var workingLength = 0;
        foreach (var thing in lines)
        {
            if (workingLength + thing.Length + 1 > maxLength)
            {
                submessage.Add(working);
                working = [];
                workingLength = 0;
            }
            working.Add(thing);
            workingLength += thing.Length + 1;
        }
        if (working.Count != 0) submessage.Add(working);
        return submessage;
    }
}
