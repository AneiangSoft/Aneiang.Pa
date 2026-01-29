using System;
using Aneiang.Pa.Core.News;

namespace Aneiang.Pa.Dynamic.Attributes
{
    /// <summary>
    /// HTML头特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HtmlHeaderAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the URL associated with the resource.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the user agent string to be sent with HTTP requests.
        /// </summary>
        /// <remarks>The user agent string identifies the client application to the server. Setting this
        /// property allows customization of the user agent for compatibility or analytics purposes. If not set, a
        /// default user agent may be used depending on the implementation.</remarks>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the URL of the referring page associated with the current request.
        /// </summary>
        public string? Referrer { get; set; }

        /// <summary>
        /// Initializes a new instance of the HtmlHeaderAttribute class with the specified URL, referrer, and user agent
        /// information.
        /// </summary>
        /// <remarks>If <paramref name="userAgent"/> is null, a random user agent string is generated
        /// automatically. The <paramref name="referrer"/> parameter is optional and can be omitted if not
        /// required.</remarks>
        /// <param name="url">The URL to associate with the HTML header. Cannot be null.</param>
        /// <param name="referrer">The referrer URL to include in the header, or null to omit the referrer.</param>
        /// <param name="userAgent">The user agent string to use for the header, or null to generate a random user agent.</param>
        public HtmlHeaderAttribute(string url, string? referrer = null, string? userAgent = null)
        {
            Url = url;
            UserAgent = userAgent ?? UserAgentGenerator.GetRandomUserAgent();
            Referrer = referrer;
        }
    }
}
