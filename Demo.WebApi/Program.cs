using AspNetCore.OpenApi.Xml.Extensions;
using AspNetCore.OpenApi.Xml.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// 注册自定义 XML 文档生成器
builder.Services.AddApiDocument();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

// Enable static files for Razor Pages
app.UseStaticFiles();

// 暴露 XML 文档端点
app.MapGet("/__api-doc.xml", (IApiXmlDocumentGenerator gen) =>
{
    var xml = gen.GenerateXml("Demo API", "v1");
    return Results.Text(xml, "application/xml");
});

// 映射 API 文档页面
app.MapApiDocument();

// 示例根端点
app.MapGet("/", () => "Demo Web API for XML Doc - Visit /api-doc for documentation page");

app.MapControllers();

app.Run();
