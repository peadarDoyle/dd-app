using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DirectDebits.ViewModels.Banking
{
    public class CreateBankFileViewModel
    {
        /// <summary>
        /// We don't validate the BatchNum - if it is changed on the client side, so be it.
        /// It will either be a valid batch number or it will cause an exception to be thrown
        /// further down the stack.
        /// </summary>
        public int BatchNum { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("the Process Date")]
        public DateTime? ProcessDate { get; set; }
    }
}
