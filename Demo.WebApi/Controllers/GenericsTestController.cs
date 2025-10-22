using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Demo.WebApi.Controllers;

/// <summary>
/// Controller for testing various generic types and dictionary scenarios in API documentation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GenericsTestController : ControllerBase
{
    /// <summary>
    /// Test endpoint returning a KeyValuePair (2 generic parameters)
    /// </summary>
    [HttpGet("kvp")]
    public ActionResult<KeyValuePair<string, int>> GetKeyValuePair()
    {
        return Ok(new KeyValuePair<string, int>("count", 42));
    }

    /// <summary>
    /// Test endpoint returning a list of KeyValuePairs
    /// </summary>
    [HttpGet("kvp-list")]
    public ActionResult<List<KeyValuePair<string, UserDto>>> GetKeyValuePairList()
    {
        var list = new List<KeyValuePair<string, UserDto>>
        {
            new("user1", new UserDto(1, "Alice", "alice@example.com", new[] { "admin" })),
            new("user2", new UserDto(2, "Bob", "bob@example.com", new[] { "user" }))
        };
        return Ok(list);
    }

    /// <summary>
    /// Test endpoint returning a Tuple with 2 type parameters
    /// </summary>
    [HttpGet("tuple2")]
    public ActionResult<Tuple<string, int>> GetTuple2()
    {
        return Ok(Tuple.Create("example", 123));
    }

    /// <summary>
    /// Test endpoint returning a Tuple with 3 type parameters
    /// </summary>
    [HttpGet("tuple3")]
    public ActionResult<Tuple<string, int, DateTime>> GetTuple3()
    {
        return Ok(Tuple.Create("example", 123, DateTime.UtcNow));
    }

    /// <summary>
    /// Test endpoint returning a ValueTuple with multiple parameters
    /// </summary>
    [HttpGet("value-tuple")]
    public ActionResult<(string Name, int Id, DateTime CreatedAt)> GetValueTuple()
    {
        return Ok(("TestUser", 1, DateTime.UtcNow));
    }

    /// <summary>
    /// Test endpoint returning a custom generic type with 2 type parameters
    /// </summary>
    [HttpGet("result")]
    public ActionResult<Result<UserDto, string>> GetResult()
    {
        return Ok(new Result<UserDto, string>(
            Success: true,
            Data: new UserDto(1, "Alice", "alice@example.com", new[] { "admin" }),
            Error: null
        ));
    }

    /// <summary>
    /// Test endpoint returning a custom generic type with 3 type parameters
    /// </summary>
    [HttpGet("triple")]
    public ActionResult<Triple<int, string, UserDto>> GetTriple()
    {
        return Ok(new Triple<int, string, UserDto>(
            First: 1,
            Second: "test",
            Third: new UserDto(1, "Alice", "alice@example.com", new[] { "admin" })
        ));
    }

    /// <summary>
    /// Test endpoint with Dictionary as input parameter
    /// </summary>
    [HttpPost("dict-input")]
    public ActionResult<Dictionary<string, string>> ProcessDictionary([FromBody] Dictionary<string, string> input)
    {
        var result = input.ToDictionary(
            kvp => kvp.Key.ToUpper(),
            kvp => kvp.Value.ToLower()
        );
        return Ok(result);
    }

    /// <summary>
    /// Test endpoint returning nested dictionaries
    /// </summary>
    [HttpGet("nested-dict")]
    public ActionResult<Dictionary<string, Dictionary<string, int>>> GetNestedDictionary()
    {
        var dict = new Dictionary<string, Dictionary<string, int>>
        {
            ["metrics"] = new Dictionary<string, int> { ["count"] = 10, ["total"] = 100 },
            ["stats"] = new Dictionary<string, int> { ["active"] = 5, ["inactive"] = 15 }
        };
        return Ok(dict);
    }

    /// <summary>
    /// Test endpoint returning a dictionary with complex value types
    /// </summary>
    [HttpGet("dict-complex")]
    public ActionResult<Dictionary<int, UserDetailDto>> GetDictionaryWithComplexValues()
    {
        var dict = new Dictionary<int, UserDetailDto>
        {
            [1] = new UserDetailDto(
                Id: 1,
                Name: "Alice",
                Email: "alice@example.com",
                Roles: new[] { "admin" },
                CreatedAt: DateTime.UtcNow.AddDays(-30),
                Profile: new UserProfile
                {
                    Age = 25,
                    Address = new Address { City = "New York", Country = "US", Street = "5th Avenue" },
                    BirthDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-25)),
                    PreferredContactTime = new TimeOnly(9, 0)
                },
                Tags: new[] { "active", "premium" },
                Metadata: new Dictionary<string, string> { ["tier"] = "gold" },
                Extra: new Dictionary<string, UserProfile>()
            )
        };
        return Ok(dict);
    }

    /// <summary>
    /// Test endpoint accepting a generic wrapper with multiple parameters
    /// </summary>
    [HttpPost("paged-result")]
    public ActionResult<PagedResult<UserDto>> GetPagedResult([FromBody] PageRequest request)
    {
        var items = Enumerable.Range(1, request.PageSize).Select(i =>
            new UserDto(
                Id: i + (request.Page - 1) * request.PageSize,
                Name: $"User_{i}",
                Email: $"user{i}@example.com",
                Roles: new[] { i % 2 == 0 ? "admin" : "user" }
            )).ToList();

        return Ok(new PagedResult<UserDto>(
            Items: items,
            TotalCount: 100,
            Page: request.Page,
            PageSize: request.PageSize
        ));
    }

    /// <summary>
    /// Test endpoint with Dictionary containing tuples as values
    /// </summary>
    [HttpGet("dict-tuple")]
    public ActionResult<Dictionary<string, (int Count, string Status)>> GetDictionaryWithTuples()
    {
        var dict = new Dictionary<string, (int Count, string Status)>
        {
            ["pending"] = (5, "Processing"),
            ["completed"] = (20, "Done"),
            ["failed"] = (2, "Error")
        };
        return Ok(dict);
    }

    /// <summary>
    /// Test endpoint returning a dictionary with enum keys
    /// </summary>
    [HttpGet("dict-enum-key")]
    public ActionResult<Dictionary<RoleKind, List<UserDto>>> GetDictionaryWithEnumKeys()
    {
        var dict = new Dictionary<RoleKind, List<UserDto>>
        {
            [RoleKind.Admin] = new List<UserDto>
            {
                new UserDto(1, "Admin1", "admin1@example.com", new[] { "admin" })
            },
            [RoleKind.User] = new List<UserDto>
            {
                new UserDto(2, "User1", "user1@example.com", new[] { "user" }),
                new UserDto(3, "User2", "user2@example.com", new[] { "user" })
            }
        };
        return Ok(dict);
    }
}

/// <summary>
/// Generic result type with 2 type parameters for success/error scenarios
/// </summary>
public record Result<TData, TError>(bool Success, TData? Data, TError? Error);

/// <summary>
/// Generic type with 3 type parameters for testing
/// </summary>
public record Triple<T1, T2, T3>(T1 First, T2 Second, T3 Third);

/// <summary>
/// Paged result wrapper with generic item type
/// </summary>
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Page request parameters
/// </summary>
public record PageRequest([Range(1, int.MaxValue)] int Page = 1, [Range(1, 100)] int PageSize = 10);
