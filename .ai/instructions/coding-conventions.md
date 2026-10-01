# Coding Conventions

- Follow the analyzers configured by `src/Stylecop.json` and
  `src/Directory.Build.props`.
- The shared target framework is `net10.0`; implicit usings and nullable reference
  types are enabled, and `LangVersion` is `latest`.
- Remove unused usings and keep the remaining directives ordered.
- Use primary constructors for services and controllers.
- Entity DTOs implement `IEntity`, include `RowVersion`, and use DataAnnotations
  for input validation.
- Summary DTOs implement `ISummary` and expose read-only grid or selection data.
- Database entity classes use the `Tb<Name>` convention and inherit `BaseEntity`.
- Service interfaces use the existing generic contracts and DI marker interfaces.
- Controllers use the existing authorized module-route convention.
- Frontend models inherit `ObservableModel`, implement the appropriate model
  interface, and use `Set()` in property setters.
- Refit routes must match controller routes exactly.
- Search pages inherit `SearchPage<TRequest, TSummary>` and form pages inherit
  `ModelFormPage<TModel>`.
- Create a derived `SearchRequest` only when filters beyond `SearchText` and
  `Take` are required.
- Use Mapster's conventional mapping when sufficient. Create an
  `IRegisterMapsterConfig` only when conventional mapping cannot express the
  mapping, such as renamed, flattened, calculated, or relationship-derived data.
