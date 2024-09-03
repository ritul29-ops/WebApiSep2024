using Issues.Api.Catalog;
using Marten;

namespace Issues.Api.Vendors;

public class VendorData(TimeProvider timeProvider, IDocumentSession session) : ILookupVendors
{

    public async Task<Dictionary<string, VendorInformationResponse>> GetVendorInformationAsync(CancellationToken token)
    {
        // Todo - make this more real.
        var data = await session.Query<VendorItemEntity>().ToListAsync(token);
        // this is crappy code intentional, we will fix this tomorrow with Mapperly. 
        var response = new Dictionary<string, VendorInformationResponse>();
        foreach (var item in data)
        {
            response.Add(item.Slug, new VendorInformationResponse(item.Slug, item.Name));
        }
        return response;
    }



    public async Task<VendorInformationResponse> AddVendorAsync(VendorCreateRequest request)
    {
        // don't allow duplicates -
        // TODO tomorrow: Prevent multiples. generate the slug, see if we already have it, and if we do, throw an Exception
        // Make this an "idempotent" operation.
        var entity = new VendorItemEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Added = timeProvider.GetUtcNow(),
            AddedBy = "sub of person additing it", // Tomorrow
            Slug = SlugGenerator.GenerateSlugFor(request.Name),
        };
        // Save it to the database.
        session.Store(entity);
        await session.SaveChangesAsync();
        // Todo: Figure out what we need / want to return to them.
        var response = new VendorInformationResponse(entity.Slug, entity.Name);
        return response;
    }

    async Task<bool> ILookupVendors.IsCurrentVendorAsync(string vendor)
    {
        return await session.Query<VendorItemEntity>().AnyAsync(v => v.Slug == vendor);
    }
}