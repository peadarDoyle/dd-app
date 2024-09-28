newBatch.baseTable = {}

newBatch.baseTable.checker = function (selector, table) {
    let isChecked = $(this).is(':checked');

    // uncheck header checkbox anytime a row checkbox is unchecked
    if (!isChecked) {
        $(selector + ' > thead input[type="checkbox"]').prop('checked', false);
    }

    let allChecked = true;
    let noneChecked = true;

    $(selector + ' > tbody input[type="checkbox"]').each(function () {

        // if both are set false, break, nothing more can be discovered
        if (!allChecked && !noneChecked) {
            return false;
        }

        if (allChecked && !$(this).is(':checked')) {
            allChecked = false;
        }

        if (noneChecked && $(this).is(':checked')) {
            noneChecked = false;
        }
    });

    if (allChecked) {
        $(selector + ' > thead input[type="checkbox"]').prop('checked', true);
    }

    if (noneChecked) {
        table.disableMassAllocate(true);
    } else {
        table.disableMassAllocate(false);
    }
}