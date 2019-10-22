function flowmsg(data) {
    var homeHref = document.getElementById("homeBtn").href;
    $("#btnGO").attr("disabled", false);
    if (!data.success) {
        alert(data.error);
    }
    else {
        alert("送出成功!");

        /* window.close(), 當前非彈出視窗在最新版本的chrome和firefox裡不能關閉，而在IE中是可以關閉的 
         * 非彈出視窗，即是指（opener=null 及 非window.open()開啟的視窗,比如URL直接輸入的瀏覽器窗體， 或由其它程式呼叫產生的瀏覽器視窗）
         */
        if (window.opener == null) {
            location.replace(homeHref);
        }
        else {
            window.opener.location = "javascript:BMEDRepReSubmit();";//This will call ReSubmit() function on parent window.
            //opener.location.reload();//This will refresh parent window.
            window.close();
        }
    }
}

function presend() {
    //alert('test');
    document.getElementById('btnGO').disabled = true;
}

var onFailed = function (data) {
    alert(data.responseText);
    $.Toast.hideToast();
    $('#imgLOADING_Flow').hide();
};

$.fn.addItems = function (data) {

    return this.each(function () {
        var list = this;
        $.each(data, function (val, text) {

            var option = new Option(text.Text, text.Value);
            list.add(option);
        });
    });

};

