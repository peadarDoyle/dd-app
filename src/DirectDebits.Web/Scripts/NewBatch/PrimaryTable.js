newBatch.primaryTable = {

    table: null,
    init: function (classifications) {
        let tableSettings = newBatch.primaryTable.getTableSettings(classifications);

        $.fn.dataTable.ext.errMode = function (settings, techNote, message) {
            // todo: ...
        };

        newBatch.primaryTable.table = $('#allocation-table').DataTable(tableSettings);
    },

    allocateInvoices: function (e, period) {
        e.preventDefault();
        $('#new-batch #primary-allocation-spinner').css('visibility','visible');

        setTimeout(function () {

            $('#allocation-table > tbody input:checked').each(function () {
                let account = newBatch.primaryTable.findAccount($(this).parent());

                switch (period) {
                    case 'all': account.allocateAll(); break;
                    case 'p1': account.allocateP1(); break;
                    case 'p2': account.allocateP2(); break;
                    case 'p3': account.allocateP3(); break;
                    case 'older': account.allocateOlder(); break;
                }
            });

            newBatch.primaryTable.populate();
            $('#new-batch #primary-allocation-spinner').css('visibility','hidden');
        }, 0);
    },

    deallocateInvoices: function(e, period) {
        e.preventDefault();
        $('#new-batch #primary-allocation-spinner').css('visibility','visible');

        setTimeout(function () {

            $('#allocation-table > tbody input:checked').each(function () {
                let account = newBatch.primaryTable.findAccount($(this).parent());

                switch (period) {
                    case 'all': account.deallocateAll(); break;
                    case 'p1': account.allocateP1(0); break;
                    case 'p2': account.allocateP2(0); break;
                    case 'p3': account.allocateP3(0); break;
                    case 'older': account.allocateOlder(0); break;
                }
            });

            newBatch.primaryTable.populate();
            $('#new-batch #primary-allocation-spinner').css('visibility','hidden');
        }, 0);
    },

    updateTotals: function () {
        let totalP1 = 0, totalP2 = 0, totalP3 = 0, totalOlder = 0, totalAll = 0, totalDD = 0;

        for (let i = 0; i < newBatch.accounts.length; i++) {
            let _p1 = newBatch.accounts[i].getP1Total();
            let _p2 = newBatch.accounts[i].getP2Total();
            let _p3 = newBatch.accounts[i].getP3Total();

            totalP1 += _p1;
            totalP2 += _p2;
            totalP3 += _p3;
            totalOlder += newBatch.accounts[i].getOlderTotal();
            totalAll += _p1 + _p2 + _p3;
            totalDD += newBatch.accounts[i].getAllocated();
        }

        $('#older-total').val(totalOlder);
        $('#dd-total').val(totalDD);
    },

    populate: function () {
        newBatch.primaryTable.table.clear();
        newBatch.accounts.forEach(function (account) {
            newBatch.primaryTable.table.row.add(account);
        });

        let callback = newBatch.primaryTable.update;
        newBatch.primaryTable.table.draw(callback);
    },

    update: function () {

        // on the first call the table is null - this may or may not be a bug - requires further investigation
        if (newBatch.primaryTable.table === null) return;

        let ddColumnIndex = 8;
        let column = newBatch.primaryTable.table.column(ddColumnIndex);
        let total = column.data().reduce((a, b) => a + b, 0);
        $(column.footer()).html(total.toFixed(2));
    },

    disableMassAllocate: function(toggle) {
        $('#allocate-all').prop('disabled', toggle);
        $('#deallocate-all').prop('disabled', toggle);
    },

    checkMaster: function () {
        let isChecked = $(this).is(':checked');

        newBatch.primaryTable.disableMassAllocate(!isChecked);

        let selector;

        // the selector slightly differes depending on whether we are checking or unchecking
        if (isChecked) {
            selector = '#allocation-table > tbody input:not(:checked)';
        } else {
            selector = '#allocation-table > tbody input:checked';
        }

        $(selector).each(function () {
            // update the acutal checkbox element
            $(this).prop('checked', isChecked);

            // update the data model
            let account = newBatch.primaryTable.findAccount($(this).parent());
            let check = isChecked ? 'checked ' : '';
            account.Check = '<input type="checkbox" ' + check + '/>';
        });
    },

    checkRow: function () {
        let isChecked = $(this).is(':checked');

        let account = newBatch.primaryTable.findAccount($(this).parent());

        if (isChecked) {
            account.Check = '<input type="checkbox" checked/>';
        } else {
            account.Check = '<input type="checkbox" />';
        }

        let selector = '#allocation-table';
        newBatch.baseTable.checker(selector, newBatch.primaryTable)
    },

    showIndividualInvoices: function (element, period) {

        let account = newBatch.primaryTable.findAccount(element);
        let periodText = $(element).closest('table').find('th').eq($(element).index()).data('days');
        newBatch.secondaryTable.populate(account, period, periodText);
        $('#invoice-modal').modal({
            backdrop: 'static'
        });
    },

    findAccount: function (element) {
        let displayId = $(element).parent().find('td:nth-child(2)').text();

        for (let i = 0; i < newBatch.accounts.length; i++) {
            if (newBatch.accounts[i].DisplayId === displayId) {
                return newBatch.accounts[i];
            }
        }

        return null;
    },

    getTableSettings: function(classifications) {

        let renderPeriodTotal = function (total, hasCreditNote) {
            let suffix = hasCreditNote ? '<span class="has-creditnote">*</span>' : '';
            return total.toFixed(2) + suffix;
        };

        return {
            columns: [
                { className: 'check', type: 'html', data: 'Check' },
                { className: 'displayId', type: 'num', data: 'DisplayId' },
                { className: 'account', type: 'string', data: 'Name' },
                {
                    className: 'period1', type: 'string', data: 'getP1Total', render: function (data, type, full, meta) {
                        return renderPeriodTotal(full.getP1Total(), full.anyCreditNotes('p1'));
                    }
                },
                {
                    className: 'period2', type: 'string', data: 'getP2Total', render: function (data, type, full, meta) {
                        return renderPeriodTotal(full.getP2Total(), full.anyCreditNotes('p2'));
                    }
                },
                {
                    className: 'period3', type: 'string', data: 'getP3Total', render: function (data, type, full, meta) {
                        return renderPeriodTotal(full.getP3Total(), full.anyCreditNotes('p3'));
                    }
                },
                {
                    className: 'older', type: 'string', data: 'getOlderTotal', render: function (data, type, full, meta) {
                        return renderPeriodTotal(full.getOlderTotal(), full.anyCreditNotes('older'));
                    }
                },
                {
                    className: 'total', type: 'num', data: 'getTotal', render: function (data, type, full, meta) {
                        return full.getTotal().toFixed(2);
                    }
                },
                {
                    className: 'dd', type: 'num', data: 'getAllocated', render: function (data, type, full, meta) {
                        return full.getAllocated().toFixed(2);
                    }
                }
            ],
            drawCallback: newBatch.primaryTable.update,
            ajax: {
                dataType: 'json',
                url: '/batches/' + mode.current.type + '/invoices?' + classifications,
                success: function (data) {

                    if (data.length === 0) {
                        $('#loading-records').html('No ' + mode.current.shortName + ' accounts found');
                    }

                    newBatch.accounts = data.map(function (c) {
                        return new Account(c.Id, c.DisplayId, c.Name, c.P1, c.P2, c.P3, c.Older);
                    });

                    newBatch.primaryTable.populate();
                },
                error: function () {
                    $('#loading-records i').removeClass();
                    $('#loading-records i').addClass('fa fa-exclamation-circle text-danger');
                    $('#validation').html('<p><strong>An error occured</strong></p><p>The invoice data could not be loaded. If this issue persists please contact support.</p>');
                    $('#validation p').addClass('text-danger');
                }
            },
            language: {
                loadingRecords: '<div id="loading-records"><i style="margin-bottom:10px;" class="fa fa-spinner fa-pulse fa-spin fa-1x fa-fw" aria-hidden="true"></i></div>',
                info: 'Showing _TOTAL_ ' + mode.current.accountType + 's',
                infoEmpty: '',
                emptyTable: "There are no invoices available to be processed"
            },
            paging: false,
            searching: false,
            ordering: false,
            destroy: true
        };
    }
};

