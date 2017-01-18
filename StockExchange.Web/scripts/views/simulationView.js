﻿/**
 * View for the run simulation page
 */
(function ($) {
    'use strict';

    var $companySelect = $('.company-select');
    $companySelect.select2({
        placeholder: 'Choose companies',
        //width: '100%'
    });

    $("input[type='checkbox']").iCheck({
        checkboxClass: 'icheckbox_flat'
    });

    $('#StartDate').datepicker();
    $('#EndDate').datepicker();

    // Disable companies select when AllCompanies checkbox is checked
    $('#HasTransactionLimit')
        .on('ifChecked ifUnchecked', function() {
            $('.transactionLimit').toggleClass('hidden');
        });

    // Disables button on submit to prevent multiple clicks
    $('#run-simulation-form').on('submit', function () {
        var $button = $(this).find('[type="submit"]');
        if ($(this).valid()) {
            $button.prop('disabled', true);
        } else {
            $button.prop('disabled', false);
        }
    });

})(jQuery);