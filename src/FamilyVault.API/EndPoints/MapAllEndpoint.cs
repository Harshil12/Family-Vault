using FamilyVault.API.EndPoints.Document;
using FamilyVault.API.EndPoints.Family;
using FamilyVault.API.EndPoints.FamilyMeMber;
using FamilyVault.API.EndPoints.User;

namespace FamilyVault.API.EndPoints;

public static class MapAllEndpoint
{
    public static void MapAllEndpoints(this WebApplication app)
    {
        app.MapDocumentEndPoints();
        app.MapFamilyEndPoints();
        app.MapFamilyMemberEndPoints();
        app.MapUserEndPoints();
    }
}
