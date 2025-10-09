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

Foi criado uma pasta chamada CustomValidators no AppProject.Core.Models. Caso deseje criar atributos específicos para fazer validações, poderá colocar nela. Já existe uma classe chamada ValidateCollectionAttribute, que deverá ser colocado nas propriedades que são coleções. Essa classe faz com que a API valide também cada item da lista. Isso é necessário porque nativamente o .NET não faz essa validação em cascata.

Info: sobre as validações, veja o arquivo ExceptionMiddleware que intercepta todas as exceções da API e retorna o ExceptionDetail preenchido. Ao ocorrer uma exceção, é retornado também o HttpStatusCode correto, de acordo com a exceção encontrada. Também, no Bootstrap da API, temos o método ConfigureValidations, que faz com que seja lançada uma exceção nas validações que ocorrem nas requests dos endpoints.

#### Exemplo de DTOs do tipo entidade
A seguir, veja os DTOs das entidades Country, State, City e Neighborhood. 

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
    public string Name { get; set; } = default!;

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
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid CountryId { get; set; }

    public byte[]? RowVersion { get; set; }
}

```

[`City.cs`](./src/AppProject.Core.Models.General/City.cs):

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Core.Models.CustomValidators;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class City : IEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid StateId { get; set; }

    public byte[]? RowVersion { get; set; }

    [ValidateCollection]
    public IList<CreateOrUpdateRequest<Neighborhood>> ChangedNeighborhoodRequests { get; set; } = new List<CreateOrUpdateRequest<Neighborhood>>();

    [ValidateCollection]
    public IList<DeleteRequest<Guid>> DeletedNeighborhoodRequests { get; set; } = new List<DeleteRequest<Guid>>();
}

```

[`Neighborhood.cs`](./src/AppProject.Core.Models.General/Neighborhood.cs):

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class Neighborhood : IEntity
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    public byte[]? RowVersion { get; set; }
}

```

Note que usamos atributos para as principais validações. Nos relacionamentos, usamos apenas o nome dos campos mesmo, ao invés de adicionar também a classe DTO. Isso evita problemas ao converter o DTO para a entidade do banco de dados, como por exemplo, acabar inserindo/alterando um país só porque ele está referenciado numa outra tabela. Claro que, não há problemas em ter outras entidades nos DTOs, mas desde que o propósito seja inserir/alterar tudo junto. 

Quando houver situações onde temos um DTO pai e DTOs filhos, como por exemplo no caso do DTO de Cidade que também insere, altera e exclui os DTOs de bairros, nós podemos adicionar no DTO pai coleções de requests, como fizemos no DTO chamado City. Perceba que nesse DTO nós temos as seguintes coleções:
* Para inserir ou alterar registros filhos:

```csharp
[ValidateCollection]
public IList<CreateOrUpdateRequest<Neighborhood>> ChangedNeighborhoodRequests { get; set; } = new List<CreateOrUpdateRequest<Neighborhood>>();
```

Adicionamos uma lista da CreateOrUpdateRequest<Neighborhood>, que representa os registros de bairros para inserir ou modificar. Se o Neighborhood tiver um Id preenchido, o registro é para modificar. Caso contrário, será para alterar.

Note também que na classe Neighborhood, nós não colocamos a propriedade CityId, visto que o cadastro de bairros só será feito através do DTO de cidade.

* Para excluir registros filhos:

```csharp
[ValidateCollection]
public IList<DeleteRequest<Guid>> DeletedNeighborhoodRequests { get; set; } = new List<DeleteRequest<Guid>>();
```

Para excluir registros filhos, a lista é uma coleção de DeleteRequest<>. Assim, para deletar os filhos, nós passamos apenas os Ids dos filhos.

Se não houver alterações nos filhos, não é necessário enviar essas duas listas acima preenchidas com cadastros existentes.

### DTOs de summaries
Um DTO do tipo summary é utilizado para listar dados que estão cadastrados. Por exemplo, imagine que você tem uma página que lista os países cadastrados e uma tela para inserir/alterar um desses registros cadastrados. A página que lista, vai utilizar uma lista de DTOs do tipo summary, enquanto que a página que efetivamente cadastra (insere/altera), vai utilizar um DTO do tipo entidade que citamos antes.

Também podemos utilizar DTOs do tipo summary em componentes na tela que pesquisam registros. Por exemplo, imagine que na tela de cadastro de estado, você precisa selecionar um país em um componente ComboBox. Esse componente ComboBox vai usar DTOs do tipo summary. 

Esses DTOs são importantes, porque normalmente eles reduzem a quantidade de dados que trafega. Por exemplo, imagine que página que lista os cadastros de países mostrem apenas o ID e o nome do país, não precisando de RowVersion ou do código dele. Ao usar um DTO do tipo summary, você retorna apenas os campos que realmente precisa.

Os DTOs do tipo summary ficam nos mesmos projetos que os DTOs do tipo entidade (eles ficam juntos).

Importante: não há validações nos DTOs do tipo summary (visto que eles não são usados para fazer insert/update). Por isso, não coloque DataAnnotations neles.

#### Exemplo de DTOs do tipo summary
A seguir, veja os DTOs dos summaries CountrySummary, StateSummary e CitySummary. 

[`CountrySummary.cs`](./src/AppProject.Core.Models/General/CountrySummary.cs):

```csharp
using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class CountrySummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;
}

```

[`StateSummary.cs`](./src/AppProject.Core.Models/General/StateSummary.cs):

```csharp
using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class StateSummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string CountryName { get; set; } = default!;

    public Guid CountryId { get; set; }
}

```

[`CitySummary.cs`](./src/AppProject.Core.Models/General/CitySummary.cs):

```csharp
using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class CitySummary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string StateName { get; set; } = default!;

    public Guid StateId { get; set; }

    public string CountryName { get; set; } = default!;

    public Guid CountryId { get; set; }
}

