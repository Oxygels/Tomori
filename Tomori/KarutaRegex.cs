using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tomori;

internal static partial class KarutaRegex
{

    [GeneratedRegex(@"`\d+`. `♡(\d+)` · (.+) · \*\*(.+)\*\*")]
    public static partial Regex MultiCharRegex();

    [GeneratedRegex(@"Character · \*\*(.+)\*\*\nSeries · \*\*(.+)\*\*\nWishlisted · \*\*(\d+)\*\*")]
    public static partial Regex SingleCharRegex();
}
