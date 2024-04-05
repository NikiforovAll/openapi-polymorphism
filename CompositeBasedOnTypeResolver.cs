using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace OpenApiPolymorphism.V2;

public static class CompositeBasedOnTypeResolver
{
    public static IEndpointRouteBuilder MapCompositeBasedOnTypeResolver(
        this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        app.MapGet("/v{version:apiVersion}/composite", ExecuteAsync)
            .WithName("GetCompositeForTypeResolverModels")
            .WithTags("Composite")
            .WithApiVersionSet(versionSet)
            .HasApiVersion(2)
            .WithOpenApi(operation => new(operation)
            {
                Summary = "Polymorphism via TypeResolver",
                Description = "Composite based on polymorphic serialization with type resolver",
            })
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
    private static Results<Ok<Component>, BadRequest<ProblemDetails>> ExecuteAsync()
    {
        Node node = new("Root2")
        {
            Children =
            [
                new Node("N1")
                {
                    Children =
                    [
                        new Leaf("L1"),
                        new Leaf("L2")
                    ]
                },
                new Node("N2")
                {
                    Children =
                    [
                        new Node("N3")
                        {
                            Children =
                            [
                                new Leaf("L3"),
                                new Leaf("L4")
                            ]
                        }
                    ]
                }
            ]
        };

        return TypedResults.Ok<Component>(node);
    }
}

public abstract record Component(string Name);

public record Leaf(string Name) : Component(Name);

public record Node(string Name, IList<Component>? Children = default) : Component(Name)
{
    public IList<Component> Children { get; init; } = Children ?? [];

    public void Add(Component component) => Children.Add(component);
};

public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

        Type basePointType = typeof(Component);
        if (jsonTypeInfo.Type == basePointType)
        {
            jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(Node), nameof(Node)),
                    new JsonDerivedType(typeof(Leaf), nameof(Leaf)),
                }
            };
        }

        return jsonTypeInfo;
    }
}

/* 
***

Collection initializers based on duck-typing, but it doesn't work with OpenAPI Generator because of recursion. The structure like this throws an error: Stack Overflow.

Node node = new("Root")
{
    new Node("N1")
    {
        new Leaf("L1"),
        new Leaf("L2")
    },
    new Node("N2")
    {
        new Node("N3")
        {
            new Leaf("L3"),
            new Leaf("L4")
        }
    }
};

public abstract record Component(string Name);

public record Leaf(string Name) : Component(Name);

public record Node(string Name) : Component(Name), IEnumerable<Component>
{
    public IList<Component> Children { get; init; } = [];

    public void Add(Component component) => Children.Add(component);

    public IEnumerator<Component> GetEnumerator() => Children.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
};

***
*/