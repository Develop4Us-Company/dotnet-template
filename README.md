# dotnet-architecture-template
Template para criar projetos em .NET

// TODO: Explicar cada pasta do projeto e os arquivos principais

# Especificações do projeto
Segue algumas especificações do projeto.
* Usamos o idioma Ingles nos códigos e nos nomes de arquivos.

# Exemplo de CRUD
Vamos ver a seguir, como podemos criar um CRUD completo utilizando esse template.

## Identifique qual o módulo
Primeiramente, identifique qual o módulo que você deseja colocar a sua nova entidade. Por exemplo, imagine os cadastros de estados, cidades e países. Esses cadastros são de um módulo Geral. Por isso, usaremos as pastas General sempre que possível dentro do projeto.

### Se for necessário criar um novo módulo
Caso seja identificado que precisa criar um novo módulo, então será preciso criar os seguintes projetos/pastas:
// TODO: Listar pastas e arquivos que precisam ser criados

Também será preciso editar os seguintes arquivos:
// TODO: Listar quais arquivos precisam alterar para incluir o Assembly do novo projeto.

### Se for algo compartilhado entre vários módulos
Caso você esteja adicionando arquivos que são compartilhado entre os módulos, será necessário colocar esses arquivos no projeto raíz ao invés do projeto que leva o nome do módulo. Por exemplo, imagine que você esteja adicionando a tabela Customer. Customer é uma tabela que pode ser usada em vários módulos (invoice, financial, etc.). Nesse caso, ao invés de ter um módulo General ou Customer, o ideal seria colocar no projeto raíz, dentro de uma pasta que leva o nome do módulo.

Veja a seguir uma lista dos projetos raíz onde podemos criar pastas que representam parte dos módulos que serão compartilhados:
* AppProject.Core.Services
// TODO: Listar todos os módulos que tem dados compartilhados.

## Adicionando os DTOs na API
No projeto, há os DTOs do lado da API e os DTOs do lado da WEB (ou client). Eles são diferentes, porque do lado da WEB os DTOs podem ter notificações de mudanças (INotifyPropertyChanged), enquanto que no lado da API não tem.

Os DTOs são adicionados nos projetos:
* AppProject.Core.Models (para DTOs que são compartilhados entre os módulos [nesse caso, não esqueça de criar uma pasta, caso não exista, com o nome do módulo, para que o arquivo fique dentro dela (exemplo: pasta General no projeto AppProject.Core.Models significa que o conteúdo dentro dela pertence ao módulo General, porém está no projeto compartilhado entre todos os módulos)]);
* AppProject.Core.Models.General (para DTOs do módulo General);
* AppProject.Core.Models.ModuleName (para DTOs de outros módulos, deve-se ter um projeto específico onde ModuleName é o nome do módulo).

Normalmente num CRUD, teremos dois tipos de DTOs: os do tipo Entidade (herdando de IEntity) e os do tipo Summary (herdando de ISummary). 

### DTOs de entidades
Os do tipo entidade representam os campos de uma tabela. Normalmente eles terão quase todos os campos que uma tabela tem (exceto os campos de log, que deverão ser colocados apenas quando necessário). Ao herdar de IEntity, você obrigatoriamente deverá colocar a propriedade RowVersion que está nessa interface. Toda entidade tem um RowVersion que serve para controlar alterações simultâneas. 

Para validar campos obrigatórios, tamanho máximo de campos strings ou ranges de valores, você poderá utilizar os DataAnnotations. Isso fará com que eles sejam validados no momento em que um endpoint da API for chamado. 

#### Validações de entidades
Ao validar uma entidade, a API retorna uma instância da classe ExceptionDetail, contendo o valor RequestValidation na propriedade ExceptionCode. Na propriedade AdditionalInfo, contém os erros encontrados nos campos escritos no idioma Inglês. Não traduzimos esses campos, pois o preferível é que normalmente os DTOs sejam validados do lado do client que consome a API.

Info: sobre as validações, veja o arquivo ExceptionMiddleware que intercepta todas as exceções da API e retorna o ExceptionDetail preenchido. Ao ocorrer uma exceção, é retornado também o HttpStatusCode correto, de acordo com a exceção encontrada. Também, no Bootstrap da API, temos o método ConfigureValidations, que faz com que seja lançada uma exceção nas validações que ocorrem nas requests dos endpoints.

#### Exemplo de DTOs do tipo entidade
A seguir, veja os DTOs das entidades Country e State. 

