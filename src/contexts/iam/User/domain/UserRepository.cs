namespace Iam.User.Domain;

public interface IUserRepository
{
    Object FindByEmail(string email);
}
