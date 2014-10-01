namespace Cedar.Testing
{
    using System.Net.Http.Headers;

    public interface IAuthorization
    {
        string Id { get; }

        AuthenticationHeaderValue AuthorizationHeader { get; }
    }
}