namespace EGovPay.Models.Common;

/// <summary>Common query parameters for "list" endpoints across resources.</summary>
public class ListOptions
{
    /// <summary>Max number of objects to return (API default/limit TBD; Stripe's default is 10, max 100).</summary>
    public int? Limit { get; set; }

    /// <summary>Cursor: return objects created after this object id.</summary>
    public string? StartingAfter { get; set; }

    /// <summary>Cursor: return objects created before this object id.</summary>
    public string? EndingBefore { get; set; }

    /// <summary>Converts set properties into query-string key/value pairs.</summary>
    public virtual IEnumerable<KeyValuePair<string, string>> ToQueryParameters()
    {
        if (Limit is { } limit) yield return new("limit", limit.ToString());
        if (StartingAfter is { } sa) yield return new("starting_after", sa);
        if (EndingBefore is { } eb) yield return new("ending_before", eb);
    }
}
