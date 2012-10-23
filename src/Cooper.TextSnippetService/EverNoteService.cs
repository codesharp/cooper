using System;
using System.Collections.Generic;
using Evernote.EDAM.NoteStore;
using Evernote.EDAM.Type;
using Evernote.EDAM.UserStore;
using Thrift.Protocol;
using Thrift.Transport;

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
    public class EverNoteService : IEverNoteService
    {
        private string _userStoreUrl;

        public EverNoteService(string userStoreUrl)
        {
            _userStoreUrl = userStoreUrl;
        }

        public IEnumerable<EverNote> GetDefaultNoteBookNotes(string authToken, int maxReturnCount = 1000)
        {
            var noteStoreUrl = GetNoteStoreUrl(authToken);
            var transport = new THttpClient(new Uri(noteStoreUrl));
            var protocol = new TBinaryProtocol(transport);
            var noteStore = new NoteStore.Client(protocol);
            var notes = new List<EverNote>();

            var notebooks = noteStore.listNotebooks(authToken);

            foreach (Notebook notebook in notebooks)
            {
                if (notebook.DefaultNotebook)
                {
                    var findResult = noteStore.findNotes(authToken, new NoteFilter { NotebookGuid = notebook.Guid }, 0, maxReturnCount);
                    foreach (var note in findResult.Notes)
                    {
                        notes.Add(new EverNote(note.Guid, note.Title));
                    }
                    break;
                }
            }

            return notes;
        }
        public string GetNoteContent(string authToken, string noteId)
        {
            var noteStoreUrl = GetNoteStoreUrl(authToken);
            var transport = new THttpClient(new Uri(noteStoreUrl));
            var protocol = new TBinaryProtocol(transport);
            var noteStore = new NoteStore.Client(protocol);

            var note = noteStore.getNote(authToken, noteId, false, false, false, false);
            var body = noteStore.getNoteSearchText(authToken, noteId, false, false);

            if (!string.IsNullOrEmpty(body))
            {
                var index = body.LastIndexOf(note.Title);
                if (index > 0)
                {
                    body = body.Substring(0, index);
                }
                body = body.Trim();
            }

            return body;
        }

        private string GetNoteStoreUrl(string authToken)
        {
            var userStore = new UserStore.Client(new TBinaryProtocol(new THttpClient(new Uri(_userStoreUrl))));
            return userStore.getNoteStoreUrl(authToken);
        }
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
