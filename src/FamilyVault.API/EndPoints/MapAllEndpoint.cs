using FamilyVault.API.EndPoints.Audit;
using FamilyVault.API.EndPoints.Document;
using FamilyVault.API.EndPoints.FinancialDetails;
using FamilyVault.API.EndPoints.Family;
using FamilyVault.API.EndPoints.FamilyMeMber;
using FamilyVault.API.EndPoints.Login;
using FamilyVault.API.EndPoints.User;

namespace FamilyVault.API.EndPoints;

/// <summary>
/// Represents MapAllEndpoint.
/// </summary>
public static class MapAllEndpoint
{
    /// <summary>
    /// Performs the MapAllEndpoints operation.
    /// </summary>
    public static void MapAllEndpoints(this WebApplication app)
    {
        app.MapFinancialDetailsEndPoints();
        app.MapAuditEndPoints();
        app.MapDocumentEndPoints();
        app.MapFamilyEndPoints();
        app.MapFamilyMemberEndPoints();
        app.MapUserEndPoints();
        app.MapLoginPoints();
    }
}
