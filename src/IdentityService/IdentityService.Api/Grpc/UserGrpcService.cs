using System;
using System.Threading.Tasks;
using Grpc.Core;
using IdentityService.Application.Interfaces; // جایی که IUserRepository هست
using IdentityService.Grpc;

namespace IdentityService.Api.Grpc;

public class UserGrpcService : UserService.UserServiceBase
{
    private readonly IUserRepository _userRepository;

    public UserGrpcService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public override async Task<UserReply> GetUserById(
        GetUserByIdRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user id"));
        }

        // این متدها را بر اساس ریپازیتوری خودت تنظیم کن
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
        }

        var roles = await _userRepository.GetUserRolesAsync(userId, context.CancellationToken);
        // اگر چنین متدی نداری، می‌تونی مشابه جایی که برای JWT رول‌ها را می‌خواندی، یک متد اضافه کنی

        var reply = new UserReply
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            DisplayName = user.DisplayName ?? "",
            AvatarUrl = user.AvatarUrl ?? ""
        };
        reply.Roles.AddRange(roles);

        return reply;
    }
}