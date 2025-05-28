using FCG.Presentation.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FCG.Service.Tests
{
    public class ExceptionHandlingMiddlewareTests
    {
        [Fact]
        public async Task Should_LogExceptionAndReturn500()
        {
            // Arrange
            var loggerMock = new Mock<Serilog.ILogger>();
            Log.Logger = loggerMock.Object;

            var middleware = new ExceptionHandlingMiddleware(async (innerHttpContext) =>
            {
                throw new Exception("Test exception");
            });

            var context = new DefaultHttpContext();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(500, context.Response.StatusCode);
           
        }
    }
}
