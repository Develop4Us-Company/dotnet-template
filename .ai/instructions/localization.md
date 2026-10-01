# Localization Instructions

The supported cultures are `en-US`, `pt-BR`, and `es-ES`. Every resource change
must be applied to all three files:

- `src/AppProject.Resources/Resource.resx`;
- `src/AppProject.Resources/Resource.pt-BR.resx`;
- `src/AppProject.Resources/Resource.es-ES.resx`.

Preserve existing comments, logical grouping, order, indentation, and XML
characteristics. Keep forms in their own groups while keeping shared validation
and menu entries grouped according to the existing convention.

Use the existing `<Module>_<Context>_<Element>` key convention. Use `{{}}` rather
than `{}` when reserving literal placeholder text so the resource parser preserves
it. Never hardcode new user-facing text when a resource key should be used.
