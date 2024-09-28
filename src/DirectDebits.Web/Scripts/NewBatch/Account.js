// The Account class is used to model the data for the NewBatch page.
// All data in the primary allocation table and the modal allocation tables
// is sourced directly from this class.
//
// N.B. The data in the allocation tables should never be directly manipulated with
// code. Data should be updated solely by redrawing the tables (be it the primary
// or modal table).
function Account(id, displayId, name, p1, p2, p3, older) {
    p1.map(datesToStrongTyping);
    p2.map(datesToStrongTyping);
    p3.map(datesToStrongTyping);
    older.map(datesToStrongTyping);

    this.Check = '<input type="checkbox"/>';
    this.Id = id;
    this.DisplayId = displayId;
    this.Name = name;
    this.P1 = p1;
    this.P2 = p2;
    this.P3 = p3;
    this.Older = older;

    this.hasAllocations = function () {
        return this.getAllocatedInvoices().Invoices.length > 0;
    }

    this.hasValidTotalAllocation = function() {
        return this.getAllocated() > 0;
    }

    function datesToStrongTyping(invoice) {
        let cleanDate = invoice.InvoiceDate
            .replace("/Date(", "")
            .replace(")/", "");

        let intDate = parseInt(cleanDate, 10)
        invoice.InvoiceDate = new Date(intDate);
    }

    this.anyCreditNotes = function (period) {
        switch (period) {
            case 'p1': invoices = this.P1;
                break;
            case 'p2': invoices = this.P2;
                break;
            case 'p3': invoices = this.P3;
                break;
            case 'older': invoices = this.Older;
                break;
        }

        for (var i = 0; i < invoices.length; i++) {
            if (invoices[i].Amount < 0) return true;
        }

        return false;
    };

    this.getP1Total = function () {
        return this.P1.reduce((a, b) => a + b.Amount, 0);
    };
    this.getP2Total = function () {
        return this.P2.reduce((a, b) => a + b.Amount, 0);
    };
    this.getP3Total = function () {
        return this.P3.reduce((a, b) => a + b.Amount, 0);
    };
    this.getOlderTotal = function () {
        return this.Older.reduce((a, b) => a + b.Amount, 0);
    };
    this.getTotal = function () {
        return this.getP1Total() +
            this.getP2Total() +
            this.getP3Total() +
            this.getOlderTotal();
    };
    this.getAllocated = function () {
        return this.P1.reduce((a, b) => a + b.Alloc, 0) +
            this.P2.reduce((a, b) => a + b.Alloc, 0) +
            this.P3.reduce((a, b) => a + b.Alloc, 0) +
            this.Older.reduce((a, b) => a + b.Alloc, 0);
    };

    this.allocateAll = function () {
        this.allocateP1();
        this.allocateP2();
        this.allocateP3();
        this.allocateOlder();
    };

    this.deallocateAll = function () {
        this.allocateP1(0);
        this.allocateP2(0);
        this.allocateP3(0);
        this.allocateOlder(0);
    };

    _isValidAllocation = function (invoice, alloc) {
        var isValidNumber = !isNaN(alloc);
        if (!isValidNumber) return { isValid: false, msg: invoice.Id + ' is not valid' };

        var isCreditNote = invoice.Amount < 0;

        if (isCreditNote) {
            if (alloc > 0) return { isValid: false, msg: invoice.Id + ' credit notes require a negative match' };
            else if (alloc < invoice.Amount) return { isValid: false, msg: invoice.Id + ' cannot exceeed amount' };
        } else {
            if (alloc < 0) return { isValid: false, msg: invoice.Id + ' cannot be negative' };
            else if (alloc > invoice.Amount) return { isValid: false, msg: invoice.Id + ' cannot exceeed amount' };
        }

        return { isValid: true };
    };

    this.allocate = function (period, id, alloc) {
        var result = { isValid: false };

        switch (period) {
            case 'p1':
                result = _allocate(this.P1, id, alloc);
                break;
            case 'p2':
                result = _allocate(this.P2, id, alloc);
                break;
            case 'p3':
                result = _allocate(this.P3, id, alloc);
                break;
            case 'older':
                result = _allocate(this.Older, id, alloc);
                break;
        }

        return result;
    };

    this.validateCollection = function (collection) {

        var invoices;

        switch (collection.period) {
            case 'p1': invoices = this.P1;
                break;
            case 'p2': invoices = this.P2;
                break;
            case 'p3': invoices = this.P3;
                break;
            case 'older': invoices = this.Older;
                break;
        }

        var overview = {
            isValid: true,
            results: []
        };

        $.each(collection.allocations, function (index, value) {
            var invoice = $.grep(invoices, function (invoice) {
                return invoice.Id === value.id;
            })[0];

            var result = _isValidAllocation(invoice, value.alloc);

            if (!result.isValid) {
                overview.isValid = false;
            }

            overview.results.push(result);
        });

        return overview;
    };

    this.allocateCollection = function (collection) {

        var invoices;

        switch (collection.period) {
            case 'p1': invoices = this.P1;
                break;
            case 'p2': invoices = this.P2;
                break;
            case 'p3': invoices = this.P3;
                break;
            case 'older': invoices = this.Older;
                break;
        }

        $.each(collection.allocations, function (index, value) {
            var invoice = $.grep(invoices, function (invoice) {
                return invoice.Id === value.id;
            })[0];

            _allocate(invoice, value.alloc);
        });
    };

    _allocate = function (invoice, alloc) {
        if (typeof alloc === 'undefined') {
            invoice.Alloc = invoice.Amount;
        } else {
            var val = parseFloat(alloc);
            invoice.Alloc = val;
        }
    };

    this.allocateP1 = function (val) {
        if (typeof val === 'undefined') {
            this.P1.map(function (invoice) {
                invoice.Alloc = invoice.Amount;
            });

            return true;
        }

        this.P1.map(function (invoice) {
            invoice.Alloc = val;
        });
    };

    this.allocateP2 = function (val) {
        if (typeof val === 'undefined') {
            this.P2.map(function (invoice) {
                invoice.Alloc = invoice.Amount;
            });

            return true;
        }

        this.P2.map(function (invoice) {
            invoice.Alloc = val;
        });
    };

    this.allocateP3 = function (val) {
        if (typeof val === 'undefined') {
            this.P3.map(function (invoice) {
                invoice.Alloc = invoice.Amount;
            });

            return true;
        }

        this.P3.map(function (invoice) {
            invoice.Alloc = val;
        });
    };

    this.allocateOlder = function (val) {
        if (typeof val === 'undefined') {
            this.Older.map(function (invoice) {
                invoice.Alloc = invoice.Amount;
            });

            return true;
        }

        this.Older.map(function (invoice) {
            invoice.Alloc = val;
        });
    };

    this.getAllocatedInvoices = function () {

        var allocatedP1Invoices = this.P1.filter(function (invoice) { return invoice.Alloc != 0; });
        var allocatedP2Invoices = this.P2.filter(function (invoice) { return invoice.Alloc != 0; });
        var allocatedP3Invoices = this.P3.filter(function (invoice) { return invoice.Alloc != 0; });
        var allocatedOlderInvoices = this.Older.filter(function (invoice) { return invoice.Alloc != 0; });

        return {
            Id: this.Id,
            DisplayId: this.DisplayId,
            Name: this.Name,
            Invoices: $.merge(allocatedOlderInvoices,
                $.merge(allocatedP3Invoices,
                    $.merge(allocatedP2Invoices, allocatedP1Invoices)
                )
            ).map(function (invoice) {
                return {
                    Id: invoice.Id,
                    Alloc: invoice.Alloc,
                    Amount: invoice.Amount
                };
            })
        };
    };
}