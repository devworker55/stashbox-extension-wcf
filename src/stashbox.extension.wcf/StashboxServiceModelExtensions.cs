using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;

namespace Stashbox.Extension.Wcf
{
    public static class StashboxServiceModelExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        internal static ServiceMetadata GetMetaData(this ServiceHostBase host)
        {
            return ServiceMetadataProvider.GetMetadata(host.Description.ServiceType);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        internal static ServiceMetadata GetMetaData(this Type serviceType)
        {
            return ServiceMetadataProvider.GetMetadata(serviceType);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host"></param>
        /// <param name="behavior"></param>
        public static void AddServiceBehavior(this ServiceHostBase host, IServiceBehavior behavior)
        {
            host.Description.Behaviors.Add(behavior);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host"></param>
        /// <param name="operationBehavior"></param>
        public static void AddOperationBehavior(this ServiceHost host, IOperationBehavior operationBehavior)
        {
            var contractionDescriptions = host.Description
                                              .Endpoints.Select(ep => ep.Contract)
                                                        .Distinct()
                                                        .ToList();
            contractionDescriptions.ForEach(cd =>
            {
                foreach (var operationDescription in cd.Operations)
                {
                    operationDescription.Behaviors.Add(operationBehavior);
                }
            });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host"></param>
        /// <param name="operationBehaviorFunc"></param>
        public static void AddOperationBehavior(this ServiceHost host, Func<IOperationBehavior> operationBehaviorFunc)
        {
            var contractionDescriptions = host.Description
                                              .Endpoints.Select(ep => ep.Contract)
                                                        .Distinct()
                                                        .ToList();
            contractionDescriptions.ForEach(cd =>
            {
                foreach (var operationDescription in cd.Operations)
                {
                    operationDescription.Behaviors.Add(operationBehaviorFunc());
                }
            });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host"></param>
        /// <param name="behavior"></param>
        /// <param name="endpointName"></param>
        /// <param name="operationName"></param>
        public static void AddOperationBehavior(this ServiceHostBase host, IOperationBehavior behavior, string endpointName, string operationName)
        {
            var serviceEndpoint = host.Description.Endpoints.FirstOrDefault(ep => ep.Name == endpointName);
            var operationDescription = serviceEndpoint?.Contract.Operations.Find(operationName);
            operationDescription?.Behaviors.Add(behavior);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TOperationBehavior"></typeparam>
        /// <param name="host"></param>
        public static void InjectDependencies<TOperationBehavior>(this ServiceHostBase host) where TOperationBehavior : IOperationBehavior
        {
            var operationBehaviors = host.Description.Endpoints
                                         .SelectMany(epd => epd.Contract.Operations
                                                               .SelectMany(opd => opd.Behaviors
                                                                                     .Where(opb => (opb is TOperationBehavior))))
                                         .ToList();

            foreach (var behavior in operationBehaviors)
            {
                StashboxServiceHostFactoryBase.Container.BuildUp(behavior);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host"></param>
        /// <param name="operationName"></param>
        public static void InjectDependencies(this ServiceHostBase host, string operationName)
        {
            var serviceEndpoint = host.Description.Endpoints.FirstOrDefault();
            var operationDescription = serviceEndpoint?.Contract.Operations.Find(operationName);

            if (operationDescription != null)
                foreach (var operatoinbehavior in operationDescription.Behaviors)
                {
                    StashboxServiceHostFactoryBase.Container.BuildUp(operatoinbehavior);
                }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="host"></param>
        /// <param name="endpointName"></param>
        /// <param name="operationName"></param>
        public static void InjectDependencies(this ServiceHostBase host, string endpointName, string operationName)
        {
            var serviceEndpoint = host.Description.Endpoints.FirstOrDefault(ep => ep.Name == endpointName);
            var operationDescription = serviceEndpoint?.Contract.Operations.Find(operationName);

            if (operationDescription != null)
                foreach (var operatoinbehavior in operationDescription.Behaviors)
                {
                    StashboxServiceHostFactoryBase.Container.BuildUp(operatoinbehavior);
                }
        }
    }
}
