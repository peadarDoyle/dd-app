using DirectDebits.Common;
using MvcPaging;

namespace DirectDebits.ViewModels.Batches
{
    public class BatchListViewModel
    {
        public IPagedList<BatchHeaderViewModel> Batches { get; set; }
        public BatchType Type { get; set; }
    }
}
