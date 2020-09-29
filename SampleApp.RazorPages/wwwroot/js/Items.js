$("#gotoItem").change(function () {
    window.location = "/Items/" + $(this).val();
});

$("#btnDelete").click(function () {
    if (confirm("Are you sure?")) {
        let rowId = $("#rowId").val();
        let frm = document.getElementById("frmDelete");
        $("#deleteId").val(rowId);
        frm.submit();
    }
})