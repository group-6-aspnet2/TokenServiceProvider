using Microsoft.AspNetCore.Http;
using System.Text;

namespace Tests.Helpers;

internal static class TestHelpers
{
    public static HttpRequest CreateHttpRequest(string body)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        request.Body = stream;
        request.ContentLength = stream.Length;
        request.ContentType = "application/json";
        return request;
    }
}
