function showmsg5(data) {
    if (data.isExist) {
        alert(data.error);
        // Refresh the scrapList table.
        var id = $("#DocId").val();
        //console.log("id=" + id);
        $.ajax({
            url: "../../ScrapAsset/GetScrapList",
            type: "GET",
            data: { 'docId': id },
            async: true,
            success: function (result) {
                $('#pnlSCRAPLIST').html(result);
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

    $(document).on("click", "#pnlSCRAPLIST a", function () {
        if (confirm("確定要刪除此資料?")) {
            var docId = $("#DocId").val();
            var tr = $(this).parents('tr');
            var seq = $(this).parents('tr').children();
            $.ajax({
                url: "../../ScrapAsset/Delete",
                type: "POST",
                data: { 'docId': docId, 'assetNo': seq.get(0).innerText.trim() },
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

    $("#modalASSETS").on("hidden.bs.modal", function () {
        var id = $("#DocId").val();
        //console.log("id=" + id);
        $.ajax({
            url: "../../ScrapAsset/GetScrapList",
            type: "GET",
            data: { 'docId': id, 'printType': 2 },
            async: true,
            success: function (result) {
                $('#pnlSCRAPLIST2').html(result);
            },
            error: function (msg) {
                alert(msg);
            }
        });
    });
});