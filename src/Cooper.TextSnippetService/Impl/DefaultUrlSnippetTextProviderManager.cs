//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using CodeSharp.Core;
using CodeSharp.Core.Services;

namespace Cooper.TextSnippetService
{
    [Component]
    public class DefaultUrlSnippetTextProviderManager : IUrlSnippetTextProviderManager
    {
        private IDictionary<UrlType, IUrlSnippetTextProvider> _typeProviderMappings;

        public DefaultUrlSnippetTextProviderManager()
        {
            _typeProviderMappings = new Dictionary<UrlType, IUrlSnippetTextProvider>();
            _typeProviderMappings.Add(UrlType.UnKnown, DependencyResolver.Resolve<UnKnownUrlSnippetTextProvider>());
            _typeProviderMappings.Add(UrlType.GithubIssue, DependencyResolver.Resolve<GithubIssueUrlSnippetTextProvider>());
        }

        public void RegisterProvider(UrlType urlType, IUrlSnippetTextProvider provider)
        {
            _typeProviderMappings.Add(urlType, provider);
        }
        public IUrlSnippetTextProvider GetProvider(UrlType urlType)
        {
            return _typeProviderMappings[urlType];
        }
    }
}
