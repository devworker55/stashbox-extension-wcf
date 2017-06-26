using stashbox.extension.wcf.sample.domain;
using System.ServiceModel;

namespace stashbox.extension.wcf.sample
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IDefaultSampleService" in both code and config file together.
    [ServiceContract]
    public interface IDefaultSampleService
    {
        [OperationContract]
        User GetUserInfo(int userId);

        [OperationContract]
        Product GetProductInfo(int productId);
    }
}
