
$(function () {

    $("#pnlKEEPCOSTLIST a").click(function () {

        if (confirm("確定要刪除此資料?")) {
            var id = $("#DocId").val();
            var tr = $(this).parents('tr');
            var seq = $(this).parents('tr').children();
            $.ajax({
                url: "../../KeepCost/Delete",
                type: "POST",
                data: { docid: id, seqno: seq.get(0).innerText.trim() },
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