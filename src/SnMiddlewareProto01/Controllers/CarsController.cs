using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SenseNet.Client;
using Newtonsoft.Json.Linq;

namespace SnMiddlewareProto01.Controllers;

[Route("[controller]")]
[ApiController]
public class CarsController : ControllerBase
{
    private readonly IRepositoryCollection _repositoryCollection;

    public CarsController(IRepositoryCollection repositoryCollection)
    {
        _repositoryCollection = repositoryCollection;
    }

    // GET: <CarsController>
    [HttpGet]
    public async Task<ContentResult> Get()
    {
        var repository = await _repositoryCollection.GetRepositoryAsync(HttpContext.RequestAborted);

        var contents = await repository.LoadCollectionAsync(new()
        {
            Path = "/Root/Content/Cars",
            Expand = new[] {"Actions"},
            Select = new[] {"Id", "Type", "Path", "Name", "Actions"},
            Parameters = { new KeyValuePair<string, string>("scenario", "ContextMenu") }
        }, HttpContext.RequestAborted);

        return new ContentResult
        {
            Content = JsonConvert.SerializeObject(contents.Select(c => new
            {
                c.Id,
                c.Type,
                c.Path,
                c.Name,
                Actions = GetActionNames((JArray)c["Actions"])
            })),
            ContentType = "application/json",
            StatusCode = 200
        };
    }
    private string[] GetActionNames(JArray jArray)
    {
        return jArray.Select(item => item["Name"]?.ToString()).Where(x => x != null).OfType<string>().ToArray();
    }

    // GET <CarsController>/ExecuteAction/contentId
    [HttpPost("ExecuteAction/{contentId}")]
    public async Task<ContentResult> ExecuteAction(int contentId, [FromBody] string action)
    {
        var repository = await _repositoryCollection.GetRepositoryAsync(HttpContext.RequestAborted);

        var content = await repository.LoadContentAsync(contentId, HttpContext.RequestAborted);


        string operationResult = null;
        try
        {
            operationResult = await repository.InvokeActionAsync<string>(new OperationRequest
            {
                ContentId = contentId,
                OperationName = action
            }, HttpContext.RequestAborted);
        }
        catch (Exception e)
        {
            operationResult = e.Message;
        }

        return new ContentResult
        {
            Content = JsonConvert.SerializeObject(new
            {
                ContentId = contentId,
                Path = content?.Path,
                Type = content?.Type,
                Action = action,
                ActionResult = operationResult
            }),
            ContentType = "application/json",
            StatusCode = 200
        };
    }




    /*
    // GET <CarsController>/5
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST <CarsController>
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT <CarsController>/5
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE <CarsController>/5
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
    */
}