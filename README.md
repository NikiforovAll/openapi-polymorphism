# Open API and Polymorphic Serialization with System.Text.Json

This project, "openapi-polymorphism", is a .NET 9.0 application showcasing polymorphism and OpenAPI specifications. Polymorphism is demonstrated with `Component`, `Leaf`, and `Node` classes.

## Demo

Without `Use*OfForPolymorphism`:

```csharp
services.AddSwaggerGen(options =>
{
    // options.UseOneOfForPolymorphism();
    // options.UseAllOfForInheritance();

    options.SwaggerDoc("v1", new() { Title = "Composite V1", Version = "v1" });
    options.SwaggerDoc("v2", new() { Title = "Composite V2", Version = "v2" });

    options.OperationFilter<SwaggerDefaultValues>();
});
```

OpenAPI without:

![with-open-api](/assets/without-open-api.png)

OpenAPI with:

![with-open-api](/assets/with-open-api.png)

Without `JsonDerivedType`:

```csharp
// [JsonDerivedType(typeof(Node), typeDiscriminator: nameof(Node))]
// [JsonDerivedType(typeof(Leaf), typeDiscriminator: nameof(Leaf))]
public abstract record Component(string Name);

public record Leaf(string Name) : Component(Name);

public record Node(string Name, IList<Component>? Children = default) : Component(Name)
{
    public IList<Component> Children { get; init; } = Children ?? [];

    public void Add(Component component) => Children.Add(component);
};
```

Output without:

```json
{
  "children": [
    {
      "name": "N1"
    },
    {
      "name": "N2"
    }
  ],
  "name": "Root1"
}
```

Output with:

```json
{
  "$type": "Node",
  "children": [
    {
      "$type": "Node",
      "children": [
        {
          "$type": "Leaf",
          "name": "L1"
        },
        {
          "$type": "Leaf",
          "name": "L2"
        }
      ],
      "name": "N1"
    },
    {
      "$type": "Node",
      "children": [
        {
          "$type": "Node",
          "children": [
            {
              "$type": "Leaf",
              "name": "L3"
            },
            {
              "$type": "Leaf",
              "name": "L4"
            }
          ],
          "name": "N3"
        }
      ],
      "name": "N2"
    }
  ],
  "name": "Root1"
}
```

## Reference

* <https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism>
* <https://github.com/dotnet/aspnet-api-versioning>
* <https://github.com/dotnet/aspnet-api-versioning/blob/main/examples/AspNetCore/WebApi/MinimalOpenApiExample/Program.cs>
* <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/handle-errors?view=aspnetcore-8.0#problem-details>
* <https://github.com/dotnet/aspnetcore/issues/54599>
* <https://learn.microsoft.com/en-us/entra/identity-platform/index-web-api>