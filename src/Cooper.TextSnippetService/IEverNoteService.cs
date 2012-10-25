//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

using System.Collections.Generic;

namespace Cooper.TextSnippetService
{
    /// <summary>
    /// 提供提取指定EventNote账号中的笔记信息的服务
    /// </summary>
    public interface IEverNoteService
    {
        IEnumerable<EverNote> GetDefaultNoteBookNotes(string authToken, int maxReturnCount = 1000);
        string GetNoteContent(string authToken, string noteId);
    }

    public class EverNote
    {
        public string Id { get; private set; }
        public string Title { get; private set; }

        public EverNote(string id, string title)
        {
            Id = id;
            Title = title;
        }
    }
}