[`Country.cs`](./src/AppProject.Core.Models.General/Country.cs):

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class Country : IEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Code { get; set; }

    public byte[]? RowVersion { get; set; }
}
```

[`State.cs`](./src/AppProject.Core.Models.General/State.cs):

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class State : IEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid CountryId { get; set; }

    public byte[]? RowVersion { get; set; }
}
```

Note que usamos atributos para as principais validações. Nos relacionamentos, usamos apenas o nome dos campos mesmo, ao invés de adicionar também a classe DTO. Isso evita problemas ao converter o DTO para a entidade do banco de dados, como por exemplo, acabar inserindo/alterando um país só porque ele está referenciado numa outra tabela. Claro que, não há problemas em ter outras entidades nos DTOs, inclusive listas, mas desde que o propósito seja inserir/alterar tudo junto. Por exemplo, imagine um DTO de Pedido que tem uma lista de Itens (que é outro DTO). Nesse caso, ao inserir/alterar o DTO de Pedido, ele vai manipular também os itens, porque estão no DTO de Pedido. Avalie sempre qual o melhor cenário para cada caso. Pode ser que em cadastros gigantes, com muitos DTOs filhos, talvez seja melhor inserir eles separadamente, para não trafegar todos os dados de uma vez (mas claro, não é uma regra e isso se aplica a casos muito grandes).

### DTOs de summaries
Um DTO do tipo summary é utilizado para listar dados que estão cadastrados. Por exemplo, imagine que você tem uma página que lista os países cadastrados e uma tela para inserir/alterar um desses registros cadastrados. A página que lista, vai utilizar uma lista de DTOs do tipo summary, enquanto que a página que efetivamente cadastra (insere/altera), vai utilizar um DTO do tipo entidade que citamos antes.

Também podemos utilizar DTOs do tipo summary em componentes na tela que pesquisam registros. Por exemplo, imagine que na tela de cadastro de estado, você precisa selecionar um país em um componente ComboBox. Esse componente ComboBox vai usar DTOs do tipo summary. 

Esses DTOs são importantes, porque normalmente eles reduzem a quantidade de dados que trafega. Por exemplo, imagine que página que lista os cadastros de países mostrem apenas o ID e o nome do país, não precisando de RowVersion ou do código dele. Ao usar um DTO do tipo summary, você retorna apenas os campos que realmente precisa.

Os DTOs do tipo summary ficam nos mesmos projetos que os DTOs do tipo entidade (eles ficam juntos).

Importante: não há validações nos DTOs do tipo summary (visto que eles não são usados para fazer insert/update). Por isso, não coloque DataAnnotations neles.

#### Exemplo de DTOs do tipo summary
A seguir, veja os DTOs dos summaries CountrySummary e StateSummary. 

[`CountrySummary.cs`](./src/AppProject.Core.Models.General/CountrySummary.cs):

```csharp
using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class CountrySummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
```

[`StateSummary.cs`](./src/AppProject.Core.Models.General/StateSummary.cs):

```csharp
using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class StateSummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string CountryName { get; set; } = string.Empty;

    public Guid CountryId { get; set; }
}
```

Perceba que o Summary herda de ISummary. Note também que, no caso do StateSummary, nós trouxemos o CountryId e, ao invés de carregarmos o CountrySummary, trouxemos o CountryName, que é o campo que vamos utilizar. No entanto, não há objeções para que nos Summaries tragamos outros summaries aninhados, tomando os devidos cuidados para que apenas não tenha referências circulares infinitas entre eles.

## Adicionando as entidades de banco
No projeto, utilizamos o EF (no estilo code first) para podermos manipular um banco de dados SQL Server. 

As entidades de banco são adicionadas no projeto AppProject.Core.Infrastructure.Database, dentro da pasta Entities. Nesta pasta Entities, nós adicionamos uma subpasta com o nome do módulo daquela tabela.

Os arquivos de entidades (.cs) que representam as tabelas de banco começam com a sigla Tb (de Table). 

Veja a seguir os arquivos TbCountry e TbCity.

[`TbCountry.cs`](./src/AppProject.Core.Infrastructure.Database/Entities/General/TbCountry.cs):

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.General;

[Table("Countries")]
public class TbCountry : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Code { get; set; }

    public ICollection<TbState> States { get; set; } = new List<TbState>();
}
```

[`TbState.cs`](./src/AppProject.Core.Infrastructure.Database/Entities/General/TbState.cs):

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.General;

[Table("States")]
public class TbState : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid CountryId { get; set; }

    [ForeignKey(nameof(CountryId))]
    public TbCountry Country { get; set; } = default!;

    public ICollection<TbCity> Cities { get; set; } = new List<TbCity>();
}
```

