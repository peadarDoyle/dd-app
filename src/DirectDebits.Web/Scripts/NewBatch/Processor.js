newBatch.processor = {

    openModal: function(e) {
        let totalAllocated = 0;
        let numberOfAccountsWithAllocations = 0;
        let invalidAccounts = [];
        
        $.each(newBatch.accounts, function(index, acc) {

            if (acc.hasAllocations()) {
                totalAllocated += acc.getAllocated();
                numberOfAccountsWithAllocations++;

                if (!acc.hasValidTotalAllocation()) {
                    invalidAccounts.push(acc.Name);
                }
            }
        });

        $('#process-modal  button.btn.btn-primary').prop('disabled', true);
        $('#process-modal > div > div > div.modal-body > div').hide();

        if (numberOfAccountsWithAllocations === 0) {
            $('#process-message').html('No allocations have been made!');
        } else if (totalAllocated <= 0) {
            $('#process-message').html('Cannot continue because the total allocation is <strong>' + totalAllocated.toFixed(2) + '</strong> but it must be greater than 0');
        } else if (invalidAccounts.length > 0) {
            let ul = htmlHelper.makeUnorderedList(invalidAccounts);
            $('#process-message').html('Cannot continue because the following account(s) have a zero or negative allocation:<br/>' + ul.outerHTML);
        } else {
            $('#process-modal  button.btn.btn-primary').prop('disabled', false);
            $('#process-modal > div > div > div.modal-body > div').show();

            let processMsg = 'The sum of <span id="process-total"></span> will be processed for <span id="process-account-count"></span> ' +
                              mode.current.accountType + '(s)';
            $('#process-message').html(processMsg); 
        }

        $('#process-total').text(totalAllocated.toFixed(2));
        $('#process-account-count').text(numberOfAccountsWithAllocations);
        $('#process-modal').modal({
            backdrop: 'static'
        });

        $('.input-group.date').datepicker({
            format: 'dd M yyyy',
            autoclose: true,
            forceParse: false,
            todayHighlight: true
        });

        let date = new Date();
        let offset = mode.current.getOffset(date.getDay());

        date.setDate(date.getDate() + offset);

        $('.input-group.date').datepicker('setDate', date);
    },

    beginProcessing: function(e) {
        $('#process .alert').css('display','block');
         $('#validation').html('');

        let invoiceAllocations = newBatch.accounts.map(function (account) {
            return account.getAllocatedInvoices();
        }).filter(function (account) {
            return account.Invoices.length > 0;
        });

        let url = '/batches/'+ mode.current.type + '/create';
        let date = $('#process-date-selector').val();
        let json = JSON.stringify(invoiceAllocations);

        $.post(url, { date: date, json: json })
         .done(function (result, a, b) {
             if (result.status === 401) {
                 $('#process .alert').css('display','none');
                 $('#validation').html('<span class=\"text-danger\">You have been automatically logged out.</span>');
             }
             else if (result.isSuccess) {
                 window.location.href = result.value;
             }
             else {
                 $('#process .alert').css('display','none');
                 $('#validation').html('<span class=\"text-danger\">' + result.errorMessage + '</span>');
             }
         })
         .fail(function (result) {
             $('#process .alert').css('display','none');
             $('#validation').html('<span class=\"text-danger\">An error occured when attempting to create the new batch.</span>');
         })
         .always(function () {
         });
    }

};