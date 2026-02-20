---
mode: agent
description: "Add a nested/child entity to an existing parent form (like Neighborhoods in City)"
---

# Add Nested Entity

Add a nested/child entity to an existing parent entity's form, following the City → Neighborhoods pattern.

## Required Information

You MUST ask the user for:
1. **Parent entity name** (e.g., `City`, `Invoice`)
2. **Child entity name** (e.g., `Neighborhood`, `InvoiceItem`)
3. **Module name** (e.g., `General`, `Finance`)
4. **Child entity fields** (name, type, validations)

## Instructions

### Backend Changes

1. **Create child DTO** in `AppProject.Core.Models.<Module>`:
   ```csharp
   public class <ChildEntity> : IEntity
   {
       public Guid? Id { get; set; }
       public string Name { get; set; } = default!;
       public byte[]? RowVersion { get; set; }
   }
   ```

2. **Add collections to parent DTO** for synchronization:
   ```csharp
   [ValidateCollection]
   public ICollection<CreateOrUpdateRequest<ChildEntity>>? Changed<ChildEntity>Requests { get; set; }
   public ICollection<DeleteRequest<Guid>>? Deleted<ChildEntity>Requests { get; set; }
   ```

3. **Create child database entity** with FK to parent

4. **Update parent service** to:
   - Validate child items (duplicate names, belong to parent)
   - Process `Changed<ChildEntity>Requests` (insert/update)
   - Process `Deleted<ChildEntity>Requests` (delete)
   - Use `InsertAsync`/`UpdateAsync` + `SaveAsync` for atomicity

5. **Add controller endpoint** for fetching children:
   ```csharp
   [HttpGet]
   public async Task<IActionResult> Get<ChildPlural>Async([FromQuery] GetByParentIdRequest<Guid> request, CancellationToken cancellationToken)
   ```

### Frontend Changes

6. **Create child web model** (`ObservableModel` + `IEntity`)

7. **Add collections to parent web model**

8. **Add Refit client method** for fetching children

9. **Update parent form page**:
   - Add `DataGridControl` with `PreferAddOverNew` for child items
   - Load existing children in `OnInitializedAsync`
   - Open child dialog on add/edit
   - Track changes in `ChangedChildRequests`/`DeletedChildRequests`

10. **Create child form dialog page** (`ModelFormPage<ChildEntity>`) that returns via `CloseDialogAsync`

11. **Add resource keys** for all child UI elements

## Reference Implementation

- Parent form: `src/AppProject.Web.General/Pages/CityFormPage.razor`
- Child form: `src/AppProject.Web.General/Pages/NeighborhoodFormPage.razor`
- Service: `src/AppProject.Core.Services.General/CityService.cs`
