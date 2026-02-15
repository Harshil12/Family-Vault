namespace FamilyVault.Domain.Enums;

public enum PolicyStatus : byte
{
    Unknown = 0,
    Active = 1,
    Matured = 2,
    Lapsed = 3,
    Closed = 4
}
