newBatch.filter = function(callback) {
    let classifications = $("#classification-filter .chosen-select").val();

    if (typeof classifications !== 'undefined' && classifications !== null) {
        classifications = classifications.map(function (classification) {
            return 'classificationIds=' + classification;
        }).join('&');
    }

    $('#process > div.button-container > button').prop('disabled', false);
    $('#allocation-table > thead > tr > th.check > input').prop('disabled', false);
    $('#allocation-table > thead > tr > th.check > input').prop('checked', false);

    callback(classifications);
};