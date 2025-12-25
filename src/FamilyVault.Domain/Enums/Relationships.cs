using System;
using System.Collections.Generic;
using System.Text;

namespace FamilyVault.Domain.Enums
{
    /// <summary>
    /// Common family relationship types used across the domain.
    /// </summary>
    public enum Relationships
    {
        /// <summary>
        /// The entity refers to the same person.
        /// </summary>
        Self = 0,

        /// <summary>
        /// Spouse or legally married partner.
        /// </summary>
        Spouse = 1,

        /// <summary>
        /// Parent (mother or father).
        /// </summary>
        Parent = 2,

        /// <summary>
        /// Child (son or daughter).
        /// </summary>
        Child = 3,

        /// <summary>
        /// Sibling (brother or sister).
        /// </summary>
        Sibling = 4,

        /// <summary>
        /// Domestic partner, significant other (non-married).
        /// </summary>
        Partner = 10,

        /// <summary>
        /// Any other relationship not covered above.
        /// </summary>
        Other = 99
    }
}
