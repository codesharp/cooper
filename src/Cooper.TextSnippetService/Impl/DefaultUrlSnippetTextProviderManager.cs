//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;
using System.Linq;
using CodeSharp.Core;

namespace Cooper.TextSnippetService
{
    [Component(LifeStyle = LifeStyle.Singleton)]
    public class DefaultUrlSnippetTextProviderManager : IUrlSnippetTextProviderManager
    {
        private DefaultUrlSnippetTextProvider _defaultUrlSnippetTextProvider;
        private IDictionary<UrlSnippetTextProviderKey, IUrlSnippetTextProvider> _typeProviderMappings;

        public DefaultUrlSnippetTextProviderManager()
        {
            _defaultUrlSnippetTextProvider = new DefaultUrlSnippetTextProvider();
            _typeProviderMappings = new Dictionary<UrlSnippetTextProviderKey, IUrlSnippetTextProvider>();

            _typeProviderMappings.Add(new UrlSnippetTextProviderKey
            {
                UrlRegexPattern = @"https://github\.com/([^/]+?)/([^/]+?)/issues/(\d+)$"
            },
            new GithubIssueUrlSnippetTextProvider());
        }

        public void RegisterProvider(UrlSnippetTextProviderKey providerKey, IUrlSnippetTextProvider provider)
        {
            _typeProviderMappings.Add(providerKey, provider);
        }
        public IUrlSnippetTextProvider GetProvider(string url)
        {
            var key = _typeProviderMappings.Keys.FirstOrDefault(x => x.IsUrlMatch(url));
            if (key != null)
            {
                return _typeProviderMappings[key];
            }

            return _defaultUrlSnippetTextProvider;
        }
    }
}
