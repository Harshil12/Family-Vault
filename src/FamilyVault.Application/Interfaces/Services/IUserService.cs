using FamilyVault.Application.DTOs.User;

namespace FamilyVault.Application.Interfaces.Services;

/// <summary>
/// Defines operations for managing application users.
/// Implementations handle creation, retrieval, update and deletion of users.
/// </summary>
public interface IUserService
{

    /// <summary>
    /// Retrieves a user by its unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to retrieve.</param>
    /// <returns>
    /// A <see cref="Task{UserDto}"/> that completes with the <see cref="UserDto"/> representing the requested user.
    /// If the user is not found, the returned task may complete with <c>null</c> or an implementation-specific behavior.
    /// </returns>
    public Task<UserDto> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that completes with a read-only list of <see cref="UserDto"/> containing all users.
    /// </returns>
    public Task<IReadOnlyList<UserDto>> GetUserAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing user using the provided create-style request.
    /// Note: the request type is named <see cref="updateUserRequest"/>; callers should provide the appropriate data expected by implementations.
    /// </summary>
    /// <param name="updateUserRequest">The request containing user data to update.</param>
    /// <returns>
    /// A <see cref="Task{UserDto}"/> that completes with the updated <see cref="UserDto"/>.
    /// </returns>
    public Task<UserDto> UpdateUserAsync(UpdateUserRequest updateUserRequest, Guid createdByUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new user using the provided update-style request.
    /// Note: the request type is named <see cref="createUserRequest"/>; callers should provide the appropriate data expected by implementations.
    /// </summary>
    /// <param name="createUserRequest">The request containing data for the new user.</param>
    /// <returns>
    /// A <see cref="Task{UserDto}"/> that completes with the newly created <see cref="UserDto"/>.
    /// </returns>
    public Task<UserDto> CreateUserAsync(CreateUserRequest createUserRequest, Guid createdByUserId, CancellationToken cancellationToken);

    /// <summary>
    /// Registers a new user without requiring an authenticated caller.
    /// </summary>
    /// <param name="createUserRequest">The request containing data for the new user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task{UserDto}"/> with the created user.</returns>
    public Task<UserDto> RegisterUserAsync(CreateUserRequest createUserRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the user with the specified unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user to delete.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous delete operation.</returns>
    public Task DeleteUserByIdAsync(Guid userId, Guid createdByUserId, CancellationToken cancellationToken);
           
}
