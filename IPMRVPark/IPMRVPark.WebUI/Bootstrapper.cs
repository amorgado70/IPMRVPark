using System.Web.Mvc;
using Microsoft.Practices.Unity;
using Unity.Mvc3;
using IPMRVPark.Contracts.Repositories;
using IPMRVPark.Models;

namespace IPMRVPark.WebUI
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers
            
            // e.g. container.RegisterType<ITestService, TestService>();            
           
            container.RegisterType<IRepositoryBase<customer>, CustomerRepository>();
            container.RegisterType<IRepositoryBase<customer_view>, CustomerViewRepository>();
            container.RegisterType<IRepositoryBase<reservation_view>, ReservationViewRepository>();
            container.RegisterType<IRepositoryBase<provincecode>, ProvinceRepository>();
            container.RegisterType<IRepositoryBase<countrycode>, CountryRepository>();
            container.RegisterType<IRepositoryBase<ipmevent>, IPMEventRepository>();
            container.RegisterType<IRepositoryBase<session>, SessionRepository>();
            container.RegisterType<IRepositoryBase<selecteditem>, SelectedItemRepository>();
            container.RegisterType<IRepositoryBase<staff>, StaffRepository>();
            container.RegisterType<IRepositoryBase<rvsite_available_view>, RVSiteAvailableViewRepository>();
            container.RegisterType<IRepositoryBase<total_per_selecteditem_view>, TotalPerSeletedItemViewRepository>();
            container.RegisterType<IRepositoryBase<site_description_rate_view>, SiteDescriptionRateViewRepository>();

            container.RegisterType<IRepositoryBase<placeinmap>, PlaceInMapRepository>();
            container.RegisterType<IRepositoryBase<reasonforpayment>, ReasonForPaymentRepository>();
            container.RegisterType<IRepositoryBase<paymentmethod>, PaymentMethodRepository>();
            container.RegisterType<IRepositoryBase<payment>, PaymentRepository>();
            container.RegisterType<IRepositoryBase<reservationitem>, ReservationItemRepository>();
            container.RegisterType<IRepositoryBase<total_per_session_view>, TotalPerSessionViewRepository>();
            container.RegisterType<IRepositoryBase<total_per_payment_view>, TotalPerPaymentViewRepository>();
            container.RegisterType<IRepositoryBase<total_per_reservationitem_view>, TotalPerReservationItemViewRepository>();
            container.RegisterType<IRepositoryBase<paymentreservationitem>, PaymentReservationItemRepository>();

            return container;
        }
    }
}