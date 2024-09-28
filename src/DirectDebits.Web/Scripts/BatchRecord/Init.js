batchRecord.onWindowLoad = function () {
    batchRecord.registerEvents();
    batchRecord.init();
}

batchRecord.registerEvents = function () {
    $('#generate-file').on('click', batchRecord.bankFile.openModal);
    $('#batch-record').on('click', '#hasnewprocessdate', batchRecord.bankFile.displayProcessDateSelector);
    $('#generate-file-modal .btn-primary').on('click', batchRecord.bankFile.download);
};

batchRecord.init = function() {

    var batchNo = $('#batch-no').text();

    $('.input-group.date').datepicker({
        format: 'dd M yyyy',
        autoclose: true,
        forceParse: false,
        todayHighlight: true
    });

    $('#report').load('/batches/' + mode.current.type + '/viewlines/' + batchNo);
};