using Kentor.OwinCookieSaver;
using Microsoft.Owin.Extensions;

namespace Owin
{
    /// <summary>
    /// Extension method for registering middleware.
    /// </summary>
    public static class KentorOwinCookieSaverExtensions
    {
        /// <summary>
        /// Enables the Kentor Owin Cookie Saver middleware, that prevents owin
        /// auth cookies from disappearing. Add above any cookie middleware.
        /// </summary>
        /// <param name="app">Owin app</param>
        /// <returns>Owin app</returns>
        public static IAppBuilder UseKentorOwinCookieSaver(this IAppBuilder app)
        {
            app.Use(typeof(KentorOwinCookieSaverMiddleware));

            return app;
        }

        /// <summary>
        /// Enables the Kentor Owin Cookie Saver middleware, that prevents owin
        /// auth cookies from disappearing. Add above any cookie middleware.
        /// </summary>
        /// <param name="app">Owin app</param>
        /// <param name="stage"> stage in the integrated pipeline prior middleware should run</param>
        /// <returns>Owin app</returns>
        public static IAppBuilder UseKentorOwinCookieSaver(this IAppBuilder app, PipelineStage stage)
        {
            app.Use(typeof(KentorOwinCookieSaverMiddleware));
            app.UseStageMarker(stage);

            return app;
        }
    }
}