```

Perceba que o Summary herda de ISummary. Note também que, no caso do StateSummary, nós trouxemos o CountryId e, ao invés de carregarmos o CountrySummary, trouxemos o CountryName, que é o campo que vamos utilizar. No entanto, não há objeções para que nos Summaries tragamos outros summaries aninhados, apenas tomando os devidos cuidados para que não tenha referências circulares infinitas entre eles.

No caso do StateSummary e do CitySummary, nós também criamos duas Search Requests que herdam da classe SearchRequest. Fizemos isso para podermos adicionar mais opções de filtros para essas pesquisas. Veja o código dessas classes abaixo:

[`StateSearchRequest.cs`](./src/AppProject.Core.Models/General/StateSearchRequest.cs):

```csharp
using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class StateSearchRequest : SearchRequest
{
    public Guid? CountryId { get; set; }
}

```

[`CitySearchRequest.cs`](./src/AppProject.Core.Models/General/CitySearchRequest.cs):

```csharp
using System;
using AppProject.Models;

namespace AppProject.Core.Models.General;

public class CitySearchRequest : SearchRequest
{
    public Guid? StateId { get; set; }
}

```

## Adicionando as entidades de banco
No projeto, utilizamos o EF (no estilo code first) para podermos manipular um banco de dados SQL Server. 

As entidades de banco são adicionadas no projeto AppProject.Core.Infrastructure.Database, dentro da pasta Entities. Nesta pasta Entities, nós adicionamos uma subpasta com o nome do módulo daquela tabela.

Os arquivos de entidades (.cs) que representam as tabelas de banco começam com a sigla Tb (de Table). 

Veja a seguir os arquivos TbCountry, TbState, TbCity e TbNeighborhood.

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
    public string Name { get; set; } = default!;

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
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid CountryId { get; set; }

    [ForeignKey(nameof(CountryId))]
    public TbCountry Country { get; set; } = default!;

    public ICollection<TbCity> Cities { get; set; } = new List<TbCity>();
}

```

[`TbCity.cs`](./src/AppProject.Core.Infrastructure.Database/Entities/General/TbCity.cs):

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.General;

[Table("Cities")]
public class TbCity : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid StateId { get; set; }

    [ForeignKey(nameof(StateId))]
    public TbState State { get; set; } = default!;

    public ICollection<TbNeighborhood> Neighborhoods { get; set; } = new List<TbNeighborhood>();
}

```

[`TbNeighborhood.cs`](./src/AppProject.Core.Infrastructure.Database/Entities/General/TbNeighborhood.cs):

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.General;

[Table("Neighborhoods")]
public class TbNeighborhood : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Code { get; set; }

    [Required]
    public Guid CityId { get; set; }

    [ForeignKey(nameof(CityId))]
    public TbCity City { get; set; } = default!;
}

```

No atributo Table, nós colocamos o nome da tabela no plural, sem o prefixo Tb.

Perceba que, para as principais validações do banco, nós utilizamos os atributos DataAnnotations (como Required, ForeignKey e Key). Em campos de texto, colocamos o MaxLength com um valor que imaginamos ser aplausível. Deixe sem o MaxLength apenas em campos que realmente não tem limite.

Para chaves estrangeiras, nós adicionamos o campo que representa o nome da chave (exemplo CountryId no TbState.cs). Nesse campo, nós colocamos o atributo Required (quando ele é obrigatório). Também adicionamos uma propriedade do tipo da classe que representa o relacionamento (exemplo TbCountry no arquivo TbState.cs). Nessa propriedade, nós colocamos o atributo ForeignKey apontando para o nome do campo chave estrangeira. Por último, na TbCountry.cs nós adicionamos uma ICollection representando os registros da tabela TbState que foram adicionados, concluindo assim o relacionamento entre as tabelas.

### Adicionando os arquivos EntityTypeConfiguration
Para podermos adicionar mais configurações das tabelas, como índices, nós usamos classes que ficam no projeto AppProject.Core.Infrastructure.Database, dentro da pasta EntityTypeConfiguration. Nesta pasta, nós temos as subpastas com o nome dos módulos. E dentro de cada subpasta, temos os arquivos com o nome no formato Table + Configuration (por exemplo TbCountryConfiguration e TbStateConfiguration).

Veja os arquivos TbCountryConfiguration, TbStateConfiguration e TbCityConfiguration:

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
[`TbCityConfiguration.cs`](./src/AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/General/TbCityConfiguration.cs):

```csharp
using System;
using AppProject.Core.Infrastructure.Database.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.General;

public class TbCityConfiguration : IEntityTypeConfiguration<TbCity>
{
    public void Configure(EntityTypeBuilder<TbCity> builder)
    {
        builder.HasIndex(x => x.Name);
    }
}

```
[`TbNeighborhoodConfiguration.cs`](./src/AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/General/TbNeighborhoodConfiguration.cs):

```csharp
using System;
using AppProject.Core.Infrastructure.Database.Entities.General;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.General;

public class TbNeighborhoodConfiguration : IEntityTypeConfiguration<TbNeighborhood>
{
    public void Configure(EntityTypeBuilder<TbNeighborhood> builder)
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

    public DbSet<TbCity> Cities { get; set; }

    public DbSet<TbNeighborhood> Neighborhoods { get; set; }
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

## Adicionando as classes de configuração do Mapster
Normalmente, as classes de DTOs que representam uma tabela no banco de dados, não precisam de um arquivo de configuração do Mapster para fazer o mapeamento entre as propriedades da classe de tabela e a classe do DTO. Nessas situações, quando é feito o mapeamento entre elas, o Mapster faz o mapeamento entre elas através do nome das propriedades idênticas.

Mas há casos em que desejamos dizer que determinada propriedade no DTO referece a uma propriedade de outro nome na classe do banco. Isso pode acontecer muito nos casos dos DTOs de summaries. Por exemplo, no DTO StateSummary, nós temos a propriedade `StateName`. Essa propriedade refere-se ao campo `Name` da classe `TbCountry`. Então, ao fazermos uma consulta no banco de dados na tabela `TbState`, nós desejamos trazer também o campo `Name` da tabela `TbCountry` que está vinculado à tabela `TbState`. Para que esse mapeamento seja automático, nós precisamos ir no projeto AppProject.Core.Infrastructure.Database, dentro da pasta Mapper e dentro da subpasta que representa o módulo (nesse caso General), e criarmos um arquivo cujo nome é [NomeDoDTO]MapsterConfig.cs. Essa classe deverá herdar de `IRegisterMapsterConfig` e dentro do método `Register` deverá se fazer a configuração entre as tabelas.

Veja no exemplo a seguir, as configurações dos DTOs CitySummary e StateSummary.

[`StateSummaryMapsterConfig.cs`](./src/AppProject.Core.Infrastructure.Database/Mapper/General/StateSummaryMapsterConfig.cs):

```csharp
using System;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using Mapster;

