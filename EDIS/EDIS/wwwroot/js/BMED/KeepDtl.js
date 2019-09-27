function showmsg2(data) {
    if (!data.success) {
        alert(data.error);
        $.Toast.hideToast();
    }
    else {
        alert("儲存成功!");
        $.Toast.hideToast();
    }
}
