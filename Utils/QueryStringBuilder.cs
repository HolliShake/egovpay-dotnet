using System.Text;

namespace EGovPay.Utils;

internal static class QueryStringBuilder
{
    public static string Build(string path, IEnumerable<KeyValuePair<string, string>>? parameters)
    {
        if (parameters is null) return path;

        var list = parameters.ToList();
        if (list.Count == 0) return path;

        var sb = new StringBuilder(path);
        sb.Append('?');
        for (var i = 0; i < list.Count; i++)
        {
            if (i > 0) sb.Append('&');
            sb.Append(Uri.EscapeDataString(list[i].Key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(list[i].Value));
        }
        return sb.ToString();
    }
}