var map = new Map();

function buildGrid(data) {
    let rowIndex = 0;
    $.each(data, function (index, item) {
        if (index % 3 == 0) {
            $(`<div class='row w-100' id=Row${rowIndex}></div>`).appendTo($("div.d-grid"));
            rowIndex++;
        }
        let pictures = item.pictureLocation.split(";");
        map.set(item.name, pictures);
        $(`<div class="card col" style="max-width: 30vw">
                <h5 class="card-header">${item.name}</h5>
                <img class="card-img-top" src = "https://archieonlinestore.blob.core.windows.net/images/${pictures[0]}" alt = "Card image cap" style="max-width: 500px" >
                <div class="card-body">
                    <p class="card-text">Description: ${item.description}</p>
                    <p class="card-text">Price: $${item.price}</p>
                    <p class="card-text">Quantity: ${item.quantity} units</p>
                    <p class="card-text">Color: ${item.color}</p>
                    <p class="card-text">Category: ${item.category}</p>
                    <p class="card-text">Supplier: ${item.supplier}</p>
                </div>
                <div class="card-footer">
                    <form method="post" class="d-flex" action="/Item/Buy">
                        <input type="hidden" name="itemId" value="${item.id}" />
                        <input type="submit" class="btn btn-primary ml-1" value="Buy Item" />
                        <div>
                            <label for="item${index}">Quantity:</label> <input id="item${index}" name="itemQuantity" class="form-control w-50 d-inline" min="0" max="${item.quantity}" type="number">
                        </div>
                    </form>
                </div>
            </div>`).appendTo($(`#Row${rowIndex - 1}`));
    });
}



function main() {

    $.ajax({
        url: "/Item/GetItems",
        method: "GET",
        success: function (data, status, jqXHR) {
            buildGrid(data);
            $(".card-body").click(function (event) {
                $("#itemModal").modal("show");
            });
        },
        error: function (status, jqXHR) {
        }
    });
}

main();