﻿function showMsg(data) {
    alert("儲存成功!");
}

var onFailed = function (data) {
    alert(data.responseText);
    $.Toast.hideToast();
};

$(function () {

    /* Default setting for the view. */
    $("#Price").attr('readonly', true);
    $("#TotalCost").attr('readonly', true);

    $('#PartName').attr("readonly", false);
    $('#Price').attr("readonly", false);
    $('#btnQtyStock').hide();
    $("#pnlSIGN").hide();
    $("#pnlACCDATE").show();
    $("#CVendor").show();
    $("#pnlTICKET").show();
    $('label[for="AccountDate"]').text("發票日期");

    $('#modalSTOCK').on('hidden.bs.modal', function () {
        var $obj = $('input:radio[name="rblSELECT"]:checked').parents('tr').children();
        if ($obj.length > 0) {
            $("#PartNo").val($obj.get(0).innerText.trim());
            $("#PartName").val($obj.get(1).innerText.trim());
            $("#Standard").val($obj.get(2).innerText.trim());
            $("#Unite").val($obj.get(3).innerText.trim());
            $("#Price").val($obj.get(4).innerText.trim());
            var v1 = $("#Price").val();
            var v2 = $("#Qty").val();
            if (v1 !== null && v2 !== null) {
                $("#TotalCost").val(v1 * v2);
            }
            $("#VendorId").val('000');
            $("#VendorName").val($obj.get(4).innerText.trim());
        }
    });

    /* According stock type to change labels and input textboxs. */
    $('input:radio[name="StockType"]').click(function () {
        $('#PartName').attr("readonly", false);
        $('#Price').attr("readonly", false);
        var item = $(this).val();
        if (item === "2") {             // 點選"發票"
            $('#btnQtyStock').hide();
            $("#SignNo").val('');
            $("#pnlSIGN").hide();
            $("#pnlACCDATE").show();
            $("#CVendor").show();
            $("#pnlTICKET").show();
            $("#pnlPETTY").show();
            $('label[for="AccountDate"]').text("發票日期");
        }
        else if (item === "3") {        // 點選"簽單"
            $('#btnQtyStock').hide();
            $("#TicketDtl_TicketDtlNo").val('');
            $("#pnlTICKET").hide();
            $("#pnlPETTY").hide();
            $('#IsPettyN').prop("checked", true);
            $("#pnlACCDATE").show();
            $("#pnlSIGN").show();
            $('label[for="AccountDate"]').text("簽單日期");
            $('input:radio[name="IsPetty"]')
                .prop("disabled", true);
        }
        else {
            $('#btnQtyStock').show();    // 點選"庫存"
            $('#PartName').attr('readonly', true);
            $('#Price').attr('readonly', true);
            $("#CVendor").hide();
            $("#pnlTICKET").hide();
            $("#pnlPETTY").hide();
            $('#IsPettyN').prop("checked", true);
            $("#pnlSIGN").hide();
            $("#pnlACCDATE").hide();
        }
    });

    /* Auto calculate total price when input price or qty. */
    $('#Price').change(function () {
        var v1 = $(this).val();
        var v2 = $('#Qty').val();
        if (v1 !== null && v2 !== null) {
            $('#TotalCost').val(v1 * v2);
        }
    });
    $('#Qty').change(function () {
        var v1 = $(this).val();
        var v2 = $('#Price').val();
        if (v1 !== null && v2 !== null) {
            $('#TotalCost').val(v1 * v2);
        }
    });

    /* Get ticket seq. */
    $("#btnGETSEQ").click(function () {
        $.ajax({
            url: '../../Ticket/GetTicketSeq',
            type: "POST",
            async: true,
            success: function (data) {
                //console.log(data); // For Debug
                $("#TicketDtl_TicketDtlNo").val("AA" + data);
            }
        });

    });

    $("#modalVENDOR").on("hidden.bs.modal", function () {
        var vendorName = $("#Vno option:selected").text();
        var vendorId = $("#Vno option:selected").val();

        /* includes is not support in IE, so need to use indexOf. */
        if ($("#Vno option:selected").text() == "請選擇" || $("#Vno option:selected").text() == "查無廠商" ||
            $("#Vno option:selected").text().indexOf("請選擇廠商") != -1) {
            $("#VendorName").val("");
            $("#VendorId").val("");
        }
        else {
            $("#VendorName").val(vendorName);
            $("#VendorId").val(vendorId);
        }
    });

    /* Default settings.*/
    //$("#UniteNo").attr("disabled", "disabled");
    //$("input[type=radio][name=QryType]").change(function () {
    //    /* While select query type. */
    //    if (this.value == '關鍵字') {
    //        $("#KeyWord").removeAttr("disabled");
    //        $("#UniteNo").attr("disabled", "disabled");
    //    }
    //    else if (this.value == '統一編號') {
    //        $("#UniteNo").removeAttr("disabled");
    //        $("#KeyWord").attr("disabled", "disabled");
    //    }
    //});
});
