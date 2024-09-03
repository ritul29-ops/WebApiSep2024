

namespace Issues.Api.Vendors;

public class SlugGenerator
{

    public static string GenerateSlugFor(string value)
    {
        var options = new Slugify.SlugHelperConfiguration()
        {
            ForceLowerCase = true,
        };
        var slugger = new Slugify.SlugHelper(options);

        return slugger.GenerateSlug(value);
    }
}
