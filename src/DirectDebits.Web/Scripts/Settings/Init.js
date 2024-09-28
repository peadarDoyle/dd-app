appSettings.onWindowLoad = function () {
    appSettings.registerEvents();
}

appSettings.registerEvents = function () {
    $('#invoice-periods a').on('click', function () {
        $('#periods-modal').modal({
            backdrop: 'static'
        });
    });

    $('#bank-details a').on('click', function () {
        $('#bank-details-modal').modal({
            backdrop: 'static'
        });
    });

    $('#exact-config a').on('click', function () {
        $('#exact-config-modal').modal({
            backdrop: 'static'
        });
    });

    $('#app-config a').on('click', function () {
        $('#app-config-modal').modal({
            backdrop: 'static'
        });
    });
};