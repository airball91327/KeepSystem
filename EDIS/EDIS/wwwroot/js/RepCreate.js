var onFailed = function (data) {
    alert(data.responseText);
    $.Toast.hideToast();
};
$.fn.addItems = function (data) {

    return this.each(function () {
        var list = this;
        $.each(data, function (val, text) {

            var option = new Option(text.text, text.value);
            list.add(option);
        });
    });

};

$(function () {

    SetEngsDropDown();

    /* Prevent submit for pressing Enter. */
    $('input').keypress(function (e) {
        code = e.keyCode ? e.keyCode : e.which; // in case of browser compatibility
        if (code == 13) {
            e.preventDefault();
            // do something
            /* also can use return false; instead. */
        }
    });

    $('#Building').change(function () {

        /* Get floors. */
        $.ajax({
            url: '../Floor/GetFloors',
            type: "POST",
            dataType: "json",
            data: "bname=" + $(this).val(),
            async: false,
            success: function (data) {
                var select = $('#Floor');
                $('option', select).remove();
                select.addItems(data);
            }
        });
        /* Get places. */
        var bId = $(this).val();
        var fId = $('#Floor').val();
        $.ajax({
            url: '../Place/GetPlaces',
            type: "POST",
            dataType: "json",
            data: { buildingId: bId, floorId: fId },
            async: false,
            success: function (data) {
                var select = $('#Area');
                $('option', select).remove();
                select.addItems(data);
            }
        });

        var dptId = $('#Area').val().split("-", 1);
        
        /* Get AccDptId and Name. */
        $("#AccDpt").val(dptId);
        $.ajax({
            url: '../Repair/GetDptName',
            type: "POST",
            dataType: "json",
            data: { dptId: dptId },
            async: false,
            success: function (data) {
                if (data == "") {
                    $("#AccDptNameErrorMsg").html("查無部門!");
                }
                else {
                    $("#AccDptNameErrorMsg").html("");
                }
                $("#AccDptName").val(data);  
            }
        });  
        /* Get engineers. */
        $.ajax({
            url: '../Repair/GetAreaEngId',
            type: "POST",
            dataType: "json",
            data: {
                BuildingId: $('#Building').val(),
                FloorId: $('#Floor').val(),
                PlaceId: $('#Area').val()
            },
            async: false,
            success: function (data) {
                $('#EngId').val(data.engId);
                $('#EngName').val(data.fullName);
                //var select = $('#EngId');
                //$('option', select).remove();
                //select.addItems(data);
                //console.log(data + ";" + select.val()); // ForDebug
            }
        });
    });

    $('#Floor').change(function () {

        /* Get places. */
        var bId = $("#Building").val();
        var fId = $(this).val();
        $.ajax({
            url: '../Place/GetPlaces',
            type: "POST",
            dataType: "json",
            data: { buildingId: bId, floorId: fId },
            async: false,
            success: function (data) {
                var select = $('#Area');
                $('option', select).remove();
                select.addItems(data);
            }
        });
        $('#Area').trigger("change");
    });

    $('#Area').change(function () {
        var dptId = $('#Area').val().split("-", 1);
        /* Get AccDptId and Name. */
        $("#AccDpt").val(dptId);
        $.ajax({
            url: '../Repair/GetDptName',
            type: "POST",
            dataType: "json",
            data: { dptId: dptId },
            async: false,
            success: function (data) {
                if (data == "") {
                    $("#AccDptNameErrorMsg").html("查無部門!");
                }
                else {
                    $("#AccDptNameErrorMsg").html("");
                }
                $("#AccDptName").val(data);
            }
        });    
        /* Get engineers. */
        $.ajax({
            url: '../Repair/GetAreaEngId',
            type: "POST",
            dataType: "json",
            data: {
                BuildingId: $('#Building').val(),
                FloorId: $('#Floor').val(),
                PlaceId: $('#Area').val()
            },
            async: false,
            success: function (data) {
                $('#EngId').val(data.engId);
                $('#EngName').val(data.fullName);
                //var select = $('#EngId');
                //$('option', select).remove();
                //select.addItems(data);
                //console.log(data + ";" + select.val()); // ForDebug
            }
        });
    });

    /* Default setting. */
    $("#rowAssetNo").hide();
    $("#rowAssetAccDate").hide();
    //$("#rowAssetName").hide();

    $("input[type=radio][name=assetControl]").change(function () {
        /* While has asset, show assetNo and assetName to input. */
        if (this.value == 'true') {
            $("#rowAssetNo").show();
            $("#rowAssetAccDate").show();
            //$("#rowAssetName").show();
        }
        else if (this.value == 'false') {
            $("#rowAssetNo").hide();
            $("#rowAssetAccDate").hide();
            //$("#rowAssetName").hide();
        }       
    });

    /* While user change DptId, search the DptName. */
    $("#DptId").change(function () {
        var DptId = $(this).val();
        $.ajax({
            url: '../Repair/GetDptName',
            type: "POST",
            dataType: "json",
            data: { dptId: DptId },
            success: function (data) {
                //console.log(data);
                if (data == "") {
                    $("#DptNameErrorMsg").html("查無部門!");
                }
                else {
                    $("#DptNameErrorMsg").html("");
                }
                $("#DptName").val(data);
            }
        });       
    });
    /* While user change AccDpt, search the AccDptName. */
    $("#AccDpt").change(function () {
        var AccDptId = $(this).val();
        $.ajax({
            url: '../Repair/GetDptName',
            type: "POST",
            dataType: "json",
            data: { dptId: AccDptId },
            success: function (data) {
                //console.log(data);
                if (data == "") {
                    $("#AccDptNameErrorMsg").html("查無部門!");
                }
                else {
                    $("#AccDptNameErrorMsg").html("");
                }
                $("#AccDptName").val(data);  
            }
        });
        GetAccDptLocation(AccDptId);
    });

    /* If user select "本單位", hide select location options. */
    GetDptLocation($("#DptId").val());
    $("input[type=radio][name=LocType]").change(function () {
        if (this.value == '本單位') {

            GetDptLocation($("#DptId").val());
            
            $("#AccDpt").attr("readonly", "readonly");
            $("#AccDptName").attr("readonly", "readonly");
            /* Get AccDptId and Name. */
            $("#AccDpt").val($("#DptId").val());
            $.ajax({
                url: '../Repair/GetDptName',
                type: "POST",
                dataType: "json",
                data: { dptId: $("#DptId").val() },
                success: function (data) {
                    if (data == "") {
                        $("#AccDptNameErrorMsg").html("查無部門!");
                    }
                    else {
                        $("#AccDptNameErrorMsg").html("");
                    }
                    $("#AccDptName").val(data);
                }
            });  
        }
        else {
            $("#divLocations").show();
            $("#AccDpt").removeAttr("readonly");
            $("#AccDptName").removeAttr("readonly");
        }
    });

    /* If user select "增設", show Mgr dropdown for user to select. */
    $("#DptMgr").hide();    //Default setting.
    $("input[type=radio][name=RepType]").change(function () {
        if (this.value == '增設') {
            $("#DptMgr").show();
        }
        else {
            $("#DptMgr").hide();
        }
    });
    /* Get managers by query string. */
    $("#MgrQryBtn").click(function () {
        var queryStr = $("#DptMgrQry").val();
        $.ajax({
            url: '../Repair/QueryUsers',
            type: "GET",
            data: { QueryStr: queryStr },
            success: function (data) {
                var select = $('#DptMgrId');
                $('option', select).remove();
                select.addItems(data);
            }
        });
    });
    $("#CheckerQryBtn").click(function () {
        var queryStr = $("#CheckerQry").val();
        $.ajax({
            url: '../Repair/QueryUsers',
            type: "GET",
            data: { QueryStr: queryStr },
            success: function (data) {
                var select = $('#CheckerId');
                $('option', select).remove();
                select.addItems(data);
            }
        });
    });
    $('#modalFILES').on('hidden.bs.modal', function () {
        var docid = $("#DocId").val();
        $.ajax({
            url: '../AttainFile/List3',
            type: "POST",
            data: { docid: docid, doctyp: "1" },
            success: function (data) {
                $("#pnlFILES").html(data);
            }
        });
    });
});

