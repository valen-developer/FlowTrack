

using Shared.Domain.Bus.Query;

namespace Iam.User.application.Signin;



public sealed class SigninQryHandler : IQueryHandler<SigninQry, Object>
{



  public async Task<Object> Handle(SigninQry qry)
  {

    return new Object();

  }


}