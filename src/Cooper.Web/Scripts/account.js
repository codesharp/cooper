//Copyright (c) CodeSharp.  All rights reserved. - http://www.codesharp.cn/

///<reference path="lang.js" />
///<reference path="common.js" />

function doSyncStart(id, fn, efn) {
    debuger.debug('sync' + id);
    var flag = 0;
    $.getJSON(
        url_sync_start,
        { connectionId: id },
        function (i) { setTimeout(function () { doSyncQuery(id, flag, fn, efn); }, 1000); }
    );
}
function doSyncQuery(id, flag, fn, efn) {
    flag += 1;
    //避免等待过久
    if (flag >= 4) {
        debuger.warn('sync too long');
        fn();
        return;
    }
    $.getJSON(
        url_sync_query,
        { connectionId: id },
        function (b) {
            debuger.debug(id + ' syncstatus=' + b);
            if (b)
                fn();
            else
                setTimeout(function () { doSyncQuery(id, flag, fn, efn); }, 1000);
        }
    );
}