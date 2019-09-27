$(function () {
    $("#DocType").prop('readonly', true);
    $("#DocId").prop('readonly', true);

    //$("#btnUPLOAD").click(function () {
    //    uploadFiles();
    //});
});

//function uploadFiles() {

//    var files = document.getElementById("Files");
//    var formData = new FormData();

//    for (var i = 0; i <= files.length; i++) {
//        formData.append("files", files[i]);
//    }

//    $.ajax(
//        {
//            url: "../../AttainFile/Upload2",
//            data: formData,
//            processData: false,
//            contentType: false,
//            type: "POST",
//            success: function (data) {
//                alert("Files Uploaded!");
//            }
//        }
//    );
//}