function main(args) {
    $("#DeleteUserForm").submit(function (event) {
        event.preventDefault();
        $("#deleteUserModal").modal("show");
    });
}

function deleteUser() {

    $.ajax({
        url: "/User/Delete",
        method: "POST",
        success: function (data, status, jqXHR) {
            window.location.assign(data);
        }
    });
}

main(null);