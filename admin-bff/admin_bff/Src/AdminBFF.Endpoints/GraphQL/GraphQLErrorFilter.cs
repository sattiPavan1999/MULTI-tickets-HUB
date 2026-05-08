using HotChocolate;

namespace AdminBFF.Endpoints.GraphQL;

public class GraphQLErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is not null)
            return error.WithMessage(error.Exception.Message);
        return error;
    }
}
