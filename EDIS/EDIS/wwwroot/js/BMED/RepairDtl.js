function showmsg2() {
    alert("儲存成功!");
    window.location.reload();   //刷新RepairDtl與RepairDtl2及RepairFlow的頁面資訊
}

$(document).ready(function () {

    $("#DealState").change(function () {
        /* 3 = 已完成，4 = 報廢，8 = 退件 */
        if ($(this).val() == 3 || $(this).val() == 8) {
            $("#DealDes").attr("required", "required");
        }
        else if ($(this).val() == 4 ) {
            $("#DealDes").attr("required", "required");         
        }
        else {
            $("#DealDes").removeAttr("required");
            $("#dealDesErrorMsg").html("");
        }
    });
    $('#DealState').trigger("change");

    $('#AssetNo').change(function () {
        getAssetName();
    });
});

function getAssetName() {
    var AssetNo = $("#AssetNo").val();
    $.ajax({
        url: '../../Repair/GetAssetName',
        type: "POST",
        dataType: "json",
        data: { assetNo: AssetNo },
        success: function (data) {
            if (data == "查無資料") {
                $("#AssetName").html('查無資料');
            }
            else {
                $("#AssetName").html(data.cname);
            }
        }
    });
}