//Copyright (c) CodeSharp.  All rights reserved. - http://www.icodesharp.com/
/////////////////////////////////////////////////////////////////////////////////////////


//接口URL定义，cooper.js会调用这些定义的接口实现与Cooper服务器的数据交互
var loginUrl = null;
var getTasklistsUrl = null;
var createTaskListUrl = null;
var syncTaskUrl = null;
var getTasksUrl = null;

//私有变量，记录当前的外部api的类型
var apiType = null;

(function () {
    //初始化API配置
    function initAPI(type) {
        if (type == "webapi") {
            loginUrl = "../Account/Login";
            getTasklistsUrl = "../Personal/GetTasklists";
            createTaskListUrl = "../Personal/CreateTasklist";
            syncTaskUrl = "../Personal/Sync";
            getTasksUrl = "../Personal/GetByPriority";
            apiType = type;
        }
        else if (type == "phonegap") {
            loginUrl = "Account/Login";
            getTasklistsUrl = "Personal/GetTasklists";
            createTaskListUrl = "Personal/CreateTasklist";
            syncTaskUrl = "Personal/Sync";
            getTasksUrl = "Personal/GetByPriority";
            apiType = type;
        }
    }
    //用于与外部API交互的入口
    function postRequest(url, data, callback) {
        if (apiType == "webapi") {
            ajaxPost(url, data, callback);
        }
        else if (apiType == "phonegap") {
            execNativeAPI(url, data, callback);
        }
    }
    //内部函数，发送Ajax Post请求
    function ajaxPost(url, data, callback) {
        $.ajax({
            url: url,
            data: data,
            type: 'POST',
            dataType: 'json',
            cache: false,
            async: true,
            beforeSend: function () { },
            success: function (data) {
                if (callback) {
                    callback(data);
                }
            }
        });
    }
    //内部函数，与PhoneGap Native API进行交互
    function execNativeAPI(url, data, callback) {
        var items = url.split("/");
        var serviceName = items[0];
        var actionName = items[1].toLowerCase();

        //因为参数必须是数组，所以把参数放在一个数组中
        var params = [];
        params.push(data);

        //调用Native接口
        Cordova.exec(
            function (result) {
                if (callback != null) {
                    callback(result);
                }
            },
            function () { },
            serviceName,
            actionName,
            params
        );
    }

    window.initAPI = initAPI;
    window.postRequest = postRequest;
})();