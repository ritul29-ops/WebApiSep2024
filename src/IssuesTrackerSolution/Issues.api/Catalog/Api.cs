namespace Issues.Api.Catalog;

public class Api(ILookupVendors vendorLookup) : ControllerBase
{

    [HttpPost("/catalog/{vendor}/software")]
    public async Task<ActionResult> AddSoftwareToCatalogAsync([FromBody] CreateSoftwareCatalogItemRequest request, string vendor)
    {
        if (string.IsNullOrEmpty(request.Name)) // and description
        {
            return BadRequest();
        }

        if (await vendorLookup.IsCurrentVendorAsync(vendor) == false)
        {
            return NotFound();
        }
        return Ok(request);
    }
}

public interface ILookupVendors
{
    Task<bool> IsCurrentVendorAsync(string vendor);
}

public record CreateSoftwareCatalogItemRequest
{
    public string Name { get; set; }
    public string Description { get; set; }

}