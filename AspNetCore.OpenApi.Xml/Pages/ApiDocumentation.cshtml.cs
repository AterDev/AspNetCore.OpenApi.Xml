using AspNetCore.OpenApi.Xml.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenApi.Xml.Core.Models;

namespace AspNetCore.OpenApi.Xml.Pages;

public class ApiDocumentationModel : PageModel
{
    private readonly IApiXmlDocumentGenerator _generator;

    [BindProperty(SupportsGet = true)]
    public string? Title { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Version { get; set; }

    public ApiDocument Document { get; private set; } = null!;

    public ApiDocumentationModel(IApiXmlDocumentGenerator generator)
    {
        _generator = generator;
    }

    public void OnGet()
    {
        Document = _generator.Generate(
            Title ?? "API Documentation",
            Version ?? "1.0"
        );
    }
}
