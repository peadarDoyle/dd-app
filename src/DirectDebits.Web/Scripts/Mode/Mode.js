mode.settings = {
    keys: []
};

mode.settings.keys['directdebit'] = {
    name: 'Direct Debit',
    shortName: 'DD',
    type: 'directdebit',
    accountType: 'Customer',
    getOffset: function (day) {
        if (day === 1 || day === 2) return 3;
        else return 5;
    }
};

mode.settings.keys['payment'] = {
    name: 'Payment',
    shortName: 'PAY',
    type: 'payment',
    accountType: 'Supplier',
    getOffset: function (day) {
        if (day === 5 || day === 6 || day == 0) return 3;
        else return 1;
    }
};

mode.current = mode.settings.keys[$("#batch-type").val()];

mode.changeMode = function (e) {
    window.location.href = "/batches/" + $('#mode select').val();
}

mode.loadSelector = function (type) {
    $("#mode").load("/settings/modes", function () {
        $('#mode > select').val(type);
    });
}
