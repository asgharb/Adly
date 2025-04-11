using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Adly.Application.Extensions;

public static class ApplicationIdentityExtensions
{
    public static List<KeyValuePair<string, string>> ConvertToKeyValuePair([NotNull] this IEnumerable<IdentityError> errors)
    {
        return errors.Select(c => new KeyValuePair<string, string>("GeneralError", c.Description)).ToList();
    }
}