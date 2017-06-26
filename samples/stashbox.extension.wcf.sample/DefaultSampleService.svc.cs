using stashbox.extension.wcf.sample.domain;

namespace stashbox.extension.wcf.sample
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DefaultSampleService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select DefaultSampleService.svc or DefaultSampleService.svc.cs at the Solution Explorer and start debugging.
    public class DefaultSampleService : IDefaultSampleService
    {
        private readonly IUserRepository _userRepository;
        private readonly IProductRepository _productRepository;

        public DefaultSampleService(IUserRepository userRepository, IProductRepository productRepository)
        {
            _userRepository = userRepository;
            _productRepository = productRepository;
        }

        public User GetUserInfo(int userId) => _userRepository.GetById(userId);

        public Product GetProductInfo(int productId) => _productRepository.GetById(productId);
    }
}
