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

$(function () {

    // Add a new tab for KeepRecord.
    $("#addListBtn").click(function () {
        //Get the last list number.
        var lastListNo = $("#ListPanel li").length;

    });

    // Delete lastest KeepRecord's data and tab.
    $("#deleteListBtn").click(function () {
        //Get the last list number.
        var lastListNo = $("#ListPanel li").length;

    });
});