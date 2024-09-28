batchRecord.bankFile = {
    openModal: function () {
        batchRecord.bankFile.displayProcessDateSelector();

        $('#generate-file-modal').modal({
            backdrop: 'static'
        });
    },

    download: function () {
        $('#messages').html('<p>Bank File request submitted, please wait on download...</p>');
        $('#generate-file-modal').modal('hide');
    },

    displayProcessDateSelector: function () {

        var isChecked = $('#batch-record #hasnewprocessdate').is(':checked');

        if (isChecked) {
            $('.input-group.date').css('display', 'table');
        } else {
            $('.input-group.date').css('display', 'none');
            $('#process-date-selector').val('');
        }
    }
};
