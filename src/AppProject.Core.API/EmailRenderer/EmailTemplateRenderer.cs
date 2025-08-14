using System;
using AppProject.Core.Infrastructure.Email;
using RazorLight;

namespace AppProject.Core.API.EmailRenderer;

public class EmailTemplateRenderer : IEmailTemplateRenderer
{
    private readonly RazorLightEngine engine;

    public EmailTemplateRenderer()
    {
        this.engine = new RazorLightEngineBuilder()
            .UseEmbeddedResourcesProject(typeof(IEmailTemplateRenderer).Assembly, "AppProject.Core.Infrastructure.Email.Templates")
            .UseMemoryCachingProvider()
            .Build();
    }

    public async Task<string> RenderAsync<TModel>(string templateName, TModel model, CancellationToken cancellationToken = default)
        where TModel : class
    {
        var templatePath = $"{templateName}.cshtml";
        return await this.engine.CompileRenderAsync(templatePath, model);
    }
}
