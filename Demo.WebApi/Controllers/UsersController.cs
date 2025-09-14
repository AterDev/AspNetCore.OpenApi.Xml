using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Demo.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var data = Enumerable.Range(1, pageSize).Select(i =>
            new UserDto(
                Id: i + (page - 1) * pageSize,
                Name: $"User_{i}",
                Email: $"user{i}@example.com",
                Roles: new[] { i % 2 == 0 ? "admin" : "user" }
            ));
        return Ok(data);
    }

    // 字典返回测试: key => 用户标识, value => 详细信息
    [HttpGet("dict")]
    public ActionResult<Dictionary<string, UserDetailDto>> GetUsersDictionary()
    {
        var dict = Enumerable.Range(1, 3).ToDictionary(
            i => $"user-{i}",
            i => new UserDetailDto(
                Id: i,
                Name: $"User_{i}",
                Email: $"user{i}@example.com",
                Roles: new[] { i % 2 == 0 ? "admin" : "user" },
                CreatedAt: DateTime.UtcNow.AddDays(-i),
                Profile: new UserProfile
                {
                    Age = 20 + i,
                    Address = new Address { City = "Shanghai", Country = "CN", Street = "Nanjing Rd" },
                    BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20 - i)),
                    PreferredContactTime = new TimeOnly(9 + i, 30)
                },
                Tags: new[] { "active", i % 2 == 0 ? "vip" : "trial" },
                Metadata: new Dictionary<string, string> { ["region"] = "asia", ["seq"] = i.ToString() },
                Extra: new Dictionary<string, UserProfile>
                {
                    ["p"] = new UserProfile { Age = 30 + i, Address = new Address { City = "Beijing", Country = "CN", Street = "Changan Ave" }, BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)), PreferredContactTime = new TimeOnly(14,0) }
                }
            ));
        return Ok(dict);
    }

    // 枚举测试：返回所有角色枚举
    [HttpGet("roles")]
    public ActionResult<IEnumerable<string>> GetRoleKinds() => Ok(Enum.GetNames<RoleKind>());

    [HttpGet("{id:int}")]
    public ActionResult<UserDetailDto> GetUser(int id)
    {
        if (id <= 0) return NotFound();
        return Ok(new UserDetailDto(
            Id: id,
            Name: $"User_{id}",
            Email: $"user{id}@example.com",
            Roles: new[] { "user" },
            CreatedAt: DateTime.UtcNow.AddDays(-id),
            Profile: new UserProfile
            {
                Age = 20 + (id % 10),
                Address = new Address { City = "Shanghai", Country = "CN", Street = "Nanjing Rd" },
                BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                PreferredContactTime = new TimeOnly(10, 0)
            },
            Tags: new[] { "active", "beta" },
            Metadata: new Dictionary<string, string> { ["region"] = "asia", ["tier"] = "gold" },
            Extra: new Dictionary<string, UserProfile>()
        ));
    }

    [HttpPost]
    public ActionResult<UserDetailDto> Create([FromBody] CreateUserRequest request)
    {
        var created = new UserDetailDto(
            Id: Random.Shared.Next(1000, 9999),
            Name: request.Name,
            Email: request.Email,
            Roles: request.Roles?.ToArray() ?? Array.Empty<string>(),
            CreatedAt: DateTime.UtcNow,
            Profile: new UserProfile
            {
                Age = request.Age ?? 0,
                Address = request.Address,
                BirthDate = request.BirthDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-20)),
                PreferredContactTime = request.PreferredContactTime ?? new TimeOnly(9, 0)
            },
            Tags: request.Tags?.ToArray() ?? Array.Empty<string>(),
            Metadata: request.Metadata ?? new Dictionary<string, string>(),
            Extra: request.ExtraProfiles ?? new Dictionary<string, UserProfile>()
        );
        return CreatedAtAction(nameof(GetUser), new { id = created.Id }, created);
    }
}

public enum RoleKind
{
    [Description("Normal application user")] User = 0,
    [Description("Administrator with elevated permissions")] Admin = 1,
    [Description("Read-only auditor")] Auditor = 2
}

public record CreateUserRequest(
    [property: Required, StringLength(50, MinimumLength = 3), RegularExpression("^[A-Za-z0-9_]+$")] string Name,
    [property: Required, EmailAddress] string Email,
    [property: Range(0,120)] int? Age,
    Address? Address,
    IEnumerable<string>? Tags,
    IEnumerable<string>? Roles,
    Dictionary<string, string>? Metadata,
    DateOnly? BirthDate,
    TimeOnly? PreferredContactTime,
    Dictionary<string, UserProfile>? ExtraProfiles);

public record UserDto(int Id, string Name, string Email, string[] Roles);

public record UserDetailDto(
    int Id,
    string Name,
    string Email,
    string[] Roles,
    DateTime CreatedAt,
    UserProfile? Profile,
    string[] Tags,
    Dictionary<string, string>? Metadata,
    Dictionary<string, UserProfile> Extra) : UserDto(Id, Name, Email, Roles);

public class UserProfile
{
    [Range(0,150)] public int Age { get; set; }
    public Address? Address { get; set; }
    public DateOnly BirthDate { get; set; }
    public TimeOnly PreferredContactTime { get; set; }
}

public class Address
{
    [StringLength(100)] public string Street { get; set; } = string.Empty;
    [StringLength(60)] public string City { get; set; } = string.Empty;
    [StringLength(2, MinimumLength = 2), RegularExpression("^[A-Z]{2}$")] public string Country { get; set; } = string.Empty;
}
