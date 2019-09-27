var onFailed = function (data) {
    alert(data.responseText);
    $.Toast.hideToast();
};
$.fn.addItems = function (data) {

    return this.each(function () {
        var list = this;
        $.each(data, function (val, text) {

            var option = new Option(text.text, text.value);
            list.add(option);
        });
    });

};

$(function () {

    $('input').keypress(function (e) {
        code = e.keyCode ? e.keyCode : e.which; // in case of browser compatibility
        if (code == 13) {
            e.preventDefault();
            // do something
            /* also can use return false; instead. */
        }
    });

    $('#AssetNo').change(function () {
        /* Get engineers. */
        $.ajax({
            url: '../Repair/GetAssetEngId',
            type: "POST",
            dataType: "json",
            data: {
                AssetNo: $('#AssetNo').val()
            },
            async: false,
            success: function (data) {
                $('#EngId').val(data.engId);
                $('#EngName').val(data.fullName);
                //var select = $('#EngId');
                //$('option', select).remove();
                //select.addItems(data);
                //console.log(data + ";" + select.val()); // ForDebug
            }
        });
    });

    /* While user change DptId, search the DptName. */
    $("#DptId").change(function () {
        var DptId = $(this).val();
        $.ajax({
            url: '../Repair/GetDptName',
            type: "POST",
            dataType: "json",
            data: { dptId: DptId },
            success: function (data) {
                //console.log(data);
                if (data == "") {
                    $("#DptNameErrorMsg").html("查無部門!");
                }
                else {
                    $("#DptNameErrorMsg").html("");
                }
                $("#DptName").val(data);
            }
        });       
    });
    /* While user change AccDpt, search the AccDptName. */
    $("#AccDpt").change(function () {
        var AccDptId = $(this).val();
        $.ajax({
            url: '../Repair/GetDptName',
            type: "POST",
            dataType: "json",
            data: { dptId: AccDptId },
            success: function (data) {
                //console.log(data);
                if (data == "") {
                    $("#AccDptNameErrorMsg").html("查無部門!");
                }
                else {
                    $("#AccDptNameErrorMsg").html("");
                }
                $("#AccDptName").val(data);  
            }
        });    
    });

    /* If user select "本單位" */
    $("input[type=radio][name=LocType]").change(function () {
        if (this.value == '本單位') {

            $("#AccDpt").attr("readonly", "readonly");
            $("#AccDptName").attr("readonly", "readonly");
            /* Get AccDptId and Name. */
            $("#AccDpt").val($("#DptId").val());
            $.ajax({
                url: '../Repair/GetDptName',
                type: "POST",
                dataType: "json",
                data: { dptId: $("#DptId").val() },
                success: function (data) {
                    if (data == "") {
                        $("#AccDptNameErrorMsg").html("查無部門!");
                    }
                    else {
                        $("#AccDptNameErrorMsg").html("");
                    }
                    $("#AccDptName").val(data);
                }
            });
        }
        else {
            $("#AccDpt").removeAttr("readonly");
            $("#AccDptName").removeAttr("readonly");
        }
    });

    /* If user select "增設", show Mgr dropdown for user to select. */
    $("#DptMgr").hide();    //Default setting.
    $("input[type=radio][name=RepType]").change(function () {
        if (this.value == '增設') {
            $("#DptMgr").show();
        }
        else {
            $("#DptMgr").hide();
        }
    });

    /* Get managers by query string. */
    $("#MgrQryBtn").click(function () {
        var queryStr = $("#DptMgrQry").val();
        $.ajax({
            url: '../Repair/QueryUsers',
            type: "GET",
            data: { QueryStr: queryStr },
            success: function (data) {
                var select = $('#DptMgrId');
                $('option', select).remove();
                select.addItems(data);
            }
        });
    });
    /* Get Assets by query string. */
    $("#AssetQryBtn").click(function () {
        var queryStr = $("#AssetQry").val();
        var queryAccDpt = $("#AssetAccDptQry").val();
        var queryDelivDpt = $("#AssetDelivDptQry").val();
        $.ajax({
            url: '../Repair/QueryAssets',
            type: "GET",
            data: { QueryStr: queryStr, QueryAccDpt: queryAccDpt, QueryDelivDpt: queryDelivDpt },
            success: function (data) {
                console.log(data);
                var select = $('#AssetNo');
                $('option', select).remove();
                if (data.length == 0) {
                    $("#AssetNoErrorMsg").html("查無資料!");
                }
                else if (data.length == 1) {
                    select.addItems(data);
                    $('#AssetNo').trigger("change");
                    $("#AssetNoErrorMsg").html("");
                }
                else {
                    select.append($('<option selected="selected" disabled="disabled"></option>').text("請選擇").val(""));
                    select.addItems(data);
                    $("#AssetNoErrorMsg").html("");
                }
            }
        });
    });
    $("#AssetNo").change(function () {
        var assetName = $('#AssetNo option:selected').text().split("(", 1);
        $("#AssetName").val(assetName);
    });

    $("#CheckerQryBtn").click(function () {
        var queryStr = $("#CheckerQry").val();
        $.ajax({
            url: '../Repair/QueryUsers',
            type: "GET",
            data: { QueryStr: queryStr },
            success: function (data) {
                var select = $('#CheckerId');
                $('option', select).remove();
                select.addItems(data);
            }
        });
    });
    $('#modalFILES').on('hidden.bs.modal', function () {
        var docid = $("#DocId").val();
        $.ajax({
            url: '../AttainFile/List3',
            type: "POST",
            data: { docid: docid, doctyp: "1" },
            success: function (data) {
                $("#pnlFILES").html(data);
            }
        });
    });
});

function onSuccess() {
    $.Toast.hideToast();
    alert("已送出");

    var DocId = $("#DocId").val();
    var repType = $('input:radio[name="RepType"]:checked').val();
    /* Print confirm before submit. */
    var r = confirm("是否列印?");
    if (r == true) {
        window.printRepairDoc(DocId);
    }

    location.href = '../../Home/Index';
}

function getAssetName() {
    var AssetNo = $("#AssetNo").val();
    $.ajax({
        url: '../Repair/GetAssetName',
        type: "POST",
        dataType: "json",
        data: { assetNo: AssetNo },
        success: function (data) {
            //console.log(data); // debug
            if (data == "查無資料") {
                $("#AssetNameErrorMsg").html("查無資料!");
            }
            else {
                $("#AssetNameErrorMsg").html("");
            }
            $("#AssetName").val(data.cname);
            $("#assetAccDate").val(data.accDate);
        }
    });  
}


function printRepairDoc(DocId) {
    
    var printContent = "";
    /* Get print page. */
    $.ajax({
        url: '../Repair/PrintRepairDoc',
        type: "GET",
        async: false,
        data: { docId: DocId, printType : 1 },
        success: function (data) {
            printContent = data;
        }
    });
    $.ajaxSettings.async = true; // Set this ajax async back to true.
    /* New a window to print. */
    var printPage = window.open();
    printPage.document.open();
    printPage.document.write("<BODY onload='window.print();window.close()'>");
    printPage.document.write(printContent);
    printPage.document.close();
}
