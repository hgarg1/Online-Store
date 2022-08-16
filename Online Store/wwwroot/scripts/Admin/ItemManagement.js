
const ItemManagement = {

    Items: [],
    currentId: 0,
    bindItems: function () {
        var _this = this;
        $.each(this.Items, function (index, element) {
            $(`<tr id=Row${index}></tr>`).appendTo($("#TableBody"));
            $(`<td>${index+1}</td>`).appendTo($(`#Row${index}`));
            $(`<td>${element.name}</td>`).appendTo($(`#Row${index}`));
            $(`<td>$${element.price}</td>`).appendTo($(`#Row${index}`));
            $(`<td>${element.quantity}</td>`).appendTo($(`#Row${index}`));
            $(`<td>${element.supplier}</td>`).appendTo($(`#Row${index}`));
            $(`<button type="button" onclick="ItemManagement.EditItem(${element.id});" class="btn btn-warning bg-warning w-100"><svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-pencil-square" viewBox="0 0 16 16"><path d="M15.502 1.94a.5.5 0 0 1 0 .706L14.459 3.69l-2-2L13.502.646a.5.5 0 0 1 .707 0l1.293 1.293zm-1.75 2.456-2-2L4.939 9.21a.5.5 0 0 0-.121.196l-.805 2.414a.25.25 0 0 0 .316.316l2.414-.805a.5.5 0 0 0 .196-.12l6.813-6.814z"/><path fill-rule="evenodd" d="M1 13.5A1.5 1.5 0 0 0 2.5 15h11a1.5 1.5 0 0 0 1.5-1.5v-6a.5.5 0 0 0-1 0v6a.5.5 0 0 1-.5.5h-11a.5.5 0 0 1-.5-.5v-11a.5.5 0 0 1 .5-.5H9a.5.5 0 0 0 0-1H2.5A1.5 1.5 0 0 0 1 2.5v11z"/></svg> Edit Item</button>`).appendTo($(`#Row${index}`));
            $(`<button type="button" onclick="ItemManagement.ViewItem(${element.id});" class="btn btn-primar bg-primary w-100"><svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-search" viewBox="0 0 16 16"><path d="M11.742 10.344a6.5 6.5 0 1 0-1.397 1.398h-.001c.03.04.062.078.098.115l3.85 3.85a1 1 0 0 0 1.415-1.414l-3.85-3.85a1.007 1.007 0 0 0-.115-.1zM12 6.5a5.5 5.5 0 1 1-11 0 5.5 5.5 0 0 1 11 0z"/></svg> Details</button>`).appendTo($(`#Row${index}`));
            $(`<button type="button" onclick="ItemManagement.DeleteItem(${element.id});" class="btn btn-danger bg-danger w-100"><svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-trash" viewBox="0 0 16 16"><path d="M5.5 5.5A.5.5 0 0 1 6 6v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm2.5 0a.5.5 0 0 1 .5.5v6a.5.5 0 0 1-1 0V6a.5.5 0 0 1 .5-.5zm3 .5a.5.5 0 0 0-1 0v6a.5.5 0 0 0 1 0V6z"/><path fill-rule="evenodd" d="M14.5 3a1 1 0 0 1-1 1H13v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V4h-.5a1 1 0 0 1-1-1V2a1 1 0 0 1 1-1H6a1 1 0 0 1 1-1h2a1 1 0 0 1 1 1h3.5a1 1 0 0 1 1 1v1zM4.118 4 4 4.059V13a1 1 0 0 0 1 1h6a1 1 0 0 0 1-1V4.059L11.882 4H4.118zM2.5 3V2h11v1h-11z"/></svg> Delete Item</button>`).appendTo($(`#Row${index}`));
        });
    },
    EditItem: function (id) {
        this.currentId = id;
    },
    ViewItem: function (id) {
        this.currentId = id;
    },
    DeleteItemAll: function () {
        $("#DeleteItemModal .modal-body").text("Are you sure you want to delete ALL items? Users will no longer be able to purchase any item and will disappear from the client side immediately.");
        $("#DeleteItemModal  .modal-footer .btn-danger").text("Delete ALL Items");
        $("#DeleteItemModal .modal-footer .btn-danger").removeAttr("onclick");
        $("#DeleteItemModal .modal-footer .btn-danger").attr("onclick", "ItemManagement.RequestDeleteItemAll();");
        $("#DeleteItemModal").modal("show");
    },
    RequestDeleteItemAll: function () {

        var _this = this;
        $.each(_this.Items, function (index, item) {
            _this.currentId = item.id;
            _this.RequestDeleteItem();
        });
    },
    RequestDeleteItem: function () {
        $.ajax({
            url: "/Admin/Item/DeleteItem/" + this.currentId,
            method: "DELETE",
            success: function (data, status, jqXHR) {
                window.location.reload(true);
            }
        });
    },
    DeleteItem: function (id) {
        this.currentId = id;
        $("#DeleteItemModal modal-footer > button").text("Delete Item");
        $("#DeleteItemModal").modal("show");
    },
    BindCategories: function () {

        $.ajax({
            url: "/Item/GetCategories",
            method: "GET",
            success: function (data, status, jqXHR) {

                $.each(data, function (index, element) {
                    $(`<option>${element.name}</option>`).appendTo($("#Category"));
                });
            }
        });
    },
    ItemManagement: function (data) {
        var _this = this;
        $.each(data, function (index, element) {
            _this.Items.push({
                id: element.id,
                name: element.name,
                pictureLocation: element.pictureLocation,
                price: element.price,
                quantity: element.quantity,
                supplier: element.supplier
            })
        });
        console.log(this.Items);
    }
}


function main() {

    $.ajax({
        url: "/Item/GetItems",
        method: "GET",
        success: function (data, status, jqXHR) {
            ItemManagement.ItemManagement(data);
            ItemManagement.bindItems();
        },
        error: function (jqXHR, status) {
        }
    });
    ItemManagement.BindCategories();

    window.onload = function () {
        let queryParams = new URLSearchParams(window.location.search);
        let success = queryParams.get("success");
        if (success == 'true') {
            $("#AddItemSuccessModal .modal-header").text("Success! Item Added Successfully!")
            $("#AddItemSuccessModal .modal-body").text("The item you keyed in was added successfully! It will now appear on the client side immediately.")
            $("#AddItemSuccessModal").modal("show");

        } else if (success == 'false') {
            $("#AddItemSuccessModal .modal-header").text("Error! Item Added NOT Successfully!")
            $("#AddItemSuccessModal .modal-body").text("The item you keyed in was NOT added successfully! It will NOT appear on the client side, please try again and check for form errors.")
            $("#AddItemSuccessModal").modal("show");
        }
    }
}
main();