namespace AppProject.Core.Infrastructure.Database.Mapper.General;

public class StateSummaryMapsterConfig : IRegisterMapsterConfig
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TbState, StateSummary>()
            .Map(dest => dest.CountryName, src => src.Country.Name);
    }
}
```

[`CitySummaryMapsterConfig.cs`](./src/AppProject.Core.Infrastructure.Database/Mapper/General/CitySummaryMapsterConfig.cs):

```csharp
using System;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using Mapster;

namespace AppProject.Core.Infrastructure.Database.Mapper.General;

public class CitySummaryMapsterConfig : IRegisterMapsterConfig
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<TbCity, CitySummary>()
            .Map(dest => dest.StateName, src => src.State.Name)
            .Map(dest => dest.CountryName, src => src.State.Country.Name);
    }
}
```

Uma vez que essas classes fazem a herança de `IRegisterMapsterConfig`, elas já são automaticamente colocadas na configuração do Mapster para serem usadas.

## Criando as interfaces e classes de serviço
As classes de serviço servem para implementarmos as regras de negócio e manipularmos o banco de dados. Toda classe de serviço tem uma interface associada a ela.

As classes de serviço ficam nos projetos abaixo:
* AppProject.Core.Services (para serviços que são compartilhados entre os módulos [nesse caso, não esqueça de criar uma pasta, caso não exista, com o nome do módulo, para que o arquivo fique dentro dela (exemplo: pasta General no projeto AppProject.Core.Services significa que o conteúdo dentro dela pertence ao módulo General, porém está no projeto compartilhado entre todos os módulos)]);
* AppProject.Core.Services.General (para serviços do módulo General);
* AppProject.Core.Services.ModuleName (para serviços de outros módulos, deve-se ter um projeto específico onde ModuleName é o nome do módulo).

As interfaces e classes de serviços tem normalmente a estrutura a seguir.

### Interface da classe de serviço
Uma interface de uma classe de serviço implementa uma outra interface que diz como que ela será registrada no DI. As opções são:
* IScopedService (será registrada como scoped);
* ITransientService (será registrada como transient);
* ISingletonService (será registrada como singleton).

Automaticamente, todas as classes e interfaces de serviços são colocadas na DI automaticamente.

O nome de toda interface começa com a letra I, de interface.

Sendo o objetivo da classe de serviço fazer um CRUD no banco de dados, a interface poderá fazer também as seguintes implementações:
* IGetEntity<GetByIdRequest<Guid>, EntityResponse<Country>>: Isso fará com que tenha um método para trazer uma entidade. O parâmetro do método será uma request que contém como propriedade um Id do tipo informado (que nesse exemplo é Guid). A resposta será uma classe que contém o DTO da entidade informada, que nesse caso será Country;
* IPostEntity<CreateOrUpdateRequest<Country>, KeyResponse<Guid>>: Isso fará com que tenha um método para inserir um novo registro de entidade. O parâmetro será uma request que contém como propriedade uma instância do DTO da entidade que será inserido (que nesse caso é Country). A resposta será uma classe que contém o Id do tipo informado (que nesse caso é Guid) com o valor do Id do registro que foi inserido no banco de dados.
* IPutEntity<CreateOrUpdateRequest<Country>, KeyResponse<Guid>>: Isso fará com que tenha um método para atualizar um registro já existente. O parâmetro será uma request que contém como propriedade uma instância do DTO da entidade que será inserido (que nesse caso é Country). A resposta será uma classe que contém o Id do tipo informado (que nesse caso é Guid) com o valor do Id do registro que foi alterado no banco de dados.
* IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>: Isso fará com que tenha um método para deletar um registro do banco de dados. O parâmetro será uma request contendo como propriedade o Id do tipo especificado (que nesse caso é Guid). Esse é o Id que será utilizado para deletar o registro. A resposta será uma classe EmptyResponse, que não tem nenhuma propriedade dentro.

Em casos onde há outras entidades agregadas que são pesquisadas pela entidade pai (exemplo: os bairros de uma cidade), poderá se adicionar um método para trazer essas entidades, similar ao exemplo abaixo. O parâmetro deste método será uma request que contém o ParentId (que é o Id do pai, sendo nesse caso o Id da City). Também informamos o tipo desse ParentId (que nesse caso é um Guid). O retorno será uma classe que tenha como propriedade uma coleção (IReadOnlyCollection) da entidade (que nesse caso será a Neighborhood).

```csharp
Task<EntitiesResponse<Neighborhood>> GetNeighborhoodEntitiesAsync(GetByParentIdRequest<Guid> request, CancellationToken cancellationToken = default);
```

Continuando com o nosso exemplo de CRUD, veja nos códigos a seguir as interfaces de CRUD para as entidades Country, State e City.

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

[`ICityService.cs`](./src/AppProject.Core.Services.General/ICityService.cs):

```csharp
using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface ICityService
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<City>>,
    IPostEntity<CreateOrUpdateRequest<City>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<City>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
    Task<EntitiesResponse<Neighborhood>> GetNeighborhoodEntitiesAsync(GetByParentIdRequest<Guid> request, CancellationToken cancellationToken = default);
}
```

No caso das interfaces de serviços para retornar summaries, nós fazemos a implementação conforme abaixo:
* IGetSummaries<SearchRequest, SummariesResponse<CountrySummary>>: Isso fará com que tenha um método para trazer uma coleção de summaries. O parâmetro do método será uma SearchRequest que contém algumas propriedades básicas de uma pesquisa: Take e SearchText. A propriedade Take contém quantos registros deseja que sejam trazidos na consulta. Se o valor de Take for nulo, todos os registros serão trazidos. No caso do SearchText, temos o valor de algo que o usuário digitou na pesquisa. Poderemos usar essa propriedade para filtrar um ou mais campos da tabela para encontrar os registros. Caso essa propriedade seja nula, também serão trazidos todos os registros do banco. Embora estejamos usando uma classe SearchRequest, poderá também ser criado outras classes que herdem de SearchRequest para serem usadas aqui. Por exemplo, imagine que numa página seja possível também filtrar por datas ou outros campos da tabela. Nesses casos, poderá ser criado uma nova classe herdando da SearchRequest e tendo esses campos de pesquisa como propriedades. Nesse IGetSummaries, temos como resposta uma SummariesResponse, que é uma classe que contém uma coleção (IReadOnlyCollection) de DTOs do summary especificado (que nesse caso será CountrySummary);
* IGetSummary<GetByIdRequest<Guid>, SummaryResponse<CountrySummary>>: Com essa implementação, teremos um método que retorna apenas um summary, encontrado através do Id. Assim, o parâmetro desse método será um GetByIdRequest do tipo do Id (que nesse caso é Guid). O retorno será uma classe SummaryResponse que contém o summary encontrado, que nesse caso será CountrySummary. O objetivo dessa implementação é proporcionar consultas que retornem apenas um registro.

A seguir, veja as interfaces ICountrySummaryService, IStateSummaryService e ICitySummaryService.

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
    IGetSummaries<StateSearchRequest, SummariesResponse<StateSummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<StateSummary>>
{
}
```

