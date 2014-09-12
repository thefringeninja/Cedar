namespace Cedar.Example.AspNet
{
    using System;
    using System.Web;

    public class Global : HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {
            // The activation order of Global and Strtup
            var instance = App.Instance;
        }

        protected void Session_Start(object sender, EventArgs e)
        {}

        protected void Application_BeginRequest(object sender, EventArgs e)
        {}

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {}

        protected void Application_Error(object sender, EventArgs e)
        {}

        protected void Session_End(object sender, EventArgs e)
        {}

        protected void Application_End(object sender, EventArgs e)
        {
            App.Instance.Dispose();
        }
    }
}