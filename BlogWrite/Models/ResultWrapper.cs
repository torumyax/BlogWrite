﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogWrite.Models
{
    // ErrorInfo Class
    public class ErrorObject
    {
        public enum ErrTypes
        {
            DB, API, HTTP, Other
        };

        public ErrTypes ErrType { get; set; } // eg "API, DB, other"
        public string ErrCode { get; set; } // HTTP ERROR CODE?
        public string ErrText { get; set; } // eg API error code translated via dictionaly.
        public string ErrPlace { get; set; } // eg PATH info for REST or method 
        public string ErrPlaceParent { get; set; } // ? site or class name
        public DateTime ErrDatetime { get; set; }
        public string ErrDescription { get; set; } // 自前の補足説明。
    }

    // Result Wrapper Class
    public abstract class ResultWrapper
    {
        public ErrorObject Error = new ErrorObject();
        public bool IsError = false;
    }

    public class SqliteDataAccessResultWrapper: ResultWrapper
    { }

    public class SqliteDataAccessInsertResultWrapper: SqliteDataAccessResultWrapper
    {
        public int InsertedCount;
    }

    public class SqliteDataAccessSelectResultWrapper: SqliteDataAccessResultWrapper
    {
        public int UnreadCount;
    }

    public class HttpClientEntryItemCollectionResultWrapper
    {
        public ErrorObject Error = new ErrorObject();
        public bool IsError = false;
        public ObservableCollection<EntryItem> Entries;
    }
}