No atributo Table, nós colocamos o nome da tabela no plural, sem o prefixo Tb.

Perceba que, para as principais validações do banco, nós utilizamos os atributos DataAnnotations (como Required, ForeignKey e Key). Em campos de texto, colocamos o MaxLength com um valor que imaginamos ser aplausível. Deixe sem o MaxLength apenas em campos que realmente não tem limite.

Para chaves estrangeiras, nós adicionamos o campo que representa o nome da chave (exemplo CountryId no TbState.cs). Nesse campo, nós colocamos o atributo Required (quando ele é obrigatório). Também adicionamos uma propriedade do tipo da classe que representa o relacionamento (exemplo TbCountry no arquivo TbState.cs). Nessa propriedade, nós colocamos o atributo ForeignKey apontando para o nome do campo chave estrangeira. Por último, na TbCountry.cs nós adicionamos uma ICollection representando os registros da tabela TbState que foram adicionados, concluindo assim o relacionamento entre as tabelas.

### Adicionando os arquivos EntityTypeConfiguration
Para podermos adicionar mais configurações das tabelas, como índices, nós usamos classes que ficam no projeto AppProject.Core.Infrastructure.Database, dentro da pasta EntityTypeConfiguration. Nesta pasta, nós temos as subpastas com o nome dos módulos. E dentro de cada subpasta, temos os arquivos com o nome no formato Table + Configuration (por exemplo TbCountryConfiguration e TbStateConfiguration).

Veja os arquivos TbCountryConfiguration e TbStateConfiguration:

[`TbCountryConfiguration.cs`](./src/AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/General/TbCountryConfiguration.cs):

```csharp
using System;
using AppProject.Core.Infrastructure.Database.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.General;

public class TbCountryConfiguration : IEntityTypeConfiguration<TbCountry>
{
    public void Configure(EntityTypeBuilder<TbCountry> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
```

[`TbStateConfiguration.cs`](./src/AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/General/TbStateConfiguration.cs):

```csharp
using System;
using AppProject.Core.Infrastructure.Database.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.General;

public class TbStateConfiguration : IEntityTypeConfiguration<TbState>
{
    public void Configure(EntityTypeBuilder<TbState> builder)
    {
        builder.HasIndex(x => x.Name);
    }
}
```

Note que, esses arquivos herdam de IEntityTypeConfiguration. Dentro do método Configure, temos as configurações adicionais daquela tabela. As validações, podem ser colocadas como DataAnnotations nas classes que representam as tabelas. Mas outras validações podem ser colocadas nesses arquivos de configurações.

Importante: ao adicionar um novo arquivo herdando de IEntityTypeConfiguration, não precisamos adicionar código na classe ApplicationDbContext, pois o método OnModelCreating dessa classe já lê todos os arquivos que herdam de IEntityTypeConfiguration do assembly corrente.

### Adicionando DbSet no ApplicationDbContext
No arquivo [`ApplicationDbContext`](./src/AppProject.Core.Infrastructure.Database/ApplicationDbContext.cs), colocamos um DbSet para cada tabela que adicionamos, como no trecho a seguir:

```csharp
    public DbSet<TbCountry> Countries { get; set; }

    public DbSet<TbState> States { get; set; }
```

O nome da propriedade vai ser o mesmo nome da tabela (que no caso deve ser no plural e sem o Tb).

### Rodando o migration
Para que possamos criar os scripts do banco, nós precisamos rodar o migration do Entity Framework. Para isso, abra o terminal na pasta src do projeto e execute o comando a seguir:

```bash
dotnet ef migrations add MigrationName --project AppProject.Core.Infrastructure.Database --startup-project AppProject.Core.API --output-dir Migrations
```

No lugar de MigrationName, dê um nome que identifique o seu migration, como talvez o nome de uma das tabelas que você está modificando ou algum assunto que remeta à alteração.

Esse comando fará com que, na pasta Migrations do projeto AppProject.Core.Infrastructure.Database, contenha os arquivos de script de migração.

Importante: não precisa aplicar o migration, pois ele já é aplicado automaticamente quando a API sobe.

## Criando as interfaces e classes de serviço
As classes de serviço servem para implementarmos as regras de negócio e manipularmos o banco de dados. Toda classe de serviço tem uma interface associada a ela.