function onSuccess() {
    $.Toast.hideToast();
    alert("已送出");

    var DocId = $("#DocId").val();
    var repType = $('input:radio[name="RepType"]:checked').val();
    /* Print confirm before submit. */
    var r = confirm("是否列印?");
    if (r == true) {
        window.printRepairDoc(DocId);
    }

    var isMobile = $("#isMobile").val();
    if (isMobile == 'Y') {
        location.href = '../Mobile/Repair/Index';
    }
    else {
        location.href = '../Home/Index';
    }  
}

function getAssetName() {
    var AssetNo = $("#AssetNo").val();
    $.ajax({
        url: '../Repair/GetAssetName',
        type: "POST",
        dataType: "json",
        data: { assetNo: AssetNo },
        success: function (data) {
            //console.log(data); // debug
            if (data == "") {
                $("#AssetNameErrorMsg").html("查無資料!");
            }
            else {
                $("#AssetNameErrorMsg").html("");
            }
            $("#AssetName").val(data.cname);
            $("#assetAccDate").val(data.accDate);
        }
    });  
}


function printRepairDoc(DocId) {
    
    var printContent = "";
    /* Get print page. */
    $.ajax({
        url: '../Repair/PrintRepairDoc',
        type: "GET",
        async: false,
        data: { docId: DocId, printType : 1 },
        success: function (data) {
            printContent = data;
        }
    });
    $.ajaxSettings.async = true; // Set this ajax async back to true.
    /* New a window to print. */
    var printPage = window.open();
    printPage.document.open();
    printPage.document.write("<BODY onload='window.print();window.close()'>");
    printPage.document.write(printContent);
    printPage.document.close();
}

