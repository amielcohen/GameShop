$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/product/getall' },
        "columns": [
            { data: 'gameName', "width": "15%" },
            { data: 'sku', "width": "5%" },

            {

                data: 'imageUrl',
                render: function (data, type, row) {
                    return '<img src="' + data + '" width="100%" style="border-radius:5px; border:1px solid #bbb9b9">';
                },
                "width": "15%" },
            { data: 'price', "width": "5%" },
            { data: 'discount', "width": "5%" },
            {

                data: null,
                render: function (data, type, row) {
                    let currentPrice;
                    if (row.discount > 0) {
                        currentPrice = (row.price - (row.price * row.discount / 100)).toFixed(2);
                    } else {
                        currentPrice = row.price.toFixed(2);
                    }
                    // Remove trailing zeros after the decimal point
                    currentPrice = currentPrice.replace(/\.?0+$/, "");
                    return currentPrice;
                },
                "width": "5%"
            },
            { data: 'releaseYear', "width": "5%" },
            { data: 'platform', "width": "5%" },
            { data: 'category.name', "width": "5%" },
            { data: 'stock', "width": "10%" },
            { data: 'soldSoFar', "width": "15%" },

            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                    <a href="/product/upsert?id=${data}" class="btn btn-primary mx-2" > <i class="bi bi-pencil-square"></i> EDIT</a>
                    <a onClick="Delete('/product/delete/${data}')" class="btn btn-danger mx-2"> <i class="bi bi-trash3"></i> DELETE</a>
                    </div>`
                },
                "width": "5%"
            }

        ]
    });
}



function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    });
}

function doNothing(url) {

}