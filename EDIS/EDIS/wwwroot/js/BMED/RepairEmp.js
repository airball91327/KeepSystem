function showmsg4(data) {
    if (data.isExist) {
        alert(data.error);
        // Refresh the emp table.
        var id = $("#DocId").val();
        $.ajax({
            url: "../../RepairEmp/GetEmpList",
            type: "GET",
            data: { 'docId': id },
            async: true,
            success: function (result) {
                $('#pnlREPEMPLIST').html(result);
            },
            error: function (msg) {
                alert(msg);
            }
        });
    }
    else
        alert("儲存成功!");
}

$(function () {

    $(document).on("click", "#pnlREPEMPLIST a", function () {
        if (confirm("確定要刪除此資料?")) {
            var id = $("#DocId").val();
            var tr = $(this).parents('tr');
            var seq = $(this).parents('tr').children();
            $.ajax({
                url: "../../RepairEmp/Delete",
                type: "POST",
                data: { 'id': id, 'uName': seq.get(0).innerText.trim() },
                async: true,
                dataType: "json",
                success: function (data) {
                    if (data.success) {
                        tr.remove();
                    }
                    else {
                        alert(data.error);
                    }
                }
            });
            return false;
        }
        else
            return false;
    });
});