/* Get and check the user department's location, if has many location, show #divLocations. */
function GetDptLocation(DptId) { 
    $.ajax({
        url: '../Repair/GetDptLoc',
        type: "POST",
        dataType: "json",
        data: { dptId: DptId },
        success: function (data) {
            //console.log(data); //Debug
            if (data == "查無地點") {
                $("#divLocations").hide();
                alert("查無部門地點!");
            }
            else if (data == "多個地點") {
                $("#divLocations").show();
            }
            else {          
                $("#Building").val(data.buildingId);
                $('#Building').trigger("change");
                $("#Floor").val(data.floorId);
                $('#Floor').trigger("change");
                $("#Area").val(data.placeId);
                $('#Area').trigger("change");
       
                $("#divLocations").hide();
            }
        }
    });
}

/* Get and check the AccDpt's location. */
function GetAccDptLocation(DptId) {
    $.ajax({
        url: '../Repair/GetDptLoc',
        type: "POST",
        dataType: "json",
        data: { dptId: DptId },
        success: function (data) {
            //console.log(data); //Debug
            if (data == "查無地點") {
                alert("查無部門地點!");
            }
            else if (data == "多個地點") {
                alert("地點不只一處，請自行選取!");
            }
            else {
                $("#Building").val(data.buildingId);
                $('#Building').trigger("change");
                $("#Floor").val(data.floorId);
                $('#Floor').trigger("change");
                $("#Area").val(data.placeId);
                $('#Area').trigger("change");
            }
        }
    });
}

function SetEngsDropDown() {
    $.ajax({
        url: '../Repair/GetAllEngs',
        type: "GET",
        dataType: "json",
        data: { },
        success: function (data) {
            //console.log(data); // For debug.
            var select = $('#PrimaryEngId');
            var i = 0;
            var defaultOption = 0;
            var displayTrigger = 0;
            select.empty();
            $.each(data, function (index, item) {  // item is now an object containing properties 
                if (i === defaultOption) {
                    select.append($('<option selected="selected"></option>').text("無").val(0));
                }
                if (item.dptId != displayTrigger) {
                    switch (item.dptId) {
                        case '8411':
                            select.append($('<optgroup label="工務一課"></optgroup>'));
                            break;
                        case '8412':
                            select.append($('<optgroup label="工務二課"></optgroup>'));
                            break;
                        case '8413':
                            select.append($('<optgroup label="工務三課-中華院區工務組"></optgroup>'));
                            break;
                        case '8414':
                            select.append($('<optgroup label="工務三課-教研工務組"></optgroup>'));
                            break;
                        case '8430':
                            select.append($('<optgroup label="營建部"></optgroup>'));
                            break;
                    }
                    displayTrigger = item.dptId;
                }
                select.append($('<option></option>').text(item.fullName).val(item.id));
                i++;
            });
        }
    });
}