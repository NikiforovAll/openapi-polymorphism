using System.Collections;
using System.Text.Json.Serialization;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace OpenApiPolymorphism.V1;

public static class CompositeBasedOnAttribute
{
    public static IEndpointRouteBuilder MapBasedOnAttribute(
        this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        app.MapGet("/v{version:apiVersion}/composite", ExecuteAsync)
        .WithName("GetCompositeForAttributeAnnotatedModels")
        .WithTags("Composite")
        .WithApiVersionSet(versionSet)
        .HasApiVersion(1)
        .WithOpenApi(operation => new(operation)
        {
            Summary = "Polymorphism via JsonDerivedTypeAttribute",
            Description = "Composite based on polymorphic serialization with attributes",
        })
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }

    private static Results<Ok<Component>, BadRequest<ProblemDetails>> ExecuteAsync()
    {
        Node node = new("Root1")
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

[JsonDerivedType(typeof(Node), typeDiscriminator: nameof(Node))]
[JsonDerivedType(typeof(Leaf), typeDiscriminator: nameof(Leaf))]
public abstract record Component(string Name);

public record Leaf(string Name) : Component(Name);

public record Node(string Name, IList<Component>? Children = default) : Component(Name)
{
    public IList<Component> Children { get; init; } = Children ?? [];

    public void Add(Component component) => Children.Add(component);
};