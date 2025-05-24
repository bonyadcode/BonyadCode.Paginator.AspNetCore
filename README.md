# BonyadCode.Paginator.AspNetCore

A minimal yet powerful pagination utility for .NET developers. Enables easy paging over `IQueryable<T>` or `IEnumerable<T>` sources with optional dynamic ordering via expressions or property names — ideal for APIs, dashboards, and any list-heavy UIs.

---

## ✨ Features

* **Unified Paging API**: Works seamlessly with both `IQueryable` (e.g., EF Core) and `IEnumerable`.
* **Dynamic Ordering**: Sort by expression or string field name (e.g., `"CreatedAt"`).
* **Smart Caching**: Expression caching ensures optimal performance for repeated queries.
* **Async + Sync Support**: Optimized methods for both LINQ-to-Objects and LINQ-to-Entities.
* **Safe Defaults**: Sensible defaults for page size, number, and ordering.

---

## 📦 Installation

```bash
dotnet add package BonyadCode.Paginator.AspNetCore
```

---

## 🚀 Quick Examples

### ✅ Paginate an EF Core Query

```csharp
var request = new PagedRequest
{
    PageNumber = 2,
    PageSize = 10,
    OrderBy = "CreatedAt",
    AscendingOrder = true
};

var result = await dbContext.Users
    .Where(u => u.IsActive)
    .ToPagedResponseAsync(request, cancellationToken: cancellationToken);
```
or
```csharp
var request = new PagedRequest
{
    PageNumber = 2,
    PageSize = 10,
    OrderBy = nameof(Request.CreatedAt),
    AscendingOrder = true
};

var result = await dbContext.Users
    .Where(u => u.IsActive)
    .ToPagedResponseAsync(request, cancellationToken: cancellationToken);
```

**Result JSON:**

```json
{
  "pageNumber": 2,
  "pageSize": 10,
  "pageCount": 5,
  "itemCount": 42,
  "hasPreviousPage": true,
  "hasNextPage": true,
  "items": [ /* paginated users */ ]
}
```

### ✅ Paginate an In-Memory List

```csharp
var users = new List<User> { /* ... */ };

var request = new PagedRequest
{
    PageNumber = 1,
    PageSize = 5,
    OrderBy = "Username",
    AscendingOrder = false
};

var result = users.ToPagedResponse(request);
```

---

## 📘 API Overview

### 📟 `PagedRequest`

| Property         | Type      | Description               | Default |
| ---------------- | --------- | ------------------------- | ------- |
| `PageNumber`     | `uint?`   | Page to retrieve          | `1`     |
| `PageSize`       | `uint?`   | Number of items per page  | `20`    |
| `OrderBy`        | `string?` | Field/property to sort by | `"Id"`  |
| `AscendingOrder` | `bool?`   | Whether to sort ascending | `false` |

### 📦 `PageBuilder<T>`

| Property          | Type       | Description                    |
| ----------------- | ---------- | ------------------------------ |
| `Items`           | `IList<T>` | The data for the current page  |
| `PageNumber`      | `uint`     | Current page number            |
| `PageSize`        | `uint`     | Items per page                 |
| `PageCount`       | `uint`     | Total number of pages          |
| `ItemCount`       | `uint`     | Total number of items          |
| `HasPreviousPage` | `bool`     | Whether a previous page exists |
| `HasNextPage`     | `bool`     | Whether a next page exists     |

---

## 🧐 Advanced Usage

### 📌 Paginate with Expression

```csharp
Expression<Func<User, object>> orderByExpr = x => x.Email;

var result = await dbContext.Users
    .ToPagedResponseAsync(request, orderByExpr, cancellationToken);
```

### 🔄 Runtime Property Sorting

```csharp
var result = await dbContext.Products
    .ToPagedResponseAsync(new PagedRequest
    {
        OrderBy = "Price",
        AscendingOrder = true
    });
```

> Field name must match a public property of the type (case-insensitive).

---

## 🔧 Internals & Performance

* Uses `ConcurrentDictionary<Type, Dictionary<string, Expression>>` for dynamic sorting — field expressions are cached per type for reuse.
* Null-safe and validated defaults — throws if `PageSize == 0`.
* `CancellationToken` respected on all async methods.
* Compatible with both `System.Linq` and `Microsoft.EntityFrameworkCore`.

---

## 🤝 Contributing

Contributions, suggestions, and PRs are welcome! [GitHub →](https://github.com/bonyadcode/Paginator.AspNetCore)

## 📄 License

Apache 2.0 — see the [LICENSE](LICENSE) file.

## 📦 Links

* [NuGet](https://www.nuget.org/packages/BonyadCode.Paginator.AspNetCore)
* [GitHub](https://github.com/bonyadcode/Paginator.AspNetCore)
