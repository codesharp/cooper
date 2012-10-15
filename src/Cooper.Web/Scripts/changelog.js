//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/

///<reference path="lang.js" />
///<reference path="common.js" />

//changelog sync support

(function () {
    var changes = {};

    //[flag][id][name][log]
    function appendChangelog(flag, id, name, val, changeType) {
        if (!changes)
            changes = {};
        if (!changes[flag])
            changes[flag] = {};
        if (!changes[flag][id])
            changes[flag][id] = {};
        var log = {};
        log['ID'] = id;
        log['Name'] = name;
        log['Value'] = val;
        log['Type'] = changeType;
        log['Flag'] = flag;
        //统一增加时间戳
        log['CreateTime'] = new Date().toUTCString();
        if (changeType == 0)
            //更新变更只记录最后一次
            changes[flag][id][name] = log;
        else
            changes[flag][id][name + new Date()] = log;
        debuger.info('new change log of ' + flag + '#' + id, log);
    }

    window.appendDeleteChange = function (flag, id, name, value) { appendChangelog(flag, id, name, value, 1); }
    window.appendInsertChange = function (flag, id, name, value) { appendChangelog(flag, id, name, value, 2); }
    window.appendUpdateChange = function (flag, id, name, value) { appendChangelog(flag, id, name, value, 0); }

    //TODO:是否有必要按时间顺序排序？
    window.popChangelogs = function (flag) {
        var logs = [];
        if (!changes || !changes[flag])
            return logs;
        var temp = changes[flag];
        for (var id in temp)
            if (temp[id])
                for (var name in temp[id])
                    if (temp[id][name])
                        $.merge(logs, [temp[id][name]]);
        changes = null;
        return logs;
    }
})();
