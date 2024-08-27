$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {

    dataTable = $('#tableData').DataTable({
        "ajax": { url: '/api/apiData/GetAllCompanies' },
        "columns": [
            { data: 'name', "width": "25%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'streetAddress', "width": "10%" },
            { data: 'city', "width": "15%" },
            { data: 'state', "width": "10%" },
            {
                data: 'id',
                'render': function (data) {
                    return `<div class="w-75 btn-group" role="group">
            <a href="/company/edit/${data}" class="btn btn-success mx-2">
            <i class="bi bi-pencil-square"></i> Edit
            </a>

            <a href="/company/delete/${data}" class="btn btn-danger mx-2" onclick="confirmDelete(${data})">
            <i class="bi bi-trash"></i> Delete
            </a>

            <a href="/company/details/${data}" class="btn btn-primary mx-2">
            <i class="bi bi-info-circle"></i> Details
            </a>

        </div>`
                },
                "width": "25%"
            }

        ]
    });
}