$(function () {
    $('#btnSelUsr').hide();      //選擇人員
    $('#pnlFLOWVENDOR').hide();  //流程廠商
    $('#pnlUPDATE').hide();
    $("#searchUid").hide();

    if ($('#Cls').val() === "申請人") {
        $('#pnlUPDATE').show();
    }
    $('#FlowCls').change(function () {
        $('#btnSelUsr').hide();
        $('#pnlFLOWVENDOR').hide();
        var select = $('#FlowUid');
        $('option', select).remove();

        //if ($(this).val() === "維修工程師") {
        //    $('#pnlFLOWVENDOR').show();
        //}
        //else if ($(this).val() === "驗收人") {
        //    $('#btnSelUsr').show();
        //}
        if ($(this).val() === "結案" || $(this).val() === "廢除") {
            var appenddata;
            appenddata += "<option value = '0' selected=true></option>";
            select.html(appenddata);
        }
        else if ($(this).val() === "單位主管" || $(this).val() === "單位主任" || $(this).val() === "其他")
        {
            $("#searchUid").show();
        }
        else {
            $("#searchUid").hide();
            $('#FlowVendor').val('');
            $('#imgLOADING_Flow').show();
            var docid = $('#DocId').val();
            $.ajax({
                url: '../../RepairFlow/GetNextEmp',
                type: "POST",
                dataType: "json",
                data: "cls=" + $(this).val() + "&docid=" + docid,
                error: onFailed,
                success: function (data) {
                    $('#imgLOADING_Flow').hide();
                    if (data.success === false) {
                        $('#FlowCls').val('請選擇');
                        alert(data.error);
                    }
                    else {
                        //console.log(data); // For debug.
                        var select = $('#FlowUid');
                        //$('option', select).remove();
                        //select.addItems(data);
                        var i = 0;
                        var defaultOption = 0;
                        select.empty();
                        $.each(data, function (index, item) {  // item is now an object containing properties 
                            if (i === defaultOption) {
                                select.append($('<option selected="selected"></option>').text(item.text).val(item.value));
                            }
                            else {
                                select.append($('<option></option>').text(item.text).val(item.value));
                            }
                            i++;
                        });
                        if ($('#FlowCls').val() === "驗收人") {
                            $("#searchUid").show();
                        }
                    }
                }
            });
        }
    });

    /* Get managers by query string. */
    $("#MgrQryBtn").click(function () {
        var queryStr = $("#DptMgrQry").val();
        $('#imgLOADING_Flow').show();
        $.ajax({
            url: '../../Repair/QueryUsers',
            type: "GET",
            data: { QueryStr: queryStr },
            success: function (data) {
                $('#imgLOADING_Flow').hide();
                var select = $('#FlowUid');
                var i = 0;
                var defaultOption = 0;
                select.empty();
                $.each(data, function (index, item) {  // item is now an object containing properties 
                    if (i === defaultOption) {
                        select.append($('<option selected="selected"></option>').text(item.text).val(item.value));
                    }
                    else {
                        select.append($('<option></option>').text(item.text).val(item.value));
                    }
                    i++;
                });
            }
        });
    });

    $('#FlowVendor').change(function () {
        $('#imgLOADING').show();
        $.ajax({
            url: '../../RepairFlows/GetNextEmp',
            type: "POST",
            dataType: "json",
            data: "cls=維修工程師&docid=" + $('#DocId').val() + "&vendor=" + $(this).val(),
            success: function (data) {
                $('#imgLOADING').hide();
                if (data.success === false) {
                    $('#FlowCls').val('請選擇');
                    alert(data.error);
                }
                else {
                    var select = $('#FlowUid');
                    $('option', select).remove();
                    select.addItems(data);
                }
            }
        });
    });
    $('#modalSELUSER').on('hidden.bs.modal', function () {
        var select = $('#FlowUid');
        var selitem = $('#Suserid option:selected');
        if (selitem.val() !== "") {
            $('option', select).remove();
            var appenddata;
            appenddata += "<option value = ''>請選擇</option>";
            appenddata += "<option value = '" + selitem.val() + "' selected=true>" + selitem.text() + " </option>";
            select.html(appenddata);
        }
    });

    $('input[name="AssignCls"]:radio').change(function () {
        if ($(this).val() === "同意") {
            $("#FlowCls option").each(function () {
                if ($(this).val() === "結案") {
                    $(this).prop('disabled', false);
                    $(this).show();
                }
            });
            if ($("#Cls").val() === "驗收人") {
                $("#FlowCls option").each(function () {
                    if ($(this).is(":selected")) {
                        $('#FlowCls option[value=""]').prop('selected', true);
                        $('#FlowUid').val("");
                    }
                    if ($(this).val() !== "結案") {
                        $(this).prop('disabled', true);
                        $(this).hide();
                    }
                });
            }
        }
        else {
            $("#FlowCls option").each(function () {
                if ($(this).val() === "結案") {
                    if ($(this).is(":selected")) {
                        $('#FlowCls option[value=""]').prop('selected', true);
                    }
                    $(this).prop('disabled', true);
                    $(this).hide();
                }
                else if ($("#Cls").val() === "驗收人") {
                    $(this).prop('disabled', false);
                    $(this).show();
                }
            });
        }
    });

    /* Query users. */
    $("#CheckerQryBtn").click(function () {
        var queryStr = $("#CheckerQry").val();
        var queryDptId = $("#CheckerQryDptId").val();
        $('#imgLOADING_CHK').show();
        $.ajax({
            url: '../../Repair/QueryUsers2',
            type: "GET",
            data: { QueryStr: queryStr, QueryDptId: queryDptId },
            success: function (data) {
                $('#imgLOADING_CHK').hide();
                var select = $('#UpdChecker');
                var i = 0;
                var defaultOption = 0;
                //console.log(select);
                select.empty();
                $.each(data, function (index, item) {  // item is now an object containing properties 
                    if (i === defaultOption) {
                        select.append($('<option selected="selected"></option>').text(item.text).val(item.value));
                    }
                    else {
                        select.append($('<option></option>').text(item.text).val(item.value));
                    }
                    i++;
                });
            }
        });
    });

    /* Update checker. */
    $("#UpdCheckerBtn").click(function () {
        var updChecker = $("#UpdChecker").val();
        var docId = $("#DocId").val();
        $('#imgLOADING_CHK').show();
        $.ajax({
            url: '../../Repair/UpdateChecker',
            type: "POST",
            data: { DocId: docId, UpdChecker: updChecker },
            error: onFailed,
            success: function (data) {
                alert("修改成功!");
                $.Toast.showToast({
                    'title': '頁面重整中，請稍待...',
                    'icon': 'loading',
                    'duration': 0
                });
                window.location.reload();
            }
        });
    });

});
