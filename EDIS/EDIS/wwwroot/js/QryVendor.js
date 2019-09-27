$(function () {
    $("#createVendorBtn").click(function () {
        $.ajax({
            url: '../../Vendor/Create2',
            type: "GET",
            async: false,
            success: function (data) {
                $("#pnlSELECTVENDOR").html(data);
            }
        });
    });

    $('#keyWordBtn').click(function () {
        $('#QryType').val("關鍵字");
    });
    $('#uniteNoBtn').click(function () {
        $('#QryType').val("統一編號");
    });

});