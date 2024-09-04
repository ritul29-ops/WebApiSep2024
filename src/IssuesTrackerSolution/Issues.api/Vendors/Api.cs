using FluentValidation;
using Marten;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;


namespace Issues.Api.Vendors;
[Authorize] // don't accept any requests without a valid Authorization header
public class Api(VendorData vendor) : ControllerBase
{

    [HttpGet("/vendors")]
    public async Task<ActionResult<Dictionary<string, VendorInformationResponse>>> GetVendorsAsync(CancellationToken token)
    {


        // var vendorLookup = new VendorData(); // the new keyword cannot be used on ANYTHING that is touching a backing service.
        var vendors = await vendor.GetVendorInformationAsync(token);
        return Ok(vendors);
    }

    [HttpPost("/vendors")] // nobody should be able to do this unless they meet the security policy
    [Authorize(Policy = "IsSoftwareCenterAdmin")]
    public async Task<ActionResult> AddVendorAsync(
        [FromBody] VendorCreateRequest request,
        [FromServices] IValidator<VendorCreateRequest> validator)
    {

        var validations = await validator.ValidateAsync(request);


        if (!validations.IsValid)
        {
            return BadRequest(validations.ToDictionary());
        }

        VendorInformationResponse response = await vendor.AddVendorAsync(request);

        return Ok(response);
    }
}


public record VendorCreateRequest
{

    public string Name { get; set; } = string.Empty;
}

public class VendorCreateRequestValidator : AbstractValidator<VendorCreateRequest>
{
    public VendorCreateRequestValidator(IDocumentSession session)
    {
        RuleFor(v => v.Name).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(v => v.Name).MustAsync(async (name, cancellation) =>
        {
            var slug = SlugGenerator.GenerateSlugFor(name);
            var exists = await session.Query<VendorItemEntity>().AnyAsync(v => v.Slug == slug, cancellation);
            return !exists;
        }).WithMessage("That Vendor Already Exists");
    }
}
public record VendorInformationResponse // "Write Model" (stuff I'm sending to client)
{

    [Required]
    public string Id { get; set; } = string.Empty;
    [Required]
    public string Name { get; set; } = string.Empty;
};