As classes de serviço ficam nos projetos abaixo:
* AppProject.Core.Services (para serviços que são compartilhados entre os módulos [nesse caso, não esqueça de criar uma pasta, caso não exista, com o nome do módulo, para que o arquivo fique dentro dela (exemplo: pasta General no projeto AppProject.Core.Services significa que o conteúdo dentro dela pertence ao módulo General, porém está no projeto compartilhado entre todos os módulos)]);
* AppProject.Core.Services.General (para serviços do módulo General);
* AppProject.Core.Services.ModuleName (para serviços de outros módulos, deve-se ter um projeto específico onde ModuleName é o nome do módulo).

As interfaces e classes de serviços tem normalmente a estrutura a seguir.

### Interface da classe de serviço
Uma interface de uma classe de serviço implementa uma outra interface que diz como que ela será registrada no DI. Essas são as opções:
* IScopedService (será registrada como scoped);
* ITransientService (será registrada como transient);
* ISingletonService (será registrada como singleton).

Automaticamente, todas as classes e interfaces de serviços são colocadas na DI automaticamente.

O nome de toda interface começa com a letra I, de interface.

Sendo o objetivo da classe de serviço fazer um CRUD no banco de dados, a interface poderá fazer também as seguintes implementações:
* IGetEntity<GetByIdRequest<Guid>, EntityResponse<Country>>: Isso fará com que tenha um método para trazer uma entidade. O parâmetro do método será uma request que contém como propriedade um Id do tipo informado (que nesse exemplo é Guid). A resposta será uma classe que contém o DTO da entidade informada, que nesse caso será Country;
* IGetEntities<GetByParentIdRequest<Guid>, EntitiesResponse<CountryLanguage>>: Esse método não tem em todas as circunstâncias de CRUD. Ele serve normalmente para lidar com entidades que estão agregadas. Por exemplo, diríamos que tenhamos uma entidade chamada CountryLanguage. Essa entidade permite adicionar vários idiomas à uma entidade Country. Eu quero que tenha um método que retorne todos os idiomas de acordo com o Id do país. Então, eu posso usar esse IGetEntities para ter um método que retorne essas entidades. O parâmetro será uma request que contém o ParentId (que é o Id do pai, sendo nesse caso o Id do Country). Também informamos o tipo desse ParentId (que nesse caso é um Guid). O retorno será uma classe que tenha como propriedade uma coleção (IReadOnlyCollection) da entidade (que nesse caso será a CountryLanguage);
* IPostEntity<CreateOrUpdateRequest<Country>, KeyResponse<Guid>>: Isso fará com que tenha um método para inserir um novo registro de entidade. O parâmetro será uma request que contém como propriedade uma instância do DTO da entidade que será inserido (que nesse caso é Country). A resposta será uma classe que contém o Id do tipo informado (que nesse caso é Guid) com o valor do Id do registro que foi inserido no banco de dados.
* IPutEntity<CreateOrUpdateRequest<Country>, KeyResponse<Guid>>: Isso fará com que tenha um método para atualizar um registro já existente. O parâmetro será uma request que contém como propriedade uma instância do DTO da entidade que será inserido (que nesse caso é Country). A resposta será uma classe que contém o Id do tipo informado (que nesse caso é Guid) com o valor do Id do registro que foi alterado no banco de dados.
* IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>: Isso fará cm que tenha um método para deletar um registro do banco de dados. O parâmetro será uma request contendo como propriedade o Id do tipo especificado (que nesse caso é Guid). Esse é o Id que será utilizado para deletar o registro. A resposta será uma classe EmptyResponse, que não tem nenhuma propriedade dentro.

Veja nos exemplos a seguir, duas interfaces de CRUD, uma para a entidade Country e outra para State.

[`ICountryService.cs`](./src/AppProject.Core.Services.General/ICountryService.cs):

```csharp
using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface ICountryService
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<Country>>,
    IPostEntity<CreateOrUpdateRequest<Country>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<Country>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
}
```

[`IStateService.cs`](./src/AppProject.Core.Services.General/IStateService.cs):

```csharp
using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface IStateService
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<State>>,
    IPostEntity<CreateOrUpdateRequest<State>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<State>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
}
```

