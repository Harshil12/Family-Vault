using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyVault.Domain.Enums
{
    /// <summary>
    /// Common document types used across the domain.
    /// </summary>
    public enum DocumentTypes
    {
        /// <summary>
        /// Unspecified or unknown document type.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Passport document.
        /// </summary>
        Passport = 1,

        /// <summary>
        /// Driver's license or driving permit.
        /// </summary>
        DriverLicense = 2,

        /// <summary>
        /// Birth certificate.
        /// </summary>
        BirthCertificate = 3,

        /// <summary>
        /// Marriage certificate.
        /// </summary>
        MarriageCertificate = 4,

        /// <summary>
        /// Death certificate.
        /// </summary>
        DeathCertificate = 5,

        ///// <summary>
        ///// National identity card.
        ///// </summary>
        //NationalId = 6,

        ///// <summary>
        ///// Last will and testament.
        ///// </summary>
        //Will = 10,

        ///// <summary>
        ///// Property deed.
        ///// </summary>
        //Deed = 11,

        /// <summary>
        /// Insurance policy documents.
        /// </summary>
        InsurancePolicy = 12,

        ///// <summary>
        ///// Medical records.
        ///// </summary>
        //MedicalRecord = 13,

        ///// <summary>
        ///// Tax return or tax documents.
        ///// </summary>
        //TaxReturn = 14,

        ///// <summary>
        ///// Financial statements (bank statements, investment records).
        ///// </summary>
        //FinancialStatement = 15,

        /// <summary>
        /// Financial statements (bank statements, investment records).
        /// </summary>
        PAN = 15,

        /// <summary>
        /// Financial statements (bank statements, investment records).
        /// </summary>
        Aadhar= 15,

        /// <summary>
        /// Any other document type not covered above.
        /// </summary>
        Other = 99
    }
}
