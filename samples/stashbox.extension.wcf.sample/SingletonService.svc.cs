using stashbox.extension.wcf.sample.domain;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace stashbox.extension.wcf.sample
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, AddressFilterMode = AddressFilterMode.Any)]
    public class SingletonService : ISingletonService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;

        public SingletonService(IUserRepository userRepository, IProductRepository productRepository)
        {
            _userRepository = userRepository;
            _productRepository = productRepository;
        }

        public User GetUserInfo(int userId) => _userRepository.GetById(userId);

        public Product GetProductInfo(int productId) => _productRepository.GetById(productId);
    }
}
