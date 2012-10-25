//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 提供提取文本缩略信息，以及缓存文本缩略信息的服务
    /// </summary>
    public class DefaultSnippetTextService : ISnippetTextService
    {
        private IUrlSnippetTextProviderManager _urlSnippetTextProviderManager;
        private IEverNoteService _everNoteService;
        private ISnippetTextRepository _snippetTextRepository;

        public DefaultSnippetTextService(
            IUrlSnippetTextProviderManager urlSnippetTextProviderManager,
            IEverNoteService everNoteService,
            ISnippetTextRepository snippetTextRepository)
        {
            _urlSnippetTextProviderManager = urlSnippetTextProviderManager;
            _everNoteService = everNoteService;
            _snippetTextRepository = snippetTextRepository;
        }

        public string GetHtmlSnippetText(string url)
        {
            var snippetText = _snippetTextRepository.GetSnippetText(SnippetTextType.Html, url);
            if (snippetText != null)
            {
                return snippetText.Text;
            }

            var text = _urlSnippetTextProviderManager.GetProvider(url).GetSnippetText(url);

            _snippetTextRepository.AddSnippetText(SnippetTextType.Html, url, text);

            return text;
        }
        public string GetEverNoteSnippetText(string authToken, string noteId)
        {
            var snippetText = _snippetTextRepository.GetSnippetText(SnippetTextType.EverNote, noteId);
            if (snippetText != null)
            {
                return snippetText.Text;
            }

            var text = _everNoteService.GetNoteContent(authToken, noteId);
            _snippetTextRepository.AddSnippetText(SnippetTextType.EverNote, noteId, text);
            return text;
        }
        public void ClearSnippetText(long id)
        {
            _snippetTextRepository.RemoveSnippetText(id);
        }
    }
}
