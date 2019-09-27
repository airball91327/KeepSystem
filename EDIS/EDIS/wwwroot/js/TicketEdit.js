function showmsg(data) {
    if (!data.success) {
        alert(data.error);
        $.Toast.hideToast();
    }
    else {
        alert("儲存成功!");
        window.location.reload();
    }
}

$(function () {
    $("#modalVENDOR").on("hidden.bs.modal", function () {
        var vendorName = $("#Vno option:selected").text();
        var vendorId = $("#Vno option:selected").val();

        if ($("#Vno option:selected").text() == "請選擇") {
            $("#VendorName").val("");
            $("#VendorId").val("");
        }
        else {
            $("#VendorName").val(vendorName);
            $("#VendorId").val(vendorId);
        }
    });
});