[`ICitySummaryService.cs`](./src/AppProject.Core.Services.General/ICitySummaryService.cs):

```csharp
using System;
using AppProject.Core.Models.General;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public interface ICitySummaryService
    : ITransientService,
    IGetSummaries<CitySearchRequest, SummariesResponse<CitySummary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<CitySummary>>
{
}
```

### Classes de serviço
As classes de serviços herdam da classe base chamada BaseService e implementam a interface que leva o seu nome. A herança da BaseService é útil caso, em algum momento, precise implementar algum código que se aplique a todos os serviços. 

Dentro das classes de serviços, nós vamos ter as implementações dos métodos que foram colocados na interface. Elas podem ter quaisquer métodos que sejam úteis em validações de regras de negócios, envio de e-mails, consultas a outros mecanismos (como IA), execuções de Jobs ou acesso ao banco de dados para fazer CRUDs.

Veja a seguir, como fica a classe CountryService, StateService e CityService, que fazem operações de CRUD no banco de dados.

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

        var country = await databaseRepository.GetFirstOrDefaultAsync<TbCountry, Country>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (country == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<Country>
        {
            Entity = country
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

        var tbCountry = await databaseRepository.GetFirstOrDefaultAsync<TbCountry>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (tbCountry == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        request.Entity.Adapt(tbCountry);

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
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

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
            query => query.Where(x => x.Name == country.Name && x.Id != country.Id),
            cancellationToken))
        {
            throw new AppException(ExceptionCode.General_Country_DuplicateName);
        }
    }
}
```

[`StateService.cs`](./src/AppProject.Core.Services.General/StateService.cs):

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

public class StateService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, IStateService
{
    public async Task<EntityResponse<State>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var state = await databaseRepository.GetFirstOrDefaultAsync<TbState, State>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (state == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<State>
        {
            Entity = state
        };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<State> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateStateAsync(request.Entity, cancellationToken);

        var tbState = request.Entity.Adapt<TbState>();
        await databaseRepository.InsertAndSaveAsync(tbState, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbState.Id
        };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<State> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateStateAsync(request.Entity, cancellationToken);

        var tbState = await databaseRepository.GetFirstOrDefaultAsync<TbState>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (tbState == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        request.Entity.Adapt(tbState);

        await databaseRepository.UpdateAndSaveAsync(tbState, cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbState.Id
        };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var tbState = await databaseRepository.GetFirstOrDefaultAsync<TbState>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (tbState == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await databaseRepository.DeleteAndSaveAsync(tbState, cancellationToken);

        return new EmptyResponse();
    }

    private async Task ValidateStateAsync(State state, CancellationToken cancellationToken = default)
    {
        if (await databaseRepository.HasAnyAsync<TbState>(
            query => query.Where(x =>
                x.CountryId == state.CountryId
                && x.Name == state.Name
                && x.Id != state.Id), cancellationToken))
        {
            throw new AppException(ExceptionCode.General_State_DuplicateName);
        }
    }
}
```

[`CityService.cs`](./src/AppProject.Core.Services.General/CityService.cs):

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

