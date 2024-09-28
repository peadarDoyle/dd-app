registration.onWindowLoad = function () {
    registration.registerEvents();
}

registration.registerEvents = function () {
    $('#register-button').on('click', function () {
        $('#register-button').css('display','none');
        $('#register-spinner').css('display','block');
    });
};