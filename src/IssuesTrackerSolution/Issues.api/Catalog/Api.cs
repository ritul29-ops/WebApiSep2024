using FluentValidation;
using Issues.Api.Vendors;
using Marten;
using Riok.Mapperly.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Issues.Api.Catalog;

public class Api(ILookupVendors vendorLookup, TimeProvider timeProvider, IDocumentSession session) : ControllerBase
{
    // Todo: Only members of SoftwareCenter role should be able to do this.
    [HttpPost("/vendors/{vendor}/software")]
    public async Task<ActionResult<SoftwareCatalogItemResponse>> AddSoftwareToCatalogAsync(
        [FromBody] CreateSoftwareCatalogItemRequest request,
        [FromRoute] string vendor,
        [FromServices] IValidator<CreateSoftwareCatalogItemRequest> validator,
        CancellationToken token
        )
    {

        var validations = await validator.ValidateAsync(request, token);

        if (!validations.IsValid)
        {
            return BadRequest(validations.ToDictionary());
        }
        // Todo: This could be a filter.
        if (await vendorLookup.IsCurrentVendorAsync(vendor) == false)
        {
            return NotFound();
        }

        // create an entity and save it to the database
        var entity = request.MapToEntity(vendor, timeProvider.GetUtcNow());
        // save it to the database
        session.Store(entity);
        await session.SaveChangesAsync(token);



        var response = entity.MapToResponse();
        // we need to return something back again, but not the entity or the request.
        return Ok(response);
    }
}

public interface ILookupVendors
{
    Task<bool> IsCurrentVendorAsync(string vendor);
}

public record CreateSoftwareCatalogItemRequest
{
    // at least five characters and no more than 100. Required
    public string Name { get; set; } = string.Empty;
    // at least 10 characters and now more than 1024. Required.
    public string Description { get; set; } = string.Empty;

}

public record SoftwareCatalogItemResponse
{
    // Todo: Maybe add validation attributes to show what we promise will be there.
    public string Id { get; set; } = string.Empty; // slug
    [Required]
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTimeOffset Added { get; set; }

}

public class CreateSoftwareCatalogItemRequestValidator : AbstractValidator<CreateSoftwareCatalogItemRequest>
{
    public CreateSoftwareCatalogItemRequestValidator(IDocumentSession session)
    {
        RuleFor(c => c.Name).NotEmpty().MinimumLength(5).MaximumLength(100);
        RuleFor(c => c.Description).NotEmpty().MinimumLength(10).MaximumLength(1024);
        RuleFor(v => v.Name).MustAsync(async (name, cancellation) =>
        {
            var slug = SlugGenerator.GenerateSlugFor(name);
            var exists = await session.Query<CatalogItemEntity>().AnyAsync(v => v.Slug == slug, cancellation);
            return !exists;
        }).WithMessage("That Software Item Already Exists");
    }
}

public static class CatalogMappingExtensions
{
    public static CatalogItemEntity MapToEntity(this CreateSoftwareCatalogItemRequest request, string vendor, DateTimeOffset createdTime)
    {
        return new CatalogItemEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            AddedBy = "sub of the person that added this",
            DateAdded = createdTime,
            Slug = SlugGenerator.GenerateSlugFor(request.Name),
            Vendor = vendor,
        };
    }

    //public static SoftwareCatalogItemResponse MapToResponse(this CatalogItemEntity entity)
    //{
    //    return new SoftwareCatalogItemResponse
    //    {
    //        Id = entity.Slug,
    //        Name = entity.Name,
    //        Description = entity.Description,
    //        Added = entity.DateAdded
    //    };
    //}
}

[Mapper]
public static partial class CatalogMappers
{


    [MapPropertyFromSource(nameof(SoftwareCatalogItemResponse.Id), Use = nameof(ConvertIdToSlug))]
    public static partial SoftwareCatalogItemResponse MapToResponse(this CatalogItemEntity entity);

    private static string ConvertIdToSlug(CatalogItemEntity entity) => entity.Slug;
}