No caso das interfaces de serviços para retornar summaries, nós fazemos a implementação conforme abaixo:
* IGetSummaries<SearchRequest, SummariesResponse<CountrySummary>>: Isso fará com que tenha um método para trazer uma coleção de summaries. O parâmetro do método será uma SearchRequest que contém algumas propriedades básicas de uma pesquisa: Take e SearchText. A propriedade Take contém quantos registros deseja que sejam trazidos na consulta. Se o valor de Take for nulo, todos os registros serão trazidos. No caso do SearchText, temos o valor de algo que o usuário digitou na pesquisa. Poderemos usar essa propriedade para filtrar um ou mais campos da tabela para encontrar os registros. Caso essa propriedade seja nula, também serão trazidos todos os registros do banco. Embora estejamos usando uma classe SearchRequest, poderá também ser criado outras classes que herdem de SearchRequest para serem usadas aqui. Por exemplo, imagine que na página, seja possível também filtrar por datas ou outros campos da tabela. Nesses casos, poderá ser criado uma nova classe herdando da SearchRequest e tendo esses campos de pesquisa como propriedades. Nesse IGetSummaries, temos como resposta uma classe que contém uma coleção (IReadOnlyCollection) de DTOs do summary especificado (que nesse caso será CountrySummary);
* IGetSummary<GetByIdRequest<Guid>, SummaryResponse<CountrySummary>>: Com essa implementação, teremos um método que retorna apenas um summary, encontrado através do Id. Assim, o parâmetro desse método será um GetByIdRequest do tipo do Id (que nesse caso é Guid). O retorno será uma classe SummaryResponse que contém o summary encontrado, que nesse caso será CountrySummary. O objetivo dessa implementação é proporcionar consultas que retornem apenas um registro.

A seguir, veja as interfaces ICountrySummaryService e IStateSummaryService.

[`ICountrySummaryService.cs`](./src/AppProject.Core.Services.General/ICountrySummaryService.cs):

```csharp
using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface ICountrySummaryService
    : ITransientService,
    IGetSummaries<SearchRequest, SummariesResponse<CountrySummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<CountrySummary>>
{
}
```

[`IStateSummaryService.cs`](./src/AppProject.Core.Services.General/IStateSummaryService.cs):

```csharp
using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface IStateSummaryService
    : ITransientService,
    IGetSummaries<SearchRequest, SummariesResponse<StateSummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<StateSummary>>
{
}
```

### Classes de serviço
As classes de serviços herdam da classe base chamada BaseService e implementam a interface que leva o seu nome. A herança da BaseService é útil caso, em algum momento, precise implementar algum código que se aplique a todos os serviços. 

Dentro das classes de serviços, nós vamos ter as implementações dos métodos que foram colocados na interface. Elas podem ter quaisquer métodos que sejam úteis em validações de regras de negócios, envio de e-mails, consultas a outros mecanismos (como IA), execuções de Jobs ou acesso ao banco de dados para fazer CRUDs.

Veja a seguir, como fica a classe SummaryService e StateService, que fazem um CRUD no banco de dados.

[`CountryService.cs`](./src/AppProject.Core.Services.General/CountryService.cs):

```csharp
using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Core.Services.Auth;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;
using Mapster;

namespace AppProject.Core.Services.General;

public class CountryService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, ICountryService
{
    public async Task<EntityResponse<Country>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var tbCountry = await databaseRepository.GetFirstOrDefaultAsync<TbCountry>(
            query => query.Where(x => x.Id == request.Id), cancellationToken);

        if (tbCountry == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<Country>
        {
            Entity = tbCountry.Adapt<Country>()
        };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<Country> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateCountryAsync(request.Entity, cancellationToken);

        var tbCountry = request.Entity.Adapt<TbCountry>();
        await databaseRepository.InsertAndSaveAsync(tbCountry, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbCountry.Id
        };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<Country> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateCountryAsync(request.Entity, cancellationToken);

        var tbCountry = request.Entity.Adapt<TbCountry>();
        await databaseRepository.UpdateAndSaveAsync(tbCountry, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbCountry.Id
        };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var tbCountry = await databaseRepository.GetFirstOrDefaultAsync<TbCountry>(
            query => query.Where(x => x.Id == request.Id), cancellationToken);

        if (tbCountry == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await databaseRepository.DeleteAndSaveAsync(tbCountry, cancellationToken);

        return new EmptyResponse();
    }

    private async Task ValidateCountryAsync(Country country, CancellationToken cancellationToken = default)
    {
        if (await databaseRepository.HasAnyAsync<TbCountry>(
            query => query.Where(x => x.Name == country.Name && x.Id != country.Id), cancellationToken))
        {
            throw new AppException(ExceptionCode.General_Country_DuplicateName);
        }
    }
}
```

[`StateService.cs`](./src/AppProject.Core.Services.General/StateService.cs):

```csharp

```