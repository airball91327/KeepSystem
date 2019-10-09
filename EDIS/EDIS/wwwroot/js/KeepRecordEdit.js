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


});