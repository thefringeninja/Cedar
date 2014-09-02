namespace Cedar.Testing
{
    using System;
    using System.Net.Http.Headers;
    using System.Text;

    public static class Authorization
    {
        public static IAuthorization Basic(string userName, string password)
        {
            return new BasicAuthorization(userName, password);
        }
        class BasicAuthorization : IAuthorization
        {
            private readonly string _userName;

            public BasicAuthorization(string userName, string password)
            {
                _userName = userName;

                AuthorizationHeader = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", userName, password))));
            }

            string IAuthorization.Id
            {
                get { return _userName; }
            }

            public AuthenticationHeaderValue AuthorizationHeader { get; private set; }
        }


    }
}