using System.Reflection;
using EliteAcademy.Application.Features.Wishlist.Commands.AddToWishlist;
using EliteAcademy.Domain.Common;
using EliteAcademy.Infrastructure.Persistence;
using FluentAssertions;
using NetArchTest.Rules;

namespace EliteAcademy.Tests.Architecture;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(DomainResult<>).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(AddToWishlistHandler).Assembly;
    private static readonly Assembly InfrastructureAssembly = typeof(ApplicationDbContext).Assembly;

    // ── Dependency direction ─────────────────────────────────────────────

    [Fact]
    public void Domain_Should_Not_Depend_On_Application()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should().NotHaveDependencyOn("EliteAcademy.Application")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should().NotHaveDependencyOn("EliteAcademy.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should().NotHaveDependencyOn("EliteAcademy.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Web()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should().NotHaveDependencyOn("EliteAcademy.Web")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Web()
    {
        var result = Types.InAssembly(InfrastructureAssembly)
            .Should().NotHaveDependencyOn("EliteAcademy.Web")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    // ── Naming conventions ───────────────────────────────────────────────

    [Fact]
    public void Services_Should_Reside_In_Application_Services_Namespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().HaveNameEndingWith("Service")
            .And().AreNotInterfaces()
            .Should().ResideInNamespace("EliteAcademy.Application.Services")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Service_Interfaces_Should_Reside_In_Interfaces_Namespace()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().HaveNameEndingWith("Service")
            .And().AreInterfaces()
            .Should().ResideInNamespaceStartingWith("EliteAcademy.Application.Interfaces")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Entities_Should_Reside_In_Entities_Namespace()
    {
        var result = Types.InAssembly(DomainAssembly)
            .That().AreClasses()
            .And().ResideInNamespaceStartingWith("EliteAcademy.Domain.Entities")
            .Should().ResideInNamespaceStartingWith("EliteAcademy.Domain.Entities")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