public class CityService(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, ICityService
{
    public async Task<EntityResponse<City>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var city = await databaseRepository.GetFirstOrDefaultAsync<TbCity, City>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (city == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<City>
        {
            Entity = city
        };
    }

    public async Task<EntitiesResponse<Neighborhood>> GetNeighborhoodEntitiesAsync(GetByParentIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var neighborhood = await databaseRepository.GetByConditionAsync<TbNeighborhood, Neighborhood>(
            query => query.Where(x => x.CityId == request.ParentId),
            cancellationToken);

        if (neighborhood == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntitiesResponse<Neighborhood>
        {
            Entities = neighborhood
        };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<City> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateCityAsync(request.Entity, cancellationToken);

        var tbCity = request.Entity.Adapt<TbCity>();
        await databaseRepository.InsertAsync(tbCity, cancellationToken);

        foreach (var neighborhood in request.Entity.ChangedNeighborhoodRequests)
        {
            var tbNeighborhood = neighborhood.Entity.Adapt<TbNeighborhood>();
            tbNeighborhood.CityId = tbCity.Id;
            await databaseRepository.InsertAsync(tbNeighborhood, cancellationToken);
        }

        await databaseRepository.SaveAsync(cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbCity.Id
        };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<City> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);
        await this.ValidateCityAsync(request.Entity, cancellationToken);

        var tbCity = await databaseRepository.GetFirstOrDefaultAsync<TbCity>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (tbCity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        request.Entity.Adapt(tbCity);

        await databaseRepository.UpdateAsync(tbCity, cancellationToken);

        foreach (var neighborhood in request.Entity.ChangedNeighborhoodRequests)
        {
            var tbNeighborhood = await databaseRepository.GetFirstOrDefaultAsync<TbNeighborhood>(
                query => query.Where(x => x.Id == neighborhood.Entity.Id),
                cancellationToken);

            if (tbNeighborhood == null)
            {
                tbNeighborhood = neighborhood.Entity.Adapt<TbNeighborhood>();
                tbNeighborhood.CityId = tbCity.Id;
                await databaseRepository.InsertAsync(tbNeighborhood, cancellationToken);
            }
            else
            {
                neighborhood.Entity.Adapt(tbNeighborhood);
                await databaseRepository.UpdateAsync(tbNeighborhood, cancellationToken);
            }
        }

        foreach (var neighborhood in request.Entity.DeletedNeighborhoodRequests)
        {
            var tbNeighborhood = await databaseRepository.GetFirstOrDefaultAsync<TbNeighborhood>(
                query => query.Where(x => x.Id == neighborhood.Id),
                cancellationToken);

            if (tbNeighborhood != null)
            {
                await databaseRepository.DeleteAsync(tbNeighborhood, cancellationToken);
            }
        }

        await databaseRepository.SaveAsync(cancellationToken);

        return new KeyResponse<Guid>
        {
            Id = tbCity.Id
        };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

        var tbCity = await databaseRepository.GetFirstOrDefaultAsync<TbCity>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (tbCity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await databaseRepository.DeleteAndSaveAsync(tbCity, cancellationToken);

        return new EmptyResponse();
    }

    private async Task ValidateCityAsync(City city, CancellationToken cancellationToken = default)
    {
        await this.ValidateNeighborhoodsBelongToCityAsync(city, cancellationToken);

        if (await databaseRepository.HasAnyAsync<TbCity>(
            query => query.Where(x =>
                x.StateId == city.StateId
                && x.Name == city.Name
                && x.Id != city.Id), cancellationToken))
        {
            throw new AppException(ExceptionCode.General_City_DuplicateName);
        }

        var neighborhoodNames = city.ChangedNeighborhoodRequests.Select(x => x.Entity.Name);
        var neighborhoodIds = city.ChangedNeighborhoodRequests.Select(x => x.Entity.Id);

        if (await databaseRepository.HasAnyAsync<TbNeighborhood>(
            query => query.Where(x =>
                x.CityId == city.Id
                && neighborhoodNames.Contains(x.Name)
                && !neighborhoodIds.Contains(x.Id)),
            cancellationToken))
        {
            throw new AppException(ExceptionCode.General_City_Neighborhood_DuplicateName);
        }

        if (neighborhoodNames.Count() != neighborhoodNames.Distinct().Count())
        {
            throw new AppException(ExceptionCode.General_City_Neighborhood_DuplicateName);
        }
    }

    private async Task ValidateNeighborhoodsBelongToCityAsync(City city, CancellationToken cancellationToken = default)
    {
        var neighborhoodIds = city.ChangedNeighborhoodRequests
            .Where(x => x.Entity.Id != Guid.Empty)
            .Select(x => x.Entity.Id)
            .Union(city.DeletedNeighborhoodRequests.Select(x => x.Id));

        if (neighborhoodIds.Any())
        {
            return;
        }

        if (await databaseRepository.HasAnyAsync<TbNeighborhood>(
            query => query.Where(x =>
                neighborhoodIds.Contains(x.Id)
                && x.CityId != city.Id),
            cancellationToken))
        {
            throw new InvalidOperationException();
        }
    }
}
```

Conforme vimos, todos os métodos do CRUD chamam o `permissionService.ValidateCurrentUserPermissionAsync` para validar se o usuário tem permissão para executar aquela operação. Essa validação analisa se o usuário corrente tem o tipo de permissão informado. Isso é importante para a segurança da aplicação.

No método `GetEntityAsync`, nós consultamos o banco de dados utilizando o método `databaseRepository.GetFirstOrDefaultAsync<TbCity, City>`. Esse método retornará o primeiro registro no banco de dados de acordo com as condições da consulta. Note que esse parâmetro genérico exige a classe que representa o banco de dados (que nesse caso é a TbCity) e a classe que representa o DTO (que nesse caso é a City). Isso fará com que a consulta já seja feita levando em conta os campos desejados no DTO. Há uma sobrecarga desse método onde não é informado o DTO, ficando apenas `databaseRepository.GetFirstOrDefaultAsync<TbCity>`, para situações em que deseje trazer a tabela sem levar em conta o DTO. 

Ainda no método `GetEntityAsync`, nós fazemos uma verificação caso a consulta no banco de dados retorne nulo. Nesses casos, estouramos uma exceção `throw new AppException(ExceptionCode.EntityNotFound);` passando o ExceptionCode EntityNotFound.

No método `PostEntityAsync`, após a análise de segurança, chamamos um método para validar o post da nova entidade. Isso é interessante para fazermos as validações de regras de negócios. Por exemplo, no caso do cadastro de país, o método `ValidateCountryAsync` valida se há algum outro cadastro de país com o mesmo nome. Caso haja, ele estoura uma exceção `throw new AppException(ExceptionCode.General_Country_DuplicateName);`. 

Note que, na maioria das vezes que desejamos estourar uma exceção, nós usamos a `AppException` e passamos um `ExceptionCode`, que é um enum que representa a exceção que desejamos. Ao criar uma exceção, nós também colocamos uma descrição para ela nos arquivos de resources de idiomas que ficam no projeto `AppProject.Resources`. Dentro desse projeto, nós temos os arquivos `Resource.resx` que contém os conteúdos em ingles e o arquivo `Resource.pt-BR.resx` que contém a tradução em português. Para informarmos uma nova resource é bem simples, bastando adicionar o conteúdo como abaixo:

Inglês:

```xml
<data name="General_Country_DuplicateName" xml:space="preserve">
    <value>This country name already exists.</value>
</data>
```

Portugues:

```xml
<data name="General_Country_DuplicateName" xml:space="preserve">
    <value>Esse nome de país já existe.</value>
</data>
```

Ainda falando sobre os métodos de validações, perceba que podemos fazer consultas no banco de dados utilizando o `databaseRepository.HasAnyAsync<TbCountry>` que faz uma consulta e retorna true ou false caso haja ou não registros com as condições passadas para o método. 

Voltando ao método `PostEntityAsync`, note que no caso da classe `CountryService`, nós fazemos uma conversão do DTO para a classe de banco e chamamos o método `InsertAndSaveAsync`:

```csharp
var tbCountry = request.Entity.Adapt<TbCountry>();
await databaseRepository.InsertAndSaveAsync(tbCountry, cancellationToken);
```

O método `InsertAndSaveAsync` fará o insert da nova entidade e automaticamente salvará esses dados no banco, dando um commit na transação.

Perceba que, no caso da classe `CityService`, nós fazemos um pouco diferente. Visto que com o DTO City, é possível inserir dados de cidade e de bairros (tabela `TbNeighborhood`), nós primeiro vamos inserir a cidade e depois percorrer as requests que inserem os bairros, fazendo então a inserção deles e, só no final, que vamos chamar o método `SaveAsync`:

```csharp
var tbCity = request.Entity.Adapt<TbCity>();
await databaseRepository.InsertAsync(tbCity, cancellationToken);

foreach (var neighborhood in request.Entity.ChangedNeighborhoodRequests)
{
    var tbNeighborhood = neighborhood.Entity.Adapt<TbNeighborhood>();
    tbNeighborhood.CityId = tbCity.Id;
    await databaseRepository.InsertAsync(tbNeighborhood, cancellationToken);
}

await databaseRepository.SaveAsync(cancellationToken);
```

Fazer dessa forma fará com os métodos `InsertAsync` apenas coloquem os dados em memória. Apenas ao chamar o método `SaveAsync` é que a transação será comitada no banco. Dando algum erro, todos os inserts feitos serão dados um rollback.

Nos métodos `PutEntityAsync`, após as validações de segurança nós também fazemos as validações da regra de negócio. Nos exemplos que vimos, nós estamos utilizando os mesmos métodos de validações que foram usados nos `PostEntityAsync`. Após fazermos essas validações, nós temos o código que busca a entidade que vamos alterar no banco de dados e faz uma mapeamento nela com o DTO que está vindo. Isso resulta em atualizar os campos da tabela com os campos do DTO. Fazer essa consulta é importante, porque pegará todos os campos da tabela que vamos alterar. Isso evitará colocar nulo nos campos que há na tabela mas não tem no DTO. Veja essa parte no exemplo a seguir do `CountryService`:

```csharp
var tbCountry = await databaseRepository.GetFirstOrDefaultAsync<TbCountry>(
    query => query.Where(x => x.Id == request.Entity.Id),
    cancellationToken);

if (tbCountry == null)
{
    throw new AppException(ExceptionCode.EntityNotFound);
}

request.Entity.Adapt(tbCountry);

await databaseRepository.UpdateAndSaveAsync(tbCountry, cancellationToken);
```

Note que, novamente lançamos uma exceção caso a entidade não seja localizada no banco. Após isso, chamamos o código que lê o DTO e atualiza a entidade com os campos do DTO `request.Entity.Adapt(tbCountry);`. Por último, chamamos o método `UpdateAndSaveAsync`, que fará a alteração e salvará no banco de dados, comitando a transação.

No caso do `CityService`, visto que temos os bairros aninhados, nós precisamos fazer mais alguns passos.

```csharp
var tbCity = await databaseRepository.GetFirstOrDefaultAsync<TbCity>(
    query => query.Where(x => x.Id == request.Entity.Id),
    cancellationToken);

if (tbCity == null)
{
    throw new AppException(ExceptionCode.EntityNotFound);
}

request.Entity.Adapt(tbCity);

await databaseRepository.UpdateAsync(tbCity, cancellationToken);

foreach (var neighborhood in request.Entity.ChangedNeighborhoodRequests)
{
    var tbNeighborhood = await databaseRepository.GetFirstOrDefaultAsync<TbNeighborhood>(
        query => query.Where(x => x.Id == neighborhood.Entity.Id),
        cancellationToken);

    if (tbNeighborhood == null)
    {
        tbNeighborhood = neighborhood.Entity.Adapt<TbNeighborhood>();
        tbNeighborhood.CityId = tbCity.Id;
        await databaseRepository.InsertAsync(tbNeighborhood, cancellationToken);
    }
    else
    {
        neighborhood.Entity.Adapt(tbNeighborhood);
        await databaseRepository.UpdateAsync(tbNeighborhood, cancellationToken);
    }
}

foreach (var neighborhood in request.Entity.DeletedNeighborhoodRequests)
{
    var tbNeighborhood = await databaseRepository.GetFirstOrDefaultAsync<TbNeighborhood>(
        query => query.Where(x => x.Id == neighborhood.Id),
        cancellationToken);

    if (tbNeighborhood != null)
    {
        await databaseRepository.DeleteAsync(tbNeighborhood, cancellationToken);
    }
}

await databaseRepository.SaveAsync(cancellationToken);
```

Perceba que usamos o método `UpdateAsync` para atualizar os dados da cidade e depois, corremos as requests que inserem ou alteram os bairros. Se for para inserir um bairro, chamamos o `InsertAsync` para a tabela `TbNeighborhood` e se for para atualizar, chamamos o `UpdateAsync`. Também corremos as requests que deletam os bairros e persistimos as deleções chamando o método `DeleteAsync`. Apenas no final de lidarmos com tudo de banco de dados é que nós chamamos o `SaveAsync` para persistir as alterações e comitarmos a transação.

Note no caso acima como lidamos com os bairros, fazendo a consulta deles e usando o Adapt para converter as entidades de DTO para a classe de tabela. 

Ainda falando sobre o caso de cidades, note que quando temos registros aninhados, nós precisamos garantir que os IDs dos registros que estão alterados e excluídos pertençam à mesma cidade (registro pai). Assim, no método `ValidateNeighborhoodsBelongToCityAsync`, nós lançamos uma `InvalidOperationException` caso tenha algum bairro cuja cidade não seja a mesma da request que está vindo. 

Agora falando sobre os métodos `DeleteEntityAsync`, note que, após as validações de permissões, nós localizamos o registro no banco de dados de acordo com o Id que veio na request. Caso não seja encontrado, lançamos uma exceção `throw new AppException(ExceptionCode.EntityNotFound);` também do tipo EntityNotFound. Chamamos o método `databaseRepository.DeleteAndSaveAsync` para excluir e comitar a transação no banco. Note que, no caso de cidades, mesmo tendo registros aninhados (que são os bairros), não precisamos deletar cada um dos registros individualmente, porque eles são deletados automaticamente em cascata pelo EF Core. Mas, caso algum registro dependente tenha uma configuração para não deletar em cascata, nós precisaríamos excluir eles manualmente.

Por último, veja também no caso de cidades, que temos o método na service para trazer os registros dos bairros:

```csharp
public async Task<EntitiesResponse<Neighborhood>> GetNeighborhoodEntitiesAsync(GetByParentIdRequest<Guid> request, CancellationToken cancellationToken = default)
{
    await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.System_ManageSettings, cancellationToken: cancellationToken);

    var neighborhood = await databaseRepository.GetByConditionAsync<TbNeighborhood, Neighborhood>(
        query => query.Where(x => x.CityId == request.ParentId),
        cancellationToken);

    return new EntitiesResponse<Neighborhood>
    {
        Entities = neighborhood
    };
}
```

Nesses casos, após validarmos as permissões, nós chamamos o método `databaseRepository.GetByConditionAsync` para trazer os registros de bairros do banco de dados. 

### Classes de serviços de summaries
No caso dos summaries, as classes de serviços delas também herdam da classe base chamada `BaseService`.

Note nos exemplos a seguir, como podemos escrever o código dessas classes.

[`CountrySummaryService.cs`](./src/AppProject.Core.Services/General/CountrySummaryService.cs):

```csharp
using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Exceptions;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public class CountrySummaryService(
    IDatabaseRepository databaseRepository)
    : BaseService, ICountrySummaryService
{
    public async Task<SummariesResponse<CountrySummary>> GetSummariesAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        var searchText = request.SearchText?.Trim();

        var countrySummaries = await databaseRepository.GetByConditionAsync<TbCountry, CountrySummary>(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchText) || x.Name.Contains(searchText) || (x.Code ?? string.Empty).Contains(searchText));
                }

                query = query.OrderBy(x => x.Name);

                if (request.Take.HasValue)
                {
                    query = query.Take(request.Take.Value);
                }

                return query;
            },
            cancellationToken);

        return new SummariesResponse<CountrySummary>
        {
            Summaries = countrySummaries
        };
    }

    public async Task<SummaryResponse<CountrySummary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        var countrySummary = await databaseRepository.GetFirstOrDefaultAsync<TbCountry, CountrySummary>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (countrySummary == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new SummaryResponse<CountrySummary>
        {
            Summary = countrySummary
        };
    }
}
```

[`StateSummaryService.cs`](./src/AppProject.Core.Services/General/StateSummaryService.cs):

```csharp
using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Exceptions;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public class StateSummaryService(
    IDatabaseRepository databaseRepository)
    : BaseService, IStateSummaryService
{
    public async Task<SummariesResponse<StateSummary>> GetSummariesAsync(StateSearchRequest request, CancellationToken cancellationToken = default)
    {
        var searchText = request.SearchText?.Trim();

        var stateSummaries = await databaseRepository.GetByConditionAsync<TbState, StateSummary>(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchText) || x.Name.Contains(searchText) || (x.Code ?? string.Empty).Contains(searchText));
                }

                if (request.CountryId.HasValue)
                {
                    query = query.Where(x => x.CountryId == request.CountryId.Value);
                }

                query = query.OrderBy(x => x.Name);

                if (request.Take.HasValue)
                {
                    query = query.Take(request.Take.Value);
                }

                return query;
            },
            cancellationToken);

        return new SummariesResponse<StateSummary>
        {
            Summaries = stateSummaries
        };
    }

    public async Task<SummaryResponse<StateSummary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        var stateSummary = await databaseRepository.GetFirstOrDefaultAsync<TbState, StateSummary>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (stateSummary == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new SummaryResponse<StateSummary>
        {
            Summary = stateSummary
        };
    }
}
```

[`CitySummaryService.cs`](./src/AppProject.Core.Services/General/CitySummaryService.cs):

```csharp
using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.General;
using AppProject.Core.Models.General;
using AppProject.Exceptions;
using AppProject.Models;

