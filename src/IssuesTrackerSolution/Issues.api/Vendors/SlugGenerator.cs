

namespace Issues.Api.Vendors;

public class SlugGenerator
{
    private static List<string> _history = new(); // since this holds "state" it probably isn't a good candidate for a singleton.
    public static string GenerateSlugFor(string value)
    {
        var options = new Slugify.SlugHelperConfiguration()
        {
            ForceLowerCase = true,
        };
        var slugger = new Slugify.SlugHelper(options);

        var slug = slugger.GenerateSlug(value);
        _history.Add(slug);
        return slug;
    }
}
