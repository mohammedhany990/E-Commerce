$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {

    dataTable = $('#tableData').DataTable({
        "ajax": { url: '/api/apiData/GetAllProducts' },
        "columns": [
            { data: 'title', "width": "25%" },
            { data: 'isbn', "width": "15%" },
            { data: 'listPrice', "width": "10%" },
            { data: 'author', "width": "15%" },
            { data: 'category.name', "width": "10%" },
            {
                data: 'id',
                'render': function (data) {
                    return `<div class="w-75 btn-group" role="group">
            <a href="/product/edit/${data}" class="btn btn-success mx-2">
            <i class="bi bi-pencil-square"></i> Edit
            </a>

            <a href="/product/delete/${data}" class="btn btn-danger mx-2" onclick="confirmDelete(${data})">
            <i class="bi bi-trash"></i> Delete
            </a>

            <a href="/product/details/${data}" class="btn btn-primary mx-2">
            <i class="bi bi-info-circle"></i> Details
            </a>

        </div>`
                },
                "width": "25%"
            }

        ]
    });
}
