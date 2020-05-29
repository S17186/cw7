using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cw4.Middlewares
{
    public class LoggingMiddleware
    {
        
        private readonly RequestDelegate _next;
        

        public LoggingMiddleware(RequestDelegate next) 
        { 
            _next = next; 
        }


        public async Task InvokeAsync(HttpContext httpContext)
        {

            httpContext.Request.EnableBuffering();
            if (httpContext.Request != null)
            {
                string dateTime = DateTime.Now.ToString();
                string pathStr = httpContext.Request.Path;
                string methodStr = httpContext.Request.Method;
                string queryStr = httpContext.Request.QueryString.ToString();
                string bodyStr = "";

                using (var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0; 
                }

                string log = dateTime+ "\t" + pathStr + "\t" + methodStr + "\t" + queryStr + "\t" + bodyStr;
                //zapisz do pliku (domyślnie tworzy nowy plik, jeśli nie istnieje
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(@".\requestsLog.txt", true))
                {
                    await file.WriteLineAsync(bodyStr);
                }
            }

            // wywołaj nast middleware
            if (_next!=null) await _next(httpContext); 
        }
    }
}
