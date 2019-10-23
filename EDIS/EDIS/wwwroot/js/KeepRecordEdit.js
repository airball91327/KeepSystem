function smgKEEPRECORD(data) {
    if (data.success === false) {
        alert(data.error);
        $.Toast.hideToast();
    }
    else {
        alert("儲存成功!");
        $.Toast.hideToast();
    }
}

function checkDelBtn() {
    if ($("#keepRecordPanel li").length <= 1) {
        $("#deleteListBtn").hide();
    }
    else {
        $("#deleteListBtn").show();
    }
}

$(function () {

    checkDelBtn();

    // Add a new tab for KeepRecord.
    $("#addListBtn").click(function () {
        //Get the last list number.
        var lastListNo = $("#keepRecordPanel li").length;
        var newListNo = lastListNo + 1;
        var docId = $(this).val();
        $.ajax({
            url: "../../KeepRecord/GetRecordList",
            type: "GET",
            async: false,
            data: { listNo: newListNo, docid: docId },
            success: function (data) {
                $("#keepRecordPanel").append('<li role="presentation" id="List' + newListNo + '"><a href="#ListNo' + newListNo + '" data-toggle="tab" style="padding-left:20px">' + newListNo + '</a></li>');
                $("#keepRecordPanelContent").append('<div id="ListNo' + newListNo + '" class="tab-pane fade"><div>' + data + '</div ></div >');
                $("#deleteListBtn").show();
            }
        });
    });

    // Delete lastest KeepRecord's data and tab.
    $("#deleteListBtn").click(function () {
        //Get the last list number.
        var lastListNo = $("#keepRecordPanel li").length;
        var docId = $(this).val();
        if (confirm('確定要刪除第' + lastListNo + '筆紀錄?')) {
            $.ajax({
                url: "../../KeepRecord/DeleteRecords",
                type: "POST",
                async: false,
                data: { listNo: lastListNo, docid: docId },
                success: function (data) {
                    if (data.success === false) {
                        alert(data.error);
                    }
                    else {
                        alert("已刪除!");
                    }
                }
            });
            $("#List" + lastListNo).remove();
            $("#ListNo" + lastListNo).remove();
            $("#keepRecordPanel a:first").tab("show");
        }
        checkDelBtn();
    });
});