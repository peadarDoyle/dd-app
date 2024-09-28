mode.onWindowLoad = function () {
    mode.registerEvents();
}

mode.onReadyStateChange = function () {
    if (document.readyState === 'complete') {

        let type = mode.current.type;

        mode.loadSelector(type);

        $('#goto-home').attr("href", '/batches/' + type);
        $('#logo-link').attr("href", '/batches/' + type);
        $('#goto-settings').attr("href", '/settings/' + type);
    }
}

mode.registerEvents = function () {
    $('#mode').on('change', 'select', mode.changeMode);
};