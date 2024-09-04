using Issues.Api.Catalog;
using JasperFx.Core;
using Marten;
using System.Security.Claims;

namespace Issues.Api.Vendors;

public class VendorData(TimeProvider timeProvider, IDocumentSession session, IHttpContextAccessor contextAccessor) : ILookupVendors
{

    public async Task<Dictionary<string, VendorInformationResponse>> GetVendorInformationAsync(CancellationToken token)
    {

        var data = await session.Query<VendorItemEntity>()
            .Select(item => new VendorInformationResponse { Id = item.Slug, Name = item.Name })
            .ToListAsync(token); // the whole entity


        var response = new Dictionary<string, VendorInformationResponse>();
        foreach (var item in data)
        {
            response.Add(item.Id, item);
        }
        return response;
    }



    public async Task<VendorInformationResponse> AddVendorAsync(VendorCreateRequest request)
    {
        var sub = contextAccessor?.HttpContext?.User.Claims.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value ?? throw new Exception("Can only be used in authenticated sessions");
        var entity = new VendorItemEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Added = timeProvider.GetUtcNow(),
            AddedBy = sub,
            Slug = SlugGenerator.GenerateSlugFor(request.Name),
        };
        // Save it to the database.
        session.Store(entity);
        await session.SaveChangesAsync();
        // Todo: Figure out what we need / want to return to them.
        var response = new VendorInformationResponse { Id = entity.Slug, Name = entity.Name };
        return response;
    }

    async Task<bool> ILookupVendors.IsCurrentVendorAsync(string vendor)
    {
        return await session.Query<VendorItemEntity>().AnyAsync(v => v.Slug == vendor);
    }
}