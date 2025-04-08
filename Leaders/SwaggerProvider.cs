using Microsoft.OData.Edm;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.OData;
using Swashbuckle.AspNetCore.Swagger;
using System;
using ODataDemo.Controllers;
using System.Reflection;
using Microsoft.AspNetCore.Http.Extensions;

namespace ODataDemo.Leaders
{
    public class SwaggerProvider : ISwaggerProvider
    {
        public SwaggerProvider(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        private static Lazy<IEdmModel> odataModel = new Lazy<IEdmModel>(() => ODataModel.CreateV1Model());
        private readonly IHttpContextAccessor httpContextAccessor;

        public OpenApiDocument GetSwagger(string documentName, string host = null, string basePath = null)
        {
            Uri uri = new Uri(string.IsNullOrEmpty(host) ? this.httpContextAccessor?.HttpContext?.Request.GetDisplayUrl() ?? "http://localhost" : host);
            var baseUri = new Uri(uri.AbsoluteUri.Replace(uri.AbsolutePath, string.Empty));
            OpenApiConvertSettings settings = new OpenApiConvertSettings
            {
                VerifyEdmModel = false,
                AddAlternateKeyPaths = true,
                AddEnumDescriptionExtension = true,
                AddSingleQuotesForStringParameters = true,
                AppendBoundOperationsOnDerivedTypeCastSegments = true,
                EnableCount = true,
                EnableDollarCountPath = false,
                EnableDerivedTypesReferencesForRequestBody = true,
                EnableKeyAsSegment = true,
                EnableOperationId = true,
                EnablePagination = true,
                IEEE754Compatible = true,
                PathPrefix = "v1",
                EnableNavigationPropertyPath = false,
                EnableDiscriminatorValue = true,
                TopExample = 3,
                EnableDeprecationInformation= true,
                ErrorResponsesAsDefault= true,
                ShowLinks = true,
                ServiceRoot = baseUri

            };
            OpenApiDocument document = odataModel.Value.ConvertToOpenApi(settings);
            return document;
        }

    }
}