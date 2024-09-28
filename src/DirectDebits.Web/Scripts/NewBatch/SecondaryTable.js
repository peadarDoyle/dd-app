newBatch.secondaryTable = {

    table: null,

    init: function () {
        let tableSetting = newBatch.secondaryTable.getTableSettings();
        let dt = newBatch.secondaryTable.table = $('#invoice-modal .table').DataTable(tableSetting);

        function format(data) {
            let description = data.Description == null ? '…' : data.Description;
            return '<label>Description</label> ' + description;
        }

        // Array to track the ids of the details displayed rows
        var detailRows = [];

        $('#modal-allocation-table').on('click', 'tr td.details-control', function () {
            var tr = $(this).closest('tr');
            var row = dt.row(tr);
            var idx = $.inArray(tr.attr('id'), detailRows);

            if (row.child.isShown()) {
                tr.removeClass('details');
                row.child.hide();

                // Remove from the 'open' array
                detailRows.splice(idx, 1);
            }
            else {
                tr.addClass('details');
                let detailData = format(row.data());
                row.child(detailData).show();

                // Add to the 'open' array
                if (idx === -1) {
                    detailRows.push(tr.attr('id'));
                }
            }
        });

        // On each draw, loop over the `detailRows` array and show any child rows
        dt.on('draw', function () {
            $.each(detailRows, function (i, id) {
                $('#' + id + ' td.details-control').trigger('click');
            });
        });

    },

    setAllocationAmount: function(e) {
        e.preventDefault();

        $('#modal-allocation-table > tbody input:checked').each(function () {
            let account = newBatch.secondaryTable.getCurrentAccount();
            let period = $('#invoice-modal-period').data('days');
            let invoice = newBatch.secondaryTable.findInvoice($(this).parent(), period);
            $(this).parent().parent().find('td:nth-child(6) > input').val(invoice.Amount.toFixed(2));
        });
    },

    setDefaultAllocationAmount: function(e) {
        e.preventDefault();

        $('#modal-allocation-table > tbody input:checked').each(function () {
            let account = newBatch.secondaryTable.getCurrentAccount();
            let period = $('#invoice-modal-period').data('days');
            let invoice = newBatch.secondaryTable.findInvoice($(this).parent(), period);
            $(this).parent().parent().find('td:nth-child(6) > input').val(0);
        });
    },

    allocateIndividualInvoices: function(e) {
        $('#new-batch #modal-allocation-spinner').css('visibility','visible');

        setTimeout(function () {

            let allocationCollection = {
                period: $('#invoice-modal-period').data('days'),
                allocations: []
            };

            newBatch.secondaryTable.table.rows().every(function (rowIdx) {
                let allocationRows = $('#modal-allocation-table > tbody > tr:not(.details+tr)');
                let allocText = $(allocationRows[rowIdx]).find('> td:nth-child(6) > input').val();

                // an empty string input is taken to mean allocate 0 to that invoice
                if (allocText === '') allocText = '0';

                allocationCollection.allocations.push({ id: this.data().Id, alloc: allocText });
            });

            let account = newBatch.secondaryTable.getCurrentAccount();
            let result = account.validateCollection(allocationCollection);

            $('#modal-errors').html('');

            if (result.isValid) {
                account.allocateCollection(allocationCollection);
                newBatch.primaryTable.populate();
                $('#invoice-modal').modal('hide');
            } else {
                $('#modal-errors').append('<span>Resolve the following issues before continuing:</span>');
                $('#modal-errors').append('<ul>');
                $.each(result.results, function (index, value) {
                    if (!value.isValid) {
                        $('#modal-errors ul').append('<li>' + value.msg + '</li>');
                    }
                });
            }

            $('#new-batch #modal-allocation-spinner').css('visibility','hidden');

        }, 0);
    },

    cancel: function() {
        $('#modal-errors').html('');
    },

    populate: function(account, period, periodText) {

        newBatch.secondaryTable.disableMassAllocate(true);

        let invoices;

        switch (period) {
            case 'p1': invoices = account.P1;
                break;
            case 'p2': invoices = account.P2;
                break;
            case 'p3': invoices = account.P3;
                break;
            case 'older': invoices = account.Older;
                break;
        }

        $('#invoice-modal-period').data('days', period);
        $('#invoice-modal-period').text(periodText);

        // we reset the master check so it doesn't persist between modal uses
        $('#invoice-modal th input').prop('checked', false);

        $('#invoice-modal-id').text(account.DisplayId);
        $('#invoice-modal-name').text(account.Name);

        newBatch.secondaryTable.table.clear();

        invoices.forEach(function (invoice) {
            newBatch.secondaryTable.table.row.add({
                'Check': '<input type="checkbox"/>',
                'Id': invoice.Id,
                'YourRef': invoice.YourRef,
                'InvoiceDate': invoice.InvoiceDate,
                'DueDate': new Date(invoice.DueDate),
                'Description': invoice.Description,
                'Amount': invoice.Amount,
                'Alloc': '<input type="text" value="' + invoice.Alloc.toFixed(2) + '" />'
            });

        });

        newBatch.secondaryTable.table.draw();
    },

    disableMassAllocate: function(toggle) {
        $('#modal-allocate').prop('disabled', toggle);
        $('#modal-deallocate').prop('disabled', toggle);
    },

    checkMaster: function () {
        let isChecked = $(this).is(':checked');

        newBatch.secondaryTable.disableMassAllocate(!isChecked);

        let selector;

        // the selector slightly differs depending on whether we are checking or unchecking
        if (isChecked) {
            selector = '#invoice-modal .table > tbody input:not(:checked)';
        } else {
            selector = '#invoice-modal .table > tbody input:checked';
        }

        $(selector).each(function () {
            $(this).prop('checked', isChecked);
        });
    },

    checkRow: function () {
        let selector = '#invoice-modal .table';
        newBatch.baseTable.checker(selector, newBatch.secondaryTable)
    },

    findInvoice: function (element, period) {
        let invoiceTextId = $(element).parent().find('td:nth-child(2)').text();
        let id = parseInt(invoiceTextId, 10);

        let account = newBatch.secondaryTable.getCurrentAccount();
        let invoices;

        switch (period) {
            case 'p1': invoices = account.P1;
                break;
            case 'p2': invoices = account.P2;
                break;
            case 'p3': invoices = account.P3;
                break;
            case 'older': invoices = account.Older;
                break;
        }

        for (let i = 0; i < invoices.length; i++) {
            if (invoices[i].Id === id) {
                return invoices[i];
            }
        }

        return null;
    },

    getCurrentAccount: function () {
        let displayId = $('#invoice-modal-id').text();

        for (let i = 0; i < newBatch.accounts.length; i++) {
            if (newBatch.accounts[i].DisplayId === displayId) {
                return newBatch.accounts[i];
            }
        }

        return null;
    },

    getTableSettings: function() {
        return {
            columns: [
                { data: 'Check' },
                { data: 'Id' },
                { data: 'YourRef' },
                { render: function (data, type, full, meta) { return dateHelper.formatddMMyyyy(full.InvoiceDate); }},
                { render: function (data, type, full, meta) { return full.Amount.toFixed(2); }},
                { data: 'Alloc' },
                {
                    class: 'details-control',
                    orderable: false,
                    data: null,
                    defaultContent: ''
                }
            ],
            language: {
                info: 'Showing _TOTAL_ invoices',
                infoEmpty: '',
                emptyTable: 'No invoices for this period'
            },
            paging: false,
            searching: false,
            ordering: false
        };
    }
};

