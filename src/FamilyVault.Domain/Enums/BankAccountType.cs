namespace FamilyVault.Domain.Enums
{

    /// <summary>
    /// Represents bank account types used in the domain.
    /// </summary>
    public enum BankAccountType : byte
    {
        /// <summary>
        /// Unspecified or unknown account type.
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Savings account.
        /// </summary>
        Savings = 1,
        /// <summary>
        /// Current (checking) account.
        /// </summary>
        Current = 2,
        /// <summary>
        /// Salary account.
        /// </summary>
        Salary = 3,
        /// <summary>
        /// Fixed deposit account.
        /// </summary>
        FixedDeposit = 4,
        /// <summary>
        /// Recurring deposit account.
        /// </summary>
        RecurringDeposit = 5
    }

}