namespace AppProject.Core.Services.General;

public class CitySummaryService(
    IDatabaseRepository databaseRepository)
    : BaseService, ICitySummaryService
{
    public async Task<SummariesResponse<CitySummary>> GetSummariesAsync(CitySearchRequest request, CancellationToken cancellationToken = default)
    {
        var searchText = request.SearchText?.Trim();

        var citySummaries = await databaseRepository.GetByConditionAsync<TbCity, CitySummary>(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchText) || x.Name.Contains(searchText) || (x.Code ?? string.Empty).Contains(searchText));
                }

                if (request.StateId.HasValue)
                {
                    query = query.Where(x => x.StateId == request.StateId.Value);
                }

                query = query.OrderBy(x => x.Name);

                if (request.Take.HasValue)
                {
                    query = query.Take(request.Take.Value);
                }

                return query;
            },
            cancellationToken);

        return new SummariesResponse<CitySummary>
        {
            Summaries = citySummaries
        };
    }

    public async Task<SummaryResponse<CitySummary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        var citySummary = await databaseRepository.GetFirstOrDefaultAsync<TbCity, CitySummary>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (citySummary == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new SummaryResponse<CitySummary>
        {
            Summary = citySummary
        };
    }
}
```

Perceba que, nos métodos `GetSummariesAsync`, nós chamamos o `databaseRepository.GetByConditionAsync` para fazer as consultas no banco de dados, passando a tabela e o DTO, como no exemplo `databaseRepository.GetByConditionAsync<TbCity, CitySummary>`. Note também que, dentro da query, nós colocamos o Take e o SearchText caso eles sejam preenchidos.

```csharp
var citySummaries = await databaseRepository.GetByConditionAsync<TbCity, CitySummary>(
    query =>
    {
        if (request.Take.HasValue)
        {
            query = query.Take(request.Take.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(x =>
                x.Name.Contains(searchText) || (x.Code ?? string.Empty).Contains(searchText));
        }

        return query;
    },
    cancellationToken);
```

Já nos métodos `GetSummaryAsync`, nós localizamos as entidades por Id, como no exemplo `databaseRepository.GetFirstOrDefaultAsync<TbCity, CitySummary>` e, caso não encontre a entidade pelo Id enviado, nós estouramos a exceção `throw new AppException(ExceptionCode.EntityNotFound);`.

## Criando as classes de controller
Os controllers ficam em projetos que levam o nome dos módulos, como `AppProject.Core.Controllers.General`. Caso não tenha um projeto ainda criado para o módulo, deverá ser criado um. 

Seguindo as boas práticas, o nome do controller deverá ser o nome do DTO + o sufixo Controller. Todo controller deverá ter apenas o código necessário para chamar a service e retornar os dados.

Veja alguns exemplos a seguir:

[`CountryController.cs`](./src/AppProject.Core.Controllers.General/CountryController.cs):

```csharp
using System;
using AppProject.Core.Models.General;
using AppProject.Core.Services.General;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.General;

[Route("api/general/[controller]/[action]")]
[ApiController]
[Authorize]
public class CountryController(ICountryService countryService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await countryService.GetEntityAsync(request, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CreateOrUpdateRequest<Country> request, CancellationToken cancellationToken)
    {
        return this.Ok(await countryService.PostEntityAsync(request, cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] CreateOrUpdateRequest<Country> request, CancellationToken cancellationToken)
    {
        return this.Ok(await countryService.PutEntityAsync(request, cancellationToken));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAsync([FromQuery] DeleteRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await countryService.DeleteEntityAsync(request, cancellationToken));
    }
}
```

[`CountrySummaryController.cs`](./src/AppProject.Core.Controllers.General/CountrySummaryController.cs):

```csharp
using System;
using AppProject.Core.Services.General;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.General;

[Route("api/general/[controller]/[action]")]
[ApiController]
[Authorize]
public class CountrySummaryController(ICountrySummaryService countrySummaryService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSummariesAsync([FromQuery] SearchRequest request, CancellationToken cancellationToken = default)
    {
        return this.Ok(await countrySummaryService.GetSummariesAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetSummaryAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await countrySummaryService.GetSummaryAsync(request, cancellationToken));
    }
}
```

[`StateController.cs`](./src/AppProject.Core.Controllers.General/StateController.cs):

```csharp
using System;
using AppProject.Core.Models.General;
using AppProject.Core.Services.General;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.General;

[Route("api/general/[controller]/[action]")]
[ApiController]
[Authorize]
public class StateController(IStateService stateService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await stateService.GetEntityAsync(request, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CreateOrUpdateRequest<State> request, CancellationToken cancellationToken)
    {
        return this.Ok(await stateService.PostEntityAsync(request, cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] CreateOrUpdateRequest<State> request, CancellationToken cancellationToken)
    {
        return this.Ok(await stateService.PutEntityAsync(request, cancellationToken));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAsync([FromQuery] DeleteRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await stateService.DeleteEntityAsync(request, cancellationToken));
    }
}
```

[`StateSummaryController.cs`](./src/AppProject.Core.Controllers.General/StateSummaryController.cs):

```csharp
using System;
using AppProject.Core.Services.General;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.General;

[Route("api/general/[controller]/[action]")]
[ApiController]
[Authorize]
public class StateSummaryController(IStateSummaryService stateSummaryService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSummariesAsync([FromQuery] SearchRequest request, CancellationToken cancellationToken = default)
    {
        return this.Ok(await stateSummaryService.GetSummariesAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetSummaryAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await stateSummaryService.GetSummaryAsync(request, cancellationToken));
    }
}
```

[`CityController.cs`](./src/AppProject.Core.Controllers.General/CityController.cs):

```csharp
using System;
using AppProject.Core.Models.General;
using AppProject.Core.Services.General;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.General;

[Route("api/general/[controller]/[action]")]
[ApiController]
[Authorize]
public class CityController(ICityService cityService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await cityService.GetEntityAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetNeighborhoodsAsync([FromQuery] GetByParentIdRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await cityService.GetNeighborhoodEntitiesAsync(request, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] CreateOrUpdateRequest<City> request, CancellationToken cancellationToken)
    {
        return this.Ok(await cityService.PostEntityAsync(request, cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> PutAsync([FromBody] CreateOrUpdateRequest<City> request, CancellationToken cancellationToken)
    {
        return this.Ok(await cityService.PutEntityAsync(request, cancellationToken));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAsync([FromQuery] DeleteRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await cityService.DeleteEntityAsync(request, cancellationToken));
    }
}
```

[`CitySummaryController.cs`](./src/AppProject.Core.Controllers.General/CitySummaryController.cs):

```csharp
using System;
using AppProject.Core.Services.General;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.General;

[Route("api/general/[controller]/[action]")]
[ApiController]
[Authorize]
public class CitySummaryController(ICitySummaryService citySummaryService)
    : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSummariesAsync([FromQuery] SearchRequest request, CancellationToken cancellationToken = default)
    {
        return this.Ok(await citySummaryService.GetSummariesAsync(request, cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetSummaryAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
    {
        return this.Ok(await citySummaryService.GetSummaryAsync(request, cancellationToken));
    }
}
```

Perceba alguns detalhes importantes nos controllers:

* Sempre colocamos o atributo `[Authorize]` no controller para que apenas usuários autenticados possam usar;
* Sempre colocamos o atributo `[ApiController]` no controller para indicar que ele é um controller;
* Padronizamos o nome da rota para `[Route("api/general/[controller]/[action]")]`, substituindo apenas o "general" pelo nome do módulo;
* Colocamos os atributos nos métodos: `[HttpGet]`, `[HttpPost]`, `[HttpPut]` e `[HttpDelete]`;
* Sempre temos uma request nos parâmetros dos métodos e a indicação se elas vem do body ou da query;
* O retorno sempre é um IActionResult e um Ok;
* O nome dos métodos também ficaram padronizados. 

## Frontend

