using StoreManagement.Application.DTOs;
using StoreManagement.Domain.Enums;
namespace StoreManagement.Application.Interfaces;
public interface IUserAdminService
{
    Task<IReadOnlyCollection<UserListResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserInfoResponse?> UpdateAsync(long id, UpdateUserRequest request, CancellationToken cancellationToken);
}
