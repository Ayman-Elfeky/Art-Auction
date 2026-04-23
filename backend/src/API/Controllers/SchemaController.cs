using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/schema")]
[AllowAnonymous]
public sealed class SchemaController : ControllerBase
{
    [HttpGet]
    public IActionResult GetSchemaHtml()
    {
        var schemaPath = Path.GetFullPath(
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "docs", "db-schema.html"));

        if (!System.IO.File.Exists(schemaPath))
        {
            return NotFound(new { error = "Schema document not found." });
        }

        var html = System.IO.File.ReadAllText(schemaPath);
        return Content(html, "text/html");
    }
}
