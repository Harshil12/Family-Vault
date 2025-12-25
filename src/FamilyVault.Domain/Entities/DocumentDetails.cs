using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyVault.Domain.Entities
{
    public class DocumentDetails :BaseEntity
    {
        /// <summary>
        /// Represents the type of the document (e.g., Passport, ID Card, License).
        /// </summary>
        public string DocumentType { get; set; } = null!;

        /// <summary>
        /// Unique identifier or number assigned to the document.
        /// </summary>
        public string DocumentNumber { get; set; } = null!;

        /// <summary>
        /// The physical or digital location where the document is stored.
        /// </summary>
        public string SavedLocation { get; set; } = null!;

        /// <summary>
        /// The date when the document was issued.
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// The date when the document will expire.
        /// </summary>
        public DateTime ExpiryDate { get; set; }

        /// <summary>
        /// Identifier linking the document to a specific family member.
        /// </summary>
        public Guid FamilyMemberId { get; set; }

        /// <summary>
        /// Navigation property representing the family member associated with the document.
        /// </summary>
        public FamilyMember FamilyMember { get; set; } = null!;
    }
}
