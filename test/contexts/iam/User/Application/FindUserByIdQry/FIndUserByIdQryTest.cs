using FlowTrack.Iam.Application;
using FlowTrack.Iam.Domain;
using FlowTrack.Iam.Test;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace FlowTrack.Test.Iam;

public class FindUserByIdQryTest
{
    private FindUserByIdQryHandler handler;
    private Mock<IUserRepository> userRepositoryMock = new();

    public FindUserByIdQryTest()
    {
        var services = new ServiceCollection();

        services.AddSingleton(userRepositoryMock.Object);
        services.AddScoped<FindUserByIdQryHandler>();

        var serviceProvider = services.BuildServiceProvider();
        handler = serviceProvider.GetRequiredService<FindUserByIdQryHandler>();
    }

    [Fact]
    public async Task Should_Return_User()
    {
        var user = UserMother.Random();
        userRepositoryMock.Setup(r => r.FindById(user.Id)).ReturnsAsync(user);

        var qry = new FindUserByIdQry(user.Id);
        var result = await handler.Handle(qry);

        Assert.Equivalent(user, result);
    }

    [Fact]
    public async Task Should_Throw_UserNotFoundException()
    {
        var userId = Guid.NewGuid();
        userRepositoryMock.Setup(r => r.FindById(userId)).ReturnsAsync((User)null);

        var qry = new FindUserByIdQry(userId);
        await Assert.ThrowsAsync<UserNotFound>(() => handler.Handle(qry));
    }
}
