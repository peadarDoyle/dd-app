newBatch.onWindowLoad = function () {
    newBatch.registerEvents();
    newBatch.secondaryTable.init();
}

newBatch.onReadyStateChange = function () {
    if (document.readyState === 'complete') {
        $('.batch-setting-name').text(mode.current.name);
        $('.batch-setting-short-name').text(mode.current.shortName);
        $('.batch-setting-account-type').text(mode.current.accountType);
    }
}

newBatch.registerEvents = function () {

    $('#load-accounts button').on('click', function () { newBatch.filter(newBatch.primaryTable.init) });

    // handles the processing of the user select allocations
    $('#process button.btn-primary').on('click', newBatch.processor.openModal);
    $('#process-modal button.btn-primary').on('click', newBatch.processor.beginProcessing);

    // accessing individual invoices
    $('#allocation-table').on('click', '> tbody > tr > td:nth-child(4)', function () { newBatch.primaryTable.showIndividualInvoices(this, "p1"); });
    $('#allocation-table').on('click', '> tbody > tr > td:nth-child(5)', function () { newBatch.primaryTable.showIndividualInvoices(this, "p2"); });
    $('#allocation-table').on('click', '> tbody > tr > td:nth-child(6)', function () { newBatch.primaryTable.showIndividualInvoices(this, "p3"); });
    $('#allocation-table').on('click', '> tbody > tr > td:nth-child(7)', function () { newBatch.primaryTable.showIndividualInvoices(this, "older"); });

    // handles checkbox ticking/unticking for the primary allocation table
    $('#allocation-table').on('click', '> thead input[type="checkbox"]', newBatch.primaryTable.checkMaster);
    $('#allocation-table').on('click', '> tbody input[type="checkbox"]', newBatch.primaryTable.checkRow);

    // handles checkbox ticking/unticking for the modal allocation table
    $('#invoice-modal .table').on('click', '> thead input[type="checkbox"]', newBatch.secondaryTable.checkMaster);
    $('#invoice-modal .table').on('click', '> tbody input[type="checkbox"]', newBatch.secondaryTable.checkRow);

    // handles batch allocating for the primary allocation table
    $('#allocate-all + .dropdown-menu').on('click', '.all-invoices', function (e) { newBatch.primaryTable.allocateInvoices(e, 'all'); });
    $('#allocate-all + .dropdown-menu').on('click', '.period1-invoices', function (e) { newBatch.primaryTable.allocateInvoices(e, 'p1'); });
    $('#allocate-all + .dropdown-menu').on('click', '.period2-invoices', function (e) { newBatch.primaryTable.allocateInvoices(e, 'p2'); });
    $('#allocate-all + .dropdown-menu').on('click', '.period3-invoices', function (e) { newBatch.primaryTable.allocateInvoices(e, 'p3'); });
    $('#allocate-all + .dropdown-menu').on('click', '.old-period-invoices', function (e) { newBatch.primaryTable.allocateInvoices(e, 'older'); });

    // handles batch deallocating for the primary allocation table
    $('#deallocate-all + .dropdown-menu').on('click', '.all-invoices', function (e) { newBatch.primaryTable.deallocateInvoices(e, 'all'); });
    $('#deallocate-all + .dropdown-menu').on('click', '.period1-invoices', function(e) { newBatch.primaryTable.deallocateInvoices(e, 'p1'); });
    $('#deallocate-all + .dropdown-menu').on('click', '.period2-invoices', function(e) { newBatch.primaryTable.deallocateInvoices(e, 'p2'); });
    $('#deallocate-all + .dropdown-menu').on('click', '.period3-invoices', function(e) { newBatch.primaryTable.deallocateInvoices(e, 'p3'); });
    $('#deallocate-all + .dropdown-menu').on('click', '.old-period-invoices', function(e) { newBatch.primaryTable.deallocateInvoices(e, 'older'); });

    $('#modal-allocate').on('click', newBatch.secondaryTable.setAllocationAmount);
    $('#modal-deallocate').on('click', newBatch.secondaryTable.setDefaultAllocationAmount);

    $('#invoice-modal .modal-footer button.btn-default').on('click', newBatch.secondaryTable.cancel);
    $('#invoice-modal .modal-footer button.btn-primary').on('click', newBatch.secondaryTable.allocateIndividualInvoices);
};