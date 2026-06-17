namespace FlowTrack.Iam.Users.Application
{
    internal record FindUserByIdQry(string Id) : IQuery<User>;
}
