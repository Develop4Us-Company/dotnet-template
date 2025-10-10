# dotnet-template
Template para criar projetos em .NET

## Sumário
- [Guia rápido de uso do template](#guia-rápido-de-uso-do-template)
- [Estrutura de projetos](#estrutura-de-projetos)
- [Especificações do projeto](#especificações-do-projeto)
- [Integrações externas](#integrações-externas)
- [Exemplo de CRUD](#exemplo-de-crud)
  - [Backend](#backend)
  - [Frontend](#frontend)
  - [Testes](#testes)
- [Preparando para produção](#preparando-para-produção)

## Guia rápido de uso do template

### Pré-requisitos
- .NET SDK (o `TargetFramework` já está definido em `Directory.Build.props`).
- Visual Studio ou Visual Studio Code com a extensão C# para trabalhar com a solução.
- SQL Server. É possível subir um container local com o comando:
  ```bash
  docker run --name appproject-sqlserver -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=yourStrong(!)Password' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
  ```
- Contas ativas no Auth0, SendGrid e GitHub (com acesso aos GitHub Models) para preencher as integrações descritas a seguir.
- Opcional: instale a ferramenta global `dotnet-ef` para rodar comandos de migração (`dotnet tool install --global dotnet-ef`).

### Passo a passo para configurar o ambiente
1. Clone o repositório e restaure as dependências com `dotnet restore AppProject.slnx`.
2. Verifique a instalação do .NET com `dotnet --info` e confirme que a versão 9.0 está disponível.
3. Garanta que o arquivo `src/AppProject.Core.API/appsettings.Development.json` esteja configurado para apontar para seus recursos locais (ex.: connection string `Server=localhost,1433;Database=AppProject;...`) antes de subir a API. Os demais valores (Auth0, SendGrid, GitHub Models etc.) continuam com placeholders para você preencher.
4. Configure o SQL Server local (ou container) e valide a conexão com `sqlcmd` ou ferramenta de sua preferência.
5. Preencha os placeholders de `src/AppProject.Core.API/appsettings.json` e `src/AppProject.Web/wwwroot/appsettings.json` antes de gerar builds para produção. Esses arquivos agora contêm marcadores `<<SET_...>>` que sinalizam o que precisa ser configurado.
6. Configure as integrações externas (Auth0, SendGrid e GitHub Models) seguindo as instruções detalhadas mais adiante e copie os valores gerados para os arquivos de configuração.
7. Crie o banco de dados aplicando as migrações com:
   ```bash
   dotnet ef database update --project src/AppProject.Core.Infrastructure.Database --startup-project src/AppProject.Core.API
   ```
8. Execute a API com `dotnet run --project src/AppProject.Core.API` (porta padrão `https://localhost:7121`).
9. Execute o frontend com `dotnet run --project src/AppProject.Web` (porta padrão `https://localhost:7035`).
10. Acesse `https://localhost:7035` no navegador para utilizar a aplicação e `https://localhost:7121/swagger` para testar os endpoints.

### Checklist de arquivos de configuração
- `src/AppProject.Core.API/appsettings.json` — arquivo base usado em produção. Preencha os placeholders:
  - `<<SET_SQLSERVER_DATABASE_CONNECTION_STRING>>` e `<<SET_HANGFIRE_SQLSERVER_CONNECTION_STRING>>`: strings de conexão (podem ser iguais).
  - `<<SET_AUTH0_AUTHORITY>>`, `<<SET_AUTH0_CLIENT_ID>>`, `<<SET_AUTH0_AUDIENCE>>`: dados do aplicativo Auth0.
  - `<<SET_SYSTEM_ADMIN_NAME>>`, `<<SET_SYSTEM_ADMIN_EMAIL>>`: usuário administrador que será criado automaticamente.
  - `<<SET_ALLOWED_CORS_ORIGINS>>`: URLs autorizadas a consumir a API.
  - `<<SET_ALLOWED_HOSTS>>`: hosts aceitos quando a aplicação estiver em produção.
  - `<<SET_SENDGRID_API_KEY>>`, `<<SET_SENDGRID_FROM_EMAIL>>`, `<<SET_SENDGRID_FROM_NAME>>`: credenciais de envio de e-mail.
  - `<<SET_GITHUB_AI_ENDPOINT>>`, `<<SET_GITHUB_AI_TOKEN>>`: configurações da integração com GitHub Models.
- `src/AppProject.Core.API/appsettings.Development.json` — já aponta para conexões locais (`Server=localhost,1433;...`) e mantém placeholders para credenciais sensíveis (Auth0, SendGrid, GitHub Models). Ajuste conforme o seu ambiente e evite versionar dados sigilosos.
- `src/AppProject.Web/wwwroot/appsettings.json` — placeholders do frontend (`Auth0` e `Api:BaseUrl`). O arquivo publicado deve apontar para as URLs de produção.
- `src/AppProject.Web/wwwroot/appsettings.Development.json` — traz `Api:BaseUrl` apontando para `https://localhost:7121` e mantém placeholders para Auth0.
- `src/AppProject.Web/Constants/AppProjectConstants.cs` — atualize `ProjectName` e o prefixo de armazenamento local ao renomear o template.
- `src/AppProject.Web/Constants/ThemeConstants.cs` — mantém as chaves de armazenamento de tema alinhadas ao nome do projeto.
- `src/AppProject.Core.API/Bootstraps/Bootstrap.cs` — ao criar novos módulos, registre os assemblies nos métodos `GetControllerAssemblies()` e `GetServiceAssemblies()`.
- `src/AppProject.Web/Bootstraps/WebBootstrap.cs` — inclua os assemblies de clientes Refit em `GetApiClientAssemblies()` e valide o `Api:BaseUrl`.
- `src/AppProject.Web/App.razor` — registre assemblies adicionais no método `OnNavigateAsync` para habilitar o lazy loading de novos módulos.
- `src/AppProject.Web/AppProject.Web.csproj` — adicione novos `ProjectReference` e entradas `BlazorWebAssemblyLazyLoad` quando criar módulos adicionais.
- `src/AppProject.Web/Layout/NavMenu.razor` — inclua itens de menu e permissões para os novos módulos.
- `src/AppProject.Resources/Resource*.resx` — mantenha as traduções sincronizadas ao adicionar novos textos.

## Estrutura de projetos
- **Backend**
  - `src/AppProject.Core.API`: API ASP.NET Core com autenticação, middleware de exceção, configuração de CORS, Rate Limiting e bootstrap de serviços.
  - `src/AppProject.Core.Controllers.<Módulo>` (ex.: `AppProject.Core.Controllers.General`): controllers REST de cada módulo.
  - `src/AppProject.Core.Services.<Módulo>` (ex.: `AppProject.Core.Services.General`): serviços transacionais (CRUD) com regras de negócio específicas.
  - `src/AppProject.Core.Services/<Módulo>` (ex.: `AppProject.Core.Services/General`): serviços de leitura e summaries compartilhados.
  - `src/AppProject.Core.Models` / `src/AppProject.Core.Models.<Módulo>`: DTOs e objetos de request. Use a pasta comum para artefatos compartilhados e a pasta com o nome do módulo para itens específicos.
  - `src/AppProject.Core.Infrastructure.Database`: contexto EF Core, repositório genérico, entidades e `EntityTypeConfiguration`.
  - `src/AppProject.Core.Infrastructure.Email`: abstração de envio de e-mails via SendGrid.
  - `src/AppProject.Core.Infrastructure.AI`: integração com GitHub Models para cenários de IA.
- **Frontend**
  - `src/AppProject.Web`: host Blazor WebAssembly, autenticação OIDC, layout, navegação e bootstrap.
  - `src/AppProject.Web.<Módulo>` (ex.: `AppProject.Web.General`): páginas e componentes específicos do módulo carregados via lazy loading.
  - `src/AppProject.Web.ApiClient` / `src/AppProject.Web.ApiClient.<Módulo>`: interfaces Refit para consumo da API (separe clientes compartilhados dos específicos de módulo).
  - `src/AppProject.Web.Models` / `src/AppProject.Web.Models.<Módulo>`: modelos observáveis usados nos formulários.
  - `src/AppProject.Web.Framework`: componentes e páginas base (SearchControl, DataGridControl, ModelFormPage etc.).
  - `src/AppProject.Web.Shared`: componentes compartilhados por múltiplos módulos (ex.: dropdowns com grid).
- **Testes**
  - `src/AppProject.Core.Tests.<Módulo>` (ex.: `AppProject.Core.Tests.General`): testes unitários das services do backend utilizando NUnit, Moq, Shouldly e Bogus. Crie projetos adicionais conforme novos módulos ou mantenha cenários compartilhados em projetos sem sufixo de módulo.
  - `src/AppProject.Web.Tests.<Módulo>` (ex.: `AppProject.Web.Tests.General`): ponto de partida para testes do frontend; adapte para novos módulos ou use projetos compartilhados quando fizer sentido.

## Especificações do projeto
Seguem algumas especificações do projeto.
* Usamos o idioma Inglês nos códigos e nos nomes de arquivos.
* O template já suporta localização (`en-US`, `pt-BR` e `es-ES`) tanto na API quanto no frontend.
* O frontend usa Radzen para os componentes UI, Refit para os clientes HTTP e autenticação OIDC com Auth0.
* O estilo de código é validado com StyleCop (veja `Stylecop.json`) e com as configurações compartilhadas em `Directory.Build.props`.
* Os projetos backend e frontend executam com o `TargetFramework` `net9.0` e utilizam `implicit usings` e `nullable` habilitados.

## Integrações externas
As seções abaixo descrevem os cadastros necessários para que todas as integrações funcionem. Após concluir cada etapa, copie os valores para os arquivos `appsettings`.

### Auth0
1. Crie uma aplicação do tipo **Single Page Application**.
2. Configure o logo da aplicação em *Settings* se desejar.
3. Preencha as URLs (ajuste as portas caso altere o `launchSettings.json`):
   - **Allowed Callback URLs**: `https://localhost:7035/authentication/login-callback`, `https://localhost:7121/swagger/oauth2-redirect.html`
   - **Allowed Logout URLs**: `https://localhost:7035`, `https://localhost:7121/swagger/`
   - **Allowed Web Origins**: `https://localhost:7035`, `https://localhost:7121`
4. Crie uma **API** no Auth0 e use como *Identifier* o mesmo valor configurado em `Auth0:Audience` (`https://appproject.api` por padrão).
5. Para incluir `email`, `name` e `roles` no JWT, crie uma Action do tipo `post_login` com o script abaixo:
   ```javascript
   if (api.accessToken) {
       if (event.user && event.user.email) {
         api.accessToken.setCustomClaim("email", event.user.email);
       }

       if (event.user && event.user.name) {
         api.accessToken.setCustomClaim("name", event.user.name);
       }

       // Adiciona roles se existirem
       if (event.authorization && event.authorization.roles) {
         api.accessToken.setCustomClaim("roles", event.authorization.roles);
       }
     }
   ```
6. Copie `Authority`, `ClientId` e `Audience` para os `appsettings`.

### SendGrid
1. Crie uma conta no [site do SendGrid](https://sendgrid.com/).
2. Configure uma identidade (domain authentication ou single sender). Autorize a identidade através do e-mail recebido.
3. No menu **Email API > Integration Guide**, gere uma API Key.
4. Envie o primeiro e-mail de teste e confirme o envio no painel.
5. Copie a key e o remetente configurado (`SendEmail:ApiKey`, `SendEmail:FromEmailAddress`, `SendEmail:FromName`).

### GitHub AI Models
1. Siga a documentação oficial: <https://docs.github.com/en/github-models/use-github-models/prototyping-with-ai-models>.
2. Gere um token com permissão para usar os modelos hospedados pelo GitHub.
3. Preencha `AI:Endpoint` (padrão `https://models.github.ai/inference`) e `AI:Token` nos `appsettings`.

### Usuário administrador
Ao subir a API pela primeira vez, o bootstrap cria ou atualiza o usuário administrador definido em `SystemAdminUser`. Utilize esse usuário para garantir que existe ao menos uma conta com permissão para acessar todos os cadastros.

## Exemplo de CRUD
O exemplo abaixo mostra, passo a passo, como o módulo General implementa os cadastros de País, Estado, Cidade e Bairros. Siga as mesmas etapas quando criar novos módulos.

### Backend

#### 1. Identifique qual o módulo
Primeiramente, identifique qual o módulo que você deseja colocar a sua nova entidade. Por exemplo, imagine os cadastros de estados, cidades e países. Esses cadastros são de um módulo Geral. Por isso, usaremos as pastas General sempre que possível dentro do projeto.

##### Se for necessário criar um novo módulo
Caso seja identificado que precisa criar um novo módulo, use o módulo **General** como referência e crie/ajuste os itens abaixo (substitua `NovoModulo` pelo nome desejado):
- **Backend**
  - Projeto `AppProject.Core.Models.NovoModulo` com os DTOs do módulo.
  - Projeto `AppProject.Core.Services.NovoModulo` contendo as services de CRUD.
  - Pasta `NovoModulo` dentro de `AppProject.Core.Services` para os serviços de summary.
  - Projeto `AppProject.Core.Controllers.NovoModulo`.
  - Pastas `Entities/NovoModulo` e `EntityTypeConfiguration/NovoModulo` dentro de `AppProject.Core.Infrastructure.Database` para as entidades e configurações EF Core.
  - Migrações específicas no projeto `AppProject.Core.Infrastructure.Database`.
- **Frontend**
  - Projeto `AppProject.Web.NovoModulo` com as páginas e componentes Blazor.
  - Projeto `AppProject.Web.ApiClient.NovoModulo` com as interfaces Refit de CRUD.
  - Pasta `NovoModulo` dentro de `AppProject.Web.ApiClient` para os clientes de summary.
  - Projeto `AppProject.Web.Models.NovoModulo` com os modelos observáveis do client.
  - Pasta `NovoModulo` em `AppProject.Web.Shared` para componentes compartilháveis (dropdowns, cards etc.).
- **Testes**
  - Projeto `AppProject.Core.Tests.NovoModulo` cobrindo as services do backend.
  - Projeto `AppProject.Web.Tests.NovoModulo` (opcional) para cenários de UI/integração.

Também será preciso editar os seguintes arquivos para registrar o assembly do novo módulo:
- `src/AppProject.Core.API/Bootstraps/Bootstrap.cs` — inclua o assembly nos métodos `GetControllerAssemblies()` e `GetServiceAssemblies()`.
- `src/AppProject.Web/Bootstraps/WebBootstrap.cs` — registre o assembly em `GetApiClientAssemblies()` e ajuste o carregamento lazy se necessário.
- `src/AppProject.Web/App.razor` — adicione o assembly às condições de `OnNavigateAsync` para habilitar o lazy loading.
- `src/AppProject.Web/AppProject.Web.csproj` — crie os `ProjectReference` e entradas `BlazorWebAssemblyLazyLoad`.
- `src/AppProject.Web/Layout/NavMenu.razor` — inclua o item de menu e as permissões relacionadas.
- `src/AppProject.Resources/Resource*.resx` — adicione as novas chaves de tradução.

#### 2. Conteúdos compartilhados entre módulos
Caso você esteja adicionando arquivos que são compartilhados entre os módulos, será necessário colocar esses arquivos no projeto raiz em vez do projeto que leva o nome do módulo. Por exemplo, imagine que você esteja adicionando a tabela Customer. Customer é uma tabela que pode ser usada em vários módulos (invoice, financial, etc.). Nesse caso, em vez de ter um módulo General ou Customer, o ideal seria colocar no projeto raiz, dentro de uma pasta que leva o nome do módulo.

Veja a seguir uma lista dos projetos raíz onde podemos criar pastas que representam parte dos módulos que serão compartilhados:
* `AppProject.Core.Models` — DTOs e requests compartilhados.
* `AppProject.Core.Services` — serviços comuns (por exemplo, summaries visíveis em vários módulos).
* `AppProject.Web` — componentes de layout, autenticação, bootstrap e navegação.
* `AppProject.Web.ApiClient` — interfaces Refit reutilizadas em mais de um módulo.
* `AppProject.Web.Models` — modelos observáveis usados por múltiplos módulos.
* `AppProject.Web.Shared` — componentes Blazor genéricos (dropdowns, cards, controles auxiliares).
* `AppProject.Resources` — traduções reutilizadas em diferentes áreas.

#### 3. Adicionando os DTOs na API
No projeto, há os DTOs do lado da API e os DTOs do lado da WEB (ou client). Eles são diferentes, porque do lado da WEB os DTOs podem ter notificações de mudanças (INotifyPropertyChanged), enquanto que no lado da API não tem.

Os DTOs são adicionados nos projetos:
* AppProject.Core.Models (para DTOs compartilhados entre os módulos; crie pastas internas com o nome do módulo quando necessário — por exemplo, `AppProject.Core.Models/General`).
* AppProject.Core.Models.<Módulo> (ex.: `AppProject.Core.Models.General`) para DTOs específicos de um módulo.
* Crie projetos adicionais como `AppProject.Core.Models.<NovoModulo>` se precisar separar DTOs por módulo.

Normalmente num CRUD, teremos dois tipos de DTOs: os do tipo Entidade (herdando de IEntity) e os do tipo Summary (herdando de ISummary). 

##### DTOs de entidades
Os DTOs de entidade representam os campos das tabelas e devem herdar de `IEntity`. Eles precisam expor `RowVersion` para suportar concorrência otimista e utilizar DataAnnotations para validar obrigatoriedade, tamanho máximo e regras básicas. Referências:
- `AppProject.Core.Models.General/Country.cs` — entidade simples com `Name`, `Code` e `RowVersion`.
- `AppProject.Core.Models.General/State.cs` — adiciona `CountryId` validado por `RequiredGuid`.
- `AppProject.Core.Models.General/City.cs` — além dos campos principais, mantém as coleções `ChangedNeighborhoodRequests` e `DeletedNeighborhoodRequests` para sincronizar bairros.
- `AppProject.Core.Models.General/Neighborhood.cs` — estrutura básica de bairros, utilizada tanto na API quanto no frontend.

###### Validações de entidades
As exceções de validação retornam `ExceptionDetail` com o código `RequestValidation`. Todos os atributos personalizados residem em `AppProject.Models.CustomValidators`:
- `ValidateCollectionAttribute` garante a validação em cascata de listas.
- `RequiredGuidAttribute` impede o envio de GUIDs vazios.

O middleware [`AppProject.Core.API/Middlewares/ExceptionMiddleware.cs`](./src/AppProject.Core.API/Middlewares/ExceptionMiddleware.cs) converte exceções em respostas padronizadas, enquanto `Bootstrap.ConfigureValidations` força o lançamento de `AppException` quando o `ModelState` é inválido.

Para relacionamentos, mantenha apenas os identificadores (ex.: `StateId` em `City`). Ao manipular entidades agregadas, como bairros da cidade, utilize as coleções de `CreateOrUpdateRequest`/`DeleteRequest` do DTO pai (`City.cs`) para indicar inserções, atualizações e exclusões.

##### DTOs de summaries
Use DTOs de summary para alimentar grids, combos e demais consultas de leitura. Eles herdam de `ISummary`, não possuem DataAnnotations e contêm apenas os campos necessários para exibir informações na interface:
- `AppProject.Core.Models/General/CountrySummary.cs` — exemplo do módulo General que mantém `Id` e `Name`.
- `AppProject.Core.Models/General/StateSummary.cs` — exemplo do módulo General com `CountryName` e `CountryId`.
- `AppProject.Core.Models/General/CitySummary.cs` — exemplo do módulo General com `StateName`, `StateId`, `CountryName` e `CountryId`.

Para pesquisas avançadas, utilize `SearchRequest` como classe base e adicione propriedades específicas:
- `AppProject.Core.Models/General/StateSummarySearchRequest.cs` — permite filtrar estados por `CountryId`.
- `AppProject.Core.Models/General/CitySummarySearchRequest.cs` — filtra cidades por `StateId`.

Sempre avalie se o summary deve trazer nomes agregados em vez de objetos completos. Isso facilita a serialização e evita cargas desnecessárias ou ciclos de referência.

#### 4. Adicionando as entidades de banco
As entidades de banco residem em `AppProject.Core.Infrastructure.Database/Entities` e seguem o padrão `Tb[Nome]`. Recomendações:
- `TbCountry.cs`, `TbState.cs`, `TbCity.cs`, `TbNeighborhood.cs` — mantêm DataAnnotations para chaves, tamanho de colunas e relacionamentos. Utilize nomes de tabela no plural (`[Table("Countries")]`) e configure coleções de navegação (`States`, `Cities`, `Neighborhoods`) para facilitar o carregamento.
- Armazene apenas informações necessárias para persistência; qualquer lógica adicional deve ficar nas services.
- Aplique `MaxLength` em colunas de texto e mantenha valores plausíveis para os campos compartilhados com os DTOs.

Os relacionamentos são modelados com campos de chave estrangeira (ex.: `CountryId` em `TbState`) e propriedades de navegação com `[ForeignKey]`. Ao criar entidades adicionais, siga o mesmo padrão para garantir que o EF Core configure as constraints automaticamente.

##### Adicionando os arquivos EntityTypeConfiguration
Classes de configuração complementam as entidades com índices, restrições adicionais e regras específicas do EF Core. Elas ficam em `AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/[Modulo]` e seguem o padrão `Tb[Nome]Configuration`. Exemplos:
- `TbCountryConfiguration.cs` — define índice único para `Name`.
- `TbStateConfiguration.cs` — cria índice para facilitar buscas por `Name`.
- `TbCityConfiguration.cs` e `TbNeighborhoodConfiguration.cs` — configuram índices para as entidades dependentes.

Todas herdam de `IEntityTypeConfiguration<T>` e são carregadas automaticamente por `ApplicationDbContext`. Caso precise adicionar novas constraints (por exemplo, índices compostos), implemente-as nesses arquivos em vez de inflar as entidades com lógica adicional.

Importante: ao adicionar um novo arquivo herdando de `IEntityTypeConfiguration`, não é necessário registrar manualmente no `ApplicationDbContext`; o método `OnModelCreating` já percorre o assembly e aplica cada configuração.

##### Adicionando DbSet no ApplicationDbContext
Atualize [`ApplicationDbContext`](./src/AppProject.Core.Infrastructure.Database/ApplicationDbContext.cs) sempre que criar uma nova entidade. Cada tabela deve possuir um `DbSet<T>` com nome no plural (por exemplo, `Countries`, `States`, `Cities`, `Neighborhoods`). Manter essa convenção facilita a leitura e evita divergências entre EF Core e o banco.

##### Rodando migrations
Para que possamos criar os scripts do banco, nós precisamos rodar o migration do Entity Framework. Para isso, abra o terminal na pasta src do projeto e execute o comando a seguir:

```bash
dotnet ef migrations add MigrationName --project AppProject.Core.Infrastructure.Database --startup-project AppProject.Core.API --output-dir Migrations
```

No lugar de MigrationName, dê um nome que identifique o seu migration, como talvez o nome de uma das tabelas que você está modificando ou algum assunto que remeta à alteração.

Esse comando fará com que, na pasta Migrations do projeto AppProject.Core.Infrastructure.Database, contenha os arquivos de script de migração.

Importante: não precisa aplicar o migration, pois ele já é aplicado automaticamente quando a API sobe.

#### 5. Adicionando as classes de configuração do Mapster
Por padrão, o Mapster consegue mapear entidades para DTOs quando os nomes das propriedades coincidem. Crie configurações adicionais apenas quando precisar transformar campos ou incluir dados de relacionamentos. No módulo General utilizamos:
- `AppProject.Core.Infrastructure.Database/Mapper/General/StateSummaryMapsterConfig.cs` — injeta `CountryName` ao mapear `TbState` → `StateSummary`.
- `AppProject.Core.Infrastructure.Database/Mapper/General/CitySummaryMapsterConfig.cs` — expõe `StateName` e `CountryName` a partir das entidades relacionadas.

Essas classes implementam `IRegisterMapsterConfig` e são carregadas no bootstrap (`Bootstrap.ConfigureMapper`). Ao adicionar novas configurações:
1. Crie o arquivo `[Dto]MapsterConfig.cs` dentro da pasta `Mapper/[Modulo]`.
2. Configure o `TypeAdapterConfig` no método `Register`.
3. Evite lógica complexa no mapper; utilize as services para manipular regras de negócio.

As services centralizam regras de negócio, validações e orquestração do repositório. Elas residem em `AppProject.Core.Services` para itens compartilhados e em `AppProject.Core.Services.<Módulo>` (por exemplo, `AppProject.Core.Services.General`) para implementações específicas. Todos os tipos compatíveis são registrados automaticamente na DI por `Bootstrap.ConfigureServices`.

##### Interface da classe de serviço
- `ICountryService.cs`, `IStateService.cs`, `ICityService.cs` implementam `ITransientService` e os contratos genéricos `IGetEntity`, `IPostEntity`, `IPutEntity` e `IDeleteEntity`. Isso padroniza as assinaturas de CRUD e mantém a API consistente.
- `ICityService` adiciona `GetNeighborhoodEntitiesAsync`, que retorna bairros associados a uma cidade usando `GetByParentIdRequest<Guid>`.
- As interfaces de summary (`ICountrySummaryService.cs`, `IStateSummaryService.cs`, `ICitySummaryService.cs`) expõem `IGetSummaries`/`IGetSummary` com seus respectivos requests (`SearchRequest`, `StateSummarySearchRequest`, `CitySummarySearchRequest`). Utilize essa abordagem ao criar novos summaries.

##### Classes de serviço
- `CountryService.cs`, `StateService.cs` e `CityService.cs` são responsáveis por:
  1. Validar permissões com `IPermissionService.ValidateCurrentUserPermissionAsync` usando `PermissionType.System_ManageSettings`.
  2. Executar validações de negócio (`ValidateCountryAsync`, `ValidateStateAsync`, `ValidateCityAsync`) para evitar duplicidades e inconsistências.
  3. Usar `IDatabaseRepository` para consultar (`GetFirstOrDefaultAsync`, `GetByConditionAsync`), inserir (`InsertAndSaveAsync`), atualizar (`UpdateAndSaveAsync`) ou excluir (`DeleteAndSaveAsync`) registros.
  4. Mapear DTOs ↔ entidades via `Mapster` (`Adapt`).
- `CityService` merece destaque porque manipula agregados:
  - Persistência dos bairros ocorre por meio das listas `ChangedNeighborhoodRequests` e `DeletedNeighborhoodRequests` vindas do DTO de cidade.
  - Os métodos `ValidateCityAsync` e `ValidateNeighborhoodsBelongToCityAsync` evitam nomes duplicados e garantem que os bairros realmente pertençam à cidade em edição.
  - Inserções múltiplas utilizam `InsertAsync`/`UpdateAsync` seguida de `SaveAsync` para garantir atomicidade.
- Todas as services lançam `AppException` com `ExceptionCode` apropriado (`EntityNotFound`, `General_*_DuplicateName`, etc.), assegurando mensagens traduzidas via resources.

##### Classes de serviços de summaries
- `CountrySummaryService.cs`, `StateSummaryService.cs` e `CitySummaryService.cs` tratam consultas de leitura. Eles chamam `GetByConditionAsync` com filtros (`SearchText`, `Take`, `CountryId`, `StateId`) e utilizam `SummariesResponse<T>` para devolver coleções imutáveis.
- Quando `GetSummaryAsync` não encontra o registro, as classes lançam `AppException(ExceptionCode.EntityNotFound)` para manter consistência com as services de escrita.
- Ao expandir o template, siga este padrão: mantenha serviços de leitura livres de validações de permissão custosas (a não ser que haja requisitos específicos) e centralize filtros em objetos de request para reutilização no frontend.

#### 7. Criando as classes de controller
Os controllers ficam em projetos específicos de cada módulo, como `AppProject.Core.Controllers.<Módulo>` (ex.: `AppProject.Core.Controllers.General`). Eles expõem apenas a lógica necessária para receber as requests, chamar as services e retornar o resultado padronizado (`Ok(...)`). Exemplos:
- `CountryController.cs`, `StateController.cs`, `CityController.cs` — implementam endpoints de CRUD para cada entidade.
- `CityController` também oferece `GetNeighborhoodsAsync` para consultar os bairros relacionados.
- `CountrySummaryController.cs`, `StateSummaryController.cs`, `CitySummaryController.cs` — expõem endpoints de consulta (`GetSummariesAsync`, `GetSummaryAsync`).

Diretrizes gerais:
- Aplique `[Authorize]` para proteger os endpoints e `[ApiController]` para habilitar validação automática de modelo.
- Utilize o padrão de rota `api/<módulo>/[controller]/[action]` (por exemplo, `api/general/Country/Post`).
- Receba parâmetros usando `[FromQuery]` para buscas (`GetByIdRequest`, `DeleteRequest`) e `[FromBody]` para mutações (`CreateOrUpdateRequest`).
- Retorne sempre `IActionResult` com `Ok(...)` para manter consistência e facilitar tratamento global de erros.

### Frontend
O frontend é um aplicativo **Blazor WebAssembly** que consome a API via Refit e utiliza os componentes do Radzen. A implementação do módulo General serve como guia para novos módulos.

#### Visão geral e arquivos principais
- Os modelos client-side ficam em `AppProject.Web.Models` e `AppProject.Web.Models.<Módulo>` (ex.: `AppProject.Web.Models.General`). Todos herdam de [`ObservableModel`](./src/AppProject.Web.Models/ObservableModel.cs), que implementa `INotifyPropertyChanged` para atualizar a UI automaticamente.
- As classes de entidade, como [`Country.cs`](./src/AppProject.Web.Models.General/Country.cs), [`State.cs`](./src/AppProject.Web.Models.General/State.cs), [`City.cs`](./src/AppProject.Web.Models.General/City.cs) e [`Neighborhood.cs`](./src/AppProject.Web.Models.General/Neighborhood.cs), espelham os DTOs da API. No caso de `City`, mantemos as coleções `ChangedNeighborhoodRequests` e `DeletedNeighborhoodRequests` para enviar alterações aninhadas.
- Os summaries utilizados em grids e combos ficam em pastas como [`AppProject.Web.Models/<Módulo>`](./src/AppProject.Web.Models/General). Exemplos para o módulo General: [`CountrySummary.cs`](./src/AppProject.Web.Models/General/CountrySummary.cs), [`StateSummary.cs`](./src/AppProject.Web.Models/General/StateSummary.cs) e [`CitySummary.cs`](./src/AppProject.Web.Models/General/CitySummary.cs).

#### Clientes HTTP com Refit
- As interfaces de CRUD residem em projetos como [`AppProject.Web.ApiClient.<Módulo>`](./src/AppProject.Web.ApiClient.General). Exemplo: [`ICityClient.cs`](./src/AppProject.Web.ApiClient.General/ICityClient.cs) cobre todos os endpoints de `CityController` no módulo General.
- Os summaries possuem clientes separados em pastas como [`AppProject.Web.ApiClient/<Módulo>`](./src/AppProject.Web.ApiClient/General). Veja os arquivos do módulo General, como [`ICitySummaryClient.cs`](./src/AppProject.Web.ApiClient/General/ICitySummaryClient.cs) e [`ICountrySummaryClient.cs`](./src/AppProject.Web.ApiClient/General/ICountrySummaryClient.cs).
- O bootstrap [`WebBootstrap.cs`](./src/AppProject.Web/Bootstraps/WebBootstrap.cs) registra dinamicamente todas as interfaces Refit definidas nos assemblies listados por `GetApiClientAssemblies()`. Ao adicionar um módulo novo, inclua o assembly correspondente.

#### Páginas de pesquisa (Search)
- As páginas de pesquisa herdam de [`SearchPage<TRequest,TSummary>`](./src/AppProject.Web.Framework/Pages/SearchPage.cs), que encapsula a execução de buscas, a seleção de itens e a exibição de alertas quando o limite `Take` é atingido.
- O componente [`SearchControl.razor`](./src/AppProject.Web.Framework/Components/SearchControl.razor) disponibiliza formulário padrão com campo de texto, filtros avançados e alerta configurável.
- Exemplos:
  - [`CountrySummaryPage.razor`](./src/AppProject.Web.General/Pages/CountrySummaryPage.razor) mostra o grid com países e operações `New`, `Edit` e `Delete`.
- [`StateSummaryPage.razor`](./src/AppProject.Web.General/Pages/StateSummaryPage.razor) adiciona filtro por país via `CountrySummaryDropDownDataGridControl`.
- [`CitySummaryPage.razor`](./src/AppProject.Web.General/Pages/CitySummaryPage.razor) filtra por estado e exibe colunas adicionais.
- O grid padrão é [`DataGridControl.razor`](./src/AppProject.Web.Framework/Components/DataGridControl.razor), que aceita `GlobalActions`, `ContextActions` e se integra com seleção múltipla.

#### Páginas de formulário e itens aninhados
- Formulários herdam de [`ModelFormPage<TModel>`](./src/AppProject.Web.Framework/Pages/ModelFormPage.cs) e utilizam o componente [`ModelFormControl.razor`](./src/AppProject.Web.Framework/Components/ModelFormControl.razor).
- `ModelFormControl` permite customizações importantes:
  - `ShowNewAction`, `ShowEditAction` e `ShowDeleteAction` em `DataGridControl` controlam quais botões são exibidos.
  - `PreferAddOverNew` troca o texto padrão do botão para “Add” (caso de [`CityFormPage.razor`](./src/AppProject.Web.General/Pages/CityFormPage.razor) ao gerenciar bairros).
  - `PreferOpenOverEdit` exibe “Open” no lugar de “Edit”, útil para telas somente leitura.
  - `PreferExecuteOverSave` renomeia o botão principal para “Executar”, adequado para telas de processamento.
  - `PreferCloseOverCancel` aplica o estilo de fechamento ao botão secundário.
- Para relacionamentos aninhados, siga o exemplo de `CityFormPage`:
  - Carregue registros filhos via cliente dedicado (`ICityClient.GetNeighborhoodsAsync`).
  - Ao inserir/alterar um item, adicione-o em `ChangedNeighborhoodRequests`.
  - Ao excluir, mova o identificador para `DeletedNeighborhoodRequests`.
  - Use diálogos especializados (como [`NeighborhoodFormPage.razor`](./src/AppProject.Web.General/Pages/NeighborhoodFormPage.razor)) para editar os itens filhos.

#### Fluxo completo dos cadastros General
1. **País**
   - Pesquisa: [`CountrySummaryPage.razor`](./src/AppProject.Web.General/Pages/CountrySummaryPage.razor) chama `ICountrySummaryClient` e, após confirmar exclusões, usa `ICountryClient.DeleteAsync`.
   - Formulário: [`CountryFormPage.razor`](./src/AppProject.Web.General/Pages/CountryFormPage.razor) abre em `DialogService`, reutilizando o mesmo componente para criar ou editar. As validações visuais usam `RadzenRequiredValidator` e `RadzenLengthValidator`.
2. **Estado**
   - Pesquisa: [`StateSummaryPage.razor`](./src/AppProject.Web.General/Pages/StateSummaryPage.razor) acrescenta filtro de país via [`CountrySummaryDropDownDataGridControl.razor`](./src/AppProject.Web.Shared/General/Components/CountrySummaryDropDownDataGridControl.razor).
   - Formulário: [`StateFormPage.razor`](./src/AppProject.Web.General/Pages/StateFormPage.razor) exige a seleção de um país. A validação do `Guid` usa `RadzenCustomValidator` para impedir `Guid.Empty`.
3. **Cidade**
   - Pesquisa: [`CitySummaryPage.razor`](./src/AppProject.Web.General/Pages/CitySummaryPage.razor) combina filtros por estado, ordenação e colunas adicionais (`StateName`, `CountryName`).
   - Formulário: [`CityFormPage.razor`](./src/AppProject.Web.General/Pages/CityFormPage.razor) utiliza outro `DataGridControl` para gerenciar bairros e marca `PreferAddOverNew` para refletir a ação de inserir filhos. As listas `ChangedNeighborhoodRequests` e `DeletedNeighborhoodRequests` são atualizadas sempre que o usuário confirma o diálogo de bairro.
4. **Bairros**
   - Formulário filho: [`NeighborhoodFormPage.razor`](./src/AppProject.Web.General/Pages/NeighborhoodFormPage.razor) herda de `ModelFormPage<Neighborhood>` e retorna o objeto via `CloseDialogAsync`. O componente é usado tanto para criar quanto para editar registros dentro da cidade.

#### Componentes reutilizáveis
- [`DataGridControl.razor`](./src/AppProject.Web.Framework/Components/DataGridControl.razor) encapsula o `RadzenDataGrid` com seleção múltipla, localização e botões configuráveis (`ShowNewAction`, `ShowEditAction`, `ShowDeleteAction`, `PreferAddOverNew`, `PreferOpenOverEdit`).
- [`ModelFormControl.razor`](./src/AppProject.Web.Framework/Components/ModelFormControl.razor) padroniza o cabeçalho de formulários, inclui slots `GlobalActions` e os toggles `PreferExecuteOverSave` / `PreferCloseOverCancel`.
- [`FieldsetControl.razor`](./src/AppProject.Web.Framework/Components/FieldsetControl.razor) gera fieldsets colapsáveis com traduções centralizadas.
- [`SearchControl.razor`](./src/AppProject.Web.Framework/Components/SearchControl.razor) fornece filtros básicos/avançados e alerta automático para o `Take`.
- [`DropDownDataGridControl.cs`](./src/AppProject.Web.Framework/Components/DropDownDataGridControl.cs) ajusta textos do `RadzenDropDownDataGrid` e é reaproveitado em [`CountrySummaryDropDownDataGridControl.razor`](./src/AppProject.Web.Shared/General/Components/CountrySummaryDropDownDataGridControl.razor) e [`StateSummaryDropDownDataGridControl.razor`](./src/AppProject.Web.Shared/General/Components/StateSummaryDropDownDataGridControl.razor), garantindo que o item selecionado seja carregado mesmo após paginação/filtragem.
- [`BusyIndicatorControl.razor`](./src/AppProject.Web.Framework/Components/BusyIndicatorControl.razor) é utilizado por [`AppProjectComponentBase`](./src/AppProject.Web.Framework/Components/AppProjectComponentBase.cs) para exibir diálogos de progresso e tratar exceções (incluindo respostas `ApiException` do Refit).
- Layout e preferências globais:
  - [`LanguageSelector.razor`](./src/AppProject.Web/Layout/LanguageSelector.razor) persiste a cultura selecionada usando `Blazored.LocalStorage`.
  - [`ThemeToggle.razor`](./src/AppProject.Web/Layout/ThemeToggle.razor) alterna entre os temas definidos em [`ThemeConstants`](./src/AppProject.Web/Constants/ThemeConstants.cs).
  - [`Login.razor`](./src/AppProject.Web/Layout/Login.razor) resume o fluxo de autenticação OIDC (login/logout).

#### Localização e resources
- Tanto a API quanto o frontend consomem os resources definidos em `AppProject.Resources`. O helper [`StringResource.cs`](./src/AppProject.Resources/StringResource.cs) lê os arquivos [`Resource.resx`](./src/AppProject.Resources/Resource.resx), [`Resource.pt-BR.resx`](./src/AppProject.Resources/Resource.pt-BR.resx) e [`Resource.es-ES.resx`](./src/AppProject.Resources/Resource.es-ES.resx).
- Ao adicionar novas telas ou mensagens, inclua as chaves nos três arquivos para manter o suporte multilíngue.
- Os componentes reutilizáveis consomem chaves específicas (`DataGridControl_NewButton_Text`, `DataGridControl_AddButton_Text`, `ModelFormControl_SaveButton_Text`, `ModelFormControl_ExecuteButton_Text`, entre outras). Ajuste essas chaves para personalizar rótulos sem alterar o código.

#### Lazy loading, navegação e bootstrap
- [`AppProject.Web/App.razor`](./src/AppProject.Web/App.razor) carrega assemblies sob demanda. Rotas que começam com `general` carregam `AppProject.Web.General.dll`. Para novos módulos, replique a lógica adicionando o prefixo de rota e o assembly.
- [`WebBootstrap.ConfigureRefit`](./src/AppProject.Web/Bootstraps/WebBootstrap.cs) registra todos os clientes HTTP. Atualize `GetApiClientAssemblies()` com os novos assemblies para que as interfaces sejam registradas automaticamente.
- [`AppProjectConstants`](./src/AppProject.Web/Constants/AppProjectConstants.cs) define o nome exibido no cabeçalho e os prefixos de armazenamento; ajuste ao personalizar o template.

#### Menu e permissões
- [`NavMenu.razor`](./src/AppProject.Web/Layout/NavMenu.razor) consulta `IPermissionClient` e exibe os itens do módulo General apenas para usuários com `PermissionType.System_ManageSettings`.
- Ao criar novos módulos, adicione os itens de menu correspondentes e avalie quais permissões serão exigidas.

### Testes
Os testes unitários residem em projetos como `src/AppProject.Core.Tests.<Módulo>` (ex.: `src/AppProject.Core.Tests.General`) e utilizam **NUnit**, **Moq**, **Shouldly** e **Bogus**. Eles validam tanto cenários positivos quanto exceções esperadas.

- [`CountryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CountryServiceTests.cs): cobre leitura, inserção, atualização e exclusão de países, além de validar duplicidade e autorização.
- [`StateServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/StateServiceTests.cs): garante a validação de nomes duplicados por país e o comportamento do CRUD de estados.
- [`CityServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CityServiceTests.cs): exercita a lógica de bairros aninhados, duplicidades e relacionamentos durante `Post`, `Put` e `Delete`.
- [`CountrySummaryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CountrySummaryServiceTests.cs): testa filtros por texto e o tratamento de entidades inexistentes.
- [`StateSummaryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/StateSummaryServiceTests.cs): avalia filtros por `CountryId` e busca individual.
- [`CitySummaryServiceTests.cs`](./src/AppProject.Core.Tests.General/Services/CitySummaryServiceTests.cs): assegura que filtros por `StateId` e `SearchText` funcionem e que exceções sejam lançadas corretamente.

Cada classe de teste segue o padrão Arrange/Act/Assert, inicializando *mocks* do `IDatabaseRepository` e `IPermissionService` e utilizando `Bogus` para gerar dados confiáveis. O método utilitário `AssertAppExceptionAsync` (definido em cada classe de testes) simplifica a verificação de mensagens/`ExceptionCode` retornados pelas services. Ao criar novos cenários:
- Configure o mock de permissões para retornar `Task.CompletedTask` (mantendo o comportamento padrão das services).
- Use `Setup`/`ReturnsAsync` do Moq para simular consultas EF Core (ex.: `GetFirstOrDefaultAsync`, `HasAnyAsync`, `GetByConditionAsync`).
- Valide tanto fluxos felizes quanto fluxos de exceção, garantindo que regras de negócio sejam testadas antes de tocar o banco (`HasAnyAsync`) e após (`InsertAndSaveAsync`, `UpdateAsync` etc.).
- Prefira `Shouldly` para asserts legíveis (`response.Entity.ShouldBe(expectedCountry)`), mantendo consistência e mensagens claras.

Execute todos os testes com:
```bash
dotnet test AppProject.slnx
```
Ao criar novos módulos, replique a estrutura em `AppProject.Core.Tests.<Módulo>` e `AppProject.Web.Tests.<Módulo>` (ou mantenha projetos compartilhados com subpastas nomeadas) para cobrir regras de negócio e consultas.

## Preparando para produção
- Preencha todos os placeholders em `appsettings.json` e `wwwroot/appsettings.json` com valores reais (conexões de produção, Auth0, SendGrid, GitHub Models, URLs públicas etc.).
- Configure `ASPNETCORE_ENVIRONMENT=Production` para a API e `DOTNET_ENVIRONMENT=Production` para o frontend publicado.
- Atualize `Cors:AllowedOrigins` e `AllowedHosts` com os domínios oficiais.
- Cadastre novas URLs no Auth0 (callback, logout e origens web) e gere um `ClientSecret` se necessário.
- Garanta que o banco de dados de produção esteja criado e com as migrações aplicadas (`dotnet ef database update` ou migration automática no startup).
- Gere uma key dedicada no SendGrid e valide o domínio/remetente usado pelo produto.
- Gere um token GitHub exclusivo para produção e mantenha apenas o endpoint necessário em `AI:Endpoint`.
- Ajuste o `SystemAdminUser` para um e-mail realmente monitorado pela equipe de operações.
- Revise as configurações de logging (`Serilog`) e considere direcionar os logs para um sink persistente em produção.
- Confira se o Hangfire utiliza uma base separada ou uma connection string adequada para o ambiente.
- Remova dados de exemplo e valide permissões de usuários antes do go-live.
- Use variáveis de ambiente ou Azure App Configuration/Secrets Manager para armazenar credenciais sensíveis, evitando publicá-las em repositórios.
- Execute `dotnet publish -c Release src/AppProject.Core.API/AppProject.Core.API.csproj` e `dotnet publish -c Release src/AppProject.Web/AppProject.Web.csproj` para gerar os artefatos que serão enviados aos ambientes de produção.
- Configure pipelines (GitHub Actions, Azure DevOps, etc.) para rodar `dotnet test` e publicar os projetos automaticamente, garantindo que migrações e configurações sejam aplicadas antes do deploy.
