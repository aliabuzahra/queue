using AutoMapper;
using FluentAssertions;
using Moq;
using VirtualQueue.Application.Commands.Tenants;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Application.Mappings;
using VirtualQueue.Domain.Entities;
using Xunit;

namespace VirtualQueue.UnitTests.Application.Commands;

public class CreateTenantCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CreateTenantCommandHandler _handler;

    public CreateTenantCommandHandlerTests()
    {
        _tenantRepositoryMock = new Mock<ITenantRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new CreateTenantCommandHandler(_tenantRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTenant()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Tenant", "test.com");
        var tenant = new Tenant("Test Tenant", "test.com");
        var tenantDto = new TenantDto(tenant.Id, tenant.Name, tenant.Domain, tenant.ApiKey, tenant.IsActive, tenant.CreatedAt);

        _tenantRepositoryMock.Setup(x => x.GetByDomainAsync(command.Domain, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);
        _tenantRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(tenant);
        _tenantRepositoryMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mapperMock.Setup(x => x.Map<TenantDto>(tenant))
            .Returns(tenantDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(command.Name);
        result.Domain.Should().Be(command.Domain);
        _tenantRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()), Times.Once);
        _tenantRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingDomain_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new CreateTenantCommand("Test Tenant", "test.com");
        var existingTenant = new Tenant("Existing Tenant", "test.com");

        _tenantRepositoryMock.Setup(x => x.GetByDomainAsync(command.Domain, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTenant);

        // Act & Assert
        var action = async () => await _handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }
}
