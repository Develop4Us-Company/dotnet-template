using System;

namespace AppProject.Core.Infrastructure.Email;

public interface IEmailTemplateRenderer
{
    Task<string> RenderAsync<TModel>(string templateName, TModel model, CancellationToken cancellationToken = default)
        where TModel : class;
}
