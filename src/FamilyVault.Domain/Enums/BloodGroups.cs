using System.ComponentModel.DataAnnotations;

namespace FamilyVault.Domain.Enums;

public enum BloodGroups : byte
{
    [Display(Name = "A+")] A_Positive,
    [Display(Name = "A-")] A_Negative,
    [Display(Name = "B+")] B_Positive,
    [Display(Name = "B-")] B_Negative,
    [Display(Name = "AB+")] AB_Positive,
    [Display(Name = "AB-")] AB_Negative,
    [Display(Name = "O+")] O_Positive,
    [Display(Name = "O-")] O_Negative
}
