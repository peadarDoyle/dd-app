$(window).load(function () {

    mode.onWindowLoad();

    if ($('#batch-record.page-container').length > 0) {
        batchRecord.onWindowLoad();
    }

    if ($('#new-batch.page-container').length > 0) {
        newBatch.onWindowLoad();
    }

    if ($('#settings.page-container').length > 0) {
        appSettings.onWindowLoad();
    }

    if ($('#register').length > 0) {
        registration.onWindowLoad();
    }

    // initialize Chosen
    $(".chosen-select").chosen({ max_selected_options: 8 });
});

document.onreadystatechange = function (e) {

    mode.onReadyStateChange();

    if ($('#new-batch.page-container').length > 0) {
        newBatch.onReadyStateChange();
